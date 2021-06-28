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
using Remotion.TypePipe.CodeGeneration;
using Moq;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class AssemblyContextPoolTest
  {
    [Test]
    public void Initialize_WithEmtpyList_ThrowsArgumentException ()
    {
      Assert.That (
          () => new AssemblyContextPool (Enumerable.Empty<AssemblyContext>()),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("The AssemblyContextPool cannot be initialized with an empty list.", "assemblyContexts"));
    }

    [Test]
    public void DequeueAll_WithSingleThread_AndNoDequeuedAssemblyContexts_ReturnsAllRegisteredAssemblyContexts ()
    {
      var expectedAssemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };
      var assemblyContextPool = new AssemblyContextPool (expectedAssemblyContexts);

      Assert.That (assemblyContextPool.DequeueAll(), Is.EquivalentTo (expectedAssemblyContexts));
    }

    [Test]
    public void Dequeue_WithSingleThread_AndTwoRegisteredAssemblyContexts_CanDequeueTwice ()
    {
      var expectedAssemblyContext1 = CreateAssemblyContext();
      var expectedAssemblyContext2 = CreateAssemblyContext();
      var assemblyContextPool = new AssemblyContextPool (new[] { expectedAssemblyContext1, expectedAssemblyContext2 });

      Assert.That (assemblyContextPool.Dequeue(), Is.EqualTo (expectedAssemblyContext2));
      Assert.That (assemblyContextPool.Dequeue(), Is.EqualTo (expectedAssemblyContext1));
    }

    [Test]
    public void Dequeue_AfterEnqueue_WithSingleThread_AndTwoRegisteredAssemblyContexts_ReturnsEnqueuedAssemblyContextLast ()
    {
      var expectedAssemblyContext1 = CreateAssemblyContext();
      var expectedAssemblyContext2 = CreateAssemblyContext();
      var assemblyContextPool = new AssemblyContextPool (new[] { expectedAssemblyContext1, expectedAssemblyContext2 });

      Assert.That (assemblyContextPool.Dequeue(), Is.EqualTo (expectedAssemblyContext2));
      assemblyContextPool.Enqueue (expectedAssemblyContext2);
      Assert.That (assemblyContextPool.Dequeue(), Is.EqualTo (expectedAssemblyContext2));
      Assert.That (assemblyContextPool.Dequeue(), Is.EqualTo (expectedAssemblyContext1));
    }

    [Test]
    public void Enqueue_AfterDequeue_WithSingleThread_Succeeds ()
    {
      var assemblyContextPool = new AssemblyContextPool (new[] { CreateAssemblyContext(), CreateAssemblyContext() });

      var dequeuedAssemblyContext = assemblyContextPool.Dequeue();

      Assert.That (() => assemblyContextPool.Enqueue (dequeuedAssemblyContext), Throws.Nothing);
    }

    [Test]
    public void Enqueue_AfterDequeueAll_WithSingleThread_Succeeds ()
    {
      var expectedAssemblyContext1 = CreateAssemblyContext();
      var expectedAssemblyContext2 = CreateAssemblyContext();
      var assemblyContextPool = new AssemblyContextPool (new[] { expectedAssemblyContext1, expectedAssemblyContext2 });

      assemblyContextPool.DequeueAll();

      Assert.That (() => assemblyContextPool.Enqueue (expectedAssemblyContext1), Throws.Nothing);
      Assert.That (() => assemblyContextPool.Enqueue (expectedAssemblyContext2), Throws.Nothing);
    }

    [Test]
    public void Enqueue_WithUnknownAssemblyContext_ThrowsInvalidOperationException ()
    {
      var expectedAssemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };
      var assemblyContextPool = new AssemblyContextPool (expectedAssemblyContexts);

      Assert.That (
          () => assemblyContextPool.Enqueue (CreateAssemblyContext()),
          Throws.InvalidOperationException.With.Message.EqualTo ("The provided AssemblyContext is not registered with this AssemblyContextPool."));
    }

    [Test]
    public void Enqueue_WithSingleThread_WithAssemblyContextAlreadyEnqueued_ThrowsInvalidOperationException ()
    {
      var expectedAssemblyContexts = new[] { CreateAssemblyContext(), CreateAssemblyContext() };
      var assemblyContextPool = new AssemblyContextPool (expectedAssemblyContexts);

      Assert.That (
          () => assemblyContextPool.Enqueue (expectedAssemblyContexts[0]),
          Throws.InvalidOperationException.With.Message.EqualTo ("The provided AssemblyContext is already enqueued in this AssemblyContextPool."));
    }

    private AssemblyContext CreateAssemblyContext ()
    {
      return new AssemblyContext (
          new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict).Object,
          new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict).Object);
    }
  }
}