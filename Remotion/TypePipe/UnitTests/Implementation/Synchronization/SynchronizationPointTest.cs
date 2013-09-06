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
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting.Threading;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation.Synchronization
{
  [TestFixture]
  public class SynchronizationPointTest
  {
    private IGeneratedCodeFlusher _generatedCodeFlusherMock;
    private ITypeAssembler _typeAssemblerMock;
    private IConstructorDelegateFactory _constructorDelegateFactoryMock;
    
    private IMutableTypeBatchCodeGenerator _mutableTypeBatchCodeGeneratorMock;
    private IDictionary<string, object> _participantState;

    private SynchronizationPoint _point;

    private object _codeGeneratorLock;

    [SetUp]
    public void SetUp ()
    {
      _generatedCodeFlusherMock = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler> ();

      _mutableTypeBatchCodeGeneratorMock = MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>();
      var assemblyContext = new AssemblyContext (_mutableTypeBatchCodeGeneratorMock, _generatedCodeFlusherMock);
      _participantState = assemblyContext.ParticipantState;
      _constructorDelegateFactoryMock = MockRepository.GenerateStrictMock<IConstructorDelegateFactory>();

      _point = new SynchronizationPoint (_typeAssemblerMock, _constructorDelegateFactoryMock, assemblyContext);

      _codeGeneratorLock = PrivateInvoke.GetNonPublicField (_point, "_codeGenerationLock");
    }

    [Test]
    public void DelegatingMembers_GuardedByLock ()
    {
      _generatedCodeFlusherMock.Expect (mock => mock.AssemblyDirectory).Return ("get dir").WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.AssemblyDirectory, Is.EqualTo ("get dir"));
      _generatedCodeFlusherMock.Expect (mock => mock.AssemblyNamePattern).Return ("get name pattern").WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.AssemblyNamePattern, Is.EqualTo ("get name pattern"));

      _generatedCodeFlusherMock.Expect (mock => mock.SetAssemblyDirectory ("set dir")).WhenCalled (_ => CheckLockIsHeld());
      _point.SetAssemblyDirectory ("set dir");
      _generatedCodeFlusherMock.Expect (mock => mock.SetAssemblyNamePattern ("set name pattern")).WhenCalled (_ => CheckLockIsHeld());
      _point.SetAssemblyNamePattern ("set name pattern");

      var type = ReflectionObjectMother.GetSomeType();
      var fakeIsAssembledType = BooleanObjectMother.GetRandomBoolean();
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (type)).Return (fakeIsAssembledType).WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.IsAssembledType (type), Is.EqualTo (fakeIsAssembledType));

      var fakeRequestedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Expect (mock => mock.GetRequestedType (type)).Return (fakeRequestedType).WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.GetRequestedType (type), Is.SameAs (fakeRequestedType));

      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect(mock => mock.ExtractTypeID(assembledType)).Return(fakeTypeID).WhenCalled(_ => CheckLockIsHeld());
      Assert.That(_point.GetTypeID(assembledType), Is.EqualTo(fakeTypeID));

      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      _typeAssemblerMock
          .Expect (mock => mock.GetOrAssembleAdditionalType (additionalTypeID, _participantState, _mutableTypeBatchCodeGeneratorMock))
          .Return (additionalType)
          .WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.GetOrGenerateAdditionalType (additionalTypeID), Is.SameAs (additionalType));

      _generatedCodeFlusherMock.VerifyAllExpectations();
      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    public void GetOrGenerateType_CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var types = CreateConcurrentDictionary (typeID, assembledType);

      var result = _point.GetOrGenerateType (types, typeID);

      Assert.That (result, Is.SameAs (assembledType));
    }

    [Test]
    public void GetOrGenerateType_CacheMiss ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var types = new ConcurrentDictionary<AssembledTypeID, Type>();

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)), // Use strongly typed Equals overload.
                  Arg.Is(_participantState),
                  Arg.Is(_mutableTypeBatchCodeGeneratorMock)))
          .Return (assembledType)
          .WhenCalled (_ => CheckLockIsHeld ());

      var result = _point.GetOrGenerateType (types, typeID);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledType));
      Assert.That (types[typeID], Is.SameAs (assembledType));
    }

    [Test]
    public void GetOrGenerateConstructorCall_CacheHit ()
    {
      var constructionKey = CreateConstructionKey();
      var assembledConstructorCall = (Action) (() => { });
      var constructorCalls = CreateConcurrentDictionary<ConstructionKey, Delegate> (constructionKey, assembledConstructorCall);
      var types = new ConcurrentDictionary<AssembledTypeID, Type>();

      var result = _point.GetOrGenerateConstructorCall (constructorCalls, constructionKey, types);

      Assert.That (result, Is.SameAs (assembledConstructorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss_CacheHitTypes ()
    {
      var constructionKey = CreateConstructionKey();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var constructorCalls = new ConcurrentDictionary<ConstructionKey, Delegate>();
      var types = CreateConcurrentDictionary (constructionKey.TypeID, assembledType);

      var assembledConstructorCall = (Action) (() => { });
      _constructorDelegateFactoryMock
          .Expect (
              mock => mock.CreateConstructorCall (
                  constructionKey.TypeID.RequestedType,
                  assembledType,
                  constructionKey.DelegateType,
                  constructionKey.AllowNonPublic))
          .Return (assembledConstructorCall)
          .WhenCalled (_ => CheckLockIsHeld());

      var result = _point.GetOrGenerateConstructorCall (constructorCalls, constructionKey, types);

      _constructorDelegateFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledConstructorCall));
      Assert.That (constructorCalls[constructionKey], Is.SameAs (assembledConstructorCall));
    }

    [Test]
    public void RebuildParticipantState ()
    {
      var alreadyCachedAssembledType = ReflectionObjectMother.GetSomeType();
      var loadedAssembledType = ReflectionObjectMother.GetSomeOtherType();
      var additionalType = ReflectionObjectMother.GetSomeOtherType();
      var cachedTypeKey = AssembledTypeIDObjectMother.Create (parts: new object[] { "1" });
      var loadedTypeKey = AssembledTypeIDObjectMother.Create (parts: new object[] { "2" });
      var types = CreateConcurrentDictionary (cachedTypeKey, alreadyCachedAssembledType);
      var keysToAssembledTypes =
          new[]
          {
              new KeyValuePair<AssembledTypeID, Type> (cachedTypeKey, alreadyCachedAssembledType),
              new KeyValuePair<AssembledTypeID, Type> (loadedTypeKey, loadedAssembledType)
          };
      var additionalTypes = new[] { additionalType };

      _typeAssemblerMock
          .Expect (mock => mock.RebuildParticipantState (new[] { loadedAssembledType }, new[] { additionalType }, _participantState))
          .WhenCalled (_ => CheckLockIsHeld());

      _point.RebuildParticipantState (types, keysToAssembledTypes, additionalTypes);

      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    public void GetOrGenerateConstructorCall_Reverse_CacheHit ()
    {
      var reverseConstructionKey = CreateReverseConstructionKey();
      var assembledConstructorCall = (Action) (() => { });
      var constructorCalls = CreateConcurrentDictionary<ReverseConstructionKey, Delegate> (reverseConstructionKey, assembledConstructorCall);

      var result = _point.GetOrGenerateConstructorCall (constructorCalls, reverseConstructionKey);

      Assert.That (result, Is.SameAs (assembledConstructorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_Reverse_CacheMiss ()
    {
      var reverseConstructionKey = CreateReverseConstructionKey();
      var constructorCalls = new ConcurrentDictionary<ReverseConstructionKey, Delegate>();
      var fakeRequestedType = ReflectionObjectMother.GetSomeType();

      _typeAssemblerMock
          .Expect (mock => mock.GetRequestedType (reverseConstructionKey.AssembledType)).Return (fakeRequestedType)
          .WhenCalled (_ => CheckLockIsHeld());

      var assembledConstructorCall = (Action) (() => { });
      _constructorDelegateFactoryMock
          .Expect (
              mock => mock.CreateConstructorCall (
                  fakeRequestedType,
                  reverseConstructionKey.AssembledType,
                  reverseConstructionKey.DelegateType,
                  reverseConstructionKey.AllowNonPublic))
          .Return (assembledConstructorCall)
          .WhenCalled (_ => CheckLockIsHeld());

      var result = _point.GetOrGenerateConstructorCall (constructorCalls, reverseConstructionKey);

      _typeAssemblerMock.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledConstructorCall));
      Assert.That (constructorCalls[reverseConstructionKey], Is.SameAs (assembledConstructorCall));
    }

    private void CheckLockIsHeld ()
    {
      LockTestHelper.CheckLockIsHeld (_codeGeneratorLock);
    }

    private ConstructionKey CreateConstructionKey ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();

      return new ConstructionKey (typeID, delegateType, allowNonPublic);
    }

    private ReverseConstructionKey CreateReverseConstructionKey ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();

      return new ReverseConstructionKey (assembledType, delegateType, allowNonPublic);
    }

    private ConcurrentDictionary<TKey, TValue> CreateConcurrentDictionary<TKey, TValue> (TKey key, TValue value)
    {
      var mapping = new[] { new KeyValuePair<TKey, TValue> (key, value) };
      return new ConcurrentDictionary<TKey, TValue> (mapping);
    }
  }
}