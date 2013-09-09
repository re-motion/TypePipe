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
using System.Collections.ObjectModel;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IConstructorDelegateFactory _constructorDelegateFactoryMock;
    private IAssemblyContextPool _assemblyContextPool;
    private ITypeCacheSynchronizationPoint _typeCacheSynchronizationPointMock;

    private TypeCache _cache;

    private IDictionary<AssembledTypeID, Type> _types;
    private IDictionary<ConstructionKey, Delegate> _constructorCalls;

    private readonly Type _requestedType = typeof (RequestedType);
    private readonly Type _assembledType = typeof (AssembledType);
    private readonly Delegate _generatedCtorCall = new Func<int> (() => 7);
    private Type _delegateType;
    private bool _allowNonPublic;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _constructorDelegateFactoryMock = MockRepository.GenerateStrictMock<IConstructorDelegateFactory>();
      _assemblyContextPool = MockRepository.GenerateStrictMock<IAssemblyContextPool>();
      _typeCacheSynchronizationPointMock = MockRepository.GenerateStrictMock<ITypeCacheSynchronizationPoint>();

      _cache = new TypeCache (_typeAssemblerMock, _constructorDelegateFactoryMock, _assemblyContextPool, _typeCacheSynchronizationPointMock);

      _types = (ConcurrentDictionary<AssembledTypeID, Type>) PrivateInvoke.GetNonPublicField (_cache, "_types");
      _constructorCalls = (ConcurrentDictionary<ConstructionKey, Delegate>) PrivateInvoke.GetNonPublicField (_cache, "_constructorCalls");

      _delegateType = ReflectionObjectMother.GetSomeDelegateType();
      _allowNonPublic = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void ParticipantConfigurationID ()
    {
      _typeAssemblerMock.Expect (mock => mock.ParticipantConfigurationID).Return ("configId");

      Assert.That (_cache.ParticipantConfigurationID, Is.EqualTo ("configId"));
    }

    [Test]
    public void Participants ()
    {
      var participants = new ReadOnlyCollection<IParticipant> (new IParticipant[0]);
      _typeAssemblerMock.Expect (mock => mock.Participants).Return (participants);

      Assert.That (_cache.Participants, Is.SameAs (participants));
    }

    [Test]
    public void GetOrCreateType_CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (_requestedType);
      _types.Add (typeID, _assembledType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    public void GetOrCreateType_CacheMiss_UsesAssemblyContextFromPool ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);
      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)),
                  Arg.Is (assemblyContext.ParticipantState),
                  Arg.Is (assemblyContext.MutableTypeBatchCodeGenerator)))
          .Return (_assembledType)
          .WhenCalled (mi => { isDequeued = true; });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    public void GetOrCreateType_CacheMiss_AndExceptionDuringAssembleType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var typeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (mock => mock.AssembleType (new AssembledTypeID(), null, null))
          .IgnoreArguments()
          .Throw (expectedException)
          .WhenCalled (mi => { isDequeued = true; });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      Assert.That (() => _cache.GetOrCreateType (_requestedType), Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _constructorCalls.Add (new ConstructionKey (typeID, _delegateType, _allowNonPublic), _generatedCtorCall);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      var result = _cache.GetOrCreateConstructorCall (_requestedType, _delegateType, _allowNonPublic);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss_WithAssembledTypeCacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      _types.Add (typeID, _assembledType);

      _constructorDelegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (typeID.RequestedType, _assembledType, _delegateType, _allowNonPublic))
          .Return (_generatedCtorCall);

      var result = _cache.GetOrCreateConstructorCall (_requestedType, _delegateType, _allowNonPublic);

      _typeAssemblerMock.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (_generatedCtorCall));

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[key], Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss_WithAssembledTypeCacheMiss ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)),
                  Arg.Is (assemblyContext.ParticipantState),
                  Arg.Is (assemblyContext.MutableTypeBatchCodeGenerator)))
          .Return (_assembledType)
          .WhenCalled (mi => { isDequeued = true; });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _constructorDelegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (typeID.RequestedType, _assembledType, _delegateType, _allowNonPublic))
          .Return (_generatedCtorCall)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      var result = _cache.GetOrCreateConstructorCall (_requestedType, _delegateType, _allowNonPublic);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();
      
      Assert.That (result, Is.SameAs (_generatedCtorCall));

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[key], Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss_WithAssembledTypeCacheMiss_AndExceptionDuringAssembleType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var typeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (mock => mock.AssembleType (new AssembledTypeID(), null, null))
          .IgnoreArguments()
          .Throw (expectedException)
          .WhenCalled (mi => { isDequeued = true; });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      Assert.That (
          () => _cache.GetOrCreateConstructorCall (_requestedType, _delegateType, _allowNonPublic),
          Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls.ContainsKey(key), Is.False);
    }

    [Test]
    public void LoadTypes ()
    {
      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (_assembledType)).Return (true);
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (additionalGeneratedType)).Return (false);
      var typeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ExtractTypeID (_assembledType)).Return (typeID);
      _typeCacheSynchronizationPointMock
          .Expect (
              mock => mock.RebuildParticipantState (
                  Arg.Is ((ConcurrentDictionary<AssembledTypeID, Type>) _types),
                  Arg<IEnumerable<KeyValuePair<AssembledTypeID, Type>>>.Is.Anything,
                  Arg<IEnumerable<Type>>.List.Equal (new[] { additionalGeneratedType })))
          .WhenCalled (
              mi =>
              {
                var keysToAssembledTypes = (IEnumerable<KeyValuePair<AssembledTypeID, Type>>) mi.Arguments[1];
                var pair = keysToAssembledTypes.Single();
                Assert.That (pair.Key, Is.EqualTo (typeID));
                Assert.That (pair.Value, Is.SameAs (_assembledType));
              });

      _cache.LoadTypes (new[] { _assembledType, additionalGeneratedType });

      _typeCacheSynchronizationPointMock.VerifyAllExpectations();
    }

    [Test]
    public void GetOrCreateAdditionalType ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      _typeCacheSynchronizationPointMock
          .Expect (mock => mock.GetOrGenerateAdditionalType (additionalTypeID))
          .Return (additionalType);

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeCacheSynchronizationPointMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType));
    }

    private class RequestedType {}
    private class AssembledType {}
  }
}