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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Caching.TypeCacheTests
{
  [TestFixture]
  public class GetOrCreateAdditionalType_TypeCacheTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private IAssemblyContextPool _assemblyContextPoolMock;

    private TypeCache _cache;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _assemblyContextPoolMock = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _cache = new TypeCache (_typeAssemblerMock, _assemblyContextPoolMock);
    }

    [Test]
    public void CalledOnce_UsesAssemblyContextFromPool ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPoolMock
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (
              mock => mock.AssembleAdditionalType (
                  additionalTypeID,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator))
          .Return (additionalType)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      var result = _cache.GetOrCreateAdditionalType (additionalTypeID);

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void WithExceptionDuringAssembleAdditionalType_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var additionalTypeID = new object();

      var assemblyContext = new AssemblyContext (
          MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(),
          MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>());

      bool isDequeued = false;
      _assemblyContextPoolMock
          .Expect (mock => mock.Dequeue())
          .Return (assemblyContext)
          .WhenCalled (mi => { isDequeued = true; });

      _typeAssemblerMock
          .Expect (mock => mock.AssembleAdditionalType (null, null, null))
          .IgnoreArguments()
          .Throw (expectedException)
          .WhenCalled (mi => Assert.That (isDequeued, Is.True));

      _assemblyContextPoolMock
          .Expect (mock => mock.Enqueue (assemblyContext))
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isDequeued = false;
              });

      Assert.That (
          () => _cache.GetOrCreateAdditionalType (additionalTypeID),
          Throws.Exception.SameAs (expectedException));

      _typeAssemblerMock.VerifyAllExpectations();
      _assemblyContextPoolMock.VerifyAllExpectations();
    }
  }
}