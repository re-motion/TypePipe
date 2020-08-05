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
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class PropertyBuilderDecoratorTest
  {
    private Mock<IPropertyBuilder> _innerMock;
    private Mock<IEmittableOperandProvider> _operandProviderMock;

    private PropertyBuilderDecorator _decorator;

    private IMethodBuilder _decoratedMethodBuilder;
    private MethodBuilderDecorator _methodBuilderDecorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = new Mock<IPropertyBuilder> (MockBehavior.Strict);
      _operandProviderMock = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _decorator = new PropertyBuilderDecorator (_innerMock.Object, _operandProviderMock.Object);

      _decoratedMethodBuilder = new Mock<IMethodBuilder>().Object;
      _methodBuilderDecorator = new MethodBuilderDecorator (_decoratedMethodBuilder, _operandProviderMock.Object);
    }

    [Test]
    public void SetGetMethod ()
    {
      _innerMock.Setup (mock => mock.SetGetMethod (_decoratedMethodBuilder)).Verifiable();

      _decorator.SetGetMethod (_methodBuilderDecorator);

      _innerMock.Verify();
    }

    [Test]
    public void SetSetMethod ()
    {
      _innerMock.Setup (mock => mock.SetSetMethod (_decoratedMethodBuilder)).Verifiable();

      _decorator.SetSetMethod (_methodBuilderDecorator);

      _innerMock.Verify();
    }
  }
}