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
using System.Runtime.Remoting.Messaging;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Decorates the <see cref="IAssemblyContextPool"/> with a thread-local cache. This allows the reuse of the same assembly context within a thread.
  /// </summary>
  public class ThreadLocalAssemblyContextPoolDecorator : IAssemblyContextPool
  {
    private class ThreadLocalAssemblyContext
    {
      private readonly AssemblyContext _value;
      private int _count;

      public ThreadLocalAssemblyContext (AssemblyContext value)
      {
        _value = value;
      }

      public AssemblyContext Value
      {
        get { return _value; }
      }

      public int Count
      {
        get { return _count; }
      }

      public void IncrementCount ()
      {
        _count++;
      }

      public void DecrementCount ()
      {
        Assertion.IsTrue (_count > 0, "Count can never be decremented past 0.");
        _count--;
      }
    }

    private readonly string _instanceID = Guid.NewGuid().ToString();
    private readonly IAssemblyContextPool _assemblyContextPool;

    public ThreadLocalAssemblyContextPoolDecorator (IAssemblyContextPool assemblyContextPool)
    {
      _assemblyContextPool = assemblyContextPool;
    }

    public AssemblyContext[] DequeueAll ()
    {
      var data = CallContext.GetData (_instanceID);

      if (data is ThreadLocalAssemblyContext)
      {
        throw new InvalidOperationException (
            "DequeueAll() cannot be invoked from the same thread as Dequeue() until all dequeued AssemblyContext have been returned to the pool.");
      }

      if (data is HashSet<AssemblyContext>)
      {
        throw new InvalidOperationException (
            "DequeueAll() cannot be invoked from the same thread as a previous call to DequeueAll() "
            + "until all dequeued AssemblyContext have been returned to the pool.");
      }

      Assertion.IsNull (data, "No information should be available for pool-ID '{0}'.", _instanceID);

      var assemblyContexts = _assemblyContextPool.DequeueAll();
      CallContext.SetData (_instanceID, new HashSet<AssemblyContext> (assemblyContexts));

      return assemblyContexts;
    }

    public AssemblyContext Dequeue ()
    {
      var data = CallContext.GetData (_instanceID);
      if (data is HashSet<AssemblyContext>)
      {
        throw new InvalidOperationException (
            "Dequeue() cannot be invoked from the same thread as DequeueAll() until all dequeued AssemblyContext have been returned to the pool.");
      }

      var threadLocalAssemblyContext = data as ThreadLocalAssemblyContext;
      if (threadLocalAssemblyContext == null)
      {
        var assemblyContext = _assemblyContextPool.Dequeue();
        threadLocalAssemblyContext = new ThreadLocalAssemblyContext (assemblyContext);
        CallContext.SetData (_instanceID, threadLocalAssemblyContext);
      }

      threadLocalAssemblyContext.IncrementCount();
      return threadLocalAssemblyContext.Value;
    }

    public void Enqueue (AssemblyContext assemblyContext)
    {
      ArgumentUtility.CheckNotNull ("assemblyContext", assemblyContext);

      var data = CallContext.GetData (_instanceID);
      if (data == null)
      {
        throw new InvalidOperationException (
            "No AssemblyContext has been dequeued on the current thread. "
            + "An AssemblyContext must be enqueued on the same thread it was dequeued from.");
      }

      bool isComplete;
      if (data is ThreadLocalAssemblyContext)
        isComplete = EnqueueAfterDequeue ((ThreadLocalAssemblyContext) data, assemblyContext);
      else
        isComplete = EnqueueAfterDequeueAll ((HashSet<AssemblyContext>) data, assemblyContext);

      if (isComplete)
        CallContext.FreeNamedDataSlot (_instanceID);
    }

    private bool EnqueueAfterDequeue (ThreadLocalAssemblyContext threadLocalAssemblyContext, AssemblyContext assemblyContext)
    {
      if (threadLocalAssemblyContext.Value != assemblyContext)
      {
        throw new InvalidOperationException (
            "The specified AssemblyContext has been dequeued on a different thread. "
            + "An AssemblyContext must be enqueued on the same thread it was dequeued from.");
      }

      threadLocalAssemblyContext.DecrementCount();

      if (threadLocalAssemblyContext.Count == 0)
      {
        try
        {
          _assemblyContextPool.Enqueue (assemblyContext);
        }
        catch
        {
          threadLocalAssemblyContext.IncrementCount();
          throw;
        }
      }

      return threadLocalAssemblyContext.Count == 0;
    }

    private bool EnqueueAfterDequeueAll (HashSet<AssemblyContext> dequeuedAssemblyContexts, AssemblyContext assemblyContext)
    {
      _assemblyContextPool.Enqueue (assemblyContext);
      dequeuedAssemblyContexts.Remove (assemblyContext);
      return dequeuedAssemblyContexts.Count == 0;
    }
  }
}