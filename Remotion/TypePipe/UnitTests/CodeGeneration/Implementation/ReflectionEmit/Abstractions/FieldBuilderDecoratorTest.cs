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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.Abstractions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.Implementation.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class FieldBuilderDecoratorTest
  {
    private IFieldBuilder _innerMock;
    private IEmittableOperandProvider _operandProvider;

    private FieldBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<IFieldBuilder> ();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider> ();

      _decorator = new FieldBuilderDecorator (_innerMock, _operandProvider);
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var mutableField = MutableFieldInfoObjectMother.Create();

      var helper = new DecoratorTestHelper<IFieldBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider, mutableField));
    }
  }
}