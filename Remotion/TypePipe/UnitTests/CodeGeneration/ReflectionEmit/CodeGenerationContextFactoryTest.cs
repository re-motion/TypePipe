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
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class CodeGenerationContextFactoryTest
  {
    private IReflectionEmitCodeGenerator _codeGeneratorMock;

    private CodeGenerationContextFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _codeGeneratorMock = MockRepository.GenerateStrictMock<IReflectionEmitCodeGenerator>();

      _factory = new CodeGenerationContextFactory (_codeGeneratorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_factory.CodeGenerator, Is.SameAs (_codeGeneratorMock));
    }

    [Test]
    public void CreateContext ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var proxyType = ProxyTypeObjectMother.Create (baseType, fullName: "My.AbcProxy", attributes: (TypeAttributes) 7);

      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder>();
      _codeGeneratorMock.Expect (mock => mock.DefineType ("My.AbcProxy", (TypeAttributes) 7, baseType)).Return (typeBuilderMock);
      var fakeEmittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var fakeDebugInfoGenerator = MockRepository.GenerateStub<DebugInfoGenerator>();
      _codeGeneratorMock.Expect (mock => mock.EmittableOperandProvider).Return (fakeEmittableOperandProvider);
      typeBuilderMock.Expect (mock => mock.RegisterWith (fakeEmittableOperandProvider, proxyType));
      _codeGeneratorMock.Expect (mock => mock.DebugInfoGenerator).Return (fakeDebugInfoGenerator);

      var result = _factory.CreateContext (proxyType);

      _codeGeneratorMock.VerifyAllExpectations();
      typeBuilderMock.VerifyAllExpectations();

      Assert.That (result.ProxyType, Is.SameAs (proxyType));
      Assert.That (result.TypeBuilder, Is.SameAs (typeBuilderMock));
      Assert.That (result.DebugInfoGenerator, Is.SameAs (fakeDebugInfoGenerator));
      Assert.That (result.EmittableOperandProvider, Is.SameAs (fakeEmittableOperandProvider));
      Assert.That (result.TrampolineMethods, Is.Empty);
      Assert.That (result.MemberEmitter, Is.TypeOf<MemberEmitter>());
      Assert.That (result.PostDeclarationsActionManager.Actions, Is.Empty);

      var memberEmitter = (MemberEmitter) result.MemberEmitter;
      Assert.That (memberEmitter.ExpressionPreparer, Is.TypeOf<ExpressionPreparer>());
      Assert.That (memberEmitter.ILGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());

      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) memberEmitter.ILGeneratorFactory;
      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory>());
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (fakeEmittableOperandProvider));
    }
  }
}