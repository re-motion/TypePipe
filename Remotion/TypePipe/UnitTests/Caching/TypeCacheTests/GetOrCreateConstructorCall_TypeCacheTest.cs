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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateConstructorCall_TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IConstructorDelegateFactory _constructorDelegateFactoryMock;
    private IAssemblyContextPool _assemblyContextPoolMock;

    private TypeCache _cache;

    private IDictionary<AssembledTypeID, Lazy<Type>> _types;
    private IDictionary<ConstructionKey, Delegate> _constructorCalls;

    private readonly Type _assembledType = typeof (AssembledType);
    private readonly Delegate _generatedCtorCall = new Func<int> (() => 7);
    private Type _delegateType;
    private bool _allowNonPublic;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _constructorDelegateFactoryMock = MockRepository.GenerateStrictMock<IConstructorDelegateFactory>();
      _assemblyContextPoolMock = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _cache = new TypeCache (_typeAssemblerMock, _constructorDelegateFactoryMock, _assemblyContextPoolMock);

      _types = (ConcurrentDictionary<AssembledTypeID, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_types");
      _constructorCalls = (ConcurrentDictionary<ConstructionKey, Delegate>) PrivateInvoke.GetNonPublicField (_cache, "_constructorCalls");

      _delegateType = ReflectionObjectMother.GetSomeDelegateType();
      _allowNonPublic = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void CacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _constructorCalls.Add (new ConstructionKey (typeID, _delegateType, _allowNonPublic), _generatedCtorCall);

      var result = _cache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void CacheMiss_WithAssembledTypeCacheHit ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();

      _types.Add (typeID, new Lazy<Type> (() => _assembledType, LazyThreadSafetyMode.None));

      _constructorDelegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (typeID.RequestedType, _assembledType, _delegateType, _allowNonPublic))
          .Return (_generatedCtorCall);

      var result = _cache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic);

      _typeAssemblerMock.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (_generatedCtorCall));

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[key], Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void CacheMiss_WithAssembledTypeCacheMiss ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPoolMock
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

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      _constructorDelegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (typeID.RequestedType, _assembledType, _delegateType, _allowNonPublic))
          .Return (_generatedCtorCall)
          .WhenCalled (mi => Assert.That (isDequeued, Is.False));

      var result = _cache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();
      
      Assert.That (result, Is.SameAs (_generatedCtorCall));

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[key], Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void CacheMiss_WithAssembledTypeCacheMiss_AndExceptionDuringAssembleType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var typeID = AssembledTypeIDObjectMother.Create();

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPoolMock
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (mock => mock.AssembleType (new AssembledTypeID(), null, null))
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
          () => _cache.GetOrCreateConstructorCall (typeID, _delegateType, _allowNonPublic),
          Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
      _constructorDelegateFactoryMock.VerifyAllExpectations();

      var key = new ConstructionKey (typeID, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls.ContainsKey(key), Is.False);
    }

    private class AssembledType {}
  }
}