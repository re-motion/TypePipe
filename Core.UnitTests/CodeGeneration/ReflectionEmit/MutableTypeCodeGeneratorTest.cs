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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableTypeCodeGeneratorTest
  {
    private MutableType _mutableType;
    private Mock<IMutableNestedTypeCodeGeneratorFactory> _nestedTypeCodeGeneratorFactoryMock;
    private Mock<IReflectionEmitCodeGenerator> _codeGeneratorMock;
    private Mock<IEmittableOperandProvider> _emittableOperandProviderMock;
    private Mock<IMemberEmitter> _memberEmitterMock;
    private Mock<IInitializationBuilder> _initializationBuilderMock;

    private MutableTypeCodeGenerator _generator;

    // Context members
    private Mock<ITypeBuilder> _typeBuilderMock;
    private Mock<DebugInfoGenerator> _debugInfoGeneratorMock;

    private FieldInfo _fakeInitializationField;
    private MethodInfo _fakeInitializationMethod;
    private Tuple<FieldInfo, MethodInfo> _fakeInitializationMembers;

    [SetUp]
    public virtual void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.Create();
      _nestedTypeCodeGeneratorFactoryMock = new Mock<IMutableNestedTypeCodeGeneratorFactory> (MockBehavior.Strict);
      _codeGeneratorMock = new Mock<IReflectionEmitCodeGenerator> (MockBehavior.Strict);
      _emittableOperandProviderMock = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);
      _memberEmitterMock = new Mock<IMemberEmitter> (MockBehavior.Strict);
      _initializationBuilderMock = new Mock<IInitializationBuilder> (MockBehavior.Strict);

      _generator = new MutableTypeCodeGenerator (
          _mutableType,
          _nestedTypeCodeGeneratorFactoryMock.Object,
          _codeGeneratorMock.Object,
          _emittableOperandProviderMock.Object,
          _memberEmitterMock.Object,
          _initializationBuilderMock.Object);

      _typeBuilderMock = new Mock<ITypeBuilder> (MockBehavior.Strict);
      _debugInfoGeneratorMock = new Mock<DebugInfoGenerator> (MockBehavior.Strict);

      _fakeInitializationField = ReflectionObjectMother.GetSomeField();
      _fakeInitializationMethod = ReflectionObjectMother.GetSomeMethod();
      _fakeInitializationMembers = Tuple.Create (_fakeInitializationField, _fakeInitializationMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_generator.MutableType, Is.SameAs (_mutableType));
    }

    [Test]
    public virtual void DeclareType ()
    {
      var sequence = new VerifiableSequence();
      _codeGeneratorMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.DefineType (_mutableType.FullName, _mutableType.Attributes, _emittableOperandProviderMock.Object))
          .Returns (_typeBuilderMock.Object);
      _typeBuilderMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.RegisterWith (_emittableOperandProviderMock.Object, _mutableType));
      _codeGeneratorMock
          .InVerifiableSequence (sequence)
          .SetupGet (mock => mock.DebugInfoGenerator)
          .Returns (_debugInfoGeneratorMock.Object);

      _generator.DeclareType();

      _codeGeneratorMock.Verify();
      _typeBuilderMock.Verify();
      sequence.Verify();
      var context = (CodeGenerationContext) PrivateInvoke.GetNonPublicField (_generator, "_context");
      Assert.That (context, Is.Not.Null);
      Assert.That (context.MutableType, Is.SameAs (_mutableType));
      Assert.That (context.TypeBuilder, Is.SameAs (_typeBuilderMock.Object));
      Assert.That (context.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorMock.Object));
      Assert.That (context.EmittableOperandProvider, Is.SameAs (_emittableOperandProviderMock.Object));
    }

    [Test]
    public void CreateNestedTypeGenerators ()
    {
      var nestedType = _mutableType.AddNestedType();
      PopulateContext (_generator, 1);

      var nestedCodeGeneratorStub = new Mock<IMutableTypeCodeGenerator>();
      _nestedTypeCodeGeneratorFactoryMock.Setup (mock => mock.Create (_typeBuilderMock.Object, nestedType)).Returns (nestedCodeGeneratorStub.Object).Verifiable();

      var result = _generator.CreateNestedTypeGenerators().ForceEnumeration();

      _nestedTypeCodeGeneratorFactoryMock.Verify();
      Assert.That (result, Is.EqualTo (new[] { nestedCodeGeneratorStub.Object }));
    }

    [Test]
    public void DefineTypeFacets ()
    {
      var typeInitializer = _mutableType.AddTypeInitializer (ctx => Expression.Empty());

      var instanceInitialization = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableType.AddInitialization (ctx => instanceInitialization);

      var customAttribute = CustomAttributeDeclarationObjectMother.Create();
      _mutableType.AddCustomAttribute (customAttribute);

      var @interface = typeof (IDisposable);
      _mutableType.AddInterface (@interface);

      var field = _mutableType.AddField();
      var constructor = _mutableType.AddConstructor();
      var method = _mutableType.AddMethod();
      var property = _mutableType.AddProperty();
      var event_ = _mutableType.AddEvent();


      var context = PopulateContext (_generator, 2);
      var sequence = new VerifiableSequence();

      _typeBuilderMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.SetParent (_mutableType.BaseType));

      _memberEmitterMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddConstructor (context, typeInitializer));

      _initializationBuilderMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.CreateInitializationMembers (_mutableType))
          .Returns (_fakeInitializationMembers);

      _typeBuilderMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.SetCustomAttribute (customAttribute));
      _typeBuilderMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddInterfaceImplementation (@interface));
      _memberEmitterMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddField (context, field));
      _initializationBuilderMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.WireConstructorWithInitialization (constructor, _fakeInitializationMembers));
      _memberEmitterMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddConstructor (context, constructor));
      _memberEmitterMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddMethod (context, method));
      SetupExpectationsForAccessors (sequence, _memberEmitterMock, _mutableType.AddedMethods.Except (new[] { method }));
      _memberEmitterMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddProperty (context, property));
      _memberEmitterMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AddEvent (context, event_));

      _generator.DefineTypeFacets();

      _typeBuilderMock.Verify();
      _memberEmitterMock.Verify();
      _initializationBuilderMock.Verify();
      sequence.Verify();
    }

    [Test]
    public void DefineTypeFacet_NoParent_NoNestedTypes_NoTypeInitializer_NoInitializations ()
    {
      var mutableType = MutableTypeObjectMother.CreateInterface();
      Assert.That (mutableType.BaseType, Is.Null);
      Assert.That (mutableType.MutableTypeInitializer, Is.Null);
      Assert.That (mutableType.Initialization.Expressions, Is.Empty);

      var generator = new MutableTypeCodeGenerator (
          mutableType,
          _nestedTypeCodeGeneratorFactoryMock.Object,
          _codeGeneratorMock.Object,
          _emittableOperandProviderMock.Object,
          _memberEmitterMock.Object,
          _initializationBuilderMock.Object);
      PopulateContext (generator, 2);

      // No call to SetParent because of null BaseType.
      // No call to AddConstructor because of null TypeInitializer.
      _initializationBuilderMock.Setup (mock => mock.CreateInitializationMembers (mutableType)).Returns ((Tuple<FieldInfo, MethodInfo>) null).Verifiable();

      generator.DefineTypeFacets();

      _initializationBuilderMock.Verify();
    }

    [Test]
    public void CreateType ()
    {
      var context = PopulateContext (_generator, 3);
      var wasCalled = false;
      context.PostDeclarationsActionManager.AddAction (() => wasCalled = true);
      var fakeType = ReflectionObjectMother.GetSomeType();
      _typeBuilderMock.Setup (mock => mock.CreateType()).Returns (fakeType).Verifiable();

      var result = _generator.CreateType();

      _typeBuilderMock.Verify();
      Assert.That (wasCalled, Is.True);
      Assert.That (result, Is.SameAs (fakeType));
    }

    [Test]
    public void ThrowsForInvalidOperation ()
    {
      var codeGeneratorStub = new Mock<IReflectionEmitCodeGenerator>();
      var emittableOperandProviderStub = new Mock<IEmittableOperandProvider>();
      var memberEmitterStub = new Mock<IMemberEmitter>();
      var initializationBuilderStub = new Mock<IInitializationBuilder>();
      _typeBuilderMock.Setup (stub => stub.RegisterWith (emittableOperandProviderStub.Object, _mutableType));
      _typeBuilderMock.Setup (stub => stub.SetParent (_mutableType.BaseType));
      _typeBuilderMock.Setup (stub => stub.CreateType()).Returns ((Type) null);
      codeGeneratorStub
          .Setup (stub => stub.DefineType (It.IsAny<string>(), It.IsAny<TypeAttributes>(), It.IsAny<IEmittableOperandProvider>()))
          .Returns (_typeBuilderMock.Object);
      codeGeneratorStub.SetupGet (stub => stub.DebugInfoGenerator).Returns (_debugInfoGeneratorMock.Object);

      var generator = new MutableTypeCodeGenerator (
          _mutableType,
          _nestedTypeCodeGeneratorFactoryMock.Object,
          codeGeneratorStub.Object,
          emittableOperandProviderStub.Object,
          memberEmitterStub.Object,
          initializationBuilderStub.Object);

      CheckThrowsForInvalidOperation (generator.DefineTypeFacets);
      CheckThrowsForInvalidOperation (() => generator.CreateNestedTypeGenerators().ForceEnumeration());
      CheckThrowsForInvalidOperation (() => generator.CreateType());
      Assert.That (() => generator.DeclareType(), Throws.Nothing);

      CheckThrowsForInvalidOperation (generator.DeclareType);
      CheckThrowsForInvalidOperation (generator.DefineTypeFacets);
      CheckThrowsForInvalidOperation (() => generator.CreateType());
      Assert.That (() => generator.CreateNestedTypeGenerators().ForceEnumeration(), Throws.Nothing);

      CheckThrowsForInvalidOperation (generator.DeclareType);
      CheckThrowsForInvalidOperation (() => generator.CreateNestedTypeGenerators().ForceEnumeration());
      CheckThrowsForInvalidOperation (() => generator.CreateType());
      Assert.That (() => generator.DefineTypeFacets(), Throws.Nothing);

      CheckThrowsForInvalidOperation (generator.DeclareType);
      CheckThrowsForInvalidOperation (() => generator.CreateNestedTypeGenerators().ForceEnumeration());
      CheckThrowsForInvalidOperation (generator.DefineTypeFacets);
      Assert.That (() => generator.CreateType(), Throws.Nothing);
    }

    private CodeGenerationContext PopulateContext (MutableTypeCodeGenerator generator, int currentState)
    {
      var context = new CodeGenerationContext (
          _mutableType,
          _typeBuilderMock.Object,
          _debugInfoGeneratorMock.Object,
          _emittableOperandProviderMock.Object);
      PrivateInvoke.SetNonPublicField (generator, "_context", context);
      PrivateInvoke.SetNonPublicField (generator, "_state", currentState);

      return context;
    }

    private void SetupExpectationsForAccessors (VerifiableSequence sequence, Mock<IMemberEmitter> memberEmitterMock, IEnumerable<MutableMethodInfo> methods)
    {
      foreach (var method in methods)
      {
        var m = method;
        memberEmitterMock
            .InVerifiableSequence (sequence)
            .Setup (mock => mock.AddMethod (It.IsAny<CodeGenerationContext>(), m))
            .Verifiable();
      }
    }

    private void CheckThrowsForInvalidOperation (Action action)
    {
      Assert.That (
          () => action(),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Methods DeclareType, CreateNestedTypeGenerators, DefineTypeFacets and CreateType must be called exactly once and in the correct order."));
    }
  }
}