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
using System.Linq;
using NUnit.Framework;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests.ObjectFactory
{
  [TestFixture]
  public class CachingTest : ObjectFactoryIntegrationTestBase
  {
    private readonly Type _type1 = typeof (DomainType1);
    private readonly Type _type2 = typeof (DomainType2);

    [Test]
    public void SameType_EqualCacheKey ()
    {
      var pipeline = CreateObjectFactory (t => "a", t => "b");

      var instance1 = pipeline.CreateObject (_type1);
      var instance2 = pipeline.CreateObject (_type1);

      Assert.That (instance1.GetType(), Is.SameAs (instance2.GetType()));
    }

    [Test]
    public void SameType_NullCacheKeyProvider ()
    {
      var pipeline = CreateObjectFactory (t => "a", null);

      var instance1 = pipeline.CreateObject (_type1);
      var instance2 = pipeline.CreateObject (_type1);

      Assert.That (instance1.GetType(), Is.SameAs (instance2.GetType()));
    }

    [Test]
    public void SameType_NonEqualCacheKey ()
    {
      var count = 1;
      var pipeline = CreateObjectFactory (t => "a", t => "b" + count++);

      var instance1 = pipeline.CreateObject (_type1);
      var instance2 = pipeline.CreateObject (_type1);

      Assert.That (instance1.GetType(), Is.Not.SameAs (instance2.GetType()));
    }

    [Test]
    public void DifferentTypes_EqualCacheKey ()
    {
      var factory = CreateObjectFactory (t => "a", t => "b");

      var instance1 = factory.CreateObject (_type1);
      var instance2 = factory.CreateObject (_type2);

      Assert.That (instance1.GetType(), Is.Not.SameAs (instance2.GetType()));
    }

    private IObjectFactory CreateObjectFactory (params Func<Type, object>[] cacheKeyProviders)
    {
      var cacheKeyProviderStubs = cacheKeyProviders.Select (
          providerFunc =>
          {
            if (providerFunc == null)
              return null;

            var stub = MockRepository.GenerateStub<ICacheKeyProvider>();
            stub.Stub (x => x.GetCacheKey (Arg<Type>.Is.Anything)).Do (providerFunc);
            return stub;
          });

      Action<MutableType> typeModification = pt => { };
      var participantStubs = cacheKeyProviderStubs.Select (ckp => CreateParticipant (typeModification, ckp));

      return CreateObjectFactory (participantStubs, stackFramesToSkip: 1);
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}