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
using System.Linq;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  public class AssemblyContextPool : IAssemblyContextPool
  {
    //TODO 5840: Tests
    //TODO 5840: Docs
    private readonly BlockingCollection<AssemblyContext> _queue = new BlockingCollection<AssemblyContext> (new ConcurrentQueue<AssemblyContext>());

    // Thread-safe set (for multiple readers, no writer).
    private readonly Dictionary<AssemblyContext, AssemblyContext> _allContexts;

    public AssemblyContextPool (IEnumerable<AssemblyContext> assemblyContexts)
    {
      ArgumentUtility.CheckNotNull ("assemblyContexts", assemblyContexts);

      _allContexts = assemblyContexts.ToDictionary (c => c);
      foreach (var assemblyContext in _allContexts.Keys)
        _queue.Add (assemblyContext);
    }

    public AssemblyContext[] DequeueAll ()
    {
      return _queue.GetConsumingEnumerable().Take (_allContexts.Count).ToArray();
    }

    public void Enqueue (AssemblyContext context)
    {
      ArgumentUtility.CheckNotNull ("context", context);

      if (!_allContexts.ContainsKey (context))
        throw new ArgumentException();

      _queue.Add (context);
    }

    public AssemblyContext Dequeue ()
    {
      return _queue.Take();
    }
  }
}