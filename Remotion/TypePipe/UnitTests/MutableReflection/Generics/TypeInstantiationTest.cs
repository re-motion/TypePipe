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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiationTest
  {
    private const BindingFlags c_allBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private IMemberSelector _memberSelectorMock;
    private ITypeInstantiator _typeInstantiatorMock;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _typeInstantiatorMock = MockRepository.GenerateStrictMock<ITypeInstantiator>();
    }

    [Test]
    public void Initialization_AdjustBaseType_AndNames ()
    {
      var genericTypeDefinition = CustomTypeObjectMother.Create (_memberSelectorMock, isGenericTypeDefinition: true);
      var fakeBaseType = ReflectionObjectMother.GetSomeType();
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (genericTypeDefinition.BaseType)).Return (fakeBaseType);
      _typeInstantiatorMock.Expect (mock => mock.GetSimpleName (genericTypeDefinition)).Return ("name");
      _typeInstantiatorMock.Expect (mock => mock.GetFullName (genericTypeDefinition)).Return ("full name");
      SetupExpectationsOnMemberSelector (genericTypeDefinition);

      var instantiation = CreateTypeInstantion (_typeInstantiatorMock, genericTypeDefinition);

      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.BaseType, Is.SameAs (fakeBaseType));
      Assert.That (instantiation.Name, Is.EqualTo ("name"));
      Assert.That (instantiation.Namespace, Is.EqualTo (genericTypeDefinition.Namespace));
      Assert.That (instantiation.FullName, Is.EqualTo ("full name"));
      Assert.That (instantiation.Attributes, Is.EqualTo (genericTypeDefinition.Attributes));
      Assert.That (instantiation.IsGenericType, Is.True);
      Assert.That (instantiation.IsGenericTypeDefinition, Is.True);
    }

    [Test]
    public void Initialization_AdjustInterfaces ()
    {
      var iface = ReflectionObjectMother.GetSomeInterfaceType();
      var fakeInterface = ReflectionObjectMother.GetSomeDifferentInterfaceType();
      var genericTypeDefinition = CustomTypeObjectMother.Create (_memberSelectorMock, isGenericTypeDefinition: true, interfaces: new[] { iface });
      StubBaseTypeAdjustment (genericTypeDefinition);
      SetupExpectationsOnMemberSelector (genericTypeDefinition);
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (iface)).Return (fakeInterface);

      var instantiation = CreateTypeInstantion (_typeInstantiatorMock, genericTypeDefinition);

      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.GetInterfaces(), Is.EqualTo (new[] { fakeInterface }));
    }

    [Test]
    public void Initialization_AdjustFields ()
    {
      var fields = new FieldInfo[0];
      var fakeField1 = ReflectionObjectMother.GetSomeField();
      var fakeField2 = ReflectionObjectMother.GetSomeOtherField();
      var genericTypeDefinition = CustomTypeObjectMother.Create (_memberSelectorMock, isGenericTypeDefinition: true, fields: fields);
      StubBaseTypeAdjustment (genericTypeDefinition);
      SetupExpectationsOnMemberSelector (genericTypeDefinition, inputFields: fields, outputFields: new[] { fakeField1 });
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (fakeField1)).Return (fakeField2);

      var instantiation = CreateTypeInstantion(_typeInstantiatorMock, genericTypeDefinition);

      _memberSelectorMock.VerifyAllExpectations();
      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.GetFields (c_allBindingFlags), Is.EqualTo (new[] { fakeField2 }));
    }

    [Test]
    public void Initialization_AdjustConstructors ()
    {
      var constructors = new ConstructorInfo[0];
      var fakeConstructor1 = ReflectionObjectMother.GetSomeConstructor();
      var fakeConstructor2 = ReflectionObjectMother.GetSomeOtherConstructor();
      var genericTypeDefinition = CustomTypeObjectMother.Create (_memberSelectorMock, isGenericTypeDefinition: true, constructors: constructors);
      StubBaseTypeAdjustment (genericTypeDefinition);
      SetupExpectationsOnMemberSelector (genericTypeDefinition, inputConstructors: constructors, outputConstructors: new[] { fakeConstructor1 });
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (fakeConstructor1)).Return (fakeConstructor2);

      var instantiation = CreateTypeInstantion(_typeInstantiatorMock, genericTypeDefinition);

      _memberSelectorMock.VerifyAllExpectations();
      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.GetConstructors (c_allBindingFlags), Is.EqualTo (new[] { fakeConstructor2 }));
    }

    [Test]
    public void Initialization_AdjustMethods ()
    {
      var methods = new MethodInfo[0];
      var fakeMethod1 = ReflectionObjectMother.GetSomeMethod();
      var fakeMethod2 = ReflectionObjectMother.GetSomeOtherMethod();
      var genericTypeDefinition = CustomTypeObjectMother.Create (_memberSelectorMock, isGenericTypeDefinition: true, methods: methods);
      StubBaseTypeAdjustment (genericTypeDefinition);
      SetupExpectationsOnMemberSelector (genericTypeDefinition, inputMethods: methods, outputMethods: new[] { fakeMethod1 });
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (fakeMethod1)).Return (fakeMethod2);

      var instantiation = CreateTypeInstantion(_typeInstantiatorMock, genericTypeDefinition);

      _memberSelectorMock.VerifyAllExpectations();
      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.GetMethods (c_allBindingFlags), Is.EqualTo (new[] { fakeMethod2 }));
    }

    private void SetupExpectationsOnMemberSelector (
        Type declaringType,
        FieldInfo[] inputFields = null,
        FieldInfo[] outputFields = null,
        ConstructorInfo[] inputConstructors = null,
        ConstructorInfo[] outputConstructors = null,
        MethodInfo[] inputMethods = null,
        MethodInfo[] outputMethods = null)
    {
      inputFields = inputFields ?? new FieldInfo[0];
      outputFields = outputFields ?? new FieldInfo[0];
      inputConstructors = inputConstructors ?? new ConstructorInfo[0];
      outputConstructors = outputConstructors ?? new ConstructorInfo[0];
      inputMethods = inputMethods ?? new MethodInfo[0];
      outputMethods = outputMethods ?? new MethodInfo[0];

      _memberSelectorMock.Expect (mock => mock.SelectFields (inputFields, c_allBindingFlags)).Return (outputFields);
      _memberSelectorMock.Expect (mock => mock.SelectMethods (inputConstructors, c_allBindingFlags, declaringType)).Return (outputConstructors);
      _memberSelectorMock.Expect (mock => mock.SelectMethods (inputMethods, c_allBindingFlags, declaringType)).Return (outputMethods);
    }

    private TypeInstantiation CreateTypeInstantion (ITypeInstantiator typeInstantiator, CustomType genericTypeDefinition)
    {
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var underlyingTypeFactory = new ThrowingUnderlyingTypeFactory();
      return new TypeInstantiation (memberSelector, underlyingTypeFactory, typeInstantiator, genericTypeDefinition);
    }

    private void StubBaseTypeAdjustment (Type genericTypeDefinition)
    {
      var fakeBaseType = ReflectionObjectMother.GetSomeType();
      _typeInstantiatorMock.Stub (stub => stub.SubstituteGenericParameters (genericTypeDefinition.BaseType)).Return (fakeBaseType);
      _typeInstantiatorMock.Stub (stub => stub.GetSimpleName (genericTypeDefinition)).Return ("name");
      _typeInstantiatorMock.Stub (stub => stub.GetFullName (genericTypeDefinition)).Return ("full name");
    }
  }
}