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
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.RhinoMocks.UnitTesting.Threading;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class LockingCodeGeneratorTest
  {
    private IGeneratedCodeFlusher _generatedCodeFlusherMock;
    private ITypeCache _typeCacheMock;
    private IConstructorFinder _constructorFinderMock;
    private IDelegateFactory _delegateFactoryMock;

    private LockingCodeGenerator _generator;

    private object _codeGeneratorLock;
    private ITypeAssembler _typeAssemblerMock;
    private IMutableTypeBatchCodeGenerator _mutableTypeBatchCodeGeneratorMock;

    [SetUp]
    public void SetUp ()
    {
      _generatedCodeFlusherMock = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _constructorFinderMock = MockRepository.GenerateStrictMock<IConstructorFinder>();
      _delegateFactoryMock = MockRepository.GenerateStrictMock<IDelegateFactory>();

      _generator = new LockingCodeGenerator (_generatedCodeFlusherMock, _constructorFinderMock, _delegateFactoryMock);

      _codeGeneratorLock = PrivateInvoke.GetNonPublicField (_generator, "_codeGenerationLock");
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _mutableTypeBatchCodeGeneratorMock = MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>();
    }

    [Test]
    public void DelegatingMembers_GuardedByLock ()
    {
      _generatedCodeFlusherMock.Expect (mock => mock.AssemblyDirectory).Return ("get dir").WhenCalled (_ => CheckLock (true));
      Assert.That (_generator.AssemblyDirectory, Is.EqualTo ("get dir"));
      _generatedCodeFlusherMock.Expect (mock => mock.AssemblyNamePattern).Return ("get name pattern").WhenCalled (_ => CheckLock (true));
      Assert.That (_generator.AssemblyNamePattern, Is.EqualTo ("get name pattern"));

      _generatedCodeFlusherMock.Expect (mock => mock.SetAssemblyDirectory ("set dir")).WhenCalled (_ => CheckLock (true));
      _generator.SetAssemblyDirectory ("set dir");
      _generatedCodeFlusherMock.Expect (mock => mock.SetAssemblyNamePattern ("set name pattern")).WhenCalled (_ => CheckLock (true));
      _generator.SetAssemblyNamePattern ("set name pattern");

      _generatedCodeFlusherMock.VerifyAllExpectations();
      _typeCacheMock.VerifyAllExpectations();
    }

    [Test]
    public void GetOrGenerateType_CacheHit ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var key = new object[0];
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var types = new ConcurrentDictionary<object[], Type> { { key, assembledType } };
      var participantState = new Dictionary<string, object>();

      var result = _generator.GetOrGenerateType (types, key, _typeAssemblerMock, requestedType, participantState, _mutableTypeBatchCodeGeneratorMock);

      Assert.That (result, Is.SameAs (assembledType));
    }

    [Test]
    public void GetOrGenerateType_CacheMiss ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeKey = new object[0];
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var types = new ConcurrentDictionary<object[], Type>();
      var participantState = new Dictionary<string, object>();

      _typeAssemblerMock
          .Expect (mock => mock.AssembleType (requestedType, participantState, _mutableTypeBatchCodeGeneratorMock))
          .Return (assembledType)
          .WhenCalled (_ => CheckLock (true));

      var result = _generator.GetOrGenerateType (types, typeKey, _typeAssemblerMock, requestedType, participantState, _mutableTypeBatchCodeGeneratorMock);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledType));
      Assert.That (types[typeKey], Is.SameAs (assembledType));
    }

    [Test]
    public void GetOrGenerateConstructorCall_CacheHit ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var constructorKey = new object[0];
      var typeKey = new object[0];
      var assembledConstructorCall = (Action) (() => { });
      var constructorCalls = new ConcurrentDictionary<object[], Delegate> { {constructorKey, assembledConstructorCall} };
      var types = new ConcurrentDictionary<object[], Type>();
      var participantState = new Dictionary<string, object>();

      var result = _generator.GetOrGenerateConstructorCall (
          constructorCalls,
          constructorKey,
          types,
          typeKey,
          _typeAssemblerMock,
          requestedType,
          delegateType,
          allowNonPublic,
          participantState,
          _mutableTypeBatchCodeGeneratorMock);

      Assert.That (result, Is.SameAs (assembledConstructorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss_CacheHitTypes ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var constructorKey = new object[0];
      var typeKey = new object[0];
      var assembledConstructorCall = (Action) (() => { });
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var constructorCalls = new ConcurrentDictionary<object[], Delegate>();
      var types = new ConcurrentDictionary<object[], Type> { { typeKey, assembledType } };
      var participantState = new Dictionary<string, object>();
      var fakeSignature = Tuple.Create (new[] { ReflectionObjectMother.GetSomeType() }, ReflectionObjectMother.GetSomeType());
      var fakeConstructor = ReflectionObjectMother.GetSomeConstructor();

      _delegateFactoryMock.Expect (mock => mock.GetSignature (delegateType)).Return (fakeSignature).WhenCalled (_ => CheckLock (true));
      _constructorFinderMock
          .Expect (mock => mock.GetConstructor (assembledType, fakeSignature.Item1, allowNonPublic, requestedType, fakeSignature.Item1))
          .Return (fakeConstructor)
          .WhenCalled (_ => CheckLock (true));
      _delegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (fakeConstructor, delegateType))
          .Return (assembledConstructorCall)
          .WhenCalled (_ => CheckLock (true));

      var result = _generator.GetOrGenerateConstructorCall (
          constructorCalls,
          constructorKey,
          types,
          typeKey,
          _typeAssemblerMock,
          requestedType,
          delegateType,
          allowNonPublic,
          participantState,
          _mutableTypeBatchCodeGeneratorMock);

      _delegateFactoryMock.VerifyAllExpectations();
      _constructorFinderMock.VerifyAllExpectations();
      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledConstructorCall));
      Assert.That (constructorCalls[constructorKey], Is.SameAs (assembledConstructorCall));
    }

    private void CheckLock (bool codeGenerationLockIsHeld)
    {
      if (codeGenerationLockIsHeld)
        LockTestHelper.CheckLockIsHeld (_codeGeneratorLock);
      else
        LockTestHelper.CheckLockIsNotHeld (_codeGeneratorLock);
    }
  }
}