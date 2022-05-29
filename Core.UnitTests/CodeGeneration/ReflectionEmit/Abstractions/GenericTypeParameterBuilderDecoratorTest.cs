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
using Remotion.Development.Moq.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class GenericTypeParameterBuilderDecoratorTest
  {
    private Mock<IGenericTypeParameterBuilder> _innerMock;
    private Mock<IEmittableOperandProvider> _operandProvider;

    private GenericTypeParameterBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = new Mock<IGenericTypeParameterBuilder> (MockBehavior.Strict);
      _operandProvider = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _decorator = new GenericTypeParameterBuilderDecorator (_innerMock.Object, _operandProvider.Object);
    }

    [Test]
    public void SetBaseTypeConstraint ()
    {
      var baseTypeConstraint = ReflectionObjectMother.GetSomeType();
      var emittableBaseTypeConstraint = ReflectionObjectMother.GetSomeOtherType();
      _operandProvider.Setup (mock => mock.GetEmittableType (baseTypeConstraint)).Returns (emittableBaseTypeConstraint);
      _innerMock.Setup (mock => mock.SetBaseTypeConstraint (emittableBaseTypeConstraint)).Verifiable();

      _decorator.SetBaseTypeConstraint (baseTypeConstraint);

      _operandProvider.Verify();
      _innerMock.Verify();
    }

    [Test]
    public void SetInterfaceConstraints ()
    {
      var interfaceConstraint = ReflectionObjectMother.GetSomeInterfaceType();
      var emittableInterfaceConstraint = ReflectionObjectMother.GetSomeOtherInterfaceType();
      _operandProvider.Setup (mock => mock.GetEmittableType (interfaceConstraint)).Returns (emittableInterfaceConstraint).Verifiable();
      _innerMock.Setup (mock => mock.SetInterfaceConstraints (new[] { emittableInterfaceConstraint })).Verifiable();

      _decorator.SetInterfaceConstraints (new[] { interfaceConstraint });

      _operandProvider.Verify();
      _innerMock.Verify();
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = new Mock<IEmittableOperandProvider>();
      var genericParameter = MutableGenericParameterObjectMother.Create();

      var helper = new DecoratorTestHelper<IGenericTypeParameterBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider.Object, genericParameter));
      helper.CheckDelegation (d => d.SetGenericParameterAttributes ((GenericParameterAttributes) 7));
    }
  }
}