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
using System.Threading;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateAdditionalType_TypeCacheTest
  {
    private Mock<ITypeAssembler> _typeAssemblerMock;
    private Mock<IAssemblyContextPool> _assemblyContextPoolMock;

    private TypeCache _cache;

    private IDictionary<object, Lazy<Type>> _additionalTypes;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);
      _assemblyContextPoolMock = new Mock<IAssemblyContextPool> (MockBehavior.Strict);

      _cache = new TypeCache (_typeAssemblerMock.Object, _assemblyContextPoolMock.Object);
      _additionalTypes = (IDictionary<object, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_additionalTypes");
    }

    [Test]
    public void CacheHit ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      _additionalTypes.Add (additionalTypeID, new Lazy<Type> (() => additionalType, LazyThreadSafetyMode.None));

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var assemblyContext = CreateAssemblyContext();

      var isDequeued = false;
      _assemblyContextPoolMock
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext)
          .Callback (() => { isDequeued = true; })
          .Verifiable();

      _typeAssemblerMock
          .Setup (
              mock => mock.AssembleAdditionalType (
                  additionalTypeID,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Returns (new TypeAssemblyResult (additionalType))
          .Callback (() => Assert.That (isDequeued, Is.True))
          .Verifiable();

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback (
              () =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              })
          .Verifiable();

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (additionalType));

      Assert.That (_additionalTypes[additionalTypeID].IsValueCreated, Is.True);
      Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalType));
    }

    [Test]
    public void CacheMiss_AndExceptionDuringAssembleType_DoesNotCacheException ()
    {
      var expectedException = new Exception();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var assemblyContext = CreateAssemblyContext();
      var assembleAdditionalTypeCount = 0;

      var sequence = new VerifiableSequence();
      _assemblyContextPoolMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext);
      _typeAssemblerMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AssembleAdditionalType (It.IsAny<object>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Throws (expectedException);
      _assemblyContextPoolMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.Enqueue (assemblyContext));
      _assemblyContextPoolMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext);
      _typeAssemblerMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AssembleAdditionalType (It.IsAny<object>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (new TypeAssemblyResult (additionalType))
          .Callback (
              () =>
              {
                if (assembleAdditionalTypeCount == 0)
                {
                  assembleAdditionalTypeCount++;
                  throw expectedException;
                }
              });
      _assemblyContextPoolMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.Enqueue (assemblyContext));

      Assert.That (() => _cache.GetOrCreateAdditionalType (additionalTypeID), Throws.Exception.SameAs (expectedException));
      Assert.That (_cache.GetOrCreateAdditionalType (additionalTypeID), Is.SameAs (additionalType));

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      sequence.Verify();
    }

    [Test]
    public void CacheMiss_WithExceptionDuringAssembleAdditionalType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var additionalTypeID = new object();
      var assemblyContext = CreateAssemblyContext();

      var isDequeued = false;
      _assemblyContextPoolMock
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext)
          .Callback (() => { isDequeued = true; })
          .Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleAdditionalType (It.IsAny<object>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Callback (
              (object myObject, IParticipantState participantState, IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator) =>
                  Assert.That (isDequeued, Is.True))
          .Throws (expectedException);

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback (
              (AssemblyContext _) =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              })
          .Verifiable();

      Assert.That (
          () => _cache.GetOrCreateAdditionalType (additionalTypeID),
          Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
    }

    [Test]
    public void CacheMiss_AddsAdditionalTypesToCacheBeforeReturningAssemblyContextToPool ()
    {
      var assemblyContext = CreateAssemblyContext();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var otherAdditionalTypeID = new object();
      var otherAdditionalType = ReflectionObjectMother.GetSomeType();

      _assemblyContextPoolMock.Setup (mock => mock.Dequeue()).Returns (assemblyContext).Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleAdditionalType (It.IsAny<object>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (new TypeAssemblyResult (additionalType, new Dictionary<object, Type> { { otherAdditionalTypeID, otherAdditionalType } }));

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback ((AssemblyContext _) => Assert.That (_additionalTypes[otherAdditionalTypeID].Value, Is.SameAs (otherAdditionalType)))
          .Verifiable();

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void CacheMiss_AddsAdditionalTypesToCache_OverridesPreviouslyCachedValue ()
    {
      var assemblyContext = CreateAssemblyContext();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var otherAdditionalTypeID = new object();
      var otherAdditionalType = ReflectionObjectMother.GetSomeType();
      _additionalTypes.Add (otherAdditionalTypeID, new Lazy<Type> (() => null, LazyThreadSafetyMode.None));

      _assemblyContextPoolMock.Setup (mock => mock.Dequeue()).Returns (assemblyContext).Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleAdditionalType (It.IsAny<object>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (new TypeAssemblyResult (additionalType, new Dictionary<object, Type> { { otherAdditionalTypeID, otherAdditionalType } }));

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback ((AssemblyContext _) => Assert.That (_additionalTypes[otherAdditionalTypeID].Value, Is.SameAs (otherAdditionalType)))
          .Verifiable();

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void CacheMiss_DoesNotAddRequestedAdditionalTypeToCacheAtSameTimeAsOtherAdditionalTypes ()
    {
      var assemblyContext = CreateAssemblyContext();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var otherAdditionalTypeID = new object();
      var otherAdditionalType = ReflectionObjectMother.GetSomeType();

      _assemblyContextPoolMock.Setup (mock => mock.Dequeue()).Returns (assemblyContext).Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleAdditionalType (It.IsAny<object>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (
              new TypeAssemblyResult (
                  additionalType,
                  new Dictionary<object, Type> { { additionalTypeID, additionalType }, { otherAdditionalTypeID, otherAdditionalType } }));

      _assemblyContextPoolMock.Setup (mock => mock.Enqueue (assemblyContext)).Verifiable();

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (additionalType));
      Assert.That (_additionalTypes[additionalTypeID].IsValueCreated, Is.True);
      Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalType));
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object,
          new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict).Object);
    }
  }
}