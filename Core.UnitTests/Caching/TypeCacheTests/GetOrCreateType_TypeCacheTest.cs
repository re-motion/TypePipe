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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateType_TypeCacheTest
  {
    private Mock<ITypeAssembler> _typeAssemblerMock;
    private Mock<IAssemblyContextPool> _assemblyContextPoolMock;

    private TypeCache _cache;

    private IDictionary<AssembledTypeID, Lazy<Type>> _assembledTypes;
    private IDictionary<object, Lazy<Type>> _additionalTypes;

    private readonly Type _requestedType = typeof (RequestedType);
    private readonly Type _assembledType = typeof (AssembledType);

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);
      _assemblyContextPoolMock = new Mock<IAssemblyContextPool> (MockBehavior.Strict);

      _cache = new TypeCache (_typeAssemblerMock.Object, _assemblyContextPoolMock.Object);

      _assembledTypes = (IDictionary<AssembledTypeID, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_assembledTypes");
      _additionalTypes = (IDictionary<object, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_additionalTypes");
    }

    [Test]
    public void CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (_requestedType);
      _assembledTypes.Add (typeID, new Lazy<Type> (() => _assembledType, LazyThreadSafetyMode.None));

      var result = _cache.GetOrCreateType (typeID);

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var assemblyContext = CreateAssemblyContext();

      var isDequeued = false;
      _assemblyContextPoolMock
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext)
          .Callback (() => { isDequeued = true; })
          .Verifiable();

      _typeAssemblerMock
          .Setup (
              mock => mock.AssembleType (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (typeID)),
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Returns (new TypeAssemblyResult (_assembledType))
          .Callback (() => Assert.That (isDequeued, Is.True))
          .Verifiable();

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback ((AssemblyContext _) => Assert.That (isDequeued, Is.True))
          .Verifiable();

      var result = _cache.GetOrCreateType (typeID);

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (_assembledType));

      Assert.That (_assembledTypes[typeID].IsValueCreated, Is.True);
      Assert.That (_assembledTypes[typeID].Value, Is.SameAs (_assembledType));
    }

    [Test]
    public void CacheMiss_AndExceptionDuringAssembleType_DoesNotCacheException ()
    {
      var expectedException = new Exception();
      var typeID = AssembledTypeIDObjectMother.Create();
      var assemblyContext = CreateAssemblyContext();
      var assembleTypeCount = 0;

      var sequence = new VerifiableSequence();
      _assemblyContextPoolMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext);
      _typeAssemblerMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.AssembleType (It.IsAny<AssembledTypeID>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
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
          .Setup (mock => mock.AssembleType (It.IsAny<AssembledTypeID>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (new TypeAssemblyResult (_assembledType))
          .Callback (
              () =>
              {
                if (assembleTypeCount == 0)
                {
                  assembleTypeCount++;
                  throw expectedException;
                }
              });
      _assemblyContextPoolMock
          .InVerifiableSequence (sequence)
          .Setup (mock => mock.Enqueue (assemblyContext));

      Assert.That (() => _cache.GetOrCreateType (typeID), Throws.Exception.SameAs (expectedException));
      Assert.That (_cache.GetOrCreateType (typeID), Is.SameAs (_assembledType));

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
      sequence.Verify();
    }

    [Test]
    public void CacheMiss_AndExceptionDuringAssembleType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var typeID = AssembledTypeIDObjectMother.Create();
      var assemblyContext = CreateAssemblyContext();

      var isDequeued = false;
      _assemblyContextPoolMock
          .Setup (mock => mock.Dequeue())
          .Returns (assemblyContext)
          .Callback (() => { isDequeued = true; })
          .Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleType (It.IsAny<AssembledTypeID>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Callback (
              (AssembledTypeID assembledTypeID, IParticipantState participantState, IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator) =>
                  Assert.That (isDequeued, Is.True))
          .Throws (expectedException)
          .Verifiable();

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback ((AssemblyContext _) => Assert.That (isDequeued, Is.True))
          .Verifiable();

      Assert.That (() => _cache.GetOrCreateType (typeID), Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.Verify();
      _assemblyContextPoolMock.Verify();
    }

    [Test]
    public void CacheMiss_AddsAdditionalTypesToCacheBeforeReturningAssemblyContextToPool ()
    {
      var assemblyContext = CreateAssemblyContext();
      var typeID = AssembledTypeIDObjectMother.Create();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();

      _assemblyContextPoolMock.Setup (mock => mock.Dequeue()).Returns (assemblyContext).Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleType (It.IsAny<AssembledTypeID>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (new TypeAssemblyResult (_assembledType, new Dictionary<object, Type> { { additionalTypeID, additionalType } }));

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback ((AssemblyContext _) => Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalType)))
          .Verifiable();

      var result = _cache.GetOrCreateType (typeID);

      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    [Test]
    public void CacheMiss_AddsAdditionalTypesToCache_OverridesPreviouslyCachedValue ()
    {
      var assemblyContext = CreateAssemblyContext();
      var typeID = AssembledTypeIDObjectMother.Create();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      _additionalTypes.Add (additionalTypeID, new Lazy<Type> (() => null, LazyThreadSafetyMode.None));

      _assemblyContextPoolMock.Setup (mock => mock.Dequeue()).Returns (assemblyContext).Verifiable();

      _typeAssemblerMock
          .Setup (mock => mock.AssembleType (It.IsAny<AssembledTypeID>(), It.IsAny<IParticipantState>(), It.IsAny<IMutableTypeBatchCodeGenerator>()))
          .Returns (new TypeAssemblyResult (_assembledType, new Dictionary<object, Type> { { additionalTypeID, additionalType } }))
          .Verifiable();

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContext))
          .Callback ((AssemblyContext _) => Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalType)))
          .Verifiable();

      var result = _cache.GetOrCreateType (typeID);

      _assemblyContextPoolMock.Verify();
      Assert.That (result, Is.SameAs (_assembledType));
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object,
          new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict).Object);
    }

    private class RequestedType {}
    private class AssembledType {}
  }
}