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
using Remotion.TypePipe.CodeGeneration;
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
    private MockRepository _mockRepository;

    private ICodeGenerationContextFactory _codeGenerationContextFactoryMock;
    private IMemberEmitter _memberEmitterMock;
    private IInitializationBuilder _initializationBuilderMock;
    private IProxySerializationEnabler _proxySerializationEnablerMock;

    private SubclassProxyBuilder _builder;

    // Context members
    private ITypeBuilder _typeBuilderMock;
    private DebugInfoGenerator _debugInfoGeneratorMock;
    private IEmittableOperandProvider _emittableOperandProviderMock;
    private IMethodTrampolineProvider _methodTrampolineProviderMock;

    private ProxyType _proxyType;
    private CodeGenerationContext _fakeContext;
    private FieldInfo _fakeInitializationField;
    private MethodInfo _fakeInitializationMethod;
    private Tuple<FieldInfo, MethodInfo> _fakeInitializationMembers;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();

      _codeGenerationContextFactoryMock = _mockRepository.StrictMock<ICodeGenerationContextFactory>();
      _memberEmitterMock = _mockRepository.StrictMock<IMemberEmitter>();
      _initializationBuilderMock = _mockRepository.StrictMock<IInitializationBuilder>();
      _proxySerializationEnablerMock = _mockRepository.StrictMock<IProxySerializationEnabler>();

      _typeBuilderMock = _mockRepository.StrictMock<ITypeBuilder>();
      _debugInfoGeneratorMock = _mockRepository.StrictMock<DebugInfoGenerator>();
      _emittableOperandProviderMock = _mockRepository.StrictMock<IEmittableOperandProvider>();
      _methodTrampolineProviderMock = _mockRepository.StrictMock<IMethodTrampolineProvider>();

      _builder = new SubclassProxyBuilder (
          _codeGenerationContextFactoryMock, _memberEmitterMock, _initializationBuilderMock, _proxySerializationEnablerMock);

      _proxyType = ProxyTypeObjectMother.Create();
      _fakeContext = new CodeGenerationContext (
          _proxyType, _typeBuilderMock, _debugInfoGeneratorMock, _emittableOperandProviderMock, _methodTrampolineProviderMock);
      _fakeInitializationField = ReflectionObjectMother.GetSomeField();
      _fakeInitializationMethod = ReflectionObjectMother.GetSomeMethod();
      _fakeInitializationMembers = Tuple.Create (_fakeInitializationField, _fakeInitializationMethod);
    }

    [Test]
    public void CodeGenerator ()
    {
      var fakeCodeGenerator = MockRepository.GenerateStub<ICodeGenerator>();
      _codeGenerationContextFactoryMock.Expect (mock => mock.CodeGenerator).Return (fakeCodeGenerator);
      _mockRepository.ReplayAll();

      Assert.That (_builder.CodeGenerator, Is.SameAs (fakeCodeGenerator));
    }

    [Test]
    public void Build ()
    {
      var typeInitializer = _proxyType.AddTypeInitializer (ctx => Expression.Empty());

      var instanceInitialization = ExpressionTreeObjectMother.GetSomeExpression ();
      _proxyType.AddInitialization (ctx => instanceInitialization);

      var customAttribute = CustomAttributeDeclarationObjectMother.Create ();
      _proxyType.AddCustomAttribute (customAttribute);

      var @interface = typeof (IDisposable);
      _proxyType.AddInterface (@interface);

      var field = _proxyType.AddField ("_field", typeof (int));
      var constructor = _proxyType.AddedConstructors.Single ();
      var method = _proxyType.AddMethod (
          "Method", (MethodAttributes) 7, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());

      var buildActionCalled = false;
      _fakeContext.PostDeclarationsActionManager.AddAction (() => buildActionCalled = true);
      var fakeType = ReflectionObjectMother.GetSomeType();

      using (_mockRepository.Ordered())
      {
        _codeGenerationContextFactoryMock.Expect (mock => mock.CreateContext (_proxyType)).Return (_fakeContext);

        _memberEmitterMock.Expect (mock => mock.AddConstructor (_fakeContext, typeInitializer));

        _initializationBuilderMock.Expect (mock => mock.CreateInitializationMembers (_proxyType)).Return (_fakeInitializationMembers);

        _proxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (_proxyType, _fakeInitializationMethod));

        _typeBuilderMock.Expect (mock => mock.SetCustomAttribute (customAttribute));

        _typeBuilderMock.Expect (mock => mock.AddInterfaceImplementation (@interface));

        _memberEmitterMock.Expect (mock => mock.AddField (_fakeContext, field));
        _initializationBuilderMock.Expect (
            mock => mock.WireConstructorWithInitialization (constructor, _fakeInitializationMembers, _proxySerializationEnablerMock));
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_fakeContext, constructor));
        _memberEmitterMock.Expect (mock => mock.AddMethod (_fakeContext, method, method.Attributes));

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
    public void Build_NoTypeInitializer_NoInitializations ()
    {
      var defaultCtor = _proxyType.AddedConstructors.Single();

      _codeGenerationContextFactoryMock.Expect (mock => mock.CreateContext (_proxyType)).Return (_fakeContext);
      // No call to AddConstructor for because of null type initializer.
      _initializationBuilderMock.Expect (mock => mock.CreateInitializationMembers (_proxyType)).Return (null);
      _proxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (_proxyType, null));
      // Copied default constructor.
      _initializationBuilderMock.Expect (mock => mock.WireConstructorWithInitialization (defaultCtor, null, _proxySerializationEnablerMock));
      _memberEmitterMock.Expect (mock => mock.AddConstructor (_fakeContext, defaultCtor));
      _typeBuilderMock.Expect (mock => mock.CreateType());
      _mockRepository.ReplayAll();

      _builder.Build (_proxyType);

      _mockRepository.VerifyAll();
    }
  }
}