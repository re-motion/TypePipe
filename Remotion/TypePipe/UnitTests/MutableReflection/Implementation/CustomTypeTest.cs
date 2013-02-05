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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.RhinoMocks.UnitTesting;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomTypeTest
  {
    private IMemberSelector _memberSelectorMock;
    private IUnderlyingTypeFactory _underlyingTypeFactoryMock;

    private Type _declaringType;
    private Type _baseType;
    private string _name;
    private string _namespace;
    private string _fullName;
    private TypeAttributes _attributes;

    private TestableCustomType _customType;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _underlyingTypeFactoryMock = MockRepository.GenerateStrictMock<IUnderlyingTypeFactory>();

      _declaringType = ReflectionObjectMother.GetSomeType();
      _baseType = ReflectionObjectMother.GetSomeSubclassableType();
      _name = "type name";
      _namespace = "namespace";
      _fullName = "full type name";
      _attributes = (TypeAttributes) 7;

      _customType = new TestableCustomType (
          _memberSelectorMock,
          _underlyingTypeFactoryMock,
          _declaringType,
          _baseType,
          _name,
          _namespace,
          _fullName,
          _attributes,
          isGenericType: false,
          isGenericTypeDefinition: false,
          typeArguments: Type.EmptyTypes);

      // Initialize test implementation with members.
      _customType.Interfaces = new[] { typeof (IDisposable) };
      _customType.Fields = new[] { ReflectionObjectMother.GetSomeField() };
      _customType.Constructors = new[] { ReflectionObjectMother.GetSomeConstructor() };
      _customType.Methods = new[] { ReflectionObjectMother.GetSomeMethod() };
      _customType.Properties = new[] { ReflectionObjectMother.GetSomeProperty() };
      _customType.Events = new[] { ReflectionObjectMother.GetSomeEvent() };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_customType.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_customType.BaseType, Is.SameAs (_baseType));
      Assert.That (_customType.Name, Is.EqualTo (_name));
      Assert.That (_customType.Namespace, Is.EqualTo (_namespace));
      Assert.That (_customType.FullName, Is.EqualTo (_fullName));
      Assert.That (_customType.Attributes, Is.EqualTo (_attributes));
      Assert.That (_customType.IsGenericType, Is.False);
      Assert.That (_customType.IsGenericTypeDefinition, Is.False);
      Assert.That (_customType.GetGenericArguments(), Is.Empty);

      var typeArgument = ReflectionObjectMother.GetSomeType();
      var genericCustomType = CustomTypeObjectMother.Create (
          isGenericType: true, isGenericTypeDefinition: true, typeArguments: new[] { typeArgument });
      Assert.That (genericCustomType.IsGenericType, Is.True);
      Assert.That (genericCustomType.IsGenericTypeDefinition, Is.True);
      Assert.That (genericCustomType.GetGenericArguments(), Is.EqualTo (new[] { typeArgument }));
    }

    [Test]
    public void Initialization_NullDeclaringType_AndTypeArguments ()
    {
      var customType = new TestableCustomType (
          _memberSelectorMock, _underlyingTypeFactoryMock, null, _baseType, _name, _namespace, _fullName, _attributes, false, false, Type.EmptyTypes);

      Assert.That (customType.DeclaringType, Is.Null);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_customType.Assembly, Is.Null);
    }

    [Test]
    public void Module ()
    {
      Assert.That (_customType.Module, Is.Null);
    }

    [Test]
    public void UnderlyingSystemType_GeneratedByFactoryOnDemand ()
    {
      var baseType = typeof (TypeWithMyInterface);
      var newInterface = ReflectionObjectMother.GetSomeInterfaceType();
      var allInterfaces = new[] { typeof (IMyInterface), newInterface };
      var customType = CustomTypeObjectMother.Create (
          underlyingTypeFactory: _underlyingTypeFactoryMock, baseType: baseType, interfaces: allInterfaces);

      var fakeUnderlyingType = ReflectionObjectMother.GetSomeType();
      _underlyingTypeFactoryMock
          .Expect (mock => mock.CreateUnderlyingSystemType (Arg.Is (baseType), Arg<IEnumerable<Type>>.List.Equivalent (newInterface)))
          .Return (fakeUnderlyingType).Repeat.Once();

      var result = customType.UnderlyingSystemType;

      _underlyingTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeUnderlyingType));
    }

    [Test]
    public void UnderlyingSystemType_IsCached ()
    {
      var fakeUnderlyingType = ReflectionObjectMother.GetSomeType ();
      _underlyingTypeFactoryMock
          .Stub (mock => mock.CreateUnderlyingSystemType (Arg<Type>.Is.Anything, Arg<IEnumerable<Type>>.Is.Anything))
          .Return (fakeUnderlyingType);

      var result = _customType.UnderlyingSystemType;

      Assert.That (_customType.UnderlyingSystemType, Is.SameAs (result), "Should be cached.");
      _underlyingTypeFactoryMock.AssertWasCalled (
          mock => mock.CreateUnderlyingSystemType (Arg<Type>.Is.Anything, Arg<IEnumerable<Type>>.Is.Anything), o => o.Repeat.Once());
    }

    [Test]
    public void Equals ()
    {
      var customType1 = CustomTypeObjectMother.Create (name: "Proxy");
      var customType2 = CustomTypeObjectMother.Create (name: "Proxy");

      // Equals compares references and does not use the UnderlyingSystemType property.
      Assert.That (customType1.Equals ((object) customType1), Is.True);
      Assert.That (customType1.Equals ((object) customType2), Is.False);
      Assert.That (customType1.Equals (customType1), Is.True);
      Assert.That (customType1.Equals (customType2), Is.False);
    }

    [Test]
    public new void GetHashCode ()
    {
      var customType = CustomTypeObjectMother.Create (name: "Proxy");

      var result = customType.GetHashCode();

      PrivateInvoke.InvokeNonPublicMethod (customType, "InvalidateUnderlyingSystemType");
      // UnderlyingSystemType property is not used for hash code calculation.
      Assert.That (result, Is.EqualTo (customType.GetHashCode()));

      var otherCustomType = CustomTypeObjectMother.Create (name: "Proxy");
      // This test is safe because the hash code is the object reference.
      Assert.That (result, Is.Not.EqualTo (otherCustomType.GetHashCode()));
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_customType.GetElementType(), Is.Null);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      _customType.CustomAttributeDatas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };

      Assert.That (_customType.GetCustomAttributes (false).Select (a => a.GetType()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (_customType.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_customType.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_customType.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public void GetInterfaces ()
    {
      Assert.That (_customType.GetInterfaces(), Is.EqualTo (_customType.Interfaces));
    }

    [Test]
    public void GetInterface_NoMatch ()
    {
      var result = _customType.GetInterface ("IComparable", false);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetInterface_CaseSensitive_NoMatch ()
    {
      var result = _customType.GetInterface ("idisposable", false);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetInterface_CaseSensitive ()
    {
      var result = _customType.GetInterface ("IDisposable", false);

      Assert.That (result, Is.SameAs (typeof (IDisposable)));
    }

    [Test]
    public void GetInterface_IgnoreCase ()
    {
      var result = _customType.GetInterface ("idisposable", true);

      Assert.That (result, Is.SameAs (typeof (IDisposable)));
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous interface name 'IDisposable'.")]
    public void GetInterface_IgnoreCase_Ambiguous ()
    {
      _customType.Interfaces = new[] { typeof (IDisposable), typeof (Idisposable) };

      Dev.Null = _customType.GetInterface ("IDisposable", true);
    }

    [Test]
    public void GetFields ()
    {
      Assert.That (_customType.Fields, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeField () };
      _memberSelectorMock.Expect (mock => mock.SelectFields (_customType.Fields, bindingAttr, _customType)).Return (fakeResult);

      var result = _customType.GetFields (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetField ()
    {
      Assert.That (_customType.Fields, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = ReflectionObjectMother.GetSomeField();
      _memberSelectorMock.Expect (mock => mock.SelectSingleField (_customType.Fields, bindingAttr, name, _customType)).Return (fakeResult);

      var resultField = _customType.GetField (name, bindingAttr);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (resultField, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetConstructors ()
    {
      Assert.That (_customType.Constructors, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeConstructor() };
      _memberSelectorMock.Expect (mock => mock.SelectMethods (_customType.Constructors, bindingAttr, _customType)).Return (fakeResult);

      var result = _customType.GetConstructors (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetMethods ()
    {
      Assert.That (_customType.Methods, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeMethod() };
      _memberSelectorMock.Expect (mock => mock.SelectMethods (_customType.Methods, bindingAttr, _customType)).Return (fakeResult);

      var result = _customType.GetMethods (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetProperties ()
    {
      Assert.That (_customType.Properties, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeProperty() };
      _memberSelectorMock.Expect (mock => mock.SelectProperties (_customType.Properties, bindingAttr, _customType)).Return (fakeResult);

      var result = _customType.GetProperties (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetEvents ()
    {
      Assert.That (_customType.Events, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeEvent() };
      _memberSelectorMock.Expect (mock => mock.SelectEvents (_customType.Events, bindingAttr, _customType)).Return (fakeResult);

      var result = _customType.GetEvents (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetEvent ()
    {
      Assert.That (_customType.Fields, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = ReflectionObjectMother.GetSomeEvent();
      _memberSelectorMock.Expect (mock => mock.SelectSingleEvent (_customType.Events, bindingAttr, name, _customType)).Return (fakeResult);

      var resultField = _customType.GetEvent (name, bindingAttr);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (resultField, Is.SameAs (fakeResult));
    }

    [Test]
    public void InvalidateUnderlyingSystemType_CausesRegenerationOfUnderlyingSystemType ()
    {
      var fakeUnderlyingType1 = ReflectionObjectMother.GetSomeType ();
      var fakeUnderlyingType2 = ReflectionObjectMother.GetSomeDifferentType ();
      _underlyingTypeFactoryMock
          .Stub (stub => stub.CreateUnderlyingSystemType (Arg<Type>.Is.Anything, Arg<IEnumerable<Type>>.Is.Anything))
          .Return (fakeUnderlyingType1).Repeat.Once ();

      Dev.Null = _customType.UnderlyingSystemType;

      PrivateInvoke.InvokeNonPublicMethod (_customType, "InvalidateUnderlyingSystemType");

      _underlyingTypeFactoryMock
        .Stub (stub => stub.CreateUnderlyingSystemType (Arg<Type>.Is.Anything, Arg<IEnumerable<Type>>.Is.Anything))
        .Return (fakeUnderlyingType2);

      var resultAfterInvalidation = _customType.UnderlyingSystemType;

      _underlyingTypeFactoryMock.AssertWasCalled (
          mock => mock.CreateUnderlyingSystemType (Arg<Type>.Is.Anything, Arg<IEnumerable<Type>>.Is.Anything), o => o.Repeat.Twice ());
      Assert.That (resultAfterInvalidation, Is.SameAs (fakeUnderlyingType2));
    }

    [Test]
    [TestCaseSource ("GetBinderTestCases")]
    public void GetConstructorImpl (Binder inputBinder, Binder expectedBinder)
    {
      Assert.That (_customType.Constructors, Is.Not.Null.And.Not.Empty);
      var callingConvention = CallingConventions.Any;
      var bindingAttr = BindingFlags.NonPublic;
      var typesOrNull = new[] { ReflectionObjectMother.GetSomeType() };
      var modifiersOrNull = new[] { new ParameterModifier (1) };
      var fakeResult = ReflectionObjectMother.GetSomeConstructor();
      _memberSelectorMock
          .Expect (
              mock =>
              mock.SelectSingleMethod (_customType.Constructors, expectedBinder, bindingAttr, null, _customType, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var arguments = new object[] { bindingAttr, inputBinder, callingConvention, typesOrNull, modifiersOrNull };
      var result = (ConstructorInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetConstructorImpl", arguments);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    [TestCaseSource ("GetBinderTestCases")]
    public void GetMethodImpl (Binder inputBinder, Binder expectedBinder)
    {
      Assert.That (_customType.Methods, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var callingConvention = CallingConventions.Any;
      var typesOrNull = new[] { ReflectionObjectMother.GetSomeType() };
      var modifiersOrNull = new[] { new ParameterModifier (1) };

      var fakeResult = ReflectionObjectMother.GetSomeMethod();
      _memberSelectorMock
          .Expect (
              mock => mock.SelectSingleMethod (_customType.Methods, expectedBinder, bindingAttr, name, _customType, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var arguments = new object[] { name, bindingAttr, inputBinder, callingConvention, typesOrNull, modifiersOrNull };
      var result = (MethodInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetMethodImpl", arguments);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    [TestCaseSource ("GetBinderTestCases")]
    public void GetPropertyImpl (Binder inputBinder, Binder expectedBinder)
    {
      Assert.That (_customType.Properties, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var returnTypeOrNull = ReflectionObjectMother.GetSomeType();
      var typesOrNull = new[] { ReflectionObjectMother.GetSomeType() };
      var modifiersOrNull = new[] { new ParameterModifier (1) };

      var fakeResult = ReflectionObjectMother.GetSomeProperty();
      _memberSelectorMock
          .Expect (
              mock => mock.SelectSingleProperty (
                  _customType.Properties, expectedBinder, bindingAttr, name, _customType, returnTypeOrNull, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var arguments = new object[] { name, bindingAttr, inputBinder, returnTypeOrNull, typesOrNull, modifiersOrNull };
      var result = (PropertyInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetPropertyImpl", arguments);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    public static IEnumerable GetBinderTestCases ()
    {
      var binderStub = MockRepository.GenerateStub<Binder> ();
      yield return new object[] { binderStub, binderStub };
      yield return new object[] { null, Type.DefaultBinder };
    }

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_customType.HasElementType, Is.False);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_customType.IsByRef, Is.False);
    }

    [Test]
    public void IsArrayImpl ()
    {
      Assert.That (_customType.IsArray, Is.False);
    }

    [Test]
    public void IsPointerImpl ()
    {
      Assert.That (_customType.IsPointer, Is.False);
    }

    [Test]
    public void IsPrimitiveImpl ()
    {
      Assert.That (_customType.IsPrimitive, Is.False);
    }

    [Test]
    public void IsCOMObjectImpl ()
    {
      Assert.That (_customType.IsCOMObject, Is.False);
    }

    [Test]
    public new void ToString ()
    {

      Assert.That (_customType.ToString(), Is.EqualTo ("type name"));
    }

    [Test]
    public void ToDebugString ()
    {
      Assert.That (_customType.ToDebugString(), Is.EqualTo ("TestableCustomType = \"type name\""));
    }

    [Test]
    public void VirtualMethodsImplementedByType ()
    {
      _memberSelectorMock
          .Stub (stub => stub.SelectEvents (Arg<IEnumerable<EventInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new EventInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectMethods (Arg<IEnumerable<MethodInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new MethodInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectMethods (Arg<IEnumerable<ConstructorInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new ConstructorInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectFields (Arg<IEnumerable<FieldInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new FieldInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectProperties (Arg<IEnumerable<PropertyInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new PropertyInfo[0]);
      var customTypeWithUnderlyingSystemTypeFactoryStub = CustomTypeObjectMother.Create();

      // None of these members should throw an exception.
      Dev.Null = _customType.MemberType;
      Dev.Null = _customType.DeclaringMethod;
      Dev.Null = _customType.ReflectedType;
      Dev.Null = _customType.IsGenericParameter;
      Dev.Null = CustomTypeObjectMother.Create (isGenericType: false).ContainsGenericParameters;

      Dev.Null = _customType.IsValueType; // IsValueTypeImpl()
      Dev.Null = customTypeWithUnderlyingSystemTypeFactoryStub.IsContextful; // IsContextfulImpl()
      Dev.Null = customTypeWithUnderlyingSystemTypeFactoryStub.IsMarshalByRef; // IsMarshalByRefImpl()

      Dev.Null = _customType.FindInterfaces ((type, filterCriteria) => true, filterCriteria: null);
      Dev.Null = _customType.GetEvents();
      Dev.Null = _customType.GetMember ("name", BindingFlags.Default);
      Dev.Null = _customType.GetMember ("name", MemberTypes.All, BindingFlags.Default);
      Dev.Null = _customType.IsSubclassOf (null);
      Dev.Null = _customType.IsInstanceOfType (null);
      Dev.Null = _customType.IsAssignableFrom (null);
      Dev.Null = _customType.FindMembers (MemberTypes.All, BindingFlags.Default, filter: null, filterCriteria: null);
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.MetadataToken, "MetadataToken");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.GUID, "GUID");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.AssemblyQualifiedName, "AssemblyQualifiedName");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.StructLayoutAttribute, "StructLayoutAttribute");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.GenericParameterAttributes, "GenericParameterAttributes");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.GenericParameterPosition, "GenericParameterPosition");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.TypeHandle, "TypeHandle");

      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.GetDefaultMembers(), "GetDefaultMembers");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.InvokeMember (null, 0, null, null, null, null, null, null), "InvokeMember");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.MakePointerType(), "MakePointerType");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.MakeByRefType(), "MakeByRefType");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.MakeArrayType(), "MakeArrayType");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.MakeArrayType (7), "MakeArrayType");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.GetArrayRank(), "GetArrayRank");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.GetGenericParameterConstraints(), "GetGenericParameterConstraints");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.MakeGenericType(), "MakeGenericType");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.GetGenericTypeDefinition(), "GetGenericTypeDefinition");
    }

    class TypeWithMyInterface : IMyInterface { }
    interface IMyInterface { }

    // This exists for GetInterface method with ignore case parameter.
    private interface Idisposable { }
  }
}