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
using Remotion.TypePipe;
using Remotion.Utilities;

namespace TypePipe.IntegrationTests
{
  [Ignore ("TODO 5163")]
  [TestFixture]
  public class CachingTest
  {
    private Type _type1;
    private Type _type2;

    [SetUp]
    public void SetUp ()
    {
      _type1 = typeof (DomainType1);
      _type2 = typeof (DomainType2);
    }

    [Test]
    public void SameType_EqualCacheKey ()
    {
      var pipeline = CreatePipeline (t => new DummyCacheKey ("a"), t => new DummyCacheKey ("b"));

      var instance1 = pipeline.CreateInstance (_type1);
      var instance2 = pipeline.CreateInstance (_type1);

      Assert.That (instance1.GetType(), Is.SameAs (instance2.GetType()));
    }

    [Test]
    public void SameType_NullCacheKeyProvider ()
    {
      var pipeline = CreatePipeline (t => new DummyCacheKey ("a"), null);

      var instance1 = pipeline.CreateInstance (_type1);
      var instance2 = pipeline.CreateInstance (_type1);

      Assert.That (instance1.GetType(), Is.SameAs (instance2.GetType()));
    }

    [Test]
    public void SameType_NonEqualCacheKey ()
    {
      var count = 1;
      Func<Type, CacheKey> cacheKeyProviders = t => new DummyCacheKey ("b" + count++);
      var pipeline = CreatePipeline (t => new DummyCacheKey ("a"), cacheKeyProviders);

      var instance1 = pipeline.CreateInstance (_type1);
      var instance2 = pipeline.CreateInstance (_type1);

      Assert.That (instance1.GetType(), Is.Not.SameAs (instance2.GetType()));
    }

    [Test]
    public void DifferentTypes_EqualCacheKey ()
    {
      var pipeline = CreatePipeline (t => new DummyCacheKey ("a"), t => new DummyCacheKey ("b"));

      var instance1 = pipeline.CreateInstance (_type1);
      var instance2 = pipeline.CreateInstance (_type2);

      Assert.That (instance1.GetType(), Is.Not.SameAs (instance2.GetType()));
    }

    private Pipeline CreatePipeline (params Func<Type, CacheKey>[] cacheKeyProviders)
    {
      //var cacheKeyProviderStubs = cacheKeyProviders.Select (
      //    providerFunc =>
      //    {
      //      if (providerFunc == null)
      //        return null;

      //      var stub = MockRepository.GenerateStub<ICacheKeyProvider>();
      //      stub.Stub (x => x.GetCacheKey (Arg<Type>.Is.Anything)).Do (providerFunc);
      //      return stub;
      //    });

      //var participantStubs = cacheKeyProviderStubs.Select (
      //    cacheKeyProvider =>
      //    {
      //      var stub = MockRepository.GenerateStub<IParticipant>();
      //      stub.Stub (x => x.GetCacheKeyProvider()).Return (cacheKeyProvider);
      //      return stub;
      //    });

      return PipelineObjectMother.CreatePipeline ( /*participantStubs*/);
    }

    private class DummyCacheKey : CacheKey
    {
      private readonly string _backingString;

      public DummyCacheKey (string backingString)
      {
        _backingString = backingString;
      }

      public override bool Equals (object other)
      {
        Assertion.IsTrue (other is DummyCacheKey);
        return _backingString.Equals (((DummyCacheKey) other)._backingString);
      }

      public override int GetHashCode ()
      {
        return _backingString.GetHashCode();
      }
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}