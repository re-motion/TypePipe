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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class EventBuilderDecoratorTest
  {
    private IEventBuilder _innerMock;
    private IEmittableOperandProvider _operandProviderMock;

    private EventBuilderDecorator _decorator;

    private IMethodBuilder _decoratedMethodBuilder;
    private MethodBuilderDecorator _methodBuilderDecorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<IEventBuilder>();
      _operandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = new EventBuilderDecorator (_innerMock, _operandProviderMock);

      _decoratedMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      _methodBuilderDecorator = new MethodBuilderDecorator (_decoratedMethodBuilder, _operandProviderMock);
    }

    [Test]
    public void SetAddOnMethod ()
    {
      _innerMock.Expect (mock => mock.SetAddOnMethod (_decoratedMethodBuilder));

      _decorator.SetAddOnMethod (_methodBuilderDecorator);

      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void SetRemoveOnMethod ()
    {
      _innerMock.Expect (mock => mock.SetRemoveOnMethod (_decoratedMethodBuilder));

      _decorator.SetRemoveOnMethod (_methodBuilderDecorator);

      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void SetRaiseMethod ()
    {
      _innerMock.Expect (mock => mock.SetRaiseMethod (_decoratedMethodBuilder));

      _decorator.SetRaiseMethod (_methodBuilderDecorator);

      _innerMock.VerifyAllExpectations();
    }
  }
}