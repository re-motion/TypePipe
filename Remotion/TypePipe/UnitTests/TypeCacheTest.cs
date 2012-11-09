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
using Remotion.FunctionalProgramming;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests
{
  [Ignore("TODO 5163")]
  [TestFixture]
  public class TypeCacheTest
  {
    private readonly Type _requestedType1 = typeof (RequestedType1);
    private readonly Type _requestedType2 = typeof (RequestedType2);
    private readonly Type _generatedType1 = typeof (GeneratedType1);
    private readonly Type _generatedType2 = typeof (GeneratedType2);

    private ITypeAssembler _typeAssemblerMock;
    
    private TypeCache _cache;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();

      _cache = new TypeCache (_typeAssemblerMock);
    }

    [Test]
    public void GetOrGenerate_EqualCompoundKey ()
    {
      var key1 = CreateCompoundCacheKey (_requestedType1, "a");
      var key2 = CreateCompoundCacheKey (_requestedType1, "a");
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType1)).Return (key1);
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType1)).Return (_generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType1)).Return (key2);

      var result1 = _cache.GetOrCreate (_requestedType1);
      var result2 = _cache.GetOrCreate (_requestedType1);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result1, Is.SameAs (result2));
      Assert.That (result1, Is.SameAs (_generatedType1));
    }

    [Test]
    public void GetOrGenerate_NonEqualCompoundKey ()
    {
      var key1 = CreateCompoundCacheKey (_requestedType1, "a");
      var key2 = CreateCompoundCacheKey (_requestedType1, "b");
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType1)).Return (key1);
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType1)).Return (_generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType1)).Return (key2);
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType1)).Return (_generatedType2);

      var result1 = _cache.GetOrCreate (_requestedType1);
      var result2 = _cache.GetOrCreate (_requestedType1);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1, Is.SameAs (_generatedType1));
      Assert.That (result2, Is.SameAs (_generatedType2));
    }

    [Test]
    public void GetOrGenerate_DifferentRequestedTypes ()
    {
      var key1 = CreateCompoundCacheKey (_requestedType1, "a");
      var key2 = CreateCompoundCacheKey (_requestedType2, "a");
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType1)).Return (key1);
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType1)).Return (_generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType2)).Return (key2);
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType2)).Return (_generatedType2);

      var result1 = _cache.GetOrCreate (_requestedType1);
      var result2 = _cache.GetOrCreate (_requestedType1);

      _typeAssemblerMock.VerifyAllExpectations ();
      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1, Is.SameAs (_generatedType1));
      Assert.That (result2, Is.SameAs (_generatedType2));
    }

    private object[] CreateCompoundCacheKey (Type requestedType, params object[] cacheKeyParts)
    {
      return EnumerableUtility.Singleton<object> (requestedType).Concat (cacheKeyParts).ToArray();
    }

    private class RequestedType1 { }
    private class RequestedType2 { }
    private class GeneratedType1 { }
    private class GeneratedType2 { }
  }
}