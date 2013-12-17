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
using System.Runtime.InteropServices;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class CodeManagerTest
  {
    private ITypeCache _typeCacheMock;
    private ITypeAssembler _typeAssemblerMock;
    private IAssemblyContextPool _assemblyContextPool;

    private CodeManager _manager;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _assemblyContextPool = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _manager = new CodeManager (_typeCacheMock, _typeAssemblerMock, _assemblyContextPool);
    }

    [Test]
    public void FlushCodeToDisk_FlushesMultipleAssemblies_ReturnsNonNullResultPaths ()
    {
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();

      var generatedCodeFlusherMock1 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext1 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock1);
      var participantState1 = assemblyContext1.ParticipantState;

      var generatedCodeFlusherMock2 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext2 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock2);
      var participantState2 = assemblyContext2.ParticipantState;

      var generatedCodeFlusherMock3 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext3 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock3);
      var participantState3 = assemblyContext3.ParticipantState;

      bool isDequeued = false;
      _assemblyContextPool
          .Expect (mock => mock.DequeueAll())
          .Return (new[] { assemblyContext1, assemblyContext2, assemblyContext3 })
          .WhenCalled (mi => { isDequeued = true; });

      bool isFlushed1 = false;
      generatedCodeFlusherMock1
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return ("path1")
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isFlushed1 = true;
              });

      bool isFlushed2 = false;
      generatedCodeFlusherMock2
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return (null)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isFlushed2 = true;
              });

      bool isFlushed3 = false;
      generatedCodeFlusherMock3
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return ("path3")
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                isFlushed3 = true;
              });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext1))
          .WhenCalled (
              mi =>
              {
                Assert.That (isFlushed1, Is.True);
                Assert.That (assemblyContext1.ParticipantState, Is.Not.SameAs (participantState1));
              });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext2))
          .WhenCalled (
              mi =>
              {
                Assert.That (isFlushed2, Is.True);
                Assert.That (assemblyContext2.ParticipantState, Is.Not.SameAs (participantState2));
              });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext3))
          .WhenCalled (
              mi =>
              {
                Assert.That (isFlushed3, Is.True);
                Assert.That (assemblyContext3.ParticipantState, Is.Not.SameAs (participantState3));
              });

      var result = _manager.FlushCodeToDisk (new[] { assemblyAttribute });

      _assemblyContextPool.VerifyAllExpectations();
      generatedCodeFlusherMock1.VerifyAllExpectations();
      generatedCodeFlusherMock2.VerifyAllExpectations();
      generatedCodeFlusherMock3.VerifyAllExpectations();

      Assert.That (result, Is.EquivalentTo (new[] { "path1", "path3" }));
    }

    [Test]
    public void FlushCodeToDisk_WithException_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();

      var generatedCodeFlusherMock1 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext1 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock1);

      var generatedCodeFlusherMock2 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext2 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock2);

      var generatedCodeFlusherMock3 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext3 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock3);

      _assemblyContextPool
          .Expect (mock => mock.DequeueAll())
          .Return (new[] { assemblyContext1, assemblyContext2, assemblyContext3 });

      generatedCodeFlusherMock1
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return ("path1");

      generatedCodeFlusherMock2
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Throw (expectedException);

      generatedCodeFlusherMock3
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return ("path3");

      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext1));
      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext2));
      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext3));

      var aggregateException = Assert.Throws<AggregateException> (() => _manager.FlushCodeToDisk (new[] { assemblyAttribute }));
      Assert.That (aggregateException.InnerExceptions, Is.EquivalentTo (new[] { expectedException }));

      _assemblyContextPool.VerifyAllExpectations();
      generatedCodeFlusherMock1.VerifyAllExpectations();
      generatedCodeFlusherMock2.VerifyAllExpectations();
      generatedCodeFlusherMock3.VerifyAllExpectations();
    }

    [Test]
    public void LoadFlushedCode ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var assemblyMock = CreateAssemblyMock ("config", type);
      _typeAssemblerMock.Expect (mock => mock.ParticipantConfigurationID).Return ("config");
      _typeCacheMock.Expect (mock => mock.LoadTypes (new[] { type }));

      _manager.LoadFlushedCode (assemblyMock);

      assemblyMock.VerifyAllExpectations();
      _typeCacheMock.VerifyAllExpectations();
      _typeAssemblerMock.VerifyAllExpectations();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified assembly was not generated by the pipeline.\r\nParameter name: assembly")]
    public void LoadFlushedCode_MissingTypePipeAssemblyAttribute ()
    {
      _manager.LoadFlushedCode (GetType().Assembly);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified assembly was generated with a different participant configuration: 'different config'.\r\nParameter name: assembly")]
    public void LoadFlushedCode_InvalidParticipantConfigurationID ()
    {
      _typeAssemblerMock.Stub (stub => stub.ParticipantConfigurationID).Return ("config");
      var assemblyMock = CreateAssemblyMock ("different config");

      _manager.LoadFlushedCode (assemblyMock);
    }

    private _Assembly CreateAssemblyMock (string participantConfigurationID, params Type[] types)
    {
      var assemblyMock = MockRepository.GenerateStrictMock<_Assembly>();
      var assemblyAttribute = new TypePipeAssemblyAttribute (participantConfigurationID);
      assemblyMock.Expect (mock => mock.GetCustomAttributes (typeof (TypePipeAssemblyAttribute), false)).Return (new object[] { assemblyAttribute });
      assemblyMock.Expect (mock => mock.GetTypes()).Return (types);

      return assemblyMock;
    }
  }
}