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
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class GenericTypeParameterBuilderDecoratorTest
  {
    private IGenericTypeParameterBuilder _innerMock;
    private IEmittableOperandProvider _operandProvider;

    private GenericTypeParameterBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<IGenericTypeParameterBuilder> ();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider> ();

      _decorator = new GenericTypeParameterBuilderDecorator (_innerMock, _operandProvider);
    }

    [Test]
    public void SetBaseTypeConstraint ()
    {
      var baseTypeConstraint = ReflectionObjectMother.GetSomeType();
      var emittableBaseTypeConstraint = ReflectionObjectMother.GetSomeOtherType();
      _operandProvider.Expect (mock => mock.GetEmittableType (baseTypeConstraint)).Return (emittableBaseTypeConstraint);
      _innerMock.Expect (mock => mock.SetBaseTypeConstraint (emittableBaseTypeConstraint));

      _decorator.SetBaseTypeConstraint (baseTypeConstraint);

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void SetInterfaceConstraints ()
    {
      var interfaceConstraint = ReflectionObjectMother.GetSomeInterfaceType();
      var emittableInterfaceConstraint = ReflectionObjectMother.GetSomeOtherInterfaceType();
      _operandProvider.Expect (mock => mock.GetEmittableType (interfaceConstraint)).Return (emittableInterfaceConstraint);
      _innerMock.Expect (mock => mock.SetInterfaceConstraints (new[] { emittableInterfaceConstraint }));

      _decorator.SetInterfaceConstraints (new[] { interfaceConstraint });

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var genericParameter = MutableGenericParameterObjectMother.Create();

      var helper = new DecoratorTestHelper<IGenericTypeParameterBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider, genericParameter));
      helper.CheckDelegation (d => d.SetGenericParameterAttributes ((GenericParameterAttributes) 7));
    }
  }
}