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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiationTest
  {
    private MemberSelector _memberSelector;
    private ThrowingUnderlyingTypeFactory _underlyingTypeFactory;
    private ITypeInstantiator _typeInstantiatorMock;

    [SetUp]
    public void SetUp ()
    {
      _memberSelector = new MemberSelector(new BindingFlagsEvaluator());
      _underlyingTypeFactory = new ThrowingUnderlyingTypeFactory();
      _typeInstantiatorMock = MockRepository.GenerateStrictMock<ITypeInstantiator>();
    }

    [Test]
    public void Initialization_AdjustBaseType_AndNames ()
    {
      var genericTypeDefinition = typeof (SimpleDomainType<>);
      var fakeBaseType = ReflectionObjectMother.GetSomeType();
      _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (genericTypeDefinition.BaseType)).Return (fakeBaseType);
      _typeInstantiatorMock.Expect (mock => mock.GetSimpleName (genericTypeDefinition)).Return ("name");
      _typeInstantiatorMock.Expect (mock => mock.GetFullName (genericTypeDefinition)).Return ("full name");

      var instantiation = new TypeInstantiation (_memberSelector, _underlyingTypeFactory, _typeInstantiatorMock, genericTypeDefinition);

      _typeInstantiatorMock.VerifyAllExpectations();
      Assert.That (instantiation.BaseType, Is.SameAs (fakeBaseType));
      Assert.That (instantiation.Name, Is.EqualTo ("name"));
      Assert.That (instantiation.Namespace, Is.EqualTo (genericTypeDefinition.Namespace));
      Assert.That (instantiation.FullName, Is.EqualTo ("full name"));
      Assert.That (instantiation.Attributes, Is.EqualTo (genericTypeDefinition.Attributes));
      Assert.That (instantiation.IsGenericType, Is.True);
      Assert.That (instantiation.IsGenericTypeDefinition, Is.True);
    }

    //[Test]
    //public void Initialization_AdjustInterfaces ()
    //{
    //  var genericTypeDefinition = CustomTypeObjectMother.Create(typeof (DomainTypeWithInterface<>);
    //  StubBaseTypeAdjustment (genericTypeDefinition);
    //  var fakeInterface = ReflectionObjectMother.GetSomeType();
    //  _typeInstantiatorMock.Expect (mock => mock.SubstituteGenericParameters (typeof (IMyInterface))).Return (fakeInterface);

    //  var instantiation = new TypeInstantiation (_memberSelector, _underlyingTypeFactory, _typeInstantiatorMock, genericTypeDefinition);

    //  _typeInstantiatorMock.VerifyAllExpectations();
    //  Assert.That (instantiation.GetInterfaces(), Is.EqualTo (new[] { fakeInterface }));
    //}

    private void StubBaseTypeAdjustment (Type genericTypeDefinition)
    {
      var fakeBaseType = ReflectionObjectMother.GetSomeType();
      _typeInstantiatorMock.Stub (stub => stub.SubstituteGenericParameters (genericTypeDefinition.BaseType)).Return (fakeBaseType);
      _typeInstantiatorMock.Stub (stub => stub.GetSimpleName (genericTypeDefinition)).Return ("name");
      _typeInstantiatorMock.Stub (stub => stub.GetFullName (genericTypeDefinition)).Return ("full name");
    }

    class SimpleDomainType<T> { }
    class DomainTypeWithInterface<T> : IMyInterface { }

    interface IMyInterface{}
  }
}