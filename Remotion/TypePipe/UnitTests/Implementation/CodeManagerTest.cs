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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class CodeManagerTest
  {
    private ITypeCache _typeCacheMock;
    private IAssemblyContextPool _assemblyContextPool;

    private CodeManager _manager;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _assemblyContextPool = MockRepository.GenerateStrictMock<IAssemblyContextPool>();

      _manager = new CodeManager (_typeCacheMock, _assemblyContextPool);
    }

    [Test]
    public void FlushCodeToDisk_FlushesMultipleAssemblies_ReturnsNonNullResultPaths ()
    {
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();
      var configID = "config";
      _typeCacheMock.Expect (mock => mock.ParticipantConfigurationID).Return (configID);

      var generatedCodeFlusherMock1 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext1 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock1);

      var generatedCodeFlusherMock2 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext2 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock2);
      
      var generatedCodeFlusherMock3 = MockRepository.GenerateStrictMock<IGeneratedCodeFlusher>();
      var assemblyContext3 = new AssemblyContext (MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>(), generatedCodeFlusherMock3);

      bool isDequeued = false;
      bool isFlushed = false;
      _assemblyContextPool
          .Expect (mock => mock.DequeueAll())
          .Return (new[] { assemblyContext1, assemblyContext2, assemblyContext3 })
          .WhenCalled (mi => { isDequeued = true; });

      generatedCodeFlusherMock1
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return ("path1")
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                Assert.That (isFlushed, Is.False);
              });

      generatedCodeFlusherMock2
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return (null)
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                Assert.That (isFlushed, Is.False);
              });

      generatedCodeFlusherMock3
          .Expect (mock => mock.FlushCodeToDisk (Arg<IEnumerable<CustomAttributeDeclaration>>.Is.Anything))
          .Return ("path3")
          .WhenCalled (
              mi =>
              {
                Assert.That (isDequeued, Is.True);
                Assert.That (isFlushed, Is.False);
                isFlushed = true;
              });

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext1))
          .WhenCalled (mi => Assert.That (isFlushed, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext2))
          .WhenCalled (mi => Assert.That (isFlushed, Is.True));

      _assemblyContextPool
          .Expect (mock => mock.Enqueue (assemblyContext3))
          .WhenCalled (mi => Assert.That (isFlushed, Is.True));

      var result = _manager.FlushCodeToDisk (new[] { assemblyAttribute });

      _typeCacheMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      generatedCodeFlusherMock1.VerifyAllExpectations();
      generatedCodeFlusherMock2.VerifyAllExpectations();
      generatedCodeFlusherMock3.VerifyAllExpectations();

      Assert.That (result, Is.EqualTo (new[] { "path1", "path3" }));
    }
    
    [Test]
    public void FlushCodeToDisk_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();
      var configID = "config";
      _typeCacheMock.Expect (mock => mock.ParticipantConfigurationID).Return (configID);

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
          .Repeat.Never();

      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext1));
      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext2));
      _assemblyContextPool.Expect (mock => mock.Enqueue (assemblyContext3));

      Assert.That (() => _manager.FlushCodeToDisk (new[] { assemblyAttribute }), Throws.Exception.SameAs (expectedException));

      _typeCacheMock.VerifyAllExpectations();
      _assemblyContextPool.VerifyAllExpectations();
      generatedCodeFlusherMock1.VerifyAllExpectations();
      generatedCodeFlusherMock2.VerifyAllExpectations();
      generatedCodeFlusherMock3.VerifyAllExpectations();
    }

    [Test]
    public void LoadFlushedCode ()
    {
      var type = ReflectionObjectMother.GetSomeType ();
      var assemblyMock = CreateAssemblyMock ("config", type);
      _typeCacheMock.Expect (mock => mock.ParticipantConfigurationID).Return ("config");
      _typeCacheMock.Expect (mock => mock.LoadTypes (new[] { type }));

      _manager.LoadFlushedCode (assemblyMock);

      assemblyMock.VerifyAllExpectations ();
      _typeCacheMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified assembly was not generated by the pipeline.\r\nParameter name: assembly")]
    public void LoadFlushedCode_MissingTypePipeAssemblyAttribute ()
    {
      _manager.LoadFlushedCode (GetType ().Assembly);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified assembly was generated with a different participant configuration: 'different config'.\r\nParameter name: assembly")]
    public void LoadFlushedCode_InvalidParticipantConfigurationID ()
    {
      _typeCacheMock.Stub (stub => stub.ParticipantConfigurationID).Return ("config");
      var assemblyMock = CreateAssemblyMock ("different config");

      _manager.LoadFlushedCode (assemblyMock);
    }

    private _Assembly CreateAssemblyMock (string participantConfigurationID, params Type[] types)
    {
      var assemblyMock = MockRepository.GenerateStrictMock<_Assembly> ();
      var assemblyAttribute = new TypePipeAssemblyAttribute (participantConfigurationID);
      assemblyMock.Expect (mock => mock.GetCustomAttributes (typeof (TypePipeAssemblyAttribute), false)).Return (new object[] { assemblyAttribute });
      assemblyMock.Expect (mock => mock.GetTypes ()).Return (types);

      return assemblyMock;
    }
  }
}