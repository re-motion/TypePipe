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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class SubclassProxyBuilderTest
  {
    private IMemberEmitter _memberEmitterMock;
    private IInitializationBuilder _initializationBuilderMock;
    private IProxySerializationEnabler _proxySerializationEnablerMock;

    private MockRepository _mockRepository;
    private ProxyType _proxyType;
    private ITypeBuilder _typeBuilderMock;
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private IEmittableOperandProvider _emittableOperandProviderMock;
    private IMethodTrampolineProvider _methodTrampolineProviderMock;

    private SubclassProxyBuilder _builder;

    private MemberEmitterContext _context;

    private FieldInfo _fakeInitializationField;
    private MethodInfo _fakeInitializationMethod;
    private Tuple<FieldInfo, MethodInfo> _fakeInitializationMembers;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterMock = MockRepository.GenerateStrictMock<IMemberEmitter> ();
      _initializationBuilderMock = MockRepository.GenerateStrictMock<IInitializationBuilder>();
      _proxySerializationEnablerMock = MockRepository.GenerateStrictMock<IProxySerializationEnabler>();

      _mockRepository = new MockRepository();
      _proxyType = ProxyTypeObjectMother.Create();
      _typeBuilderMock = _mockRepository.StrictMock<ITypeBuilder>();
      _debugInfoGeneratorStub = _mockRepository.Stub<DebugInfoGenerator>();
      _emittableOperandProviderMock = _mockRepository.StrictMock<IEmittableOperandProvider>();
      _methodTrampolineProviderMock = _mockRepository.StrictMock<IMethodTrampolineProvider>();

      _builder = new SubclassProxyBuilder (
          _memberEmitterMock,
          _initializationBuilderMock,
          _proxySerializationEnablerMock,
          _proxyType,
          _typeBuilderMock,
          _debugInfoGeneratorStub,
          _emittableOperandProviderMock,
          _methodTrampolineProviderMock);
      _context = _builder.MemberEmitterContext;

      _fakeInitializationField = ReflectionObjectMother.GetSomeField();
      _fakeInitializationMethod = ReflectionObjectMother.GetSomeMethod();
      _fakeInitializationMembers = Tuple.Create (_fakeInitializationField, _fakeInitializationMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_builder.MemberEmitter, Is.SameAs (_memberEmitterMock));
      Assert.That (_builder.InitializationBuilder, Is.SameAs (_initializationBuilderMock));
      Assert.That (_builder.ProxySerializationEnabler, Is.SameAs (_proxySerializationEnablerMock));

      Assert.That (_context.ProxyType, Is.SameAs (_proxyType));
      Assert.That (_context.TypeBuilder, Is.SameAs (_typeBuilderMock));
      Assert.That (_context.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
      Assert.That (_context.EmittableOperandProvider, Is.SameAs (_emittableOperandProviderMock));
      Assert.That (_context.MethodTrampolineProvider, Is.SameAs (_methodTrampolineProviderMock));
      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var builder = new SubclassProxyBuilder (
          _memberEmitterMock,
          _initializationBuilderMock,
          _proxySerializationEnablerMock,
          _proxyType,
          _typeBuilderMock,
          null,
          _emittableOperandProviderMock,
          _methodTrampolineProviderMock);
      Assert.That (builder.MemberEmitterContext.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void Build ()
    {
      var typeInitializer = _proxyType.AddTypeInitializer (ctx => Expression.Empty());
      
      var instanceInitialization = ExpressionTreeObjectMother.GetSomeExpression();
      _proxyType.AddInitialization (ctx => instanceInitialization);

      var customAttribute = CustomAttributeDeclarationObjectMother.Create();
      _proxyType.AddCustomAttribute (customAttribute);

      var @interface = typeof (IDisposable);
      _proxyType.AddInterface (@interface);

      var field = _proxyType.AddField ("_field", typeof (int));
      var constructor = _proxyType.AddedConstructors.Single();
      var method = _proxyType.AddMethod (
          "Method", (MethodAttributes) 7, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());

      var buildActionCalled = false;
      _builder.MemberEmitterContext.PostDeclarationsActionManager.AddAction (() => buildActionCalled = true);
      var fakeType = ReflectionObjectMother.GetSomeType();

      using (_mockRepository.Ordered())
      {
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, typeInitializer));

        _initializationBuilderMock.Expect (mock => mock.CreateInstanceInitializationMembers (_proxyType)).Return (_fakeInitializationMembers);

        _proxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (_proxyType, _fakeInitializationMethod));

        _typeBuilderMock.Expect (mock => mock.SetCustomAttribute (customAttribute));

        _typeBuilderMock.Expect (mock => mock.AddInterfaceImplementation (@interface));

        _memberEmitterMock.Expect (mock => mock.AddField (_context, field));
        _initializationBuilderMock.Expect (
            mock => mock.WireConstructorWithInitialization (constructor, _fakeInitializationMembers, _proxySerializationEnablerMock));
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, constructor));
        _memberEmitterMock.Expect (mock => mock.AddMethod (_context, method, method.Attributes));

        // PostDeclarationsActionManager.ExecuteAllActions() cannot setup expectations.

        _typeBuilderMock
            .Expect (mock => mock.CreateType())
            .Return (fakeType)
            .WhenCalled (mi => Assert.That (buildActionCalled, Is.True));
      }
      _mockRepository.ReplayAll();

      var result = _builder.Build (_proxyType);

      _mockRepository.VerifyAll();
      Assert.That (result, Is.SameAs (fakeType));
    }

    [Test]
    public void Build_NoInitializations ()
    {
      var defaultCtor = _proxyType.AddedConstructors.Single();

      // No call to AddConstructor for because of null type initializer.
      _initializationBuilderMock.Expect (mock => mock.CreateInstanceInitializationMembers (_proxyType)).Return (null);
      _proxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (_proxyType, null));
      // Copied default constructor.
      _initializationBuilderMock.Expect (mock => mock.WireConstructorWithInitialization (defaultCtor, null, _proxySerializationEnablerMock));
      _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, defaultCtor));
      _typeBuilderMock.Expect (mock => mock.CreateType());
      _mockRepository.ReplayAll();

      _builder.Build (_proxyType);

      _mockRepository.VerifyAll();
    }
  }
}