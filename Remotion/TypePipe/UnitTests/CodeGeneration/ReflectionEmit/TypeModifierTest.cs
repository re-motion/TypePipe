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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    private ISubclassProxyBuilderFactory _subclassProxyBuilderFactoryMock;

    private TypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      _subclassProxyBuilderFactoryMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilderFactory>();

      _typeModifier = new TypeModifier (_subclassProxyBuilderFactoryMock);
    }

    [Test]
    public void CodeGenerator ()
    {
      var fakeCodeGenerator = MockRepository.GenerateStub<ICodeGenerator>();
      _subclassProxyBuilderFactoryMock.Expect (mock => mock.CodeGenerator).Return (fakeCodeGenerator);

      Assert.That (_typeModifier.CodeGenerator, Is.SameAs (fakeCodeGenerator));
    }

    [Test]
    public void ApplyModifications ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var builderMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilder>();
      var fakeType = ReflectionObjectMother.GetSomeType();

      _subclassProxyBuilderFactoryMock.Expect (mock => mock.CreateBuilder (mutableType)).Return (builderMock);
      builderMock.Expect (mock => mock.Build (mutableType)).Return (fakeType);

      var result = _typeModifier.ApplyModifications (mutableType);

      _subclassProxyBuilderFactoryMock.VerifyAllExpectations();
      builderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeType));
    }
  }
}