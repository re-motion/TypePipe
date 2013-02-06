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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiatorTest
  {
    private Type _nonGenericType;
    private Type _genericType;
    private Type[] _typeParameters;
    private CustomType _customTypeArgument;
    private Type[] _typeArgs;
    private Type[] _typeArgsWithRuntimeTypes;

    private IMemberSelector _memberSelectorMock;
    private ThrowingUnderlyingTypeFactory _underlyingTypeFactory;

    private TypeInstantiator _typeInstantiator;
    private TypeInstantiator _typeInstantiatorWithRuntimeTypes;
    private TypeInstantiation _declaringType;

    [SetUp]
    public void SetUp ()
    {
      _nonGenericType = typeof (NonGenerictype);
      _genericType = typeof (GenericType<,>);
      _declaringType = TypeInstantiationObjectMother.Create ();

      _customTypeArgument = CustomTypeObjectMother.Create(fullName: "My.Blub");
      _typeParameters = _genericType.GetGenericArguments();
      _typeArgs = new[] { typeof (string), _customTypeArgument };
      _typeArgsWithRuntimeTypes = new[] { typeof (List<int>), typeof (string) };
      var mapping = new Dictionary<Type, Type> { { _typeParameters[0], _typeArgs[0] }, { _typeParameters[1], _typeArgs[1] } };
      var mappingWithRuntimeTypes =
          new Dictionary<Type, Type> { { _typeParameters[0], _typeArgsWithRuntimeTypes[0] }, { _typeParameters[1], _typeArgsWithRuntimeTypes[1] } };

      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _underlyingTypeFactory = new ThrowingUnderlyingTypeFactory();

      _typeInstantiator = new TypeInstantiator (_memberSelectorMock, _underlyingTypeFactory, mapping);
      _typeInstantiatorWithRuntimeTypes = new TypeInstantiator (_memberSelectorMock, _underlyingTypeFactory, mappingWithRuntimeTypes);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_typeInstantiator.TypeArguments, Is.EqualTo (_typeArgs));
    }

    [Test]
    public void GetFullName ()
    {
      var result1 = _typeInstantiator.GetFullName (_genericType);
      var result2 = _typeInstantiatorWithRuntimeTypes.GetFullName (_genericType);

      Assert.That (result1, Is.EqualTo (
          "Remotion.TypePipe.UnitTests.MutableReflection.Generics.TypeInstantiatorTest+GenericType`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[My.Blub, TypePipe_GeneratedAssembly]]"));

      var constructedType = _genericType.MakeGenericType (_typeArgsWithRuntimeTypes);
      Assert.That (result2, Is.EqualTo (constructedType.FullName), "Should be equal to original reflection.");
    }

    [Test]
    public void SubstituteGenericParameters_Type_GenericType_CustomTypeAsArgument ()
    {
      var result = _typeInstantiator.SubstituteGenericParameters (_genericType);

      Assert.That (result, Is.TypeOf<TypeInstantiation>());
      Assert.That (result.GetGenericArguments(), Is.EqualTo (_typeArgs));
    }

    [Test]
    public void SubstituteGenericParameters_Type_GenericType_OnlyRuntimeTypeAsArguments ()
    {
      var result = _typeInstantiatorWithRuntimeTypes.SubstituteGenericParameters (_genericType);

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (result.GetGenericArguments(), Is.EqualTo (_typeArgsWithRuntimeTypes));
    }

    [Test]
    public void SubstituteGenericParameters_Type_NonGenericType ()
    {
      var result = _typeInstantiator.SubstituteGenericParameters (_nonGenericType);

      Assert.That (result, Is.SameAs (_nonGenericType));
    }

    [Test]
    public void SubstituteGenericParameters_Field ()
    {
      var field = _genericType.GetField ("Field");

      var result = _typeInstantiator.SubstituteGenericParameters (_declaringType, field);

      Assert.That (result, Is.TypeOf<FieldOnTypeInstantiation>());
      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (((FieldOnTypeInstantiation) result).FieldOnGenericType, Is.SameAs (field));
    }

    [Test]
    public void SubstituteGenericParameters_Constructor ()
    {
      var ctor = _genericType.GetConstructors().Single();

      var result = _typeInstantiator.SubstituteGenericParameters (_declaringType, ctor);

      Assert.That (result, Is.TypeOf<ConstructorOnTypeInstantiation>());
      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (((ConstructorOnTypeInstantiation) result).ConstructorOnGenericType, Is.SameAs (ctor));
    }

    [Test]
    public void SubstituteGenericParameters_Method ()
    {
      var method = _genericType.GetMethod ("Method");

      var result = _typeInstantiator.SubstituteGenericParameters (_declaringType, method);

      Assert.That (result, Is.TypeOf<MethodOnTypeInstantiation> ());
      Assert.That (result.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (((MethodOnTypeInstantiation) result).MethodOnGenericType, Is.SameAs (method));
    }

    class NonGenerictype { }
    // ReSharper disable UnusedTypeParameter
    class GenericType<T1, T2>
    {
      public T1 Field = default (T1);
      public GenericType (T1 t1) { Dev.Null = t1; }
      public T1 Method (T2 t2) { Dev.Null = t2; return default (T1); }
    }
    // ReSharper restore UnusedTypeParameter
  }
}