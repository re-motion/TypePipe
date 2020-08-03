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
using Remotion.Development.Moq.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class MethodBuilderDecoratorTest
  {
    private Mock<IMethodBuilder> _innerMock;
    private Mock<IEmittableOperandProvider> _operandProviderMock;

    private MethodBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = new Mock<IMethodBuilder> (MockBehavior.Strict);
      _operandProviderMock = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _decorator = new MethodBuilderDecorator (_innerMock.Object, _operandProviderMock.Object);
    }

    [Test]
    public void DefineGenericParameters ()
    {
      var genericParameterNames = new[] { "T1", "T2" };
      var fakeGenericTypeParameterBuilder = new Mock<IGenericTypeParameterBuilder>().Object;
      _innerMock.Setup (mock => mock.DefineGenericParameters (genericParameterNames)).Returns (new[] { fakeGenericTypeParameterBuilder }).Verifiable();

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
      _operandProviderMock.Setup (mock => mock.GetEmittableType (returnType)).Returns (emittableReturnType).Verifiable();
      _innerMock.Setup (mock => mock.SetReturnType (emittableReturnType)).Verifiable();

      _decorator.SetReturnType (returnType);

      _operandProviderMock.Verify();
      _innerMock.Verify();
    }

    [Test]
    public void SetParameters ()
    {
      var parameterType = ReflectionObjectMother.GetSomeType();
      var emittableParameterType = ReflectionObjectMother.GetSomeOtherType();
      _operandProviderMock.Setup (mock => mock.GetEmittableType (parameterType)).Returns (emittableParameterType).Verifiable();
      _innerMock.Setup (mock => mock.SetParameters (new[] { emittableParameterType })).Verifiable();

      _decorator.SetParameters (new[] { parameterType });

      _operandProviderMock.Verify();
      _innerMock.Verify();
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = new Mock<IEmittableOperandProvider>();
      var mutableMethod = MutableMethodInfoObjectMother.Create();

      var helper = new DecoratorTestHelper<IMethodBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider.Object, mutableMethod));
    }
  }
}