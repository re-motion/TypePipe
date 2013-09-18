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
using System.Threading;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateType_TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IConstructorDelegateFactory _constructorDelegateFactoryMock;
    private IAssemblyContextPool _assemblyContextPool;

    private TypeCache _cache;

    private IDictionary<AssembledTypeID, Lazy<Type>> _types;

    private readonly Type _requestedType = typeof (RequestedType);
    private readonly Type _otherRequestedType = typeof (RequestedType2);
    private readonly Type _assembledType = typeof (AssembledType);
    private readonly Type _otherAssembledType = typeof (AssembledType2);

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _constructorDelegateFactoryMock = MockRepository.GenerateStrictMock<IConstructorDelegateFactory>();
      _assemblyContextPool = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _cache = new TypeCache (_typeAssemblerMock, _constructorDelegateFactoryMock, _assemblyContextPool);

      _types = (ConcurrentDictionary<AssembledTypeID, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_types");
    }

    [Test]
    public void CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (_requestedType);
      _types.Add (typeID, new Lazy<Type> (() => _assembledType, LazyThreadSafetyMode.None));
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID);

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool ()
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
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));

      Assert.That (_types[typeID].IsValueCreated, Is.True);
      Assert.That (_types[typeID].Value, Is.SameAs (_assembledType));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool_AssembleTwoTypes_UsesDifferentAssemblyContexts ()
    {
      var typeID1 = AssembledTypeIDObjectMother.Create (_requestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID1);

      var assemblyContext1 = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      _assemblyContextPool.Expect (mock => mock.Dequeue()).Return (assemblyContext1);

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID1)),
                  Arg.Is (assemblyContext1.ParticipantState),
                  Arg.Is (assemblyContext1.MutableTypeBatchCodeGenerator)))
          .Return (_assembledType);

      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext1));


      var typeID2 = AssembledTypeIDObjectMother.Create (_otherRequestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_otherRequestedType)).Return (typeID2);

      var assemblyContext2 = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      _assemblyContextPool.Expect (mock => mock.Dequeue()).Return (assemblyContext2);

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID2)),
                  Arg.Is (assemblyContext2.ParticipantState),
                  Arg.Is (assemblyContext2.MutableTypeBatchCodeGenerator)))
          .Return (_otherAssembledType);

      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext2));

      var result1 = _cache.GetOrCreateType (_requestedType);
      var result2 = _cache.GetOrCreateType (_otherRequestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result1, Is.SameAs (_assembledType));
      Assert.That (result2, Is.SameAs (_otherAssembledType));
    }

    [Test]
    public void CacheMiss_AndExceptionDuringAssembleType_ReturnsAssemblyContextToPool ()
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
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      Assert.That (() => _cache.GetOrCreateType (_requestedType), Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();

        //TODO RM-5849: See current implementation preferring to leave the exception-throwing Lazy-object
      //Assert.That (_types.ContainsKey (typeID), Is.False);
      Assert.That (() => _types[typeID].Value, Throws.Exception.SameAs (expectedException));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool_ReusesAssemblyContextForNestedCallsToGetOrCreateType ()
    {
      var typeID1 = AssembledTypeIDObjectMother.Create (_requestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID1);

      var typeID2 = AssembledTypeIDObjectMother.Create (_otherRequestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_otherRequestedType)).Return (typeID2);

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
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID1)),
                  Arg.Is (assemblyContext.ParticipantState),
                  Arg.Is (assemblyContext.MutableTypeBatchCodeGenerator)))
          .Return (_assembledType)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                _cache.GetOrCreateType (_otherRequestedType);
              });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID2)),
                  Arg.Is (assemblyContext.ParticipantState),
                  Arg.Is (assemblyContext.MutableTypeBatchCodeGenerator)))
          .Return (_otherAssembledType)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool_ReusesAssemblyContextForNestedCallsToGetOrCreateAdditionalType ()
    {
      var typeID1 = AssembledTypeIDObjectMother.Create (_requestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID1);

      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();

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
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID1)),
                  Arg.Is (assemblyContext.ParticipantState),
                  Arg.Is (assemblyContext.MutableTypeBatchCodeGenerator)))
          .Return (_assembledType)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                Assert.That (_cache.GetOrCreateAdditionalType (additionalTypeID), Is.SameAs (additionalType));
              });

      _typeAssemblerMock
          .Expect (
              mock => mock.GetOrAssembleAdditionalType (
                  additionalTypeID,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Return (additionalType)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    [Ignore("TODO RM-5849: RhinoMocks causes a deadlock, preventing the multithreaded test from completing")]
    public void CacheMiss_UsesAssemblyContextFromPool_ParallelCallsUseSeparateAssemblyContexts ()
    {
      var typeID1 = AssembledTypeIDObjectMother.Create (_requestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (typeID1);

      var typeID2 = AssembledTypeIDObjectMother.Create (_otherRequestedType);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_otherRequestedType)).Return (typeID2);

      var assemblyContext1 = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      var assemblyContext2 = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued1 = false;
      bool isDequeued2 = false;

      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext1)
          .WhenCalled (mi => { isDequeued1 = true; });

      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext2)
          .WhenCalled (mi => { isDequeued2 = true; });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID1)),
                  Arg.Is (assemblyContext1.ParticipantState),
                  Arg.Is (assemblyContext1.MutableTypeBatchCodeGenerator)))
          .Return (_assembledType)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued1, Is.True);
                var threadRunner =
                    ThreadRunner.WithTimeout (
                        () => Assert.That (_cache.GetOrCreateType (_otherRequestedType), Is.SameAs (_otherAssembledType)),
                        TimeSpan.FromMilliseconds (100));
                Assert.That (threadRunner.Run(), Is.False);
              });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID2)),
                  Arg.Is (assemblyContext2.ParticipantState),
                  Arg.Is (assemblyContext2.MutableTypeBatchCodeGenerator)))
          .Return (_otherAssembledType)
          .WhenCalled (mi => Assert.That (isDequeued2, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext1))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued1, Is.True);
                isDequeued1 = false;
              });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext2))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued2, Is.True);
                isDequeued2 = false;
              });

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    private class RequestedType {}
    private class RequestedType2 {}
    private class AssembledType {}
    private class AssembledType2 {}
  }
}