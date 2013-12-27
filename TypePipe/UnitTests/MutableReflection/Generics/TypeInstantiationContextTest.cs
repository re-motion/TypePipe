// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiationContextTest
  {
    private TypeInstantiationContext _context;

    private Type _genericTypeDefinition;
    private Type _customType;
    private TypeInstantiationInfo _info;

    private Type _parameter;
    private Type _argument;
    private Dictionary<Type, Type> _parametersToArguments;

    [SetUp]
    public void SetUp ()
    {
      _context = new TypeInstantiationContext();

      _genericTypeDefinition = typeof (List<>);
      _customType = CustomTypeObjectMother.Create();
      _info = new TypeInstantiationInfo (_genericTypeDefinition, new[] { _customType }.AsOneTime());

      _parameter = typeof (GenericType<>).GetGenericArguments().Single();
      _argument = ReflectionObjectMother.GetSomeType();
      _parametersToArguments = new Dictionary<Type, Type> { { _parameter, _argument } };
    }

    [Test]
    public void Instantiate_CustomTypeArgument ()
    {
      var result = _context.Instantiate (_info);

      Assert.That (result, Is.TypeOf<TypeInstantiation>());
      Assert.That (result.GetGenericTypeDefinition(), Is.SameAs (_genericTypeDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Instantiate_CustomGenericTypeDefinition ()
    {
      var typeParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var customGenericTypeDefinition = CustomTypeObjectMother.Create (typeArguments: new[] { typeParameter });
      var instantiationInfo = new TypeInstantiationInfo (customGenericTypeDefinition, new[] { _customType });

      var result = _context.Instantiate (instantiationInfo);

      Assert.That (result, Is.TypeOf<TypeInstantiation>());
      Assert.That (result.GetGenericTypeDefinition(), Is.SameAs (customGenericTypeDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Instantiate_RuntimeTypeArgument ()
    {
      var runtimeType = ReflectionObjectMother.GetSomeType();
      var info = new TypeInstantiationInfo (_genericTypeDefinition, new[] { runtimeType });

      var result = _context.Instantiate (info);

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (result.GetGenericTypeDefinition(), Is.SameAs (_genericTypeDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { runtimeType }));
    }

    [Test]
    public void Instantiate_AlreadyInContext ()
    {
      var instantiation = _context.Instantiate (_info);

      var result1 = _context.Instantiate (_info);
      var result2 = new TypeInstantiationContext().Instantiate (_info);

      Assert.That (result1, Is.SameAs (instantiation));
      Assert.That (result2, Is.Not.SameAs (instantiation));
    }

    [Test]
    public void Add ()
    {
      var instantiation = TypeInstantiationObjectMother.Create();
      _context.Add (_info, instantiation);

      var result = _context.Instantiate (_info);

      Assert.That (result, Is.SameAs (instantiation));
    }

    [Test]
    public void SubstituteGenericParameters_GenericParameter ()
    {
      var result = _context.SubstituteGenericParameters (_parameter, _parametersToArguments);

      Assert.That (result, Is.SameAs (_argument));
    }

    [Test]
    public void SubstituteGenericParameters_GenericParameter_NoMatch ()
    {
      var parametersToArguments = new Dictionary<Type, Type>();

      var result = _context.SubstituteGenericParameters (_parameter, parametersToArguments);

      Assert.That (result, Is.SameAs (_parameter));
    }

    [Test]
    public void SubstituteGenericParameters_RecursiveGenericType ()
    {
      var recursiveGeneric = typeof (GenericType<>).GetField ("RecursiveGeneric").FieldType;

      var list = _context.SubstituteGenericParameters (recursiveGeneric, _parametersToArguments);

      Assert.That (list.GetGenericTypeDefinition (), Is.SameAs (typeof (List<>)));
      var func = list.GetGenericArguments ().Single ();
      Assert.That (func.GetGenericTypeDefinition (), Is.SameAs (typeof (Func<>)));
      var typeArgument = func.GetGenericArguments ().Single ();
      Assert.That (typeArgument, Is.SameAs (_argument));
    }

    [Test]
    public void SubstituteGenericParameters_VectorType ()
    {
      var vectorType = _parameter.MakeArrayType();

      var result = _context.SubstituteGenericParameters (vectorType, _parametersToArguments);

      Assert.That (result.IsArray, Is.True);
      Assert.That (result.GetElementType(), Is.SameAs (_argument));
      Assert.That (result.GetArrayRank(), Is.EqualTo (1));
      Assert.That (result.GetConstructors().Length, Is.EqualTo (1), "Vectors have a single constructor.");
    }

    [Test]
    public void SubstituteGenericParameters_VectorVectorType ()
    {
      var vectorType = _parameter.MakeArrayType().MakeArrayType();

      var result = _context.SubstituteGenericParameters(vectorType, _parametersToArguments);

      Assert.That(result, Is.EqualTo(_argument.MakeArrayType().MakeArrayType()));
    }

    [Test]
    public void SubstituteGenericParameters_MultiDimensionalArrayType ()
    {
      var multiDimensionalArrayType = _parameter.MakeArrayType (2);

      var result = _context.SubstituteGenericParameters (multiDimensionalArrayType, _parametersToArguments);

      Assert.That (result.IsArray, Is.True);
      Assert.That (result.GetElementType(), Is.SameAs (_argument));
      Assert.That (result.GetArrayRank(), Is.EqualTo (2));
      Assert.That (result.GetConstructors().Length, Is.EqualTo (2), "Multi-dimensional arrays have two constructors.");
    }

    [Test]
    public void SubstituteGenericParameters_ByRefType ()
    {
      var byRefType = _parameter.MakeByRefType();

      var result = _context.SubstituteGenericParameters (byRefType, _parametersToArguments);

      Assert.That (result.IsByRef, Is.True);
      Assert.That (result.GetElementType(), Is.SameAs (_argument));
    }

    [Test]
    public void SubstituteGenericParameters_NonGenericType ()
    {
      var nonGeneric = ReflectionObjectMother.GetSomeNonGenericType ();
      var result = _context.SubstituteGenericParameters (nonGeneric, _parametersToArguments);

      Assert.That (result, Is.SameAs (nonGeneric));
    }

    class GenericType<T>
    {
      public T Field;
      public List<Func<T>> RecursiveGeneric;
    }
  }
}