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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class CachingTest : IntegrationTestBase
  {
    private readonly Type _type1 = typeof (DomainType1);
    private readonly Type _type2 = typeof (DomainType2);

    [Test]
    public void SameType_EqualCacheKey ()
    {
      var reflectionService = CreateReflectionService (t => "a", t => "b");

      var type1 = reflectionService.GetAssembledType (_type1);
      var type2 = reflectionService.GetAssembledType (_type1);

      Assert.That (type1, Is.SameAs (type2));
    }

    [Test]
    public void SameType_NullCacheKeyProvider ()
    {
      var reflectionService = CreateReflectionService (t => "a", null);

      var type1 = reflectionService.GetAssembledType (_type1);
      var type2 = reflectionService.GetAssembledType (_type1);

      Assert.That (type1, Is.SameAs (type2));
    }

    [Test]
    public void SameType_NonEqualCacheKey ()
    {
      var count = 1;
      var reflectionService = CreateReflectionService (t => "a", t => "b" + count++);

      var type1 = reflectionService.GetAssembledType (_type1);
      var type2 = reflectionService.GetAssembledType (_type1);

      Assert.That (type1, Is.Not.SameAs (type2));
    }

    [Test]
    public void DifferentTypes_EqualCacheKey ()
    {
      var reflectionService = CreateReflectionService (t => "a", t => "b");

      var type1 = reflectionService.GetAssembledType (_type1);
      var type2 = reflectionService.GetAssembledType (_type2);

      Assert.That (type1, Is.Not.SameAs (type2));
    }

    private IReflectionService CreateReflectionService (params Func<Type, object>[] cacheKeyProviders)
    {
      var cacheKeyProviderStubs = cacheKeyProviders.Select (
          providerFunc =>
          {
            if (providerFunc == null)
              return null;

            var stub = MockRepository.GenerateStub<ITypeIdentifierProvider>();
            stub.Stub (x => x.GetID (Arg<Type>.Is.Anything)).Do (providerFunc);
            stub.Stub (x => x.GetExpression (Arg<Type>.Is.Anything))
                .Return (null)
                .WhenCalled (mi => mi.ReturnValue = Expression.Constant (mi.Arguments[0]));
            return stub;
          });

      var participantStubs = cacheKeyProviderStubs.Select (typeIdProvider => CreateParticipant (typeIdentifierProvider: typeIdProvider));

      return CreatePipeline (participantStubs.ToArray()).ReflectionService;
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}