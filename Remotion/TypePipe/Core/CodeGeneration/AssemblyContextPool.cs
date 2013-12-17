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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  ///<summary>Holds a synchronized pool of <see cref="AssemblyContext"/> objects.</summary>
  /// <threadsafety static="true" instance="true"/>
  public class AssemblyContextPool : IAssemblyContextPool
  {
    // Multi-threading behavior of the AssemblyContextPool can not be verified using unit testing.
    // The code has been reviewed and should not be changed without being reviewed

    private readonly BlockingCollection<AssemblyContext> _contextPool;

    // Thread-safe set (for multiple readers, no writer).
    private readonly Dictionary<AssemblyContext, object> _registeredContexts;

    private readonly ConcurrentDictionary<AssemblyContext, object> _enqueuedContexts;

    private readonly object _dequeueAllLockObject = new object();

    public AssemblyContextPool (IEnumerable<AssemblyContext> assemblyContexts)
    {
      ArgumentUtility.CheckNotNull ("assemblyContexts", assemblyContexts);
      var allContexts = assemblyContexts.ToDictionary (c => c, c => (object) null);
      if (allContexts.Count == 0)
        throw new ArgumentException ("The AssemblyContextPool cannot be initialized with an empty list.", "assemblyContexts");

      _registeredContexts = allContexts;
      _enqueuedContexts = new ConcurrentDictionary<AssemblyContext, object> (allContexts);
      _contextPool = new BlockingCollection<AssemblyContext> (new ConcurrentStack<AssemblyContext> (allContexts.Keys));
    }

    public AssemblyContext[] DequeueAll ()
    {
      lock (_dequeueAllLockObject)
      {
        var assemblyContexts = _contextPool.GetConsumingEnumerable().Take (_registeredContexts.Count).ToArray();

        _enqueuedContexts.Clear();

        return assemblyContexts;
      }
    }

    public void Enqueue (AssemblyContext assemblyContext)
    {
      ArgumentUtility.CheckNotNull ("assemblyContext", assemblyContext);

      if (!_registeredContexts.ContainsKey (assemblyContext))
        throw new InvalidOperationException ("The provided AssemblyContext is not registered with this AssemblyContextPool.");

      if (!_enqueuedContexts.TryAdd (assemblyContext, null))
        throw new InvalidOperationException ("The provided AssemblyContext is already enqueued in this AssemblyContextPool.");

      _contextPool.Add (assemblyContext);
    }

    public AssemblyContext Dequeue ()
    {
      //TODO RM-5849: Add timeout to Dequeue, log warning, return to Dequeuing without timeout.
      var assemblyContext = _contextPool.Take();

      object value;
      var result = _enqueuedContexts.TryRemove (assemblyContext, out value);
      Assertion.IsTrue (result);

      return assemblyContext;
    }
  }
}