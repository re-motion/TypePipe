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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class MethodBuilderDecoratorTest
  {
    private IMethodBuilder _innerMock;
    private IEmittableOperandProvider _operandProviderMock;

    private MethodBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<IMethodBuilder>();
      _operandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = new MethodBuilderDecorator (_innerMock, _operandProviderMock);
    }

    [Test]
    public void DefineGenericParameters ()
    {
      var genericParameterNames = new[] { "T1", "T2" };
      var fakeGenericTypeParameterBuilder = MockRepository.GenerateStub<IGenericTypeParameterBuilder>();
      _innerMock.Expect (mock => mock.DefineGenericParameters (genericParameterNames)).Return (new[] { fakeGenericTypeParameterBuilder });

      var results = _decorator.DefineGenericParameters (genericParameterNames);

      var result = results.Single();
      Assert.That (result, Is.TypeOf<GenericTypeParameterBuilderDecorator>());
      // Use field from base class 'BuilderDecoratorBase'.
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_customAttributeTargetBuilder"), Is.SameAs (fakeGenericTypeParameterBuilder));
    }

    [Test]
    public void SetReturnType ()
    {
      var returnType = ReflectionObjectMother.GetSomeType();
      var emittableReturnType = ReflectionObjectMother.GetSomeOtherType();
      _operandProviderMock.Expect (mock => mock.GetEmittableType (returnType)).Return (emittableReturnType);
      _innerMock.Expect (mock => mock.SetReturnType (emittableReturnType));

      _decorator.SetReturnType (returnType);

      _operandProviderMock.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void SetParameters ()
    {
      var parameterType = ReflectionObjectMother.GetSomeType();
      var emittableParameterType = ReflectionObjectMother.GetSomeOtherType();
      _operandProviderMock.Expect (mock => mock.GetEmittableType (parameterType)).Return (emittableParameterType);
      _innerMock.Expect (mock => mock.SetParameters (new[] { emittableParameterType }));

      _decorator.SetParameters (new[] { parameterType });

      _operandProviderMock.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var mutableMethod = MutableMethodInfoObjectMother.Create();

      var helper = new DecoratorTestHelper<IMethodBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider, mutableMethod));
    }
  }
}