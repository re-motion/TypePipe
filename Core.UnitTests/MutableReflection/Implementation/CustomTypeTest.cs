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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Moq;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomTypeTest
  {
    private Mock<IMemberSelector> _memberSelectorMock;

    private string _name;
    private string _namespace;
    private TypeAttributes _attributes;

    private TestableCustomType _customType;

    private Type _typeArgument;
    private Type _genericTypeUnderlyingDefinition;
    private CustomType _genericType;

    private Type _typeParameter;
    private CustomType _genericTypeDefinition;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = new Mock<IMemberSelector> (MockBehavior.Strict);

      _name = "TypeName";
      _namespace = "MyNamespace";
      _attributes = (TypeAttributes) 7;

      _customType = new TestableCustomType (
          _name,
          _namespace,
          _attributes,
          genericTypeDefinition: null,
          typeArguments: Type.EmptyTypes)
                    {
                        NestedTypes = new[] { ReflectionObjectMother.GetSomeType() },
                        Interfaces = new[] { typeof (IDisposable) },
                        Fields = new[] { ReflectionObjectMother.GetSomeField() },
                        Constructors = new[] { ReflectionObjectMother.GetSomeConstructor() },
                        Methods = new[] { ReflectionObjectMother.GetSomeMethod() },
                        Properties = new[] { ReflectionObjectMother.GetSomeProperty() },
                        Events = new[] { ReflectionObjectMother.GetSomeEvent() }
                    };
      _customType.SetMemberSelector (_memberSelectorMock.Object);

      _typeArgument = ReflectionObjectMother.GetSomeType();
      _genericTypeUnderlyingDefinition = typeof (IList<>);
      _genericType = CustomTypeObjectMother.Create (
          name: "GenericType`1", genericTypeDefinition: _genericTypeUnderlyingDefinition, typeArguments: new[] { _typeArgument });

      _typeParameter = ReflectionObjectMother.GetSomeGenericParameter();
      _genericTypeDefinition = CustomTypeObjectMother.Create (name: "GenericTypeDefinition`1", typeArguments: new[] { _typeParameter });
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_customType.DeclaringType, Is.Null);
      Assert.That (_customType.BaseType, Is.Null);
      Assert.That (_customType.Name, Is.EqualTo (_name));
      Assert.That (_customType.Namespace, Is.EqualTo (_namespace));
      Assert.That (_customType.Attributes, Is.EqualTo (_attributes));
      Assert.That (_customType.IsGenericType, Is.False);
      Assert.That (_customType.IsGenericTypeDefinition, Is.False);
      Assert.That (_customType.GetGenericArguments(), Is.Empty);
      Assert.That (
          () => _customType.GetGenericTypeDefinition(),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "GetGenericTypeDefinition can only be called on generic types (IsGenericType must be true)."));
    }

    [Test]
    public void Initialization_GenericType ()
    {
      Assert.That (_genericType.IsGenericType, Is.True);
      Assert.That (_genericType.IsGenericTypeDefinition, Is.False);
      Assert.That (_genericType.GetGenericArguments(), Is.EqualTo (new[] { _typeArgument }));
      Assert.That (_genericType.GetGenericTypeDefinition(), Is.SameAs (_genericTypeUnderlyingDefinition));
    }

    [Test]
    public void Initialization_GenericTypeDefinition ()
    {
      Assert.That (_genericTypeDefinition.IsGenericType, Is.True);
      Assert.That (_genericTypeDefinition.IsGenericTypeDefinition, Is.True);
      Assert.That (_genericTypeDefinition.GetGenericArguments(), Is.EqualTo (new[] { _typeParameter }));
      Assert.That (_genericTypeDefinition.GetGenericTypeDefinition(), Is.SameAs (_genericTypeDefinition));
    }

    [Test]
    public void SetDeclaringType ()
    {
      Assert.That (_customType.DeclaringType, Is.Null);
      var declaringType = ReflectionObjectMother.GetSomeType();

      _customType.Invoke ("SetDeclaringType", declaringType);

      Assert.That (_customType.DeclaringType, Is.SameAs (declaringType));
    }

    [Test]
    public void MemberType_TypeInfo_ForNullDeclaringType ()
    {
      Assert.That (_customType.MemberType, Is.EqualTo (MemberTypes.TypeInfo));
    }

    [Test]
    public void MemberType_NestedType_ForNotNullDeclaringType ()
    {
      var declaringType = ReflectionObjectMother.GetSomeType ();

      _customType.Invoke ("SetDeclaringType", declaringType);

      Assert.That (_customType.MemberType, Is.EqualTo (MemberTypes.NestedType));
    }

    [Test]
    public void SetBaseType ()
    {
      Assert.That (_customType.BaseType, Is.Null);
      var baseType = ReflectionObjectMother.GetSomeType();

      _customType.Invoke ("SetBaseType", baseType);

      Assert.That (_customType.BaseType, Is.SameAs (baseType));
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
    public void FullName ()
    {
      Assert.That (_customType.FullName, Is.EqualTo ("MyNamespace.TypeName"));
      Assert.That (_genericType.FullName, Is.EqualTo ("GenericType`1[[" + _typeArgument.AssemblyQualifiedName + "]]"));
      Assert.That (_genericTypeDefinition.FullName, Is.EqualTo ("GenericTypeDefinition`1[[" + _typeParameter.AssemblyQualifiedName + "]]"));
    }

    [Test]
    public void FullName_NullNamespace ()
    {
      var customType = CustomTypeObjectMother.Create (name: "MyName", @namespace: null);
      Assert.That (customType.FullName, Is.EqualTo ("MyName"));
    }

    [Test]
    public void FullName_NestedType ()
    {
      var enclosingType = CustomTypeObjectMother.Create (declaringType: typeof (object), name: "EnclosingType");
      _customType.CallSetDeclaringType (enclosingType);
      Assert.That (_customType.FullName, Is.EqualTo ("MyNamespace.Object+EnclosingType+TypeName"));
    }

    [Test]
    public void AssemblyQualifiedName ()
    {
      Assert.That (_customType.AssemblyQualifiedName, Is.EqualTo ("MyNamespace.TypeName, TypePipe_GeneratedAssembly"));
    }

    [Test]
    public void MakeGenericType ()
    {
      var result = _genericTypeDefinition.MakeGenericType (_typeParameter);

      Assert.That (result, Is.TypeOf<TypeInstantiation>());
      Assert.That (result.IsGenericType, Is.True);
      Assert.That (result.IsGenericTypeDefinition, Is.False);
      Assert.That (result.GetGenericTypeDefinition (), Is.SameAs (_genericTypeDefinition));
    }

    [Test]
    public void MakeGenericType_NoGenericMethodDefinition ()
    {
      Assert.That (
          () => Dev.Null = _genericType.MakeGenericType(),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("MakeGenericType can only be called on generic type definitions (IsGenericTypeDefinition must be true)."));
    }

    [Test]
    public void MakeArrayType ()
    {
      var result = _customType.MakeArrayType();

      Assert.That (result, Is.TypeOf<VectorType>());
      Assert.That (result.GetElementType(), Is.SameAs (_customType));
    }

    [Test]
    public void MakeArrayType_Rank ()
    {
      var rank = 7;

      var result = _customType.MakeArrayType (rank);

      Assert.That (result, Is.TypeOf<MultiDimensionalArrayType>());
      Assert.That (result.GetElementType(), Is.SameAs (_customType));
      Assert.That (result.GetArrayRank(), Is.EqualTo (7));
    }

    [Test]
    public void MakeArrayType_ThrowsForInvalidRank ()
    {
      Assert.That (
          () => Dev.Null = _customType.MakeArrayType (0),
          Throws.InstanceOf<ArgumentOutOfRangeException>()
              .With.ArgumentExceptionMessageEqualTo (
                  "Array rank must be greater than zero.", "rank"));
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
    public void GetNestedTypes ()
    {
      Assert.That (_customType.NestedTypes, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeType() };
      _memberSelectorMock.Setup (mock => mock.SelectTypes (_customType.NestedTypes, bindingAttr)).Returns (fakeResult).Verifiable();

      var result = _customType.GetNestedTypes (bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetNestedType ()
    {
      Assert.That (_customType.NestedTypes, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = ReflectionObjectMother.GetSomeType();
      _memberSelectorMock.Setup (mock => mock.SelectSingleType (_customType.NestedTypes, bindingAttr, name)).Returns (fakeResult).Verifiable();

      var resultType = _customType.GetNestedType (name, bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That(resultType, Is.SameAs(fakeResult));
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
    public void GetInterface_IgnoreCase_Ambiguous ()
    {
      _customType.Interfaces = new[] { typeof (IDisposable), typeof (Idisposable) };
      Assert.That (
          () => Dev.Null = _customType.GetInterface ("IDisposable", true),
          Throws.InstanceOf<AmbiguousMatchException>()
              .With.Message.EqualTo ("Ambiguous interface name 'IDisposable'."));
    }

    [Test]
    public void GetFields ()
    {
      Assert.That (_customType.Fields, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeField () };
      _memberSelectorMock.Setup (mock => mock.SelectFields (_customType.Fields, bindingAttr, _customType)).Returns (fakeResult).Verifiable();

      var result = _customType.GetFields (bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetField ()
    {
      Assert.That (_customType.Fields, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = ReflectionObjectMother.GetSomeField();
      _memberSelectorMock.Setup (mock => mock.SelectSingleField (_customType.Fields, bindingAttr, name, _customType)).Returns (fakeResult).Verifiable();

      var resultField = _customType.GetField (name, bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (resultField, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetConstructors ()
    {
      Assert.That (_customType.Constructors, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeConstructor() };
      _memberSelectorMock.Setup (mock => mock.SelectMethods (_customType.Constructors, bindingAttr, _customType)).Returns (fakeResult).Verifiable();

      var result = _customType.GetConstructors (bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetMethods ()
    {
      Assert.That (_customType.Methods, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeMethod() };
      _memberSelectorMock.Setup (mock => mock.SelectMethods (_customType.Methods, bindingAttr, _customType)).Returns (fakeResult).Verifiable();

      var result = _customType.GetMethods (bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetProperties ()
    {
      Assert.That (_customType.Properties, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeProperty() };
      _memberSelectorMock.Setup (mock => mock.SelectProperties (_customType.Properties, bindingAttr, _customType)).Returns (fakeResult).Verifiable();

      var result = _customType.GetProperties (bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetEvents ()
    {
      Assert.That (_customType.Events, Is.Not.Null.And.Not.Empty);
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = new[] { ReflectionObjectMother.GetSomeEvent() };
      _memberSelectorMock.Setup (mock => mock.SelectEvents (_customType.Events, bindingAttr, _customType)).Returns (fakeResult).Verifiable();

      var result = _customType.GetEvents (bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetEvent ()
    {
      Assert.That (_customType.Fields, Is.Not.Null.And.Not.Empty);
      var name = "some name";
      var bindingAttr = BindingFlags.NonPublic;
      var fakeResult = ReflectionObjectMother.GetSomeEvent();
      _memberSelectorMock.Setup (mock => mock.SelectSingleEvent (_customType.Events, bindingAttr, name, _customType)).Returns (fakeResult).Verifiable();

      var resultField = _customType.GetEvent (name, bindingAttr);

      _memberSelectorMock.Verify();
      Assert.That (resultField, Is.SameAs (fakeResult));
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
          .Setup (mock => mock.SelectSingleMethod (_customType.Constructors, expectedBinder, bindingAttr, null, _customType, typesOrNull, modifiersOrNull))
          .Returns (fakeResult)
          .Verifiable();

      var arguments = new object[] { bindingAttr, inputBinder, callingConvention, typesOrNull, modifiersOrNull };
      var result = (ConstructorInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetConstructorImpl", arguments);

      _memberSelectorMock.Verify();
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
          .Setup (mock => mock.SelectSingleMethod (_customType.Methods, expectedBinder, bindingAttr, name, _customType, typesOrNull, modifiersOrNull))
          .Returns (fakeResult)
          .Verifiable();

      var arguments = new object[] { name, bindingAttr, inputBinder, callingConvention, typesOrNull, modifiersOrNull };
      var result = (MethodInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetMethodImpl", arguments);

      _memberSelectorMock.Verify();
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
          .Setup (
              mock => mock.SelectSingleProperty (
                  _customType.Properties,
                  expectedBinder,
                  bindingAttr,
                  name,
                  _customType,
                  returnTypeOrNull,
                  typesOrNull,
                  modifiersOrNull))
          .Returns (fakeResult)
          .Verifiable();

      var arguments = new object[] { name, bindingAttr, inputBinder, returnTypeOrNull, typesOrNull, modifiersOrNull };
      var result = (PropertyInfo) PrivateInvoke.InvokeNonPublicMethod (_customType, "GetPropertyImpl", arguments);

      _memberSelectorMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    public static IEnumerable<TestCaseData> GetBinderTestCases ()
    {
      var binderStub = new Mock<Binder>();
      yield return new TestCaseData (binderStub.Object, binderStub.Object).SetName ("{m}(BinderStub, BinderStub)");
      yield return new TestCaseData (null, Type.DefaultBinder);
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
    public void IsContextfulImpl ()
    {
      Assert.That (_customType.IsContextful, Is.False);
    }

    [Test]
    public void IsMarshalByRefImpl ()
    {
      Assert.That (_customType.IsMarshalByRef, Is.False);
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_customType.ToString(), Is.EqualTo ("TypeName"));
    }

    [Test]
    public void ToDebugString ()
    {
      Assert.That (_customType.ToDebugString(), Is.EqualTo ("TestableCustomType = \"TypeName\""));
    }

    [Test]
    public void GenericParameterMembers ()
    {
      Assert.That (_customType.IsGenericParameter, Is.False);

      var message = "{0} may only be called on a type for which Type.IsGenericParameter is true.";
      Assert.That (
          () => _customType.DeclaringMethod,
          Throws.InvalidOperationException.With.Message.EqualTo (string.Format (message, "Property DeclaringMethod")));
      Assert.That (
          () => _customType.GenericParameterPosition,
          Throws.InvalidOperationException.With.Message.EqualTo (string.Format (message, "Property GenericParameterPosition")));
      Assert.That (
          () => _customType.GenericParameterAttributes,
          Throws.InvalidOperationException.With.Message.EqualTo (string.Format (message, "Property GenericParameterAttributes")));
      Assert.That (
          () => _customType.GetGenericParameterConstraints(),
          Throws.InvalidOperationException.With.Message.EqualTo (string.Format (message, "Method GetGenericParameterConstraints")));
    }

    [Test]
    public void VirtualMethodsImplementedByType ()
    {
      _memberSelectorMock
          .Setup (stub => stub.SelectTypes (It.IsAny<IEnumerable<Type>>(), It.IsAny<BindingFlags>()))
          .Returns(new Type[0]);
      _memberSelectorMock
          .Setup (stub => stub.SelectEvents (It.IsAny<IEnumerable<EventInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new EventInfo[0]);
      _memberSelectorMock
          .Setup (stub => stub.SelectMethods (It.IsAny<IEnumerable<MethodInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new MethodInfo[0]);
      _memberSelectorMock
          .Setup (stub => stub.SelectMethods (It.IsAny<IEnumerable<ConstructorInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new ConstructorInfo[0]);
      _memberSelectorMock
          .Setup (stub => stub.SelectFields (It.IsAny<IEnumerable<FieldInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new FieldInfo[0]);
      _memberSelectorMock
          .Setup (stub => stub.SelectProperties (It.IsAny<IEnumerable<PropertyInfo>>(), It.IsAny<BindingFlags>(), It.IsAny<Type>()))
          .Returns (new PropertyInfo[0]);

      // None of these virtual members should throw an exception.
      Dev.Null = _customType.MemberType;
      Dev.Null = _customType.IsGenericParameter;
      Dev.Null = _customType.ContainsGenericParameters;
      Dev.Null = _customType.IsValueType; // IsValueTypeImpl()
      Dev.Null = _customType.IsContextful; // IsContextfulImpl()
      Dev.Null = _customType.IsMarshalByRef; // IsMarshalByRefImpl()

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
    public void UnderlyingSystemType ()
    {
      Assert.That (
          () => Dev.Null = _customType.UnderlyingSystemType,
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Property UnderlyingSystemType is not supported. "
                  + "Use a replacement method from class TypeExtensions (e.g. IsTypePipeAssignableFrom) to avoid accessing the property."));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.ReflectedType, "ReflectedType");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.MetadataToken, "MetadataToken");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.GUID, "GUID");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.StructLayoutAttribute, "StructLayoutAttribute");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _customType.TypeHandle, "TypeHandle");

      UnsupportedMemberTestHelper.CheckMethod (() => _customType.GetInterfaceMap (null), "GetInterfaceMap");
      UnsupportedMemberTestHelper.CheckMethod (() => _customType.GetDefaultMembers(), "GetDefaultMembers");
      UnsupportedMemberTestHelper.CheckMethod (() => _customType.InvokeMember (null, 0, null, null, null, null, null, null), "InvokeMember");
      UnsupportedMemberTestHelper.CheckMethod (() => _customType.MakePointerType(), "MakePointerType");
      UnsupportedMemberTestHelper.CheckMethod (() => _customType.GetArrayRank(), "GetArrayRank");
    }

    // This exists for GetInterface method with ignore case parameter.
    private interface Idisposable { }
  }
}