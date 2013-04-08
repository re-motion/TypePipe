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
using System.Threading;
using NUnit.Framework;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.IntegrationTests.ObjectFactory
{
  // TODO Review2: Do such tests make sense?
  [TestFixture]
  [Timeout (1000)] // Set timeout for all tests.
  public class ConcurrencyTest : IntegrationTestBase
  {
    private Mutex _blockingMutex;

    private IObjectFactory _objectFactory;

    public override void SetUp ()
    {
      base.SetUp ();

      _blockingMutex = new Mutex (true);

      var blockingParticipant = CreateParticipant (
          ctx =>
          {
            if (ctx.RequestedType == typeof (DomainType1))
              _blockingMutex.WaitOne();
          });
      _objectFactory = CreateObjectFactory (blockingParticipant);
    }

    [Test]
    public void InstancesOfCachedTypesCanBeCreatedDuringCodeGenerationOfAnotherType ()
    {
      // Generate type 2.
      _objectFactory.CreateObject<DomainType2>();

      var t = StartAndWaitUntilBlocked (() => _objectFactory.CreateObject<DomainType1>());

      // Although code is generated in [t], which is blocked by the mutex, we can create instances of already generated types.
      _objectFactory.CreateObject<DomainType2>();

      _blockingMutex.ReleaseMutex();
      WaitUntilCompleted (t);
    }

    [Test]
    public void CodeGenerationIsSerialized ()
    {
      var t1 = StartAndWaitUntilBlocked (() => _objectFactory.CreateObject<DomainType1>());
      var t2 = StartAndWaitUntilBlocked (() => _objectFactory.CreateObject<DomainType2>());

      // Both threads are now blocked. [t1] is blocked by the mutex, [t2] is blocked by the code generation in [t1].
      _blockingMutex.ReleaseMutex();

      // Now both threads run to completion (code generation is serialized).
      WaitUntilCompleted (t1, t2);
    }

    [Test]
    public void CodeManagerIsGuardedBySameLockAsCodeGenerationInternals ()
    {
      var t1 = StartAndWaitUntilBlocked (() => _objectFactory.CreateObject<DomainType1>());
      var t2 = StartAndWaitUntilBlocked (() => Dev.Null = _objectFactory.CodeManager.AssemblyName);

      // Both threads are now blocked. [t1] is blocked by the mutex, [t2] is blocked by the code generation in [t1].
      _blockingMutex.ReleaseMutex();

      // Now both threads run to completion (user APIs do not interfere with code generation).
      WaitUntilCompleted (t1, t2);
    }

    private Thread StartAndWaitUntilBlocked (ThreadStart action)
    {
      var thread = new Thread (action);
      thread.Start();

      while (thread.ThreadState != ThreadState.WaitSleepJoin)
        Thread.Sleep (10); // TODO 5057: Use Thread.Yield instead.

      return thread;
    }

    private void WaitUntilCompleted (params Thread[] threads)
    {
      foreach (var t in threads)
        t.Join();
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}