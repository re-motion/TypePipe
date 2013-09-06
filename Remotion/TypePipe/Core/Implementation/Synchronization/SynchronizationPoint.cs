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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation.Synchronization
{
  /// <summary>
  /// Guards all access to code generation capabilities.
  /// </summary>
  public class SynchronizationPoint : ISynchronizationPoint
  {
    private readonly object _codeGenerationLock = new object();

    private readonly ITypeAssembler _typeAssembler;
    private readonly AssemblyContext _assemblyContext;

    public SynchronizationPoint (ITypeAssembler typeAssembler, AssemblyContext assemblyContext)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("assemblyContext", assemblyContext);

      _typeAssembler = typeAssembler;
      _assemblyContext = assemblyContext;
    }

    //TODO 5840: Setting the AssemblyDirectory to PipelineFactory.Settings
    public string AssemblyDirectory
    {
      get { lock (_codeGenerationLock) return _assemblyContext.GeneratedCodeFlusher.AssemblyDirectory; }
    }

    //TODO 5840: Setting the AssemblyDirectory to PipelineFactory.Settings
    public string AssemblyNamePattern
    {
      get { lock (_codeGenerationLock) return _assemblyContext.GeneratedCodeFlusher.AssemblyNamePattern; }
    }

    //TODO 5840: Setting the AssemblyDirectory to PipelineFactory.Settings
    public void SetAssemblyDirectory (string assemblyDirectory)
    {
      lock (_codeGenerationLock)
        _assemblyContext.GeneratedCodeFlusher.SetAssemblyDirectory (assemblyDirectory);
    }

    //TODO 5840: Setting the AssemblyDirectory to PipelineFactory.Settings
    public void SetAssemblyNamePattern (string assemblyNamePattern)
    {
      lock (_codeGenerationLock)
        _assemblyContext.GeneratedCodeFlusher.SetAssemblyNamePattern (assemblyNamePattern);
    }

    //TODO 5840: Affects all AssemblyContexts in the pool.
    public string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      lock (_codeGenerationLock)
        return _assemblyContext.GeneratedCodeFlusher.FlushCodeToDisk (assemblyAttributes);
    }

    // TODO 5840: Move out from SyncPoint to caller.
    public bool IsAssembledType (Type type)
    {
      return _typeAssembler.IsAssembledType (type);
    }

    // TODO 5840: Move out from SyncPoint.
    public AssembledTypeID ExtractTypeID (Type assembledType)
    {
      return _typeAssembler.ExtractTypeID (assembledType);
    }

    // TODO 5840: Move out from SyncPoint.
    public Type GetRequestedType (Type assembledType)
    {
      return _typeAssembler.GetRequestedType (assembledType);
    }

    // TODO 5840: Move out from SyncPoint.
    public AssembledTypeID GetTypeID (Type assembledType)
    {
      return _typeAssembler.ExtractTypeID (assembledType);
    }

    public Type GetOrGenerateType (ConcurrentDictionary<AssembledTypeID, Type> types, AssembledTypeID typeID)
    {
      ArgumentUtility.CheckNotNull ("types", types);

      Type generatedType;
      lock (_codeGenerationLock)
      {
        if (types.TryGetValue (typeID, out generatedType))
          return generatedType;

        generatedType = _typeAssembler.AssembleType (typeID, _assemblyContext.ParticipantState, _assemblyContext.MutableTypeBatchCodeGenerator);
        AddTo (types, typeID, generatedType);
      }

      return generatedType;
    }

    public void RebuildParticipantState (
        ConcurrentDictionary<AssembledTypeID, Type> types,
        IEnumerable<KeyValuePair<AssembledTypeID, Type>> keysToAssembledTypes,
        IEnumerable<Type> additionalTypes)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("keysToAssembledTypes", keysToAssembledTypes);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);

      var loadedAssembledTypes = new List<Type>();

      lock (_codeGenerationLock)
      {
        foreach (var p in keysToAssembledTypes.Where (p => !types.ContainsKey (p.Key)))
        {
          AddTo (types, p.Key, p.Value);
          loadedAssembledTypes.Add (p.Value);
        }

        _typeAssembler.RebuildParticipantState (loadedAssembledTypes, additionalTypes, _assemblyContext.ParticipantState);
      }
    }

    public Type GetOrGenerateAdditionalType (object additionalTypeID)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);

      lock (_codeGenerationLock)
      {
        return _typeAssembler.GetOrAssembleAdditionalType (
            additionalTypeID,
            _assemblyContext.ParticipantState,
            _assemblyContext.MutableTypeBatchCodeGenerator);
      }
    }

    private void AddTo<TKey, TValue> (ConcurrentDictionary<TKey, TValue> concurrentDictionary, TKey key, TValue value)
    {
      if (!concurrentDictionary.TryAdd (key, value))
        throw new ArgumentException ("Key already exists.");
    }
  }
}