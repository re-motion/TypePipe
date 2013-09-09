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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.Implementation.Synchronization
{
  /// <summary>
  /// Guards all access to code generation capabilities.
  /// </summary>
  // TODO 5840: Inline in callers (TypeCache, RevTypeCache, CodeManager), inject AssemblyContextPool in those places.
  public class SynchronizationPoint : ISynchronizationPoint
  {
    private readonly ITypeAssembler _typeAssembler;
    private readonly IAssemblyContextPool _assemblyContextPool;

    public SynchronizationPoint (ITypeAssembler typeAssembler, IAssemblyContextPool assemblyContextPool)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("assemblyContextPool", assemblyContextPool);

      _typeAssembler = typeAssembler;
      _assemblyContextPool = assemblyContextPool;
    }

    public string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      //TODO 5840: Affects all AssemblyContexts in the pool.
      // Dequeue all contexts, flush them, enqueue again and return string[] for the flushed assembly paths.
      var assemblyContext = _assemblyContextPool.DequeueAll().Single(()=> new NotSupportedException ("Multiple AssemblyContext are not supported."));
      try
      {
        return assemblyContext.GeneratedCodeFlusher.FlushCodeToDisk (assemblyAttributes);
      }
      finally
      {
        _assemblyContextPool.Enqueue (assemblyContext);
      }
    }

    public void RebuildParticipantState (
        ConcurrentDictionary<AssembledTypeID, Type> types,
        IEnumerable<KeyValuePair<AssembledTypeID, Type>> keysToAssembledTypes,
        IEnumerable<Type> additionalTypes)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("keysToAssembledTypes", keysToAssembledTypes);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);

      // TODO 5840: Move to TypeCache
      // TODO 5840: Test Dequeue.
      // TODO 5840: Test Enqueue in finally-block.
      // TODO 5840: Figure out if this should be performed for all AssemblyContexts in the Pool, or only once but still locking all AssemblyContexts.
      var assemblyContext = _assemblyContextPool.Dequeue();
      try
      {
        var loadedAssembledTypes = new List<Type>();
        foreach (var p in keysToAssembledTypes.Where (p => !types.ContainsKey (p.Key)))
        {
          AddTo (types, p.Key, p.Value);
          loadedAssembledTypes.Add (p.Value);
        }

        _typeAssembler.RebuildParticipantState (loadedAssembledTypes, additionalTypes, assemblyContext.ParticipantState);
      }
      finally
      {
        _assemblyContextPool.Enqueue (assemblyContext);
      }
    }

    private void AddTo<TKey, TValue> (ConcurrentDictionary<TKey, TValue> concurrentDictionary, TKey key, TValue value)
    {
      if (!concurrentDictionary.TryAdd (key, value))
        throw new ArgumentException ("Key already exists.");
    }
  }
}