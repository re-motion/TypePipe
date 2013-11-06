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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateAdditionalType_TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IAssemblyContextPool _assemblyContextPoolMock;

    private TypeCache _cache;

    private IDictionary<object, Lazy<Type>> _additionalTypes;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _assemblyContextPoolMock = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _cache = new TypeCache (_typeAssemblerMock, _assemblyContextPoolMock);
      _additionalTypes = (IDictionary<object, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_additionalTypes");
    }

    [Test]
    public void CacheHit ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      _additionalTypes.Add (additionalTypeID, new Lazy<Type> (() => additionalType, LazyThreadSafetyMode.None));

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void CacheMiss_UsesAssemblyContextFromPool ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var assemblyContext = CreateAssemblyContext();

      bool isDequeued = false;
      _assemblyContextPoolMock
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleAdditionalType (
                  additionalTypeID,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Return (new TypeAssemblyResult (additionalType))
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
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

      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (assemblyContext);
      _typeAssemblerMock.Expect (mock => mock.AssembleAdditionalType (null, null, null)).IgnoreArguments().Throw (expectedException);
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContext));

      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (assemblyContext);
      _typeAssemblerMock
          .Expect (mock => mock.AssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Return (new TypeAssemblyResult (additionalType));
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContext));

      Assert.That (() => _cache.GetOrCreateAdditionalType (additionalTypeID), Throws.Exception.SameAs (expectedException));
      Assert.That (_cache.GetOrCreateAdditionalType (additionalTypeID), Is.SameAs (additionalType));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void CacheMiss_WithExceptionDuringAssembleAdditionalType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var additionalTypeID = new object();
      var assemblyContext = CreateAssemblyContext();

      bool isDequeued = false;
      _assemblyContextPoolMock
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (mock => mock.AssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Throw (expectedException)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPoolMock
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
      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void CacheMiss_AddsAdditionalTypesToCacheBeforeReturningAssemblyContextToPool ()
    {
      var assemblyContext = CreateAssemblyContext();
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();
      var otherAdditionalTypeID = new object();
      var otherAdditionalType = ReflectionObjectMother.GetSomeType();

      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (assemblyContext);

      _typeAssemblerMock
          .Expect (mock => mock.AssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Return (
              new TypeAssemblyResult (additionalType, new Dictionary<object, Type> { { otherAdditionalTypeID, otherAdditionalType } }));

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (_additionalTypes[otherAdditionalTypeID].Value, Is.SameAs (otherAdditionalType)));

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
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

      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (assemblyContext);

      _typeAssemblerMock
          .Expect (mock => mock.AssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Return (new TypeAssemblyResult (additionalType, new Dictionary<object, Type> { { otherAdditionalTypeID, otherAdditionalType } }));

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (mi => Assert.That (_additionalTypes[otherAdditionalTypeID].Value, Is.SameAs (otherAdditionalType)));

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _assemblyContextPoolMock.VerifyAllExpectations();
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

      _assemblyContextPoolMock.Expect (mock => mock.Dequeue()).Return (assemblyContext);

      _typeAssemblerMock
          .Expect (mock => mock.AssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Return (
              new TypeAssemblyResult (
                  additionalType,
                  new Dictionary<object, Type> { { additionalTypeID, additionalType }, { otherAdditionalTypeID, otherAdditionalType } }));

      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContext));

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType));
      Assert.That (_additionalTypes[additionalTypeID].IsValueCreated, Is.True);
      Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalType));
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());
    }
  }
}