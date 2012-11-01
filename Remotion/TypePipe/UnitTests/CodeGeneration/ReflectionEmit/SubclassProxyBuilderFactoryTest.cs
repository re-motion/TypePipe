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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class SubclassProxyBuilderFactoryTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private DebugInfoGenerator _debugInfoGeneratorStub;

    private SubclassProxyBuilderFactory _builderFactory;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateMock<IModuleBuilder> ();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator> ();

      _builderFactory = new SubclassProxyBuilderFactory (_moduleBuilderMock, _debugInfoGeneratorStub);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var handlerFactory = new SubclassProxyBuilderFactory (_moduleBuilderMock, null);
      Assert.That (handlerFactory.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void CreateBuilder ()
    {
      var originalType = ReflectionObjectMother.GetSomeSubclassableType();
      var mutableType = MutableTypeObjectMother.CreateForExisting (originalType);

      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder> ();
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
      _moduleBuilderMock.Expect (mock => mock.DefineType (originalType.FullName, attributes, originalType)).Return (typeBuilderMock);
      
      EmittableOperandProvider emittableOperandProvider = null;
      typeBuilderMock
          .Expect (mock => mock.RegisterWith (Arg<EmittableOperandProvider>.Is.TypeOf, Arg.Is (mutableType)))
          .WhenCalled (mi => emittableOperandProvider = ((EmittableOperandProvider) mi.Arguments[0]));

      var result = _builderFactory.CreateBuilder (mutableType);

      _moduleBuilderMock.VerifyAllExpectations();
      typeBuilderMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf<SubclassProxyBuilder>());
      var builder = (SubclassProxyBuilder) result;

      var context = builder.MemberEmitterContext;
      Assert.That (context.MutableType, Is.SameAs (mutableType));
      Assert.That (context.TypeBuilder, Is.SameAs (typeBuilderMock));
      Assert.That (context.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
      Assert.That (context.EmittableOperandProvider, Is.SameAs (emittableOperandProvider));
      Assert.That (context.MethodTrampolineProvider, Is.TypeOf<MethodTrampolineProvider>());
      Assert.That (context.PostDeclarationsActionManager.Actions, Is.Empty);

      Assert.That (builder.MemberEmitter, Is.TypeOf<MemberEmitter>());
      var memberEmitter = (MemberEmitter) builder.MemberEmitter;

      Assert.That (memberEmitter.ExpressionPreparer, Is.TypeOf<ExpandingExpressionPreparer>());
      Assert.That (memberEmitter.ILGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());
      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) memberEmitter.ILGeneratorFactory;

      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory> ());
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (emittableOperandProvider));

      var methodTrampolineProvider = (MethodTrampolineProvider) context.MethodTrampolineProvider;
      Assert.That (methodTrampolineProvider.MemberEmitter, Is.SameAs (memberEmitter));
    }

    [Test]
    public void CreateBuilder_AbstractType ()
    {
      var originalType = typeof (AbstractType);
      var mutableType = MutableTypeObjectMother.CreateForExisting (originalType);

      var typeBuilderFake = MockRepository.GenerateStub<ITypeBuilder> ();
      var attributes = TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit;
      _moduleBuilderMock.Expect (mock => mock.DefineType (originalType.FullName, attributes, originalType)).Return (typeBuilderFake);

      _builderFactory.CreateBuilder (mutableType);

      _moduleBuilderMock.VerifyAllExpectations ();
    }

    abstract class AbstractType
    {
      public abstract void Method ();
    }
  }
}