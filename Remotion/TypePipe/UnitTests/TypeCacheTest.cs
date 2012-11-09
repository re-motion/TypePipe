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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class TypeCacheTest
  {
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
    public void GetOrCreateType_EqualCompoundKey ()
    {
      var requestedType = SetupCacheHitExpectations (_typeAssemblerMock, _generatedType1);

      var result1 = _cache.GetOrCreateType (requestedType);
      var result2 = _cache.GetOrCreateType (requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result1, Is.SameAs (result2));
      Assert.That (result1, Is.SameAs (_generatedType1));
    }

    [Test]
    public void GetOrCreateType_NonEqualCompoundKey ()
    {
      var requestedType = SetupCachMissExpectations (_typeAssemblerMock, _generatedType1, _generatedType2);
      
      var result1 = _cache.GetOrCreateType (requestedType);
      var result2 = _cache.GetOrCreateType (requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1, Is.SameAs (_generatedType1));
      Assert.That (result2, Is.SameAs (_generatedType2));
    }

    [Test]
    public void GetOrCreateConstructorLookup_EqualCompoundKey ()
    {
      var requestedType = SetupCacheHitExpectations (_typeAssemblerMock, _generatedType1);

      var result1 = _cache.GetOrCreateConstructorLookup (requestedType);
      var result2 = _cache.GetOrCreateConstructorLookup (requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result1, Is.SameAs (result2));
      Assert.That (result1.DefiningType, Is.SameAs (_generatedType1));
    }

    [Test]
    public void GetOrCreateConstructorLookup_NonEqualCompoundKey ()
    {
      var requestedType = SetupCachMissExpectations (_typeAssemblerMock, _generatedType1, _generatedType2);

      var result1 = _cache.GetOrCreateConstructorLookup (requestedType);
      var result2 = _cache.GetOrCreateConstructorLookup (requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1.DefiningType, Is.SameAs (_generatedType1));
      Assert.That (result2.DefiningType, Is.SameAs (_generatedType2));
    }

    private Type SetupCacheHitExpectations (ITypeAssembler typeAssemblerMock, Type generatedType1)
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var key1 = new object[] { "key" };
      var key2 = new object[] { "key" };

      typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (requestedType)).Return (key1).Repeat.Once();
      typeAssemblerMock.Expect (mock => mock.AssembleType (requestedType)).Return (generatedType1);
      typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (requestedType)).Return (key2);

      return requestedType;
    }

    private Type SetupCachMissExpectations (ITypeAssembler typeAssemblerMock, Type generatedType1, Type generatedType2)
    {
      var requestedType = ReflectionObjectMother.GetSomeType ();
      var key1 = new object[] { "key" };
      var key2 = new object[] { "other key" };

      typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (requestedType)).Return (key1).Repeat.Once ();
      typeAssemblerMock.Expect (mock => mock.AssembleType (requestedType)).Return (generatedType1);
      typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (requestedType)).Return (key2);
      typeAssemblerMock.Expect (mock => mock.AssembleType (requestedType)).Return (generatedType2);

      return requestedType;
    }

    private class GeneratedType1 { }
    private class GeneratedType2 { }
  }
}