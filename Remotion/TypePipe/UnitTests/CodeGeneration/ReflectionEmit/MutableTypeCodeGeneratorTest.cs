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
using System.Collections.Generic;
using System.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [Ignore("TODO 5550")]
  [TestFixture]
  public class MutableTypeCodeGeneratorTest
  {
    protected MockRepository MockRepository;

    protected MutableType MutableType;
    protected IMutableNestedTypeCodeGeneratorFactory NestedTypeCodeGeneratorFactoryMock;
    protected IReflectionEmitCodeGenerator CodeGeneratorMock;
    protected IEmittableOperandProvider EmittableOperandProviderMock;
    protected IMemberEmitter MemberEmitterMock;
    protected IInitializationBuilder InitializationBuilderMock;
    protected IProxySerializationEnabler ProxySerializationEnablerMock;

    protected MutableTypeCodeGenerator Generator;

    // Context members
    protected ITypeBuilder TypeBuilderMock;
    protected DebugInfoGenerator DebugInfoGeneratorMock;
    protected IMutableTypeCodeGenerator NestedTypeCodeGeneratorMock;

    private FieldInfo _fakeInitializationField;
    private MethodInfo _fakeInitializationMethod;
    private Tuple<FieldInfo, MethodInfo> _fakeInitializationMembers;

    [SetUp]
    public virtual void SetUp ()
    {
      MockRepository = new MockRepository();

      MutableType = MutableTypeObjectMother.Create();
      NestedTypeCodeGeneratorFactoryMock = MockRepository.StrictMock<IMutableNestedTypeCodeGeneratorFactory>();
      CodeGeneratorMock = MockRepository.StrictMock<IReflectionEmitCodeGenerator>();
      EmittableOperandProviderMock = MockRepository.StrictMock<IEmittableOperandProvider> ();
      MemberEmitterMock = MockRepository.StrictMock<IMemberEmitter>();
      InitializationBuilderMock = MockRepository.StrictMock<IInitializationBuilder>();
      ProxySerializationEnablerMock = MockRepository.StrictMock<IProxySerializationEnabler>();

      Generator = new MutableTypeCodeGenerator (
          MutableType,
          NestedTypeCodeGeneratorFactoryMock,
          CodeGeneratorMock,
          EmittableOperandProviderMock,
          MemberEmitterMock,
          InitializationBuilderMock,
          ProxySerializationEnablerMock);

      TypeBuilderMock = MockRepository.StrictMock<ITypeBuilder>();
      DebugInfoGeneratorMock = MockRepository.StrictMock<DebugInfoGenerator>();
      NestedTypeCodeGeneratorMock = MockRepository.StrictMock<IMutableTypeCodeGenerator>();

      _fakeInitializationField = ReflectionObjectMother.GetSomeField();
      _fakeInitializationMethod = ReflectionObjectMother.GetSomeMethod();
      _fakeInitializationMembers = Tuple.Create (_fakeInitializationField, _fakeInitializationMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (Generator.MutableType, Is.SameAs (MutableType));
    }

    [Test]
    public virtual void DeclareType ()
    {
      var nestedType = MutableType.AddNestedType();

      using (MockRepository.Ordered())
      {
        CodeGeneratorMock
            .Expect (mock => mock.DefineType (MutableType.FullName, MutableType.Attributes, EmittableOperandProviderMock))
            .Return (TypeBuilderMock);
        TypeBuilderMock.Expect (mock => mock.RegisterWith (EmittableOperandProviderMock, MutableType));
        CodeGeneratorMock.Expect (mock => mock.DebugInfoGenerator).Return (DebugInfoGeneratorMock);

        NestedTypeCodeGeneratorFactoryMock
            .Expect (mock => mock.Create (TypeBuilderMock, nestedType))
            .Return (NestedTypeCodeGeneratorMock);
        NestedTypeCodeGeneratorMock.Expect (mock => mock.DeclareType());
      }
      MockRepository.ReplayAll();

      Generator.DeclareType();

      MockRepository.VerifyAll();
      var context = (CodeGenerationContext) PrivateInvoke.GetNonPublicField (Generator, "_context");
      Assert.That (context, Is.Not.Null);
      Assert.That (context.MutableType, Is.SameAs (MutableType));
      Assert.That (context.TypeBuilder, Is.SameAs (TypeBuilderMock));
      Assert.That (context.DebugInfoGenerator, Is.SameAs (DebugInfoGeneratorMock));
      Assert.That (context.EmittableOperandProvider, Is.SameAs (EmittableOperandProviderMock));
    }

    [Ignore("TODO 5550")]
    [Test]
    public void DefineTypeFacets ()
    {
      var typeInitializer = MutableType.AddTypeInitializer (ctx => Expression.Empty());

      var instanceInitialization = ExpressionTreeObjectMother.GetSomeExpression();
      MutableType.AddInitialization (ctx => instanceInitialization);

      var customAttribute = CustomAttributeDeclarationObjectMother.Create();
      MutableType.AddCustomAttribute (customAttribute);

      var @interface = typeof (IDisposable);
      MutableType.AddInterface (@interface);

      var field = MutableType.AddField();
      var constructor = MutableType.AddConstructor();
      var method = MutableType.AddMethod();
      var property = MutableType.AddProperty();
      var event_ = MutableType.AddEvent();

      using (MockRepository.Ordered())
      {
        var context = PopulateContext (Generator, 1, new[] { NestedTypeCodeGeneratorMock });

        TypeBuilderMock.Expect (mock => mock.SetParent (MutableType.BaseType));
        NestedTypeCodeGeneratorMock.Expect (mock => mock.DefineTypeFacets());

        MemberEmitterMock.Expect (mock => mock.AddConstructor (context, typeInitializer));

        InitializationBuilderMock.Expect (mock => mock.CreateInitializationMembers (MutableType)).Return (_fakeInitializationMembers);
        ProxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (MutableType, _fakeInitializationMethod));

        TypeBuilderMock.Expect (mock => mock.SetCustomAttribute (customAttribute));
        TypeBuilderMock.Expect (mock => mock.AddInterfaceImplementation (@interface));
        MemberEmitterMock.Expect (mock => mock.AddField (context, field));
        InitializationBuilderMock.Expect (
            mock => mock.WireConstructorWithInitialization (constructor, _fakeInitializationMembers, ProxySerializationEnablerMock));
        MemberEmitterMock.Expect (mock => mock.AddConstructor (context, constructor));
        MemberEmitterMock.Expect (mock => mock.AddMethod (context, method));
        SetupExpectationsForAccessors (MemberEmitterMock, MutableType.AddedMethods.Except (new[] { method }));
        MemberEmitterMock.Expect (mock => mock.AddProperty (context, property));
        MemberEmitterMock.Expect (mock => mock.AddEvent (context, event_));
      }
      MockRepository.ReplayAll();

      Generator.DefineTypeFacets();

      MockRepository.VerifyAll();
    }

    [Ignore("TODO 5550")]
    [Test]
    public void DefineTypeFacet_NoParent_NoNestedTypes_NoTypeInitializer_NoInitializations ()
    {
      var mutableType = MutableTypeObjectMother.CreateInterface();
      Assert.That (mutableType.BaseType, Is.Null);
      Assert.That (mutableType.MutableTypeInitializer, Is.Null);
      Assert.That (mutableType.Initialization.Expressions, Is.Empty);

      var generator = new MutableTypeCodeGenerator (
          mutableType,
          NestedTypeCodeGeneratorFactoryMock,
          CodeGeneratorMock,
          EmittableOperandProviderMock,
          MemberEmitterMock,
          InitializationBuilderMock,
          ProxySerializationEnablerMock);
      PopulateContext (generator, 1, new IMutableTypeCodeGenerator[0]);

      // No call to SetParent because of null BaseType.
      // No call to AddConstructor because of null TypeInitializer.
      InitializationBuilderMock.Expect (mock => mock.CreateInitializationMembers (mutableType)).Return (null);
      ProxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (mutableType, null));
      MockRepository.ReplayAll();

      generator.DefineTypeFacets();

      MockRepository.VerifyAll();
    }

    [Ignore("TODO 5550")]
    [Test]
    public void CreateType ()
    {
      var context = PopulateContext (Generator, 2, new [] { NestedTypeCodeGeneratorMock });
      bool wasCalled = false;
      NestedTypeCodeGeneratorMock.Expect (mock => mock.CreateType());
      context.PostDeclarationsActionManager.AddAction (() => wasCalled = true);
      var fakeType = ReflectionObjectMother.GetSomeType();
      TypeBuilderMock.Expect (mock => mock.CreateType()).Return (fakeType);
      MockRepository.ReplayAll();

      var result = Generator.CreateType();

      MockRepository.VerifyAll();
      Assert.That (wasCalled, Is.True);
      Assert.That (result, Is.SameAs (fakeType));
    }

    [Test]
    public void ThrowsForInvalidOperation ()
    {
      var codeGeneratorStub = MockRepository.GenerateStub<IReflectionEmitCodeGenerator>();
      var emittableOperandProviderStub = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var memberEmitterStub = MockRepository.GenerateStub<IMemberEmitter>();
      var initializationBuilderStub = MockRepository.GenerateStub<IInitializationBuilder>();
      var proxySerializationEnablerStub = MockRepository.GenerateStub<IProxySerializationEnabler>();
      codeGeneratorStub.Stub (stub => stub.DefineType (null, 0, null)).IgnoreArguments().Return (TypeBuilderMock);
      codeGeneratorStub.Stub (stub => stub.DebugInfoGenerator).Return (DebugInfoGeneratorMock);

      var generator = new MutableTypeCodeGenerator (
          MutableType,
          NestedTypeCodeGeneratorFactoryMock,
          codeGeneratorStub,
          emittableOperandProviderStub,
          memberEmitterStub,
          initializationBuilderStub,
          proxySerializationEnablerStub);

      CheckThrowsForInvalidOperation (generator.DefineTypeFacets);
      CheckThrowsForInvalidOperation (() => generator.CreateType());
      Assert.That (() => generator.DeclareType(), Throws.Nothing);

      CheckThrowsForInvalidOperation (generator.DeclareType);
      CheckThrowsForInvalidOperation (() => generator.CreateType());
      Assert.That (() => generator.DefineTypeFacets(), Throws.Nothing);

      CheckThrowsForInvalidOperation (generator.DeclareType);
      CheckThrowsForInvalidOperation (generator.DefineTypeFacets);
      Assert.That (() => generator.CreateType(), Throws.Nothing);
    }

    private CodeGenerationContext PopulateContext (MutableTypeCodeGenerator generator, int currentState, IMutableTypeCodeGenerator[] nestedTypeCodeGenerators = null)
    {
      nestedTypeCodeGenerators = nestedTypeCodeGenerators ?? new IMutableTypeCodeGenerator[0];

      var context = new CodeGenerationContext (MutableType, TypeBuilderMock, DebugInfoGeneratorMock, EmittableOperandProviderMock);
      PrivateInvoke.SetNonPublicField (generator, "_context", context);
      PrivateInvoke.SetNonPublicField (generator, "_state", currentState);
      PrivateInvoke.SetNonPublicField (generator, "_nestedTypeCodeGenerators", nestedTypeCodeGenerators.ToList());

      return context;
    }

    private void SetupExpectationsForAccessors (IMemberEmitter memberEmitterMock, IEnumerable<MutableMethodInfo> methods)
    {
      foreach (var method in methods)
      {
        var m = method;
        memberEmitterMock.Expect (mock => mock.AddMethod (Arg<CodeGenerationContext>.Is.Anything, Arg.Is (m)));
      }
    }

    private void CheckThrowsForInvalidOperation (Action action)
    {
      Assert.That (
          () => action(),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Methods DeclareType, DefineTypeFacets and CreateType must be called exactly once and in the correct order."));
    }
  }
}