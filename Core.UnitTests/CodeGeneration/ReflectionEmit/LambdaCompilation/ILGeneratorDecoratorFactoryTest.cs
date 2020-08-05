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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratorDecoratorFactoryTest
  {
    private Mock<IILGeneratorFactory> _innerFactoryStub;
    private Mock<IEmittableOperandProvider> _emittableOperandProviderMock;
    private ILGeneratorDecoratorFactory _decoratorFactory;

    [SetUp]
    public void SetUp ()
    {
      _innerFactoryStub = new Mock<IILGeneratorFactory>();
      _emittableOperandProviderMock = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);
      _decoratorFactory = new ILGeneratorDecoratorFactory (_innerFactoryStub.Object, _emittableOperandProviderMock.Object);
    }

    [Test]
    public void CreateAdaptedILGenerator ()
    {
      var realILGenerator = ILGeneratorObjectMother.Create();

      var fakeInnerResult = new Mock<IILGenerator>().Object;
      _innerFactoryStub.Setup (stub => stub.CreateAdaptedILGenerator (realILGenerator)).Returns (fakeInnerResult);

      var ilGenerator = _decoratorFactory.CreateAdaptedILGenerator (realILGenerator);

      Assert.That (ilGenerator, Is.TypeOf<ILGeneratorDecorator>());
      var ilGeneratorDecorator = (ILGeneratorDecorator) ilGenerator;
      Assert.That (ilGeneratorDecorator.InnerILGenerator, Is.SameAs (fakeInnerResult));
      Assert.That (ilGeneratorDecorator.EmittableOperandProvider, Is.SameAs (_emittableOperandProviderMock.Object));
    }
  }
}