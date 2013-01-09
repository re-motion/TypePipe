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
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class ConstructorBuilderDecoratorTest
  {
    private IConstructorBuilder _inner;
    private IEmittableOperandProvider _operandProvider;

    private ConstructorBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _inner = MockRepository.GenerateStrictMock<IConstructorBuilder>();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = new ConstructorBuilderDecorator (_inner, _operandProvider);
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var mutableConstructor = MutableConstructorInfoObjectMother.Create();

      var helper = new DecoratorTestHelper<IConstructorBuilder> (_decorator, _inner);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider, mutableConstructor));
    }
  }
}