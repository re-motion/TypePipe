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
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateAdditionalType_TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IConstructorDelegateFactory _constructorDelegateFactoryMock;
    private IAssemblyContextPool _assemblyContextPool;

    private TypeCache _cache;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _constructorDelegateFactoryMock = MockRepository.GenerateStrictMock<IConstructorDelegateFactory>();
      _assemblyContextPool = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _cache = new TypeCache (_typeAssemblerMock, _constructorDelegateFactoryMock, _assemblyContextPool);
    }

    [Test]
    public void CalledOnce_UsesAssemblyContextFromPool ()
    {
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

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void UsesDifferentAssemblyContextsForSubsequentCalls ()
    {
      var additionalTypeID1 = new object();
      var additionalType1 = ReflectionObjectMother.GetSomeType();

      var assemblyContext1 = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      _assemblyContextPool.Expect (mock => mock.Dequeue()).Return (assemblyContext1);

      _typeAssemblerMock
          .Expect (
              mock => mock.GetOrAssembleAdditionalType (
                  additionalTypeID1,
                  assemblyContext1.ParticipantState,
                  assemblyContext1.MutableTypeBatchCodeGenerator))
          .Return (additionalType1);

      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext1));

      var additionalTypeID2 = new object();
      var additionalType2 = ReflectionObjectMother.GetSomeType();

      var assemblyContext2 = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      _assemblyContextPool.Expect (mock => mock.Dequeue()).Return (assemblyContext2);

      _typeAssemblerMock
          .Expect (
              mock => mock.GetOrAssembleAdditionalType (
                  additionalTypeID2,
                  assemblyContext2.ParticipantState,
                  assemblyContext2.MutableTypeBatchCodeGenerator))
          .Return (additionalType2);

      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext2));

      var result1 = _cache.GetOrCreateAdditionalType (additionalTypeID1);
      var result2 = _cache.GetOrCreateAdditionalType (additionalTypeID2);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result1, Is.SameAs (additionalType1));
      Assert.That (result2, Is.SameAs (additionalType2));
    }

    [Test]
    public void WithExceptionDuringAssembleAdditionalType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var additionalTypeID = new object();

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPool
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (mock => mock.GetOrAssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Throw (expectedException)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      Assert.That (
          () => _cache.GetOrCreateAdditionalType (additionalTypeID),
          Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
    }

    [Test]
    public void ReusesAssemblyContextForNestedCallsToGetOrCreateAdditionalType ()
    {
      var additionalTypeID1 = new object();
      var additionalType1 = ReflectionObjectMother.GetSomeType();
      var additionalTypeID2 = new object();
      var additionalType2 = ReflectionObjectMother.GetSomeType();

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
              mock => mock.GetOrAssembleAdditionalType (
                  additionalTypeID1,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Return (additionalType1)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                Assert.That (_cache.GetOrCreateAdditionalType (additionalTypeID2), Is.SameAs (additionalType2));
              });

      _typeAssemblerMock
          .Expect (
              mock => mock.GetOrAssembleAdditionalType (
                  additionalTypeID2,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Return (additionalType2)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID1);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType1));
    }

    [Test]
    public void ReusesAssemblyContextForNestedCallsToGetOrCreateType ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();

      var requestedType = typeof (RequestedType);
      var assembledType = typeof (AssembledType);
      var typeID = AssembledTypeIDObjectMother.Create (requestedType);

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
              mock => mock.GetOrAssembleAdditionalType (
                  additionalTypeID,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Return (additionalType)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                Assert.That (_cache.GetOrCreateType (typeID), Is.SameAs (assembledType));
              });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)),
                  Arg.Is (assemblyContext.ParticipantState),
                  Arg.Is (assemblyContext.MutableTypeBatchCodeGenerator)))
          .Return (assembledType)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType));
    }

    private class RequestedType {}
    private class AssembledType {}
  }
}