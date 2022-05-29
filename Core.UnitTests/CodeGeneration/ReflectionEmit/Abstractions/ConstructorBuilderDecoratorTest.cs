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
using Remotion.Development.Moq.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class ConstructorBuilderDecoratorTest
  {
    private Mock<IConstructorBuilder> _innerMock;
    private Mock<IEmittableOperandProvider> _operandProvider;

    private ConstructorBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = new Mock<IConstructorBuilder> (MockBehavior.Strict);
      _operandProvider = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _decorator = new ConstructorBuilderDecorator (_innerMock.Object, _operandProvider.Object);
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = new Mock<IEmittableOperandProvider>();
      var mutableConstructor = MutableConstructorInfoObjectMother.Create();

      var helper = new DecoratorTestHelper<IConstructorBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider.Object, mutableConstructor));
    }
  }
}