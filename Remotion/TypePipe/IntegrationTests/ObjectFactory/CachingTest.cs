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
  public class CachingTest : IntegrationTestBase
  {
    private readonly Type _type1 = typeof (DomainType1);
    private readonly Type _type2 = typeof (DomainType2);

    [Test]
    public void SameType_EqualCacheKey ()
    {
      var factory = CreateObjectFactory (t => "a", t => "b");

      var type1 = factory.GetAssembledType (_type1);
      var type2 = factory.GetAssembledType (_type1);

      Assert.That (type1, Is.SameAs (type2));
    }

    [Test]
    public void SameType_NullCacheKeyProvider ()
    {
      var factory = CreateObjectFactory (t => "a", null);

      var type1 = factory.GetAssembledType (_type1);
      var type2 = factory.GetAssembledType (_type1);

      Assert.That (type1, Is.SameAs (type2));
    }

    [Test]
    public void SameType_NonEqualCacheKey ()
    {
      var count = 1;
      var factory = CreateObjectFactory (t => "a", t => "b" + count++);

      var type1 = factory.GetAssembledType (_type1);
      var type2 = factory.GetAssembledType (_type1);

      Assert.That (type1, Is.Not.SameAs (type2));
    }

    [Test]
    public void DifferentTypes_EqualCacheKey ()
    {
      var factory = CreateObjectFactory (t => "a", t => "b");

      var type1 = factory.GetAssembledType (_type1);
      var type2 = factory.GetAssembledType (_type2);

      Assert.That (type1, Is.Not.SameAs (type2));
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
      var participantStubs = cacheKeyProviderStubs.Select (ckp => CreateParticipant (typeModification, ckp)).ToArray();

      return CreateObjectFactory (participantStubs);
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}