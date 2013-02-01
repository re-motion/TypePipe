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
      SetupExpectationsOnMemberSelector();

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
      SetupExpectationsOnMemberSelector();
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
      var fakeField2 = ReflectionObjectMother.GetSomeField();
      var genericTypeDefinition = CustomTypeObjectMother.Create (_memberSelectorMock, isGenericTypeDefinition: true, fields: fields);
      StubBaseTypeAdjustment (genericTypeDefinition);
      SetupExpectationsOnMemberSelector (inputFields: fields, outputFields: new[] { fakeField1 });
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (fakeField1)).Return (fakeField2);

      var instantiation = CreateTypeInstantion(_typeInstantiatorMock, genericTypeDefinition);

      _memberSelectorMock.VerifyAllExpectations();
      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.GetFields (c_allBindingFlags), Is.EqualTo (new[] { fakeField2 }));
    }

    private void SetupExpectationsOnMemberSelector (FieldInfo[] inputFields = null, FieldInfo[] outputFields = null)
    {
      inputFields = inputFields ?? new FieldInfo[0];
      outputFields = outputFields ?? new FieldInfo[0];

      _memberSelectorMock.Expect (mock => mock.SelectFields (inputFields, c_allBindingFlags)).Return (outputFields);
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