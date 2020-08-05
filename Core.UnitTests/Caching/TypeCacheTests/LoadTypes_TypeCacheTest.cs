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
using Moq;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class LoadTypes_TypeCacheTest
  {
    private Mock<ITypeAssembler> _typeAssemblerMock;
    private Mock<IAssemblyContextPool> _assemblyContextPoolMock;

    private TypeCache _cache;

    private IDictionary<AssembledTypeID, Lazy<Type>> _assembledTypes;
    private IDictionary<object, Lazy<Type>> _additionalTypes;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);
      _assemblyContextPoolMock = new Mock<IAssemblyContextPool> (MockBehavior.Strict);

      _cache = new TypeCache (_typeAssemblerMock.Object, _assemblyContextPoolMock.Object);

      _assembledTypes = (IDictionary<AssembledTypeID, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_assembledTypes");
      _additionalTypes = (IDictionary<object, Lazy<Type>>) PrivateInvoke.GetNonPublicField (_cache, "_additionalTypes");
    }

    [Test]
    public void LoadTypes_PopulatesAssembledTypeCache ()
    {
      var assembledType = typeof (AssembledType);
      _typeAssemblerMock.Setup (mock => mock.IsAssembledType (assembledType)).Returns (true).Verifiable();

      var assembledTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Setup (mock => mock.ExtractTypeID (assembledType)).Returns (assembledTypeID).Verifiable();

      _assemblyContextPoolMock.Setup (stub => stub.DequeueAll()).Returns (new[] { CreateAssemblyContext() });
      _assemblyContextPoolMock.Setup (stub => stub.Enqueue (It.IsAny<AssemblyContext>()));

      _cache.LoadTypes (new[] { assembledType });

      Assert.That (_assembledTypes[assembledTypeID].IsValueCreated, Is.False);
      Assert.That (_assembledTypes[assembledTypeID].Value, Is.SameAs (assembledType));

      _typeAssemblerMock.Verify();
    }

    [Test]
    public void LoadTypes_PopulatesAdditionalTypeCache ()
    {
      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Setup (mock => mock.IsAssembledType (additionalGeneratedType)).Returns (false).Verifiable();

      object additionalTypeID = new object();
      _typeAssemblerMock.Setup (mock => mock.GetAdditionalTypeID (additionalGeneratedType)).Returns (additionalTypeID).Verifiable();

      _assemblyContextPoolMock.Setup (stub => stub.DequeueAll()).Returns (new[] { CreateAssemblyContext() });
      _assemblyContextPoolMock.Setup (stub => stub.Enqueue (It.IsAny<AssemblyContext>()));

      _cache.LoadTypes (new[] { additionalGeneratedType });

      Assert.That (_additionalTypes[additionalTypeID].IsValueCreated, Is.False);
      Assert.That (_additionalTypes[additionalTypeID].Value, Is.SameAs (additionalGeneratedType));

      _typeAssemblerMock.Verify();
    }

    [Test]
    public void LoadTypes_SkipsAdditionalTypesWithoutID ()
    {
      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Setup (mock => mock.IsAssembledType (additionalGeneratedType)).Returns (false).Verifiable();

      object additionalTypeID = new object();
      _typeAssemblerMock.Setup (mock => mock.GetAdditionalTypeID (additionalGeneratedType)).Returns (null).Verifiable();

      _assemblyContextPoolMock.Setup (stub => stub.DequeueAll()).Returns (new[] { CreateAssemblyContext() });
      _assemblyContextPoolMock.Setup (stub => stub.Enqueue (It.IsAny<AssemblyContext>()));

      _cache.LoadTypes (new[] { additionalGeneratedType });

      Assert.That (_additionalTypes.ContainsKey (additionalTypeID), Is.False);

      _typeAssemblerMock.Verify();
    }

    [Test]
    public void LoadTypes_DequeuesAllAssemblyContextDuringLoad ()
    {
      var assembledType = typeof (AssembledType);
      _typeAssemblerMock.Setup (stub => stub.IsAssembledType (assembledType)).Returns (true);

      var assembledTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Setup (stub => stub.ExtractTypeID (assembledType)).Returns (assembledTypeID);

      var additionalGeneratedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Setup (stub => stub.IsAssembledType (additionalGeneratedType)).Returns (false);

      object additionalTypeID = new object();
      _typeAssemblerMock.Setup (stub => stub.GetAdditionalTypeID (additionalGeneratedType)).Returns (additionalTypeID);

      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      var isDequeued0 = false;
      var isDequeued1 = false;
      _assemblyContextPoolMock
          .Setup (mock => mock.DequeueAll())
          .Returns (assemblyContexts)
          .Callback (
              () =>
              {
                isDequeued0 = true;
                isDequeued1 = true;
              })
          .Verifiable();

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContexts[0]))
          .Callback (
              (AssemblyContext _) =>
              {
                Assert.That (isDequeued0, Is.True);
                isDequeued0 = false;
              })
          .Verifiable();

      _assemblyContextPoolMock
          .Setup (mock => mock.Enqueue (assemblyContexts[1]))
          .Callback (
              (AssemblyContext _) =>
              {
                Assert.That (isDequeued0, Is.False);
                Assert.That (isDequeued1, Is.True);
                isDequeued1 = false;
                Assert.That (_assembledTypes.ContainsKey (assembledTypeID), Is.True);
                Assert.That (_additionalTypes.ContainsKey (additionalTypeID), Is.True);
              })
          .Verifiable();

      _cache.LoadTypes (new[] { assembledType, additionalGeneratedType });

      _assemblyContextPoolMock.Verify();
    }

    [Test]
    public void LoadTypes_AndExceptionDuringLoad_ReturnsAssemblyContextToPool ()
    {
      var assembledType = typeof (AssembledType);
      _typeAssemblerMock.Setup (stub => stub.IsAssembledType (assembledType)).Returns (true);

      var expectedException = new Exception();
      _typeAssemblerMock.Setup (stub => stub.ExtractTypeID (assembledType)).Throws (expectedException);

      var assemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };

      _assemblyContextPoolMock.Setup (mock => mock.DequeueAll()).Returns (assemblyContexts).Verifiable();
      _assemblyContextPoolMock.Setup (mock => mock.Enqueue (assemblyContexts[0])).Verifiable();
      _assemblyContextPoolMock.Setup (mock => mock.Enqueue (assemblyContexts[1])).Verifiable();

      var aggregateException = Assert.Throws<AggregateException> (() => _cache.LoadTypes (new[] { assembledType }));
      Assert.That (aggregateException.InnerExceptions, Is.EquivalentTo (new[] { expectedException }));

      _assemblyContextPoolMock.Verify();
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object,
          new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict).Object);
    }

    private class AssembledType {}
  }
}