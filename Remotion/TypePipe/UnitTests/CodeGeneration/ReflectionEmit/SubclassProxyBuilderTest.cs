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
    private MutableType _mutableType;
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
      _mutableType = MutableTypeObjectMother.Create (
          typeof (DomainType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      _typeBuilderMock = _mockRepository.StrictMock<ITypeBuilder>();
      _debugInfoGeneratorStub = _mockRepository.Stub<DebugInfoGenerator>();
      _emittableOperandProviderMock = _mockRepository.StrictMock<IEmittableOperandProvider>();
      _methodTrampolineProviderMock = _mockRepository.StrictMock<IMethodTrampolineProvider>();

      _builder = CreateSubclassProxyBuilder (_mutableType, _debugInfoGeneratorStub);
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

      Assert.That (_context.MutableType, Is.SameAs (_mutableType));
      Assert.That (_context.TypeBuilder, Is.SameAs (_typeBuilderMock));
      Assert.That (_context.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
      Assert.That (_context.EmittableOperandProvider, Is.SameAs (_emittableOperandProviderMock));
      Assert.That (_context.MethodTrampolineProvider, Is.SameAs (_methodTrampolineProviderMock));
      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var builder = CreateSubclassProxyBuilder (_mutableType, debugInfoGenerator: null);
      Assert.That (builder.MemberEmitterContext.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void Build ()
    {
      var typeInitialization = ExpressionTreeObjectMother.GetSomeExpression();
      var instanceInitialization = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableType.AddTypeInitialization (ctx => typeInitialization);
      _mutableType.AddInstanceInitialization (ctx => instanceInitialization);

      var customAttributeDeclaration = CustomAttributeDeclarationObjectMother.Create();
      _mutableType.AddCustomAttribute (customAttributeDeclaration);

      var addedInterface = typeof (IDisposable);
      _mutableType.AddInterface (addedInterface);

      var addedMembers = GetAddedMembers (_mutableType);
      var modifiedMembers = GetModifiedMembers (_mutableType);
      var unmodifiedMembers = GetUnmodifiedMembers (_mutableType);

      //var internalConstructor =
      //    _mutableType.GetMutableConstructor (NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (true)));
      //Assert.That (internalConstructor.IsAssembly, Is.True);

      var buildActionCalled = false;
      _builder.MemberEmitterContext.PostDeclarationsActionManager.AddAction (() => buildActionCalled = true);
      var fakeType = ReflectionObjectMother.GetSomeType();

      using (_mockRepository.Ordered())
      {
        var fakeTypeInitializer = MutableConstructorInfoObjectMother.Create();

        _initializationBuilderMock.Expect (mock => mock.CreateTypeInitializer (_mutableType)).Return (fakeTypeInitializer);
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, fakeTypeInitializer));

        _initializationBuilderMock.Expect (mock => mock.CreateInstanceInitializationMembers (_mutableType)).Return (_fakeInitializationMembers);

        _proxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (_mutableType, _fakeInitializationMethod));

        _typeBuilderMock.Expect (mock => mock.SetCustomAttribute (customAttributeDeclaration));

        _typeBuilderMock.Expect (mock => mock.AddInterfaceImplementation (addedInterface));

        _memberEmitterMock.Expect (mock => mock.AddField (_context, addedMembers.Item1));
        _initializationBuilderMock.Expect (
            mock => mock.WireConstructorWithInitialization (addedMembers.Item2, _fakeInitializationMembers, _proxySerializationEnablerMock));
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, addedMembers.Item2));
        _memberEmitterMock.Expect (mock => mock.AddMethod (_context, addedMembers.Item3, addedMembers.Item3.Attributes));

        _initializationBuilderMock.Expect (
            mock => mock.WireConstructorWithInitialization (modifiedMembers.Item2, _fakeInitializationMembers, _proxySerializationEnablerMock));
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, modifiedMembers.Item2));
        var expectedAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig;
        _memberEmitterMock.Expect (mock => mock.AddMethod (_context, modifiedMembers.Item3, expectedAttributes));

        //_emittableOperandProviderMock.Expect (mock => mock.AddMapping (unmodifiedMembers.Item1, unmodifiedMembers.Item1.UnderlyingSystemFieldInfo));
        _initializationBuilderMock.Expect (
            mock => mock.WireConstructorWithInitialization (unmodifiedMembers.Item2, _fakeInitializationMembers, _proxySerializationEnablerMock));
        _memberEmitterMock.Expect (mock => mock.AddConstructor (_context, unmodifiedMembers.Item2));
        //_emittableOperandProviderMock.Expect (mock => mock.AddMapping (unmodifiedMembers.Item3, unmodifiedMembers.Item3.UnderlyingSystemMethodInfo))
        //                             .WhenCalled (x => Assert.That (buildActionCalled, Is.False));

        // PostDeclarationsActionManager.ExecuteAllActions() cannot setup expectations.

        _typeBuilderMock
            .Expect (mock => mock.CreateType ())
            .Return (fakeType)
            .WhenCalled (mi => Assert.That (buildActionCalled, Is.True));
      }
      _mockRepository.ReplayAll();

      var result = _builder.Build (_mutableType);

      _mockRepository.VerifyAll();
      Assert.That (result, Is.SameAs (fakeType));

      //_memberEmitterMock.AssertWasNotCalled (mock => mock.AddConstructor (_context, internalConstructor));
    }

    [Test]
    public void Build_EmptyTypeInitializations ()
    {
      var mutableType = MutableTypeObjectMother.Create (
          typeof (EmptyType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      var defaultCtor = mutableType.AddedConstructors.Single();
      var builder = CreateSubclassProxyBuilder (mutableType);

      _initializationBuilderMock.Expect (mock => mock.CreateTypeInitializer (mutableType)).Return (null);
      // No call to AddConstructor for because of null type initializer.
      _initializationBuilderMock.Expect (mock => mock.CreateInstanceInitializationMembers (mutableType)).Return (null);
      _proxySerializationEnablerMock.Expect (mock => mock.MakeSerializable (mutableType, null));
      // Copied default constructor.
      _initializationBuilderMock.Expect (mock => mock.WireConstructorWithInitialization (defaultCtor, null, _proxySerializationEnablerMock));
      _memberEmitterMock.Expect (mock => mock.AddConstructor (builder.MemberEmitterContext, defaultCtor));
      _typeBuilderMock.Expect (mock => mock.CreateType());
      _mockRepository.ReplayAll();

      builder.Build (mutableType);

      _mockRepository.VerifyAll();
    }

    private SubclassProxyBuilder CreateSubclassProxyBuilder (MutableType mutableType, DebugInfoGenerator debugInfoGenerator = null)
    {
      return new SubclassProxyBuilder (
          _memberEmitterMock,
          _initializationBuilderMock,
          _proxySerializationEnablerMock,
          mutableType,
          _typeBuilderMock,
          debugInfoGenerator,
          _emittableOperandProviderMock,
          _methodTrampolineProviderMock);
    }

    private Tuple<MutableFieldInfo, MutableConstructorInfo, MutableMethodInfo> GetAddedMembers (MutableType mutableType)
    {
      var field = mutableType.AddField ("_field", typeof (int));
      var constructor = mutableType.AddConstructor (MethodAttributes.Public, ParameterDeclaration.EmptyParameters, ctx => Expression.Empty ());
      var method = mutableType.AddMethod (
          "Method", MethodAttributes.Family, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty ());

      return Tuple.Create (field, constructor, method);
    }

    private Tuple<MutableFieldInfo, MutableConstructorInfo, MutableMethodInfo> GetModifiedMembers (MutableType mutableType)
    {
      return null;
    }

    private Tuple<MutableFieldInfo, MutableConstructorInfo, MutableMethodInfo> GetUnmodifiedMembers (MutableType mutableType)
    {
      return null;
    }

    // ReSharper disable UnusedParameter.Local
    public class DomainType
    {
      internal DomainType (bool notVisibleFormSubclass) { }

      public string UnmodifiedField;

      public DomainType (int modified) { }
      public DomainType (string unmodified) { }

      public virtual void ModifiedMethod () { }
      public void UnmodifiedMethod () { }
    }

    public class EmptyType { }
  }
}