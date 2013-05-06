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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Implementation.Synchronization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation.Synchronization
{
  [TestFixture]
  public class SynchronizationPointTest
  {
    private IGeneratedCodeFlusher _generatedCodeFlusherMock;
    private ITypeAssembler _typeAssemblerMock;
    private IConstructorFinder _constructorFinderMock;
    private IDelegateFactory _delegateFactoryMock;

    private SynchronizationPoint _point;

    private object _codeGeneratorLock;
    private IDictionary<string, object> _participantState;
    private IMutableTypeBatchCodeGenerator _mutableTypeBatchCodeGeneratorMock;

    [SetUp]
    public void SetUp ()
    {
      _generatedCodeFlusherMock = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler> ();
      _constructorFinderMock = MockRepository.GenerateStrictMock<IConstructorFinder>();
      _delegateFactoryMock = MockRepository.GenerateStrictMock<IDelegateFactory>();

      _point = new SynchronizationPoint (_generatedCodeFlusherMock, _typeAssemblerMock, _constructorFinderMock, _delegateFactoryMock);

      _codeGeneratorLock = PrivateInvoke.GetNonPublicField (_point, "_codeGenerationLock");
      _participantState = new Dictionary<string, object>();
      _mutableTypeBatchCodeGeneratorMock = MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>();
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
      _typeAssemblerMock.Expect (mock => mock.ExtractTypeID (assembledType)).Return (fakeTypeID).WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.GetTypeID (assembledType), Is.EqualTo (fakeTypeID));

      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      _typeAssemblerMock
          .Expect (mock => mock.RetrieveAdditionalType (additionalTypeID, _participantState))
          .Return (additionalType)
          .WhenCalled (_ => CheckLockIsHeld());
      Assert.That (_point.GetOrGenerateAdditionalType (additionalTypeID, _participantState), Is.SameAs (additionalType));

      _generatedCodeFlusherMock.VerifyAllExpectations();
      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    public void GetOrGenerateType_CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var types = new ConcurrentDictionary<AssembledTypeID, Type> { { typeID, assembledType } };

      var result = _point.GetOrGenerateType (types, typeID, _participantState, _mutableTypeBatchCodeGeneratorMock);

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

      var result = _point.GetOrGenerateType (types, typeID, _participantState, _mutableTypeBatchCodeGeneratorMock);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledType));
      Assert.That (types[typeID], Is.SameAs (assembledType));
    }

    [Test]
    public void GetOrGenerateConstructorCall_CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var constructionKey = new ConstructionKey (typeID, delegateType, allowNonPublic);
      var assembledConstructorCall = (Action) (() => { });
      var constructorCalls = new ConcurrentDictionary<ConstructionKey, Delegate> { { constructionKey, assembledConstructorCall } };
      var types = new ConcurrentDictionary<AssembledTypeID, Type>();

      var result = _point.GetOrGenerateConstructorCall (
          constructorCalls, constructionKey, types, _participantState, _mutableTypeBatchCodeGeneratorMock);

      Assert.That (result, Is.SameAs (assembledConstructorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss_CacheHitTypes ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var delegateType = ReflectionObjectMother.GetSomeDelegateType();
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      var constructionKey = new ConstructionKey (typeID, delegateType, allowNonPublic);
      var assembledConstructorCall = (Action) (() => { });
      var assembledType = ReflectionObjectMother.GetSomeOtherType();
      var constructorCalls = new ConcurrentDictionary<ConstructionKey, Delegate>();
      var types = new ConcurrentDictionary<AssembledTypeID, Type> { { typeID, assembledType } };
      var fakeSignature = Tuple.Create (new[] { ReflectionObjectMother.GetSomeType() }, ReflectionObjectMother.GetSomeType());
      var fakeConstructor = ReflectionObjectMother.GetSomeConstructor();

      _delegateFactoryMock.Expect (mock => mock.GetSignature (delegateType)).Return (fakeSignature).WhenCalled (_ => CheckLockIsHeld());
      _constructorFinderMock
          .Expect (mock => mock.GetConstructor (typeID.RequestedType, fakeSignature.Item1, allowNonPublic, assembledType))
          .Return (fakeConstructor)
          .WhenCalled (_ => CheckLockIsHeld());
      _delegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (fakeConstructor, delegateType))
          .Return (assembledConstructorCall)
          .WhenCalled (_ => CheckLockIsHeld());

      var result = _point.GetOrGenerateConstructorCall (
          constructorCalls, constructionKey, types, _participantState, _mutableTypeBatchCodeGeneratorMock);

      _delegateFactoryMock.VerifyAllExpectations();
      _constructorFinderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (assembledConstructorCall));
      Assert.That (constructorCalls[constructionKey], Is.SameAs (assembledConstructorCall));
    }

    [Test]
    public void RebuildParticipantState ()
    {
      var alreadyCachedAssembledType = ReflectionObjectMother.GetSomeType();
      var loadedAssembledType = ReflectionObjectMother.GetSomeOtherType();
      var additionalType = ReflectionObjectMother.GetSomeOtherType();
      var cachedTypeKey = AssembledTypeIDObjectMother.Create (parts: new[] { new object() });
      var loadedTypeKey = AssembledTypeIDObjectMother.Create();
      var types = new ConcurrentDictionary<AssembledTypeID, Type> { { cachedTypeKey, alreadyCachedAssembledType } };
      var keysToAssembledTypes =
          new[]
          {
              new KeyValuePair<AssembledTypeID, Type> (cachedTypeKey, alreadyCachedAssembledType),
              new KeyValuePair<AssembledTypeID, Type> (loadedTypeKey, loadedAssembledType)
          };
      var additionalTypes = new[] { additionalType };

      _typeAssemblerMock
          .Expect (mock => mock.RebuildParticipantState (Arg<LoadedTypesContext>.Is.Anything))
          .WhenCalled (
              mi =>
              {
                var context = (LoadedTypesContext) mi.Arguments[0];
                Assert.That (context.ProxyTypes, Is.EqualTo (new[] { new LoadedProxy (loadedAssembledType) }));
                Assert.That (context.AdditionalTypes, Is.EqualTo (new[] { additionalType }));
                Assert.That (context.State, Is.SameAs (_participantState));
              })
          .WhenCalled (_ => CheckLockIsHeld());

      _point.RebuildParticipantState (types, keysToAssembledTypes, additionalTypes, _participantState);

      _typeAssemblerMock.VerifyAllExpectations();
    }

    private void CheckLockIsHeld ()
    {
      LockTestHelper.CheckLockIsHeld (_codeGeneratorLock);
    }
  }
}