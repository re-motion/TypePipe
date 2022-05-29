﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;

namespace Remotion.TypePipe.UnitTests.Caching
{
  [TestFixture]
  public class ConstructorForAssembledTypeCacheTest
  {
    private Mock<ITypeAssembler> _typeAssemblerMock;
    private Mock<IConstructorDelegateFactory> _constructorDelegateFactoryMock;
    
    private ConstructorForAssembledTypeCache _cache;

    private IDictionary<ConstructorForAssembledTypeCacheKey, Delegate> _constructorCalls;

    private Type _assembledType;
    private Type _delegateType;
    private bool _allowNonPublic;
    private Delegate _generatedCtorCall;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);
      _constructorDelegateFactoryMock = new Mock<IConstructorDelegateFactory> (MockBehavior.Strict);

      _cache = new ConstructorForAssembledTypeCache (_typeAssemblerMock.Object, _constructorDelegateFactoryMock.Object);

      _constructorCalls = (ConcurrentDictionary<ConstructorForAssembledTypeCacheKey, Delegate>) PrivateInvoke.GetNonPublicField (_cache, "_constructorCalls");

      _assembledType = ReflectionObjectMother.GetSomeType();
      _delegateType = ReflectionObjectMother.GetSomeDelegateType();
      _allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      _generatedCtorCall = new Func<int> (() => 7);
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheHit ()
    {
      _constructorCalls.Add (new ConstructorForAssembledTypeCacheKey (_assembledType, _delegateType, _allowNonPublic), _generatedCtorCall);

      var result = _cache.GetOrCreateConstructorCall (_assembledType, _delegateType, _allowNonPublic);

      Assert.That (result, Is.SameAs (_generatedCtorCall));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMiss ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      _typeAssemblerMock.Setup (mock => mock.GetRequestedType (_assembledType)).Returns (requestedType).Verifiable();

      _constructorDelegateFactoryMock
          .Setup (mock => mock.CreateConstructorCall (requestedType, _assembledType, _delegateType, _allowNonPublic))
          .Returns (_generatedCtorCall)
          .Verifiable();

      var result = _cache.GetOrCreateConstructorCall (_assembledType, _delegateType, _allowNonPublic);

      _constructorDelegateFactoryMock.Verify();
      Assert.That (result, Is.SameAs (_generatedCtorCall));
      
      var reverseConstructionKey = new ConstructorForAssembledTypeCacheKey (_assembledType, _delegateType, _allowNonPublic);
      Assert.That (_constructorCalls[reverseConstructionKey], Is.SameAs (_generatedCtorCall));
    }
  }
}