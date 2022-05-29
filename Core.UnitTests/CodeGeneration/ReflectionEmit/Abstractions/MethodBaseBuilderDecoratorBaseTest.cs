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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class MethodBaseBuilderDecoratorBaseTest
  {
    private Mock<IMethodBaseBuilder> _innerMock;
    private Mock<IEmittableOperandProvider> _operandProvider;

    private Mock<MethodBaseBuilderDecoratorBase> _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = new Mock<IMethodBaseBuilder> (MockBehavior.Strict);
      _operandProvider = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _decorator = new Mock<MethodBaseBuilderDecoratorBase> (_innerMock.Object, _operandProvider.Object) { CallBase = true };
    }

    [Test]
    public void DefineParameter ()
    {
      var iSequence = 7;
      var attributes = (ParameterAttributes) 7;
      var parameterName = "parameter";

      var fakeParameterBuilder = new Mock<IParameterBuilder>();
      _innerMock.Setup (mock => mock.DefineParameter (iSequence, attributes, parameterName)).Returns (fakeParameterBuilder.Object).Verifiable();

      var result = _decorator.Object.DefineParameter (iSequence, attributes, parameterName);

      _innerMock.Verify();
      Assert.That (result, Is.TypeOf<ParameterBuilderDecorator>());
      // Use field from base class 'BuilderDecoratorBase'.
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_customAttributeTargetBuilder"), Is.SameAs (fakeParameterBuilder.Object));
    }

    [Test]
    public void DelegatingMembers ()
    {
      var lambdaExpression = Expression.Lambda (Expression.Empty());
      var ilGeneratorFactoryStub = new Mock<IILGeneratorFactory>();
      var debugInfoGenerator = new Mock<DebugInfoGenerator>();

      var helper = new DecoratorTestHelper<IMethodBaseBuilder> (_decorator.Object, _innerMock);

      helper.CheckDelegation (d => d.SetBody (lambdaExpression, ilGeneratorFactoryStub.Object, debugInfoGenerator.Object));
    }
  }
}