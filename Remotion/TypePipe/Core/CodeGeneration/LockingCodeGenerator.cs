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
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Guards all access to code generation capabilities.
  /// </summary>
  public class LockingCodeGenerator : IGeneratedCodeFlusher, ITypeCacheCodeGenerator
  {
    private readonly object _codeGenerationLock = new object();

    private readonly IGeneratedCodeFlusher _generatedCodeFlusher;
    private readonly IConstructorFinder _constructorFinder;
    private readonly IDelegateFactory _delegateFactory;

    public LockingCodeGenerator (IGeneratedCodeFlusher generatedCodeFlusher, IConstructorFinder constructorFinder, IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("generatedCodeFlusher", generatedCodeFlusher);
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);

      _generatedCodeFlusher = generatedCodeFlusher;
      _constructorFinder = constructorFinder;
      _delegateFactory = delegateFactory;
    }

    public string AssemblyDirectory
    {
      get { lock (_codeGenerationLock) return _generatedCodeFlusher.AssemblyDirectory; }
    }

    public string AssemblyNamePattern
    {
      get { lock (_codeGenerationLock) return _generatedCodeFlusher.AssemblyNamePattern; }
    }

    public void SetAssemblyDirectory (string assemblyDirectory)
    {
      lock (_codeGenerationLock)
        _generatedCodeFlusher.SetAssemblyDirectory (assemblyDirectory);
    }

    public void SetAssemblyNamePattern (string assemblyNamePattern)
    {
      lock (_codeGenerationLock)
        _generatedCodeFlusher.SetAssemblyNamePattern (assemblyNamePattern);
    }

    public string FlushCodeToDisk (CustomAttributeDeclaration assemblyAttribute)
    {
      lock (_codeGenerationLock)
        return _generatedCodeFlusher.FlushCodeToDisk (assemblyAttribute);
    }

    public Type GetOrGenerateType (
        ConcurrentDictionary<object[], Type> types,
        object[] typeKey,
        ITypeAssembler typeAssembler,
        Type requestedType,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("typeKey", typeKey);
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      Type generatedType;
      lock (_codeGenerationLock)
      {
        if (types.TryGetValue (typeKey, out generatedType))
          return generatedType;

        generatedType = typeAssembler.AssembleType (requestedType, participantState, mutableTypeBatchCodeGenerator);
        types.Add (typeKey, generatedType);
      }

      return generatedType;
    }

    // TODO Review: Design flaw; context parameter?!
    public Delegate GetOrGenerateConstructorCall (
        ConcurrentDictionary<object[], Delegate> constructorCalls,
        object[] constructorKey,
        ConcurrentDictionary<object[], Type> types,
        object[] typeKey,
        ITypeAssembler typeAssembler,
        Type requestedType,
        Type delegateType,
        bool allowNonPublic,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("constructorCalls", constructorCalls);
      ArgumentUtility.CheckNotNull ("constructorKey", constructorKey);
      ArgumentUtility.CheckNotNull ("typeKey", typeKey);
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      Delegate constructorCall;
      lock (_codeGenerationLock)
      {
        if (constructorCalls.TryGetValue (constructorKey, out constructorCall))
          return constructorCall;

        var generatedType = GetOrGenerateType (types, typeKey, typeAssembler, requestedType, participantState, mutableTypeBatchCodeGenerator);
        var ctorSignature = _delegateFactory.GetSignature (delegateType);
        var constructor = _constructorFinder.GetConstructor (generatedType, ctorSignature.Item1, allowNonPublic, requestedType, ctorSignature.Item1);

        constructorCall = _delegateFactory.CreateConstructorCall (constructor, delegateType);
        constructorCalls.Add (constructorKey, constructorCall);
      }

      return constructorCall;
    }

    public void RebuildParticipantState (
        ConcurrentDictionary<object[], Type> types,
        IEnumerable<KeyValuePair<object[], Type>> keysToAssembledTypes,
        HashSet<Type> assembledTypes,
        IEnumerable<Type> additionalTypes,
        ITypeAssembler typeAssembler,
        IDictionary<string, object> participantState)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("keysToAssembledTypes", keysToAssembledTypes);
      ArgumentUtility.CheckNotNull ("assembledTypes", assembledTypes);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);

      lock (_codeGenerationLock)
      {
        foreach (var p in keysToAssembledTypes)
        {
          if (types.ContainsKey (p.Key))
            assembledTypes.Remove (p.Value);
          else
            types.Add (p.Key, p.Value);
        }

        var loadedTypesContext = new LoadedTypesContext (assembledTypes, additionalTypes, participantState);
        typeAssembler.RebuildParticipantState (loadedTypesContext);
      }
    }
  }
}