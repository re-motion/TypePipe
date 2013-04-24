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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class MethodBaseBuilderDecoratorBaseTest
  {
    private IMethodBaseBuilder _innerMock;
    private IEmittableOperandProvider _operandProvider;

    private MethodBaseBuilderDecoratorBase _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<IMethodBaseBuilder> ();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider> ();

      _decorator = MockRepository.GeneratePartialMock<MethodBaseBuilderDecoratorBase> (_innerMock, _operandProvider);
    }

    [Test]
    public void DefineParameter ()
    {
      var iSequence = 7;
      var attributes = (ParameterAttributes) 7;
      var parameterName = "parameter";

      var fakeParameterBuilder = MockRepository.GenerateStub<IParameterBuilder>();
      _innerMock.Expect (mock => mock.DefineParameter (iSequence, attributes, parameterName)).Return (fakeParameterBuilder);

      var result = _decorator.DefineParameter (iSequence, attributes, parameterName);

      _innerMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<ParameterBuilderDecorator>());
      // Use field from base class 'BuilderDecoratorBase'.
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_customAttributeTargetBuilder"), Is.SameAs (fakeParameterBuilder));
    }

    [Test]
    public void DelegatingMembers ()
    {
      var lambdaExpression = Expression.Lambda (Expression.Empty());
      var ilGeneratorFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory>();
      var debugInfoGenerator = MockRepository.GenerateStub<DebugInfoGenerator>();

      var helper = new DecoratorTestHelper<IMethodBaseBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.SetBody (lambdaExpression, ilGeneratorFactoryStub, debugInfoGenerator));
    }
  }
}