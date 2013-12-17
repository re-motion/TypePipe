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
using System.Threading;
using NUnit.Framework;
using Remotion.Development.UnitTesting.IO;
using Remotion.TypePipe.Configuration;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  [Timeout (1000)] // Set timeout for all tests.
  public class ConcurrencyTest_WithMultipleGenerationThreads : IntegrationTestBase
  {
    private Mutex _blockingMutexA;
    private Mutex _blockingMutexB;

    private IPipeline _pipeline;

    public override void SetUp ()
    {
      base.SetUp ();

      _blockingMutexA = new Mutex (initiallyOwned: true);
      _blockingMutexB = new Mutex (initiallyOwned: true);

      var blockingParticipant = CreateParticipant (
          participateAction: (id, ctx) =>
          {
            if (ctx.RequestedType == typeof (DomainTypeCausingParticipantToBlockMutexA))
              _blockingMutexA.WaitOne();

            if (ctx.RequestedType == typeof (DomainTypeCausingParticipantToBlockMutexB))
              _blockingMutexB.WaitOne();
          },
          additionalTypeFunc: (additionalTypeID, ctx) =>
          {
            var baseType = Type.GetType ((string)additionalTypeID, true, false);
            return ctx.CreateAddtionalProxyType (additionalTypeID, baseType);
          },
          getAdditionalTypeIDFunc: additionalType =>
          {
            if (additionalType.BaseType == typeof (DomainTypeCausingParticipantToBlockMutexA))
              _blockingMutexA.WaitOne();

            return additionalType.BaseType.FullName;
          });

      var settings = PipelineSettings.New().SetDegreeOfParallelism (2).Build();
      _pipeline = CreatePipelineWithIntegrationTestAssemblyLocation ("ConcurrencyTest_WithMultipleGenerationThreads", settings, blockingParticipant);

      // Disable saving - it would deadlock with a failing test while threads are blocked.
      SkipSavingAndPeVerification();
    }

    public override void TearDown ()
    {
      if (TestContext.CurrentContext.Result.Status == TestStatus.Passed)
        EnableSavingAndPeVerification();

      _blockingMutexA.Dispose();
      _blockingMutexB.Dispose();

      base.TearDown();
    }
    
    [Test]
    public void CodeGenerationIsParallelInSeparateAssemblies_WithDegreeOfParallelismNotExceeded_CodeGenerationDoesNotBlock ()
    {
      var t = StartAndWaitUntilBlocked (() => _pipeline.Create<DomainTypeCausingParticipantToBlockMutexA>());
      _pipeline.Create<DomainType>();

      // [t] is blocked by the mutex, main thread can continue with the code generation.
      Assert.That (t.ThreadState, Is.EqualTo (ThreadState.WaitSleepJoin));
      _blockingMutexA.ReleaseMutex();

      // Now both threads run to completion (code generation is serialized).
      WaitUntilCompleted (t);

      var typeFromMutexA = _pipeline.ReflectionService.GetAssembledType (typeof (DomainTypeCausingParticipantToBlockMutexA));
      var domainType = _pipeline.ReflectionService.GetAssembledType (typeof (DomainType));
      Assert.That (typeFromMutexA.Assembly, Is.Not.SameAs (domainType.Assembly));
    }

    [Test]
    public void CodeGenerationIsParallelInSeparateAssemblies_WithDegreeOfParallelismExceeded_CodeGenerationBlocks ()
    {
      var t1 = StartAndWaitUntilBlocked (() => _pipeline.Create<DomainTypeCausingParticipantToBlockMutexA>());
      var t2 = StartAndWaitUntilBlocked (() => _pipeline.Create<DomainTypeCausingParticipantToBlockMutexB>());
      var t3 = StartAndWaitUntilBlocked (() => _pipeline.Create<DomainType>());

      // All threads are now blocked. [t1] and [t2] are blocked by the mutex-A and mutex-B, [t3] is blocked by the code generation in [t1] and [t2].
      _blockingMutexA.ReleaseMutex();

      // Now threads [t1] and [t3] can run to completion (code generation is serialized).
      WaitUntilCompleted (t1, t3);

      // [t2] is still blocked by the mutex-B.
      Assert.That (t2.ThreadState, Is.EqualTo (ThreadState.WaitSleepJoin));
      _blockingMutexB.ReleaseMutex();

      // Now threads [t2] can run to completion.
      WaitUntilCompleted (t2);

      var typeFromMutexA = _pipeline.ReflectionService.GetAssembledType (typeof (DomainTypeCausingParticipantToBlockMutexA));
      var typeFromMutexB = _pipeline.ReflectionService.GetAssembledType (typeof (DomainTypeCausingParticipantToBlockMutexB));
      var domainType = _pipeline.ReflectionService.GetAssembledType (typeof (DomainType));

      Assert.That (typeFromMutexA.Assembly, Is.SameAs (domainType.Assembly));
      Assert.That (typeFromMutexB.Assembly, Is.Not.SameAs (domainType.Assembly));
    }

    [Test]
    public void LoadFlushedCodeInParallel_GetTypeIDForAssembledTypeBlocks ()
    {
      // Setup the assembly
      var tempPipeline = CreatePipelineWithIntegrationTestAssemblyLocation (
          "ConcurrencyTest_WithMultipleGenerationThreads",
          PipelineSettings.Defaults,
          _pipeline.Participants.ToArray());
      tempPipeline.ReflectionService.GetAdditionalType (typeof (DomainTypeCausingParticipantToBlockMutexA).FullName);
      tempPipeline.ReflectionService.GetAdditionalType (typeof (DomainType).FullName);
      var assemblyPaths = tempPipeline.CodeManager.FlushCodeToDisk();
      var assembly = AssemblyLoader.LoadWithoutLocking (assemblyPaths.Single());

      // Load the blocking code
      var t1 = StartAndWaitUntilBlocked (() =>_pipeline.CodeManager.LoadFlushedCode (assembly));
      // [t1] is blocked by the mutex-A.
      Assert.That (t1.ThreadState, Is.EqualTo (ThreadState.WaitSleepJoin));

      var t2 = StartAndWaitUntilBlocked (
          () => _pipeline.ReflectionService.GetAdditionalType (typeof (DomainTypeCausingParticipantToBlockMutexA).FullName));
      // [t2] is blocked by the mutex-A.
      Assert.That (t2.ThreadState, Is.EqualTo (ThreadState.WaitSleepJoin));
      var domainTypeProxy = _pipeline.ReflectionService.GetAdditionalType (typeof (DomainType).FullName);

      _blockingMutexA.ReleaseMutex();
      // Now threads [t1] and [t2] can run to completion.
      WaitUntilCompleted (t1, t2);
      Assert.That (domainTypeProxy.BaseType, Is.EqualTo (typeof (DomainType)));
    }

    [Test]
    public void CodeManagerAPIs_FlushIsBlockedUntilAllCodeGenerationIsComplete ()
    {
      var t1 = StartAndWaitUntilBlocked (() => _pipeline.Create<DomainTypeCausingParticipantToBlockMutexA>());
      var t2 = StartAndWaitUntilBlocked (() => Flush());
      var t3 = StartAndWaitUntilBlocked (() => _pipeline.Create<DomainType>());

      // All threads are now blocked. [t1] is blocked by the mutex, [t2] is blocked by the code generation in [t1], [3] is blocked by the flush in [t2].
      _blockingMutexA.ReleaseMutex();

      // Now all threads run to completion.
      WaitUntilCompleted (t1, t2, t3);
    }

    private Thread StartAndWaitUntilBlocked (ThreadStart action)
    {
      var thread = new Thread (action);
      thread.Start();

      var startTime = DateTime.UtcNow;
      while (thread.ThreadState != ThreadState.WaitSleepJoin && (DateTime.UtcNow - startTime) <= TimeSpan.FromSeconds(0.5))
        Thread.Yield();

      Assert.That (thread.ThreadState, Is.EqualTo (ThreadState.WaitSleepJoin), "Thread didn't block.");

      return thread;
    }

    private void WaitUntilCompleted (params Thread[] threads)
    {
      foreach (var t in threads)
        t.Join();
    }

    public class DomainTypeCausingParticipantToBlockMutexA {}
    public class DomainTypeCausingParticipantToBlockMutexB {}
    public class DomainType {}
    public class OtherDomainType {}
  }
}