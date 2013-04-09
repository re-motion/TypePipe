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
using System.Linq;
using Remotion.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Guards all access to code generation capabilities.
  /// </summary>
  public class CodeGenerationSynchronizationPoint : IGeneratedCodeFlusher, ITypeCacheSynchronizationPoint
  {
    private readonly object _codeGenerationLock = new object();

    private readonly IGeneratedCodeFlusher _generatedCodeFlusher;
    private readonly ITypeAssembler _typeAssembler;
    private readonly IConstructorFinder _constructorFinder;
    private readonly IDelegateFactory _delegateFactory;

    public CodeGenerationSynchronizationPoint (
        IGeneratedCodeFlusher generatedCodeFlusher,
        ITypeAssembler typeAssembler,
        IConstructorFinder constructorFinder,
        IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("generatedCodeFlusher", generatedCodeFlusher);
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);

      _generatedCodeFlusher = generatedCodeFlusher;
      _typeAssembler = typeAssembler;
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

    public string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      lock (_codeGenerationLock)
        return _generatedCodeFlusher.FlushCodeToDisk (assemblyAttributes);
    }

    public Type GetOrGenerateType (
        ConcurrentDictionary<object[], Type> types,
        object[] typeKey,
        Type requestedType,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("typeKey", typeKey);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      Type generatedType;
      lock (_codeGenerationLock)
      {
        if (types.TryGetValue (typeKey, out generatedType))
          return generatedType;

        generatedType = _typeAssembler.AssembleType (requestedType, participantState, mutableTypeBatchCodeGenerator);
        types.Add (typeKey, generatedType);
      }

      return generatedType;
    }

    public Delegate GetOrGenerateConstructorCall (
        ConcurrentDictionary<object[], Delegate> constructorCalls,
        object[] constructorKey,
        Type delegateType,
        bool allowNonPublic,
        ConcurrentDictionary<object[], Type> types,
        object[] typeKey,
        Type requestedType,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("constructorCalls", constructorCalls);
      ArgumentUtility.CheckNotNull ("constructorKey", constructorKey);
      ArgumentUtility.CheckNotNull ("typeKey", typeKey);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      Delegate constructorCall;
      lock (_codeGenerationLock)
      {
        if (constructorCalls.TryGetValue (constructorKey, out constructorCall))
          return constructorCall;

        var generatedType = GetOrGenerateType (types, typeKey, requestedType, participantState, mutableTypeBatchCodeGenerator);
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
        IEnumerable<Type> additionalTypes,
        IDictionary<string, object> participantState)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("keysToAssembledTypes", keysToAssembledTypes);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);
      ArgumentUtility.CheckNotNull ("participantState", participantState);

      var loadedAssembledTypes = new List<Type>();

      lock (_codeGenerationLock)
      {
        foreach (var p in keysToAssembledTypes.Where (p => !types.ContainsKey (p.Key)))
        {
          types.Add (p.Key, p.Value);
          loadedAssembledTypes.Add (p.Value);
        }

        var loadedTypesContext = new LoadedTypesContext (loadedAssembledTypes, additionalTypes, participantState);
        _typeAssembler.RebuildParticipantState (loadedTypesContext);
      }
    }
  }
}