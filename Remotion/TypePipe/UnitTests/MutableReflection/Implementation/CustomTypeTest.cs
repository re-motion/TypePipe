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
    private IUnderlyingSystemTypeFactory _underlyingSystemTypeFactoryMock;

    private Type _declaringType;
    private Type _baseType;
    private string _name;
    private string _namespace;
    private string _fullName;

    private TestableCustomType _customType;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _underlyingSystemTypeFactoryMock = MockRepository.GenerateStrictMock<IUnderlyingSystemTypeFactory>();

      _declaringType = ReflectionObjectMother.GetSomeType();
      _baseType = ReflectionObjectMother.GetSomeType();
      _name = "type name";
      _namespace = "namespace";
      _fullName = "full type name";

      _customType = new TestableCustomType (
          _memberSelectorMock,
          _underlyingSystemTypeFactoryMock,
          _declaringType,
          _baseType,
          _name,
          _namespace,
          _fullName);

      // Initialize test implementation with members.
      _customType.Interfaces = new[] { typeof (IDisposable) };
      _customType.Fields = new[] { ReflectionObjectMother.GetSomeField() };
      _customType.Constructors = new[] { ReflectionObjectMother.GetSomeConstructor() };
      _customType.Methods = new[] { ReflectionObjectMother.GetSomeMethod() };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_customType.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_customType.BaseType, Is.SameAs (_baseType));
      Assert.That (_customType.Name, Is.EqualTo (_name));
      Assert.That (_customType.Namespace, Is.EqualTo (_namespace));
      Assert.That (_customType.FullName, Is.EqualTo (_fullName));
    }

    [Test]
    public void Initialization_Null ()
    {
      var customType = new TestableCustomType (
          _memberSelectorMock,
          _underlyingSystemTypeFactoryMock,
          declaringType: null,
          baseType: _baseType,
          name: _name,
          @namespace: _namespace,
          fullName: _fullName);

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
    public void UnderlyingSystemType ()
    {
      var baseType = typeof (TypeWithMyInterface);
      var newInterface = ReflectionObjectMother.GetSomeInterfaceType();
      var allInterfaces = new[] { typeof (IMyInterface), newInterface };
      var customType = CustomTypeObjectMother.Create (
          underlyingSystemTypeFactory: _underlyingSystemTypeFactoryMock, baseType: baseType, interfaces: allInterfaces);

      var fakeType1 = ReflectionObjectMother.GetSomeType();
      var fakeType2 = ReflectionObjectMother.GetSomeDifferentType();
      _underlyingSystemTypeFactoryMock
          .Expect (mock => mock.CreateUnderlyingSystemType (Arg.Is (baseType), Arg<IEnumerable<Type>>.List.Equivalent (newInterface)))
          .Return (fakeType1).Repeat.Once();
      _underlyingSystemTypeFactoryMock
          .Expect (mock => mock.CreateUnderlyingSystemType (Arg.Is (baseType), Arg<IEnumerable<Type>>.List.Equivalent (newInterface)))
          .Return (fakeType2).Repeat.Once();

      var result1 = customType.UnderlyingSystemType;

      Assert.That (result1, Is.SameAs (fakeType1));
      Assert.That (customType.UnderlyingSystemType, Is.SameAs (result1), "Should be cached.");

      PrivateInvoke.InvokeNonPublicMethod (customType, "InvalidateUnderlyingSystemType");

      var result2 = customType.UnderlyingSystemType;

      _underlyingSystemTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result2, Is.SameAs (fakeType2));
    }

    [Test]
    public void Equals ()
    {
      var customType1 = CustomTypeObjectMother.Create (name: "Proxy");
      var customType2 = CustomTypeObjectMother.Create (name: "Proxy");

      // Equals compares references and does not use the UnderlyingSystemType property.
      Assert.That (customType1, Is.EqualTo (customType1));
      Assert.That (customType1, Is.Not.EqualTo (customType2));
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
    public void IsAssignableTo ()
    {
      Assert.That (_customType.IsAssignableTo (_customType), Is.True);

      Assert.That (_customType.IsAssignableTo (_baseType), Is.True);
      Assert.That (_customType.IsAssignableTo (typeof (object)), Is.True);

      var @interface = _customType.Interfaces.Single();
      Assert.That (_customType.IsAssignableTo (@interface), Is.True);

      var unrelatedType = ReflectionObjectMother.GetSomeType();
      Assert.That (_customType.IsAssignableTo (unrelatedType), Is.False);
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_customType.GetElementType (), Is.Null);
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
              mock.SelectSingleMethod (_customType.Constructors, expectedBinder, bindingAttr, null, _customType, typesOrNull, modifiersOrNull))
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
      // ReSharper disable PossibleMistakenCallToGetType.2
      var typeName = _customType.GetType().Name;
      // ReSharper restore PossibleMistakenCallToGetType.2
      Assert.That (_customType.ToDebugString(), Is.EqualTo (typeName + " = \"type name\""));
    }

    [Test]
    public void VirtualMethodsImplementedByType ()
    {
      var customTypeWithUnderlyingSystemTypeFactoryStub = CustomTypeObjectMother.Create();

      // None of these members should throw an exception 
      Dev.Null = _customType.MemberType;
      Dev.Null = _customType.DeclaringMethod;
      Dev.Null = _customType.ReflectedType;
      Dev.Null = _customType.IsGenericType;
      Dev.Null = _customType.IsGenericTypeDefinition;
      Dev.Null = _customType.IsGenericParameter;
      Dev.Null = _customType.ContainsGenericParameters;

      Dev.Null = _customType.IsValueType; // IsValueTypeImpl()
      Dev.Null = customTypeWithUnderlyingSystemTypeFactoryStub.IsContextful; // IsContextfulImpl()
      Dev.Null = customTypeWithUnderlyingSystemTypeFactoryStub.IsMarshalByRef; // IsMarshalByRefImpl()

      Dev.Null = _customType.FindInterfaces ((type, filterCriteria) => true, filterCriteria: null);
      Dev.Null = _customType.GetEvents ();
      Dev.Null = _customType.GetMember ("name", BindingFlags.Default);
      Dev.Null = _customType.GetMember ("name", MemberTypes.All, BindingFlags.Default);
      Dev.Null = _customType.IsSubclassOf (null);
      Dev.Null = _customType.IsInstanceOfType (null);
      Dev.Null = _customType.IsAssignableFrom (null);

      _memberSelectorMock
          .Stub (stub => stub.SelectMethods (Arg<IEnumerable<MethodInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new MethodInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectMethods (Arg<IEnumerable<ConstructorInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything, Arg<ProxyType>.Is.Anything))
          .Return (new ConstructorInfo[0]);
      _memberSelectorMock
          .Stub (stub => stub.SelectFields (Arg<IEnumerable<FieldInfo>>.Is.Anything, Arg<BindingFlags>.Is.Anything))
          .Return (new FieldInfo[0]);
      _customType.FindMembers (MemberTypes.All, BindingFlags.Default, filter: null, filterCriteria: null);
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
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.GetGenericArguments(), "GetGenericArguments");
      UnsupportedMemberTestHelper.CheckMethod (() => Dev.Null = _customType.GetGenericTypeDefinition(), "GetGenericTypeDefinition");
    }

    class TypeWithMyInterface : IMyInterface { }
    interface IMyInterface { }

    // This exists for GetInterface method with ignore case parameter.
    private interface Idisposable { }
  }
}