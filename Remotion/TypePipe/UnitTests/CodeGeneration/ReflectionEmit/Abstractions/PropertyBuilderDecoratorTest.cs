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
  public class PropertyBuilderDecoratorTest
  {
    private IPropertyBuilder _innerMock;
    private IEmittableOperandProvider _operandProviderMock;

    private PropertyBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<IPropertyBuilder>();
      _operandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = new PropertyBuilderDecorator (_innerMock, _operandProviderMock);
    }

    [Test]
    public void SetGetMethod ()
    {
      var methodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      var methodBuilderDecorator = new MethodBuilderDecorator (methodBuilder, _operandProviderMock);
      _innerMock.Expect (mock => mock.SetGetMethod (methodBuilder));

      _decorator.SetGetMethod (methodBuilderDecorator);

      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void SetSetMethod ()
    {
      var methodBuilder = MockRepository.GenerateStub<IMethodBuilder> ();
      var methodBuilderDecorator = new MethodBuilderDecorator (methodBuilder, _operandProviderMock);
      _innerMock.Expect (mock => mock.SetSetMethod (methodBuilder));

      _decorator.SetSetMethod (methodBuilderDecorator);

      _innerMock.VerifyAllExpectations ();
    }
  }
}