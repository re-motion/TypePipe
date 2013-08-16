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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation.Synchronization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class ReverseTypeCacheTest
  {
    private IReverseTypeCacheSynchronizationPoint _reverseTypeCacheSynchronizationPointMock;

    private ReverseTypeCache _cache;

    private IDictionary<ReverseConstructionKey, Delegate> _constructorCalls;

    private Type _assembledType;
    private Type _delegateType;
    private bool _allowNonPublic;
    private Delegate _generatedCtorCall;

    [SetUp]
    public void SetUp ()
    {
      _reverseTypeCacheSynchronizationPointMock = MockRepository.GenerateStrictMock<IReverseTypeCacheSynchronizationPoint>();

      _cache = new ReverseTypeCache (_reverseTypeCacheSynchronizationPointMock);

      _constructorCalls = (ConcurrentDictionary<ReverseConstructionKey, Delegate>) PrivateInvoke.GetNonPublicField (_cache, "_constructorCalls");

      _assembledType = ReflectionObjectMother.GetSomeType();
      _delegateType = ReflectionObjectMother.GetSomeDelegateType();
      _allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      _generatedCtorCall = new Func<int> (() => 7);
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheHit ()
    {
      _constructorCalls.Add (new ReverseConstructionKey (_assembledType, _delegateType, _allowNonPublic), _generatedCtorCall);

      var result = _cache.GetOrCreateConstructorCall (_assembledType, _delegateType, _allowNonPublic);

      Assert.That (result, Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss ()
    {
      var reverseConstructionKey = new ReverseConstructionKey (_assembledType, _delegateType, _allowNonPublic);
      _reverseTypeCacheSynchronizationPointMock
          .Expect (
              mock => mock.GetOrGenerateConstructorCall (
                  Arg.Is ((ConcurrentDictionary<ReverseConstructionKey, Delegate>) _constructorCalls),
                  Arg<ReverseConstructionKey>.Matches (key => key.Equals (reverseConstructionKey)))) // Use strongly typed overload.
          .Return (_generatedCtorCall);

      var result = _cache.GetOrCreateConstructorCall (_assembledType, _delegateType, _allowNonPublic);

      _reverseTypeCacheSynchronizationPointMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_generatedCtorCall));
    }
  }
}