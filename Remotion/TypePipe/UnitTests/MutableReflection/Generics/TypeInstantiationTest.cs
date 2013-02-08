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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiationTest
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private MemberSelector _memberSelector;
    private ThrowingUnderlyingTypeFactory _underlyingTypeFactory;

    private Type _genericTypeDefinition;
    private CustomType _customType;
    private Type[] _typeArguments;
    private Type _runtimeType;
    private Type[] _typeArgumentsWithRuntimeType;

    private Dictionary<InstantiationInfo, TypeInstantiation> _instantiationContext1;
    private Dictionary<InstantiationInfo, TypeInstantiation> _instantiationContext2;

    private TypeInstantiation _instantiation;
    private TypeInstantiation _instantiationWithRuntimeType;

    [SetUp]
    public void SetUp ()
    {
      _memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      _underlyingTypeFactory = new ThrowingUnderlyingTypeFactory();

      _genericTypeDefinition = typeof (GenericType<>);
      _customType = CustomTypeObjectMother.Create (fullName: "MyNs.Blub");
      _typeArguments = new Type[] { _customType };
      _runtimeType = ReflectionObjectMother.GetSomeType();
      _typeArgumentsWithRuntimeType = new[] { _runtimeType };

      _instantiationContext1 = new Dictionary<InstantiationInfo, TypeInstantiation>();
      _instantiationContext2 = new Dictionary<InstantiationInfo, TypeInstantiation>();

      var info1 = new InstantiationInfo (_genericTypeDefinition, _typeArguments);
      var info2 = new InstantiationInfo (_genericTypeDefinition, _typeArgumentsWithRuntimeType);

      _instantiation = TypeInstantiation.Create (info1, _instantiationContext1, _memberSelector, _underlyingTypeFactory);
      _instantiationWithRuntimeType = TypeInstantiation.Create (info2, _instantiationContext2, _memberSelector, _underlyingTypeFactory);
    }

    [Test]
    public void Create_Initialization ()
    {
      Assert.That (_instantiation.Name, Is.EqualTo (_genericTypeDefinition.Name));
      Assert.That (_instantiation.Namespace, Is.EqualTo (_genericTypeDefinition.Namespace));
      Assert.That (_instantiation.Attributes, Is.EqualTo (_genericTypeDefinition.Attributes));
      Assert.That (_instantiation.IsGenericType, Is.True);
      Assert.That (_instantiation.IsGenericTypeDefinition, Is.False);
      Assert.That (_instantiation.GetGenericTypeDefinition(), Is.SameAs (_genericTypeDefinition));
      Assert.That (_instantiation.GetGenericArguments(), Is.EqualTo (_typeArguments));
    }

    [Test]
    public void Create_BaseType ()
    {
      var baseType = _instantiation.BaseType;
      Assertion.IsNotNull (baseType);
      Assert.That (baseType, Is.TypeOf<TypeInstantiation>());
      Assert.That (baseType.GetGenericTypeDefinition(), Is.SameAs (typeof (BaseType<>)));
      Assert.That (_instantiation.GetGenericArguments(), Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Create_NameAndFullName ()
    {
      Assert.That (_instantiation.Name, Is.EqualTo ("GenericType`1"));
      Assert.That (
          _instantiation.FullName,
          Is.EqualTo (
              "Remotion.TypePipe.UnitTests.MutableReflection.Generics.TypeInstantiationTest+GenericType`1[[MyNs.Blub, TypePipe_GeneratedAssembly]]"));
    }

    [Test]
    public void Create_Interfaces ()
    {
      var iface = _instantiation.GetInterfaces().Single();

      Assert.That (iface, Is.TypeOf<TypeInstantiation>());
      Assert.That (iface.GetGenericTypeDefinition(), Is.SameAs (typeof (IMyInterface<>)));
      Assert.That (_instantiation.GetGenericArguments(), Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Create_Fields ()
    {
      var field = _instantiation.GetField ("Field", c_allMembers);

      var fieldOnGenericType = _genericTypeDefinition.GetField ("Field", c_allMembers);
      Assert.That (field, Is.TypeOf<FieldOnTypeInstantiation> ());
      Assert.That (field.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (field.As<FieldOnTypeInstantiation> ().FieldOnGenericType, Is.EqualTo (fieldOnGenericType));
    }

    [Test]
    public void Create_Constructors ()
    {
      var constructor = _instantiation.GetConstructors (c_allMembers).Single();

      var constructorOnGenericType = _genericTypeDefinition.GetConstructors().Single();
      Assert.That (constructor, Is.TypeOf<ConstructorOnTypeInstantiation>());
      Assert.That (constructor.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (constructor.As<ConstructorOnTypeInstantiation>().ConstructorOnGenericType, Is.EqualTo (constructorOnGenericType));
    }

    [Test]
    public void Create_Methods ()
    {
      var method = _instantiation.GetMethod ("Method", c_allMembers);

      var methodOnGenericType = _genericTypeDefinition.GetMethod ("Method", c_allMembers);
      Assert.That (method, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (method.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (method.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (methodOnGenericType));
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Property DeclaringType is not supported.")]
    public void DeclaringType ()
    {
      Dev.Null = _instantiation.DeclaringType;
    }

    [Test]
    public void SubstituteGenericParameters_CustomType ()
    {
      var recursiveGeneric = _genericTypeDefinition.GetField ("RecursiveGeneric").FieldType;
      var list = _instantiation.SubstituteGenericParameters (recursiveGeneric);

      Assert.That (list, Is.TypeOf<TypeInstantiation>());
      Assert.That (list.GetGenericTypeDefinition(), Is.SameAs (typeof (List<>)));

      var func = list.GetGenericArguments().Single();
      Assert.That (func, Is.TypeOf<TypeInstantiation>());
      Assert.That (func.GetGenericTypeDefinition(), Is.SameAs (typeof (Func<>)));

      var typeArgument = func.GetGenericArguments().Single();
      Assert.That (typeArgument, Is.SameAs (_customType));
    }

    [Test]
    public void SubstituteGenericParameters_RuntimeType ()
    {
      var recursiveGeneric = _genericTypeDefinition.GetField ("RecursiveGeneric").FieldType;
      var list = _instantiationWithRuntimeType.SubstituteGenericParameters (recursiveGeneric);

      Assert.That (list.IsRuntimeType(), Is.True);
      Assert.That (list.GetGenericTypeDefinition(), Is.SameAs (typeof (List<>)));

      var func = list.GetGenericArguments().Single();
      Assert.That (func.IsRuntimeType(), Is.True);
      Assert.That (func.GetGenericTypeDefinition(), Is.SameAs (typeof (Func<>)));

      var typeArgument = func.GetGenericArguments().Single();
      Assert.That (typeArgument, Is.SameAs (_runtimeType));
    }

    [Test]
    public void SubstituteGenericParameters_NonGenericType ()
    {
      var nonGeneric = ReflectionObjectMother.GetSomeNonGenericType();
      var result = _instantiationWithRuntimeType.SubstituteGenericParameters (nonGeneric);

      Assert.That (result, Is.SameAs (nonGeneric));
    }

    [Ignore]
    [Test]
    public void SubstituteGenericParameters_RecursiveGenericInBaseType ()
    {
      var genericRuntimeType = typeof (RecursiveGenericType<int>);
      var genericBaseRuntimeType = typeof (BaseType<RecursiveGenericType<int>>);
      Assert.That (genericRuntimeType, Is.SameAs (genericBaseRuntimeType.GetGenericArguments().Single()), "Assert original reflection behavior.");

      var genericTypeDefinition = typeof (RecursiveGenericType<>);
      var typeArguments = new Type[] { _customType };
      var info = new InstantiationInfo (genericTypeDefinition, typeArguments);
      var instantiation = TypeInstantiation.Create (info, _instantiationContext1, _memberSelector, _underlyingTypeFactory);

      Assert.That (instantiation, Is.SameAs (instantiation.BaseType.GetGenericArguments().Single()));
    }

    interface IMyInterface<T> { }
    class BaseType<T> { }
    private class GenericType<T> : BaseType<T>, IMyInterface<T>
    {
      public List<Func<T>> RecursiveGeneric;
      public T Field;
      public GenericType (T arg) { }
      public void Method (T arg) { }
    }
    class RecursiveGenericType<T> : BaseType<RecursiveGenericType<T>> { }
  }
}