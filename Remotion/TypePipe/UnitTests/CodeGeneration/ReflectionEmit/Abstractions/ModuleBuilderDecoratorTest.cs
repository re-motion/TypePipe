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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class ModuleBuilderDecoratorTest
  {
    private IModuleBuilder _inner;
    private IEmittableOperandProvider _operandProvider;
    
    private ModuleBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _inner = MockRepository.GenerateStrictMock<IModuleBuilder>();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _decorator = new ModuleBuilderDecorator (_inner, _operandProvider);
    }

    [Test]
    public void DefineType ()
    {
      var name = "method";
      var attributes = (TypeAttributes) 7;
      var baseType = ReflectionObjectMother.GetSomeType();

      var emittableBaseType = ReflectionObjectMother.GetSomeDifferentType();
      var fakeTypeBuilder = MockRepository.GenerateStub<ITypeBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (baseType)).Return (emittableBaseType);
      _inner.Expect (mock => mock.DefineType (name, attributes, emittableBaseType)).Return (fakeTypeBuilder);

      var result = _decorator.DefineType (name, attributes, baseType);

      _operandProvider.VerifyAllExpectations();
      _inner.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<TypeBuilderDecorator>());
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_typeBuilder"), Is.SameAs (fakeTypeBuilder));
    }

    [Test]
    public void DelegatingMembers ()
    {
      var helper = new DecoratorTestHelper<IModuleBuilder> (_decorator, _inner);

      helper.CheckDelegation (d => d.SaveToDisk(), "assembly path");
    }
  }
}