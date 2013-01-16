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
  public class SubclassProxyBuilderFactoryTest
  {
    private IReflectionEmitCodeGenerator _codeGeneratorMock;

    private SubclassProxyBuilderFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _codeGeneratorMock = MockRepository.GenerateStrictMock<IReflectionEmitCodeGenerator>();

      _factory = new SubclassProxyBuilderFactory (_codeGeneratorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_factory.CodeGenerator, Is.SameAs (_codeGeneratorMock));
    }

    [Test]
    public void CreateBuilder ()
    {
      var baseType = ReflectionObjectMother.GetSomeType();
      var proxyType = ProxyTypeObjectMother.Create (baseType, fullName: "My.AbcProxy", attributes: (TypeAttributes) 7);

      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder>();
      _codeGeneratorMock.Expect (mock => mock.DefineType ("My.AbcProxy", (TypeAttributes) 7, baseType)).Return (typeBuilderMock);
      var fakeEmittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var fakeDebugInfoGenerator = MockRepository.GenerateStub<DebugInfoGenerator>();
      _codeGeneratorMock.Expect (mock => mock.EmittableOperandProvider).Return (fakeEmittableOperandProvider);
      typeBuilderMock.Expect (mock => mock.RegisterWith (fakeEmittableOperandProvider, proxyType));
      _codeGeneratorMock.Expect (mock => mock.DebugInfoGenerator).Return (fakeDebugInfoGenerator);

      var result = _factory.CreateBuilder (proxyType);

      _codeGeneratorMock.VerifyAllExpectations();
      typeBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<SubclassProxyBuilder>());
      var builder = (SubclassProxyBuilder) result;

      var context = builder.MemberEmitterContext;
      Assert.That (context.ProxyType, Is.SameAs (proxyType));
      Assert.That (context.TypeBuilder, Is.SameAs (typeBuilderMock));
      Assert.That (context.DebugInfoGenerator, Is.SameAs (fakeDebugInfoGenerator));
      Assert.That (context.EmittableOperandProvider, Is.SameAs (fakeEmittableOperandProvider));
      Assert.That (context.MethodTrampolineProvider, Is.TypeOf<MethodTrampolineProvider>());
      Assert.That (context.PostDeclarationsActionManager.Actions, Is.Empty);

      Assert.That (builder.InitializationBuilder, Is.TypeOf<InitializationBuilder>());
      Assert.That (builder.ProxySerializationEnabler, Is.TypeOf<ProxySerializationEnabler>());
      Assert.That (builder.MemberEmitter, Is.TypeOf<MemberEmitter>());
      var memberEmitter = (MemberEmitter) builder.MemberEmitter;

      Assert.That (memberEmitter.ExpressionPreparer, Is.TypeOf<ExpressionPreparer>());
      Assert.That (memberEmitter.ILGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());
      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) memberEmitter.ILGeneratorFactory;

      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory>());
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (fakeEmittableOperandProvider));

      var methodTrampolineProvider = (MethodTrampolineProvider) context.MethodTrampolineProvider;
      Assert.That (methodTrampolineProvider.MemberEmitter, Is.SameAs (memberEmitter));
    }
  }
}