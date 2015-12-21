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
    private readonly ConcurrentDictionary<AssembledTypeID, Lazy<Type>> _assembledTypes = new ConcurrentDictionary<AssembledTypeID, Lazy<Type>>();
    private readonly ConcurrentDictionary<object, Lazy<Type>> _additionalTypes = new ConcurrentDictionary<object, Lazy<Type>>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly IAssemblyContextPool _assemblyContextPool;

    private readonly Func<AssembledTypeID, Lazy<Type>> _createAssembledTypeFunc;
    private readonly Func<object, Lazy<Type>> _createAdditionalTypeFunc;

    public TypeCache (ITypeAssembler typeAssembler, IAssemblyContextPool assemblyContextPool)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("assemblyContextPool", assemblyContextPool);

      _typeAssembler = typeAssembler;
      _assemblyContextPool = assemblyContextPool;

      _createAssembledTypeFunc = CreateAssembledType;
      _createAdditionalTypeFunc = CreateAdditionalType;
    }

    public Type GetOrCreateType (AssembledTypeID typeID)
    {
      var lazyType = _assembledTypes.GetOrAdd (typeID, _createAssembledTypeFunc);

      try
      {
        return lazyType.Value;
      }
      catch
      {
        // Lazy<T> with ExecutionAndPublication and a create-function caches the exception. 
        // In order to renew the Lazy for another attempt, a replace of the Lazy-object is performed, but only if the _assembledTypes dictionary
        // still holds the original Lazy (that cached the exception). This avoids a race with a parallel thread that requested the same type.
        if (_assembledTypes.TryUpdate (typeID, _createAssembledTypeFunc (typeID), lazyType))
          throw;

        // Can theoretically cause a StackOverflowException in case of starvation. We are ignoring this very remote possiblity.
        // This code path cannot be tested.
        return GetOrCreateType (typeID);
      }
    }

    private Lazy<Type> CreateAssembledType (AssembledTypeID typeID)
    {
      return new Lazy<Type> (
          () =>
          {
            var assemblyContext = _assemblyContextPool.Dequeue();
            try
            {
              var result = _typeAssembler.AssembleType (typeID, assemblyContext.ParticipantState, assemblyContext.MutableTypeBatchCodeGenerator);
              AddAdditionalTypesToCache(result.AdditionalTypes);
              return result.Type;
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

      var lazyType = _additionalTypes.GetOrAdd (additionalTypeID, _createAdditionalTypeFunc);

      try
      {
        return lazyType.Value;
      }
      catch
      {
        // Lazy<T> with ExecutionAndPublication and a create-function caches the exception. 
        // In order to renew the Lazy for another attempt, a replace of the Lazy-object is performed, but only if the _additionalTypes dictionary
        // still holds the original Lazy (that cached the exception). This avoids a race with a parallel thread that requested the same type.
        if (_additionalTypes.TryUpdate (additionalTypeID, _createAdditionalTypeFunc (additionalTypeID), lazyType))
          throw;

        // Can theoretically cause a StackOverflowException in case of starvation. We are ignoring this very remote possiblity.
        // This code path cannot be tested.
        return GetOrCreateAdditionalType (additionalTypeID);
      }
    }

    private Lazy<Type> CreateAdditionalType (object additionalTypeID)
    {
      return new Lazy<Type> (
          () =>
          {
            var assemblyContext = _assemblyContextPool.Dequeue();
            try
            {
              var result = _typeAssembler.AssembleAdditionalType (
                  additionalTypeID,
                  assemblyContext.ParticipantState,
                  assemblyContext.MutableTypeBatchCodeGenerator);

              AddAdditionalTypesToCache (result.AdditionalTypes.Where (kvp => !kvp.Key.Equals (additionalTypeID)));

              return result.Type;
            }
            finally
            {
              _assemblyContextPool.Enqueue (assemblyContext);
            }
          },
          LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public void LoadTypes (IEnumerable<Type> generatedTypes)
    {
      ArgumentUtility.CheckNotNull ("generatedTypes", generatedTypes);

      // Dequeuing all assembly contexts is not required for consistent loading of types 
      // but helps to ensure that the pre-generated types are preferred.
      var assemblyContexts = _assemblyContextPool.DequeueAll();
      try
      {
        generatedTypes
            .AsParallel()
            .WithDegreeOfParallelism (assemblyContexts.Length)
            .ForAll (
                type =>
                {
                  if (_typeAssembler.IsAssembledType (type))
                    LoadAssembledType (type);
                  else
                    LoadAdditionalType (type);
                });
      }
      finally
      {
        foreach (var assemblyContext in assemblyContexts)
          _assemblyContextPool.Enqueue (assemblyContext);
      }
    }

    private void LoadAssembledType (Type assembledType)
    {
      var typeID = _typeAssembler.ExtractTypeID (assembledType);
      _assembledTypes.TryAdd (typeID, GetLazyType (assembledType));
    }

    private void LoadAdditionalType (Type additionalType)
    {
      var additionalTypeID = _typeAssembler.GetAdditionalTypeID (additionalType);
      if (additionalTypeID == null)
        return;

      _additionalTypes.TryAdd (additionalTypeID, GetLazyType (additionalType));
    }

    private void AddAdditionalTypesToCache (IEnumerable<KeyValuePair<object, Type>> additionalTypes)
    {
      foreach (var kvp in additionalTypes)
      {
        var lazyValue = GetLazyType (kvp.Value);
        _additionalTypes.AddOrUpdate (kvp.Key, lazyValue, (o, lazy) => lazyValue);
      }
    }

    private Lazy<Type> GetLazyType (Type type)
    {
      return new Lazy<Type> (() => type, LazyThreadSafetyMode.PublicationOnly);
    }
  }
}