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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class LoadTypes_TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IAssemblyContextPool _assemblyContextPoolMock;

    private TypeCache _cache;

    private IDictionary<AssembledTypeID, Lazy<Type>> _assembledTypes;
    private IDictionary<object, Lazy<Type>> _additionalTypes;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _assemblyContextPoolMock = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _cache = new TypeCache (_typeAssemblerMock, _assemblyContextPoolMock);

      _assembledTypes = (IDictionary<AssembledTypeID, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_assembledTypes");
      _additionalTypes = (IDictionary<object, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_additionalTypes");
    }

    [Test]
    public void LoadTypes_PopulatesAssembledTypeCache ()
    {
      var assembledType = typeof (AssembledType);
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (assembledType)).Return (true);

      var assembledTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ExtractTypeID (assembledType)).Return (assembledTypeID);

      _assemblyContextPoolMock.Stub (stub => stub.DequeueAll()).Return (new[] { CreateAssemblyContext() });
      _assemblyContextPoolMock.Stub (stub => stub.Enqueue (null)).IgnoreArguments();

      _cache.LoadTypes (new[] { assembledType });

      Assert.That (_assembledTypes[assembledTypeID].IsValueCreated, Is.False);
      Assert.That (_assembledTypes[assembledTypeID].Value, Is.SameAs (assembledType));

      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    public void LoadTypes_PopulatesAdditionalTypeCache ()
    {
      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (additionalGeneratedType)).Return (false);

      object additionalTypeID = new object();
      _typeAssemblerMock.Expect (mock => mock.GetAdditionalTypeID (additionalGeneratedType)).Return (additionalTypeID);

      _assemblyContextPoolMock.Stub (stub => stub.DequeueAll()).Return (new[] { CreateAssemblyContext() });
      _assemblyContextPoolMock.Stub (stub => stub.Enqueue (null)).IgnoreArguments();

      _cache.LoadTypes (new[] { additionalGeneratedType });

      Assert.That (_additionalTypes[additionalTypeID].IsValueCreated, Is.False);
      Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalGeneratedType));

      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    public void LoadTypes_SkipsAdditionalTypesWithoutID ()
    {
      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (additionalGeneratedType)).Return (false);

      object additionalTypeID = new object();
      _typeAssemblerMock.Expect (mock => mock.GetAdditionalTypeID (additionalGeneratedType)).Return (null);

      _assemblyContextPoolMock.Stub (stub => stub.DequeueAll()).Return (new[] { CreateAssemblyContext() });
      _assemblyContextPoolMock.Stub (stub => stub.Enqueue (null)).IgnoreArguments();

      _cache.LoadTypes (new[] { additionalGeneratedType });

      Assert.That (_additionalTypes.ContainsKey (additionalTypeID), Is.False);

      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    public void LoadTypes_DequeuesAllAssemblyContextDuringLoad ()
    {
      var assembledType = typeof (AssembledType);
      _typeAssemblerMock.Stub (stub => stub.IsAssembledType (assembledType)).Return (true);

      var assembledTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Stub (stub => stub.ExtractTypeID (assembledType)).Return (assembledTypeID);

      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Stub (stub => stub.IsAssembledType (additionalGeneratedType)).Return (false);

      object additionalTypeID = new object();
      _typeAssemblerMock.Stub (stub => stub.GetAdditionalTypeID (additionalGeneratedType)).Return (additionalTypeID);

      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      bool isDequeued0 = false;
      bool isDequeued1 = false;
      _assemblyContextPoolMock
          .Expect (mock => mock.DequeueAll())
          .Return (assemblyContexts)
          .WhenCalled (
              mi =>
              {
                isDequeued0 = true;
                isDequeued1 = true;
              });

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContexts[0]))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued0, Is.True);
                isDequeued0 = false;
              });

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContexts[1]))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued0, Is.False);
                Assert.That (isDequeued1, Is.True);
                isDequeued1 = false;
                Assert.That (_assembledTypes.ContainsKey (assembledTypeID), Is.True);
                Assert.That (_additionalTypes.ContainsKey (additionalTypeID), Is.True);
              });

      _cache.LoadTypes (new[] { assembledType, additionalGeneratedType });

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    [Test]
    public void LoadTypes_AndExceptionDuringLoad_ReturnsAssemblyContextToPool ()
    {
      var assembledType = typeof (AssembledType);
      _typeAssemblerMock.Stub (stub => stub.IsAssembledType (assembledType)).Return (true);

      var expectedException = new Exception();
      _typeAssemblerMock.Stub (stub => stub.ExtractTypeID (assembledType)).Throw (expectedException);

      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      _assemblyContextPoolMock.Expect (mock => mock.DequeueAll()).Return (assemblyContexts);
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContexts[0]));
      _assemblyContextPoolMock.Expect (mock => mock.Enqueue (assemblyContexts[1]));

      var aggregateException = Assert.Throws<AggregateException> (() => _cache.LoadTypes (new[] { assembledType }));
      Assert.That (aggregateException.InnerExceptions, Is.EquivalentTo (new[] { expectedException }));

      _assemblyContextPoolMock.VerifyAllExpectations();
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());
    }

    private class AssembledType {}
  }
}