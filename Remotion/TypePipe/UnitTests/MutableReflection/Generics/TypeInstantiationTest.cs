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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;
using Remotion.Development.UnitTesting;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiationTest
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private MemberSelector _memberSelector;

    private Type _genericTypeDefinition;
    private CustomType _outerCustomType;
    private CustomType _customType;
    private Type[] _typeArguments;
    private TypeInstantiationInfo _instantiationInfo;

    private TypeInstantiation _instantiation;

    [SetUp]
    public void SetUp ()
    {
      _memberSelector = new MemberSelector (new BindingFlagsEvaluator());

      _genericTypeDefinition = typeof (DeclaringType<>.GenericType<>);
      _outerCustomType = CustomTypeObjectMother.Create();
      _customType = CustomTypeObjectMother.Create();
      _typeArguments = new Type[] { _outerCustomType, _customType };
      _instantiationInfo = new TypeInstantiationInfo (_genericTypeDefinition, _typeArguments);

      _instantiation = new TypeInstantiation (_memberSelector, _instantiationInfo, new TypeInstantiationContext());
    }

    [Test]
    public void Initialization_Initialization ()
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
    public void Initialization_DeclaringType ()
    {
      var declaringType = _instantiation.DeclaringType;

      Assert.That (declaringType, Is.TypeOf<TypeInstantiation>());
      Assert.That (declaringType.GetGenericTypeDefinition(), Is.SameAs (typeof (DeclaringType<>)));
      Assert.That (declaringType.GetGenericArguments(), Is.EqualTo (new[] { _outerCustomType }));
    }

    [Test]
    public void Initialization_BaseType ()
    {
      var baseType = _instantiation.BaseType;

      Assertion.IsNotNull (baseType);
      Assert.That (baseType, Is.TypeOf<TypeInstantiation>());
      Assert.That (baseType.GetGenericTypeDefinition(), Is.SameAs (typeof (BaseType<>)));
      Assert.That (baseType.GetGenericArguments(), Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Initialization_Interfaces ()
    {
      var iface = _instantiation.GetInterfaces().Single();

      Assert.That (iface, Is.TypeOf<TypeInstantiation>());
      Assert.That (iface.GetGenericTypeDefinition(), Is.SameAs (typeof (IMyInterface<>)));
      Assert.That (iface.GetGenericArguments(), Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Initialization_Fields ()
    {
      var field = _instantiation.GetField ("Field", c_allMembers);

      var fieldOnGenericType = _genericTypeDefinition.GetField ("Field", c_allMembers);
      Assertion.IsNotNull (field);
      Assert.That (field, Is.TypeOf<FieldOnTypeInstantiation> ());
      Assert.That (field.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (field.As<FieldOnTypeInstantiation> ().FieldOnGenericType, Is.EqualTo (fieldOnGenericType));
    }

    [Test]
    public void Initialization_Constructors ()
    {
      var constructor = _instantiation.GetConstructors (c_allMembers).Single();

      var constructorOnGenericType = _genericTypeDefinition.GetConstructors().Single();
      Assert.That (constructor, Is.TypeOf<ConstructorOnTypeInstantiation>());
      Assert.That (constructor.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (constructor.As<ConstructorOnTypeInstantiation>().ConstructorOnGenericType, Is.EqualTo (constructorOnGenericType));
    }

    [Test]
    public void Initialization_Methods ()
    {
      var method = _instantiation.GetMethod ("Method", c_allMembers);

      var methodOnGenericType = _genericTypeDefinition.GetMethod ("Method", c_allMembers);
      Assert.That (method, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (method.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (method.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (methodOnGenericType));
    }

    [Test]
    public void Initialization_Properties ()
    {
      var property = _instantiation.GetProperty ("Property", c_allMembers);

      var propertyOnGenericType = _genericTypeDefinition.GetProperty ("Property", c_allMembers);
      Assert.That (property, Is.TypeOf<PropertyOnTypeInstantiation> ());
      Assert.That (property.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (property.As<PropertyOnTypeInstantiation>().PropertyOnGenericType, Is.EqualTo (propertyOnGenericType));
      Assert.That (
          property.GetGetMethod (true).As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (propertyOnGenericType.GetGetMethod (true)));
      Assert.That (
          property.GetSetMethod (true).As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (propertyOnGenericType.GetSetMethod (true)));
    }

    [Test]
    public void Initialization_ReadOnlyAndWriteOnlyProperty ()
    {
      var info1 = new TypeInstantiationInfo (typeof (GenericTypeWithProperties<>), new Type[] { _customType });
      var instantiation = new TypeInstantiation (_memberSelector, info1, new TypeInstantiationContext());

      var property1 = instantiation.GetProperty ("ReadOnlyProperty", c_allMembers);
      var property2 = instantiation.GetProperty ("WriteOnlyProperty", c_allMembers);
      Assert.That (property1.GetSetMethod (true), Is.Null);
      Assert.That (property2.GetGetMethod (true), Is.Null);
    }

    [Test]
    public void Initialization_Events ()
    {
      var event_ = _instantiation.GetEvent ("Event", c_allMembers);
      Assertion.IsNotNull (event_);

      var eventOnGenericType = _genericTypeDefinition.GetEvent ("Event", c_allMembers);
      Assertion.IsNotNull (eventOnGenericType);
      Assert.That (event_, Is.TypeOf<EventOnTypeInstantiation>());
      Assert.That (event_.DeclaringType, Is.SameAs (_instantiation));
      Assert.That (event_.As<EventOnTypeInstantiation>().EventOnGenericType, Is.EqualTo (eventOnGenericType));
      Assert.That (
          event_.GetAddMethod (true).As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (eventOnGenericType.GetAddMethod (true)));
      Assert.That (
          event_.GetRemoveMethod (true).As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (eventOnGenericType.GetRemoveMethod (true)));
    }

    [Test]
    public void Initialization_UsesAllBindingFlagsToRetrieveMembers ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      var fields = _genericTypeDefinition.GetFields (bindingFlags);
      var ctors = _genericTypeDefinition.GetConstructors (bindingFlags);
      var methods = _genericTypeDefinition.GetMethods (bindingFlags);
      var properties = _genericTypeDefinition.GetProperties (bindingFlags);
      var events = _genericTypeDefinition.GetEvents (bindingFlags);

      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      var typeParameters = new[] { ReflectionObjectMother.GetSomeType() };
      var genericTypeDefinition = CustomTypeObjectMother.Create (memberSelectorMock, typeArguments: typeParameters,
          fields: fields, constructors: ctors, methods: methods, properties: properties, events: events);

      memberSelectorMock.Expect (mock => mock.SelectFields (fields, bindingFlags, genericTypeDefinition)).Return (fields);
      memberSelectorMock.Expect (mock => mock.SelectMethods (ctors, bindingFlags, genericTypeDefinition)).Return (ctors);
      memberSelectorMock.Expect (mock => mock.SelectMethods (methods, bindingFlags, genericTypeDefinition)).Return (methods);
      memberSelectorMock.Expect (mock => mock.SelectProperties (properties, bindingFlags, genericTypeDefinition)).Return (properties);
      memberSelectorMock.Expect (mock => mock.SelectEvents (events, bindingFlags, genericTypeDefinition)).Return (events);

      var typeArguments = new[] { ReflectionObjectMother.GetSomeType() };
      var info = new TypeInstantiationInfo (genericTypeDefinition, typeArguments);

      Dev.Null = new TypeInstantiation (memberSelectorMock, info, new TypeInstantiationContext());

      memberSelectorMock.VerifyAllExpectations();
    }

    [Test]
    public void SubstituteGenericParameters_GenericParameter ()
    {
      var genericParameter = _genericTypeDefinition.GetField ("Field").FieldType;

      var result = _instantiation.SubstituteGenericParameters (genericParameter);

      Assert.That (result, Is.SameAs (_customType));
    }

    [Test]
    public void Equals_Object ()
    {
      var info1 = new TypeInstantiationInfo (_genericTypeDefinition, _typeArguments.Reverse());
      var info2 = new TypeInstantiationInfo (_genericTypeDefinition, _typeArguments);
      Assert.That (info1, Is.Not.EqualTo (_instantiationInfo));
      Assert.That (info2, Is.EqualTo (_instantiationInfo));

      var instantiation1 = new TypeInstantiation (_memberSelector, info1, new TypeInstantiationContext());
      var instantiation2 = new TypeInstantiation (_memberSelector, info2, new TypeInstantiationContext());

      Assert.That (_instantiation.Equals ((object) null), Is.False);
      Assert.That (_instantiation.Equals (new object()), Is.False);
      Assert.That (_instantiation.Equals ((object) instantiation1), Is.False);
      Assert.That (_instantiation.Equals ((object) instantiation2), Is.True);
    }

    [Test]
    public void Equals_Type ()
    {
      var info1 = new TypeInstantiationInfo (_genericTypeDefinition, _typeArguments.Reverse());
      var info2 = new TypeInstantiationInfo (_genericTypeDefinition, _typeArguments);
      Assert.That (info1, Is.Not.EqualTo (_instantiationInfo));
      Assert.That (info2, Is.EqualTo (_instantiationInfo));

      var instantiation1 = new TypeInstantiation (_memberSelector, info1, new TypeInstantiationContext());
      var instantiation2 = new TypeInstantiation (_memberSelector, info2, new TypeInstantiationContext());

      // ReSharper disable CheckForReferenceEqualityInstead.1
      Assert.That (_instantiation.Equals (null), Is.False);
      // ReSharper restore CheckForReferenceEqualityInstead.1
      Assert.That (_instantiation.Equals (instantiation1), Is.False);
      Assert.That (_instantiation.Equals (instantiation2), Is.True);
    }

    [Test]
    public new void GetHashCode ()
    {
      Assert.That (_instantiation.GetHashCode(), Is.EqualTo (_instantiationInfo.GetHashCode()));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var result = _instantiation.GetCustomAttributeData().Single();
      Assert.That (result.Type, Is.SameAs (typeof (AbcAttribute)));
      Assert.That (result.ConstructorArguments, Is.EqualTo (new[] { "generic type def" }));
    }

    [Test]
    public void IntegrationOfInitializationAndSubstitution_RecursiveGenericInBaseType ()
    {
      var genericRuntimeType = typeof (RecursiveGenericType<int>);
      var genericBaseRuntimeType = typeof (BaseType<RecursiveGenericType<int>>);
      Assert.That (genericRuntimeType, Is.SameAs (genericBaseRuntimeType.GetGenericArguments().Single()), "Assert original reflection behavior.");
      
      var genericTypeDefinition = typeof (RecursiveGenericType<>);
      var typeArguments = new Type[] { _customType };
      var info = new TypeInstantiationInfo (genericTypeDefinition, typeArguments);
      var instantiation = new TypeInstantiation (_memberSelector, info, new TypeInstantiationContext());

      Assertion.IsNotNull (instantiation.BaseType);
      Assert.That (instantiation, Is.SameAs (instantiation.BaseType.GetGenericArguments().Single()));
    }

    [Test]
    public void IntegrationOfInitializationAndSubstitution_OuterGenericParameters ()
    {
      var type1 = _instantiation.DeclaringType.GetField ("OuterField").FieldType;
      var type2 = _instantiation.GetField ("Field").FieldType;
      var type3 = _instantiation.GetField ("FieldOfOuterType").FieldType;

      Assert.That (type1, Is.EqualTo (_outerCustomType));
      Assert.That (type2, Is.EqualTo (_customType));
      Assert.That (type3, Is.EqualTo (_outerCustomType));
    }

    interface IMyInterface<T> { }
    class BaseType<T> { }
    class DeclaringType<TOuter>
    {
      public TOuter OuterField;

      [Abc ("generic type def")]
      public class GenericType<T> : BaseType<T>, IMyInterface<T>
      {
        public T Field;
        public GenericType (T arg) { }
        public void Method (T arg) { }
        public T Property { get; internal set; }
        public event EventHandler Event;
        public TOuter FieldOfOuterType;
      }
    }
    class GenericTypeWithProperties<T>
    {
      public int ReadOnlyProperty { get { return 7; } }
      public int WriteOnlyProperty { set { Dev.Null = value; } }
    }
    class RecursiveGenericType<T> : BaseType<RecursiveGenericType<T>> { }
    class AbcAttribute : Attribute { public AbcAttribute (string s) { } }
  }
}