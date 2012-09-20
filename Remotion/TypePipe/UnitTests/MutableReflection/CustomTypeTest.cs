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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class CustomTypeTest
  {
    private IMemberSelector _memberSelectorMock;

    private Type _underlyingSystemType;
    private Type _declaringType;
    private Type _baseType;
    private TypeAttributes _typeAttributes;
    private string _name;
    private string _namespace;
    private string _fullName;

    private TestableCustomType _customType;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _underlyingSystemType = ReflectionObjectMother.GetSomeType();
      _declaringType = ReflectionObjectMother.GetSomeType();
      _baseType = ReflectionObjectMother.GetSomeType();
      _typeAttributes = (TypeAttributes) 777;
      _name = "type name";
      _namespace = "namespace";
      _fullName = "full type name";

      _customType = new TestableCustomType (
          _memberSelectorMock, _underlyingSystemType, _declaringType, _baseType, _typeAttributes, _name, _namespace, _fullName);

      // Initialize test implementation with members.
      _customType.Interfaces = new[] { typeof (IDisposable) };
      _customType.Fields = new[] { ReflectionObjectMother.GetSomeField() };
      _customType.Constructors = new[] { ReflectionObjectMother.GetSomeConstructor() };
      _customType.Methods = new[] { ReflectionObjectMother.GetSomeMethod() };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_customType.UnderlyingSystemType, Is.SameAs (_underlyingSystemType));
      Assert.That (_customType.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_customType.BaseType, Is.SameAs (_baseType));
      Assert.That (_customType.Attributes, Is.EqualTo (_typeAttributes));
      Assert.That (_customType.Name, Is.EqualTo (_name));
      Assert.That (_customType.Namespace, Is.EqualTo (_namespace));
      Assert.That (_customType.FullName, Is.EqualTo (_fullName));
    }

    [Test]
    public void Initialization_Null ()
    {
      Type declaringType = null;
      Type baseType = null;

      var customType = new TestableCustomType (
          _memberSelectorMock, _underlyingSystemType, declaringType, baseType, _typeAttributes, _name, _namespace, _fullName);

      Assert.That (customType.DeclaringType, Is.Null);
      Assert.That (customType.BaseType, Is.Null);
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
    public new void ToString ()
    {
      Assert.That (_customType.ToString (), Is.EqualTo ("type name"));
    }

    [Test]
    public void ToDebugString ()
    {
      var typeName = _customType.GetType ().Name;
      Assert.That (_customType.ToDebugString (), Is.EqualTo (typeName + " = \"type name\""));
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_customType.GetElementType (), Is.Null);
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

      _customType.GetInterface ("IDisposable", true);
    }

    [Test]
    public void GetFields ()
    {
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeField () };
      _memberSelectorMock.Expect (mock => mock.SelectFields (_customType.Fields, bindingAttr)).Return (fakeResult);

      var result = _customType.GetFields (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetField ()
    {
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = ReflectionObjectMother.GetSomeField ();
      _memberSelectorMock.Expect (mock => mock.SelectSingleField (_customType.Fields, bindingAttr, name)).Return (fakeResult);

      var resultField = _customType.GetField (name, bindingAttr);

      _memberSelectorMock.VerifyAllExpectations ();
      Assert.That (resultField, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetConstructors ()
    {
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
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeMethod() };
      _memberSelectorMock.Expect (mock => mock.SelectMethods (_customType.Methods, bindingAttr, _customType)).Return (fakeResult);

      var result = _customType.GetMethods (bindingAttr);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    [TestCaseSource ("GetBinderTestCases")]
    public void GetConstructorImpl (Binder inputBinder, Binder expectedBinder)
    {
      var callingConvention = CallingConventions.Any;
      var bindingAttr = BindingFlags.NonPublic;
      var typesOrNull = new[] { ReflectionObjectMother.GetSomeType() };
      var modifiersOrNull = new[] { new ParameterModifier (1) };
      var fakeResult = ReflectionObjectMother.GetSomeConstructor();
      _memberSelectorMock
          .Expect (
              mock =>
              mock.SelectSingleMethod (_customType.Constructors, expectedBinder, bindingAttr, ".ctor", _customType, typesOrNull, modifiersOrNull))
          .Return (fakeResult);

      var arguments = new object[] { bindingAttr, inputBinder, callingConvention, typesOrNull, modifiersOrNull };
      var resultConstructor = (ConstructorInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetConstructorImpl", arguments);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (resultConstructor, Is.SameAs (fakeResult));
    }

    [Test]
    [TestCaseSource ("GetBinderTestCases")]
    public void GetMethodImpl (Binder inputBinder, Binder expectedBinder)
    {
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
      var resultMethod = (MethodInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetMethodImpl", arguments);

      _memberSelectorMock.VerifyAllExpectations();
      Assert.That (resultMethod, Is.SameAs (fakeResult));
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
    public void GetAttributeFlagsImpl ()
    {
      Assert.That (_customType.Attributes, Is.EqualTo (_typeAttributes));
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
    public void VirtualMethodsImplementedByType ()
    {
      // None of these members should throw an exception 
      Dev.Null = _customType.MemberType;
      // TODO: DeclaringType should work correctly for nested types.
      Dev.Null = _customType.DeclaringMethod;
      Dev.Null = _customType.ReflectedType;
      Dev.Null = _customType.IsGenericType;
      Dev.Null = _customType.IsGenericTypeDefinition;
      Dev.Null = _customType.IsGenericParameter;
      Dev.Null = _customType.ContainsGenericParameters;

      Dev.Null = _customType.IsValueType; // IsValueTypeImpl()
      Dev.Null = _customType.IsContextful; // IsContextfulImpl()
      Dev.Null = _customType.IsMarshalByRef; // IsMarshalByRefImpl()

      _customType.FindInterfaces ((type, filterCriteria) => true, filterCriteria: null);
      _customType.GetEvents();
      _customType.GetMember ("name", BindingFlags.Default);
      _customType.GetMember ("name", MemberTypes.All, BindingFlags.Default);
      _customType.IsSubclassOf (null);
      _customType.IsInstanceOfType (null);
      _customType.IsAssignableFrom (null);

      _memberSelectorMock
          .Stub (stub => stub.SelectMethods (Arg<IEnumerable<MethodInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<MutableType>.Is.Anything))
          .Return (new MethodInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectMethods (Arg<IEnumerable<ConstructorInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<MutableType>.Is.Anything))
          .Return (new ConstructorInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectFields (Arg<IEnumerable<FieldInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (new FieldInfo[0]);
      _customType.FindMembers (MemberTypes.All, BindingFlags.Default, filter: null, filterCriteria: null);
    }

    [Test]
    public void UnsupportedMembers ()
    {
      CheckThrowsNotSupported (() => Dev.Null = _customType.MetadataToken, "Property", "MetadataToken");
      CheckThrowsNotSupported (() => Dev.Null = _customType.GUID, "Property", "GUID");
      CheckThrowsNotSupported (() => Dev.Null = _customType.AssemblyQualifiedName, "Property", "AssemblyQualifiedName");
      CheckThrowsNotSupported (() => Dev.Null = _customType.StructLayoutAttribute, "Property", "StructLayoutAttribute");
      CheckThrowsNotSupported (() => Dev.Null = _customType.GenericParameterAttributes, "Property", "GenericParameterAttributes");
      CheckThrowsNotSupported (() => Dev.Null = _customType.GenericParameterPosition, "Property", "GenericParameterPosition");
      CheckThrowsNotSupported (() => Dev.Null = _customType.TypeHandle, "Property", "TypeHandle");

      CheckThrowsNotSupported (() => _customType.GetDefaultMembers (), "Method", "GetDefaultMembers");
      CheckThrowsNotSupported (() => _customType.GetInterfaceMap (null), "Method", "GetInterfaceMap");
      CheckThrowsNotSupported (() => _customType.InvokeMember (null, 0, null, null, null, null, null, null), "Method", "InvokeMember");
      CheckThrowsNotSupported (() => _customType.MakePointerType (), "Method", "MakePointerType");
      CheckThrowsNotSupported (() => _customType.MakeByRefType (), "Method", "MakeByRefType");
      CheckThrowsNotSupported (() => _customType.MakeArrayType (), "Method", "MakeArrayType");
      CheckThrowsNotSupported (() => _customType.MakeArrayType (7), "Method", "MakeArrayType");
      CheckThrowsNotSupported (() => _customType.GetArrayRank (), "Method", "GetArrayRank");
      CheckThrowsNotSupported (() => _customType.GetGenericParameterConstraints (), "Method", "GetGenericParameterConstraints");
      CheckThrowsNotSupported (() => _customType.MakeGenericType (), "Method", "MakeGenericType");
      CheckThrowsNotSupported (() => _customType.GetGenericArguments (), "Method", "GetGenericArguments");
      CheckThrowsNotSupported (() => _customType.GetGenericTypeDefinition (), "Method", "GetGenericTypeDefinition");
    }

    private void CheckThrowsNotSupported (TestDelegate memberInvocation, string memberType, string memberName)
    {
      var message = string.Format ("{0} {1} is not supported.", memberType, memberName);
      Assert.That (memberInvocation, Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (message));
    }

    // This exists for GetInterface method with ignore case parameter.
    private interface Idisposable
    {
    }
  }
}