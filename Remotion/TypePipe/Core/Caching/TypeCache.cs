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
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class TypeCache : ITypeCache
  {
    private readonly ConcurrentDictionary<AssembledTypeID, Lazy<Type>> _types = new ConcurrentDictionary<AssembledTypeID, Lazy<Type>>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly IAssemblyContextPool _assemblyContextPool;

    private readonly Func<AssembledTypeID, Lazy<Type>> _createTypeFunc;

    public TypeCache (ITypeAssembler typeAssembler, IAssemblyContextPool assemblyContextPool)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("assemblyContextPool", assemblyContextPool);

      _typeAssembler = typeAssembler;
      _assemblyContextPool = assemblyContextPool;

      _createTypeFunc = CreateType;
    }

    public string ParticipantConfigurationID
    {
      get { return _typeAssembler.ParticipantConfigurationID; }
    }

    public ReadOnlyCollection<IParticipant> Participants
    {
      get { return _typeAssembler.Participants; }
    }

    public Type GetOrCreateType (AssembledTypeID typeID)
    {
      var lazyType = _types.GetOrAdd (typeID, _createTypeFunc);

      try
      {
        return lazyType.Value;
      }
      catch
      {
        // Lazy<T> with ExecutionAndPublication and a create-function caches the exception. 
        // In order to renew the Lazy for another attempt, a replace of the Lazy-object is performed, but only if the _types dictionary
        // still holds the original Lazy (that cached the exception). This avoids a race with a parallel thread that requested the same type.
        if (_types.TryUpdate (typeID, CreateType (typeID), lazyType))
          throw; //TODO RM-5849 Test 

        // Can theoretically cause a StackOverflowException in case of starvation. We are ignoring this very remote possiblity.
        // This code path cannot be tested.
        return GetOrCreateType (typeID);
      }
    }

    private Lazy<Type> CreateType (AssembledTypeID typeID)
    {
      return new Lazy<Type> (
          () =>
          {
            var assemblyContext = _assemblyContextPool.Dequeue();
            try
            {
              return _typeAssembler.AssembleType (typeID, assemblyContext.ParticipantState, assemblyContext.MutableTypeBatchCodeGenerator);
            }
            finally
            {
              _assemblyContextPool.Enqueue (assemblyContext);
            }
          },
          LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Type GetOrCreateAdditionalType (object additionalTypeID)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);

      var assemblyContext = _assemblyContextPool.Dequeue();
      try
      {
        return _typeAssembler.GetOrAssembleAdditionalType (
            additionalTypeID,
            assemblyContext.ParticipantState,
            assemblyContext.MutableTypeBatchCodeGenerator);
      }
      finally
      {
        _assemblyContextPool.Enqueue (assemblyContext);
      }
    }

    public void LoadTypes (IEnumerable<Type> generatedTypes)
    {
      ArgumentUtility.CheckNotNull ("generatedTypes", generatedTypes);

      var assembledTypes = new List<Type>();
      var additionalTypes = new List<Type>();

      foreach (var type in generatedTypes)
      {
        if (_typeAssembler.IsAssembledType (type))
          assembledTypes.Add (type);
        else
          additionalTypes.Add (type);
      }

      // ReSharper disable LoopCanBeConvertedToQuery
      var loadedAssembledTypes = new List<Type>();
      foreach (var assembledType in assembledTypes)
      {
        var typeID = _typeAssembler.ExtractTypeID (assembledType);
        Type assembledTypeForClosure = assembledType;
        if (_types.TryAdd (typeID, new Lazy<Type> (() => assembledTypeForClosure, LazyThreadSafetyMode.None)))
          loadedAssembledTypes.Add (assembledType);
      }
      // ReSharper restore LoopCanBeConvertedToQuery

      //TODO RM-5849: Reenable or completly remove RebuildParticipantState
      //var assemblyContexts = _assemblyContextPool.DequeueAll();
      //try
      //{
      //  _typeAssembler.RebuildParticipantState (loadedAssembledTypes, additionalTypes, assemblyContext.ParticipantState);
      //}
      //finally
      //{
      //  foreach (var assemblyContext in assemblyContexts)
      //    _assemblyContextPool.Enqueue (assemblyContext);
      //}
    }
  }
}