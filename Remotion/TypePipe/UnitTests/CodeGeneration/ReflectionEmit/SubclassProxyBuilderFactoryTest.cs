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
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class SubclassProxyBuilderFactoryTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderMock;
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private IExpressionPreparer _expressionPreparer;

    private SubclassProxyBuilderFactory _builderFactory;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateMock<IModuleBuilder> ();
      _subclassProxyNameProviderMock = MockRepository.GenerateMock<ISubclassProxyNameProvider> ();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator> ();
      _expressionPreparer = MockRepository.GenerateStub<IExpressionPreparer>();

      _builderFactory = new SubclassProxyBuilderFactory (
          _moduleBuilderMock, _subclassProxyNameProviderMock, _expressionPreparer, _debugInfoGeneratorStub);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var handlerFactory = new SubclassProxyBuilderFactory (_moduleBuilderMock, _subclassProxyNameProviderMock, _expressionPreparer, null);
      Assert.That (handlerFactory.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void CreateBuilder ()
    {
      var originalType = ReflectionObjectMother.GetSomeSubclassableType();
      var mutableType = MutableTypeObjectMother.CreateForExistingType(originalType: originalType);

      _subclassProxyNameProviderMock.Expect (mock => mock.GetSubclassProxyName (mutableType)).Return ("foofoo");

      var typeBuilderStub = MockRepository.GenerateStub<ITypeBuilder> ();
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", attributes, originalType))
          .Return (typeBuilderStub);

      var result = _builderFactory.CreateBuilder (mutableType);

      Assert.That (result, Is.TypeOf<SubclassProxyBuilder>());
      var builder = (SubclassProxyBuilder) result;

      Assert.That (builder.TypeBuilder, Is.SameAs (typeBuilderStub));
      Assert.That (builder.ExpressionPreparer, Is.SameAs (_expressionPreparer));
      Assert.That (builder.EmittableOperandProvider.GetEmittableOperand (mutableType), Is.SameAs (typeBuilderStub));

      Assert.That (builder.ILGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());
      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) builder.ILGeneratorFactory;
      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory> ());
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (builder.EmittableOperandProvider));

      Assert.That (builder.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
    }
  }
}