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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class CodeManagerTest
  {
    private Mock<ITypeCache> _typeCacheMock;
    private Mock<ITypeAssembler> _typeAssemblerMock;
    private Mock<IAssemblyContextPool> _assemblyContextPool;

    private CodeManager _manager;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = new Mock<ITypeCache> (MockBehavior.Strict);
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);
      _assemblyContextPool = new Mock<IAssemblyContextPool> (MockBehavior.Strict);

      _manager = new CodeManager (_typeCacheMock.Object, _typeAssemblerMock.Object, _assemblyContextPool.Object);
    }

    [Test]
    public void FlushCodeToDisk_FlushesMultipleAssemblies_ReturnsNonNullResultPaths ()
    {
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();

      var generatedCodeFlusherMock1 = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);
      var assemblyContext1 = new AssemblyContext (new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object, generatedCodeFlusherMock1.Object);
      var participantState1 = assemblyContext1.ParticipantState;

      var generatedCodeFlusherMock2 = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);
      var assemblyContext2 = new AssemblyContext (new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object, generatedCodeFlusherMock2.Object);
      var participantState2 = assemblyContext2.ParticipantState;

      var generatedCodeFlusherMock3 = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);
      var assemblyContext3 = new AssemblyContext (new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object, generatedCodeFlusherMock3.Object);
      var participantState3 = assemblyContext3.ParticipantState;

      var isDequeued = false;
      _assemblyContextPool
          .Setup (mock => mock.DequeueAll())
          .Returns (new[] { assemblyContext1, assemblyContext2, assemblyContext3 })
          .Callback (() => { isDequeued = true; })
          .Verifiable();

      var isFlushed1 = false;
      generatedCodeFlusherMock1
          .Setup (mock => mock.FlushCodeToDisk (It.IsAny<IEnumerable<CustomAttributeDeclaration>>()))
          .Returns ("path1")
          .Callback (
              (IEnumerable<CustomAttributeDeclaration> assemblyAttributes) =>
              {
                Assert.That (isDequeued, Is.True);
                isFlushed1 = true;
              })
          .Verifiable();

      var isFlushed2 = false;
      generatedCodeFlusherMock2
          .Setup (mock => mock.FlushCodeToDisk (It.IsAny<IEnumerable<CustomAttributeDeclaration>>()))
          .Returns ((string) null)
          .Callback (
              (IEnumerable<CustomAttributeDeclaration> assemblyAttributes) =>
              {
                Assert.That (isDequeued, Is.True);
                isFlushed2 = true;
              })
          .Verifiable();

      var isFlushed3 = false;
      generatedCodeFlusherMock3
          .Setup (mock => mock.FlushCodeToDisk (It.IsAny<IEnumerable<CustomAttributeDeclaration>>()))
          .Returns ("path3")
          .Callback (
              (IEnumerable<CustomAttributeDeclaration> assemblyAttributes) =>
              {
                Assert.That (isDequeued, Is.True);
                isFlushed3 = true;
              })
          .Verifiable();

      _assemblyContextPool
          .Setup (mock => mock.Enqueue (assemblyContext1))
          .Callback (
              (AssemblyContext _) =>
              {
                Assert.That (isFlushed1, Is.True);
                Assert.That (assemblyContext1.ParticipantState, Is.Not.SameAs (participantState1));
              })
          .Verifiable();

      _assemblyContextPool
          .Setup (mock => mock.Enqueue (assemblyContext2))
          .Callback (
              (AssemblyContext assemblyContext) =>
              {
                Assert.That (isFlushed2, Is.True);
                Assert.That (assemblyContext2.ParticipantState, Is.Not.SameAs (participantState2));
              })
          .Verifiable();

      _assemblyContextPool
          .Setup (mock => mock.Enqueue (assemblyContext3))
          .Callback (
              (AssemblyContext assemblyContext) =>
              {
                Assert.That (isFlushed3, Is.True);
                Assert.That (assemblyContext3.ParticipantState, Is.Not.SameAs (participantState3));
              })
          .Verifiable();

      var result = _manager.FlushCodeToDisk (new[] { assemblyAttribute });

      _assemblyContextPool.Verify();
      generatedCodeFlusherMock1.Verify();
      generatedCodeFlusherMock2.Verify();
      generatedCodeFlusherMock3.Verify();

      Assert.That (result, Is.EquivalentTo (new[] { "path1", "path3" }));
    }

    [Test]
    public void FlushCodeToDisk_WithException_ReturnsAssemblyContextToPool ()
    {
      var expectedException = new Exception();
      var assemblyAttribute = CustomAttributeDeclarationObjectMother.Create();

      var generatedCodeFlusherMock1 = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);
      var assemblyContext1 = new AssemblyContext (new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object, generatedCodeFlusherMock1.Object);

      var generatedCodeFlusherMock2 = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);
      var assemblyContext2 = new AssemblyContext (new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object, generatedCodeFlusherMock2.Object);

      var generatedCodeFlusherMock3 = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);
      var assemblyContext3 = new AssemblyContext (new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object, generatedCodeFlusherMock3.Object);

      _assemblyContextPool
          .Setup (mock => mock.DequeueAll())
          .Returns (new[] { assemblyContext1, assemblyContext2, assemblyContext3 })
          .Verifiable();

      generatedCodeFlusherMock1
          .Setup (mock => mock.FlushCodeToDisk (It.IsAny<IEnumerable<CustomAttributeDeclaration>>()))
          .Returns ("path1")
          .Verifiable();

      generatedCodeFlusherMock2
          .Setup (mock => mock.FlushCodeToDisk (It.IsAny<IEnumerable<CustomAttributeDeclaration>>()))
          .Throws (expectedException)
          .Verifiable();

      generatedCodeFlusherMock3
          .Setup (mock => mock.FlushCodeToDisk (It.IsAny<IEnumerable<CustomAttributeDeclaration>>()))
          .Returns ("path3")
          .Verifiable();

      _assemblyContextPool.Setup (mock => mock.Enqueue (assemblyContext1)).Verifiable();
      _assemblyContextPool.Setup (mock => mock.Enqueue (assemblyContext2)).Verifiable();
      _assemblyContextPool.Setup (mock => mock.Enqueue (assemblyContext3)).Verifiable();

      var aggregateException = Assert.Throws<AggregateException> (() => _manager.FlushCodeToDisk (new[] { assemblyAttribute }));
      Assert.That (aggregateException.InnerExceptions, Is.EquivalentTo (new[] { expectedException }));

      _assemblyContextPool.Verify();
      generatedCodeFlusherMock1.Verify();
      generatedCodeFlusherMock2.Verify();
      generatedCodeFlusherMock3.Verify();
    }

    [Test]
    public void LoadFlushedCode ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var assemblyMock = CreateAssemblyMock ("config", type);
      _typeAssemblerMock.SetupGet (mock => mock.ParticipantConfigurationID).Returns ("config").Verifiable();
      _typeCacheMock.Setup (mock => mock.LoadTypes (new[] { type })).Verifiable();

      _manager.LoadFlushedCode (assemblyMock.Object);

      assemblyMock.Verify();
      _typeCacheMock.Verify();
      _typeAssemblerMock.Verify();
    }

    [Test]
    public void LoadFlushedCode_MissingTypePipeAssemblyAttribute ()
    {
      Assert.That (
          () => _manager.LoadFlushedCode (GetType().Assembly),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("The specified assembly was not generated by the pipeline.", "assembly"));
    }

    [Test]
    public void LoadFlushedCode_InvalidParticipantConfigurationID ()
    {
      _typeAssemblerMock.SetupGet (stub => stub.ParticipantConfigurationID).Returns ("config");
      var assemblyMock = CreateAssemblyMock ("different config");
      Assert.That (
          () => _manager.LoadFlushedCode (assemblyMock.Object),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "The specified assembly was generated with a different participant configuration: 'different config'.", "assembly"));
    }

    private Mock<FakeAssembly> CreateAssemblyMock (string participantConfigurationID, params Type[] types)
    {
      var assemblyMock = new Mock<FakeAssembly> (MockBehavior.Strict);
      var assemblyAttribute = new TypePipeAssemblyAttribute (participantConfigurationID);
      assemblyMock.Setup (mock => mock.GetCustomAttributes (typeof (TypePipeAssemblyAttribute), false)).Returns (new object[] { assemblyAttribute }).Verifiable();
      assemblyMock.Setup (mock => mock.GetTypes()).Returns (types).Verifiable();

      return assemblyMock;
    }

    /// <remarks>
    /// Castle does not support creating a proxy for <see cref="Assembly"/> directly ("The type System.Reflection.Assembly implements ISerializable,
    /// but failed to provide a deserialization constructor"), thus this type is required. <see cref="Assembly"/> defines the needed methods
    /// <see cref="Assembly.GetCustomAttributes(System.Type,bool)"/> and <see cref="Assembly.GetTypes()"/> as virtual, allowing the type to be
    /// mocked for our purpose.
    /// </remarks>
    public class FakeAssembly : Assembly
    {
    }
  }
}