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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class TypeCacheTest
  {
    private readonly Type _generatedType1 = typeof (GeneratedType1);
    private readonly Type _generatedType2 = typeof (GeneratedType2);
    private readonly Delegate _delegate1 = new Func<int> (() => 7);
    private readonly Delegate _delegate2 = new Func<string> (() => "");
    private Type _requestedType;
    private Type _delegateType;
    private Type[] _parameterTypes;
    private bool _allowNonPublic;

    private ITypeAssembler _typeAssemblerMock;
    private IDelegateFactory _delegateFactoryMock;
    
    private TypeCache _cache;

    private Dictionary<object[], Type> _types;
    private Dictionary<object[], Delegate> _constructorCalls;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _delegateFactoryMock = MockRepository.GenerateStrictMock<IDelegateFactory>();

      _cache = new TypeCache (_typeAssemblerMock, _delegateFactoryMock);

      _types = (Dictionary<object[], Type>) PrivateInvoke.GetNonPublicField (_cache, "_types");
      _constructorCalls = (Dictionary<object[], Delegate>) PrivateInvoke.GetNonPublicField (_cache, "_constructorCalls");

      _requestedType = ReflectionObjectMother.GetSomeType();
      _delegateType = ReflectionObjectMother.GetSomeDelegateType();
      _parameterTypes = new[] { ReflectionObjectMother.GetSomeType(), ReflectionObjectMother.GetSomeType() };
      _allowNonPublic = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void GetOrCreateType_CacheHit ()
    {
      _types.Add (new object[] { "key" }, _generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType, 0)).Return (new object[] { "key" });

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_generatedType1));
    }

    [Test]
    public void GetOrCreateType_CacheMiss ()
    {
      _types.Add (new object[] { "key" }, _generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType, 0)).Return (new object[] { "other key" });
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType)).Return (_generatedType2);

      var result = _cache.GetOrCreateType (_requestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_generatedType2));
      Assert.That (_types[new object[] { "other key" }], Is.SameAs (_generatedType2));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheHit ()
    {
      _constructorCalls.Add (new object[] { _delegateType, _allowNonPublic, "key" }, _delegate1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType, 2)).Return (new object[] { null, null, "key" });

      var result = _cache.GetOrCreateConstructorCall (_requestedType, _parameterTypes, _allowNonPublic, _delegateType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_delegate1));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMissDelegates_CacheHitTypes ()
    {
      _constructorCalls.Add (new object[] { _delegateType, _allowNonPublic, "key" }, _delegate1);
      _types.Add (new object[] { "type key" }, _generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType, 2)).Return (new object[] { null, null, "type key" });
      _delegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (_generatedType1, _parameterTypes, _allowNonPublic, _delegateType))
          .Return (_delegate2);

      var result = _cache.GetOrCreateConstructorCall (_requestedType, _parameterTypes, _allowNonPublic, _delegateType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_delegate2));
      Assert.That (_constructorCalls[new object[] { _delegateType, _allowNonPublic, "type key" }], Is.SameAs (_delegate2));
    }

    [Test]
    public void GetOrCreateConstructorCall_CacheMissDelegates_CacheMissTypes ()
    {
      _constructorCalls.Add (new object[] { _delegateType, _allowNonPublic, "key" }, _delegate1);
      _types.Add (new object[] { "type key" }, _generatedType1);
      _typeAssemblerMock.Expect (mock => mock.GetCompoundCacheKey (_requestedType, 2)).Return (new object[] { null, null, "other type key" });
      _typeAssemblerMock.Expect (mock => mock.AssembleType (_requestedType)).Return (_generatedType2);
      _delegateFactoryMock
          .Expect (mock => mock.CreateConstructorCall (_generatedType2, _parameterTypes, _allowNonPublic, _delegateType))
          .Return (_delegate2);

      var result = _cache.GetOrCreateConstructorCall (_requestedType, _parameterTypes, _allowNonPublic, _delegateType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_delegate2));
      Assert.That (_types[new object[] { "other type key" }], Is.SameAs (_generatedType2));
      Assert.That (_constructorCalls[new object[] { _delegateType, _allowNonPublic, "other type key" }], Is.SameAs (_delegate2));
    }

    private class GeneratedType1 { }
    private class GeneratedType2 { }
  }
}