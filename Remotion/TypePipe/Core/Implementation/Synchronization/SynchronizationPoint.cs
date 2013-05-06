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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation.Synchronization
{
  /// <summary>
  /// Guards all access to code generation capabilities.
  /// </summary>
  public class SynchronizationPoint : ISynchronizationPoint
  {
    private readonly object _codeGenerationLock = new object();

    private readonly IGeneratedCodeFlusher _generatedCodeFlusher;
    private readonly ITypeAssembler _typeAssembler;
    private readonly IConstructorFinder _constructorFinder;
    private readonly IDelegateFactory _delegateFactory;

    public SynchronizationPoint (
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

    public bool IsAssembledType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      lock (_codeGenerationLock)
        return _typeAssembler.IsAssembledType (type);
    }

    public Type GetRequestedType (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      lock (_codeGenerationLock)
        return _typeAssembler.GetRequestedType (assembledType);
    }

    public AssembledTypeID GetTypeID (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      lock (_codeGenerationLock)
        return _typeAssembler.ExtractTypeID (assembledType);
    }

    public Type GetOrGenerateType (
        ConcurrentDictionary<AssembledTypeID, Type> types,
        AssembledTypeID typeID,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      Type generatedType;
      lock (_codeGenerationLock)
      {
        if (types.TryGetValue (typeID, out generatedType))
          return generatedType;

        generatedType = _typeAssembler.AssembleType (typeID, participantState, mutableTypeBatchCodeGenerator);
        types.Add (typeID, generatedType);
      }

      return generatedType;
    }

    public Delegate GetOrGenerateConstructorCall (
        ConcurrentDictionary<ConstructionKey, Delegate> constructorCalls,
        ConstructionKey constructionKey,
        ConcurrentDictionary<AssembledTypeID, Type> types,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("constructorCalls", constructorCalls);
      ArgumentUtility.CheckNotNull ("types", types);
      ArgumentUtility.CheckNotNull ("participantState", participantState);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      Delegate constructorCall;
      lock (_codeGenerationLock)
      {
        if (constructorCalls.TryGetValue (constructionKey, out constructorCall))
          return constructorCall;

        var typeID = constructionKey.TypeID;
        var assembledType = GetOrGenerateType (types, typeID, participantState, mutableTypeBatchCodeGenerator);
        var ctorSignature = _delegateFactory.GetSignature (constructionKey.DelegateType);
        var constructor = _constructorFinder.GetConstructor (typeID.RequestedType, ctorSignature.Item1, constructionKey.AllowNonPublic, assembledType);

        constructorCall = _delegateFactory.CreateConstructorCall (constructor, constructionKey.DelegateType);
        constructorCalls.Add (constructionKey, constructorCall);
      }

      return constructorCall;
    }

    public void RebuildParticipantState (
        ConcurrentDictionary<AssembledTypeID, Type> types,
        IEnumerable<KeyValuePair<AssembledTypeID, Type>> keysToAssembledTypes,
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

        // TODO 5553: pass in simple values, create context in TypeAssembler.
        var loadedTypesContext = new LoadedTypesContext (loadedAssembledTypes, additionalTypes, participantState);
        _typeAssembler.RebuildParticipantState (loadedTypesContext);
      }
    }

    public Type GetOrGenerateAdditionalType (object additionalTypeID, IDictionary<string, object> participantState)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);
      ArgumentUtility.CheckNotNull ("participantState", participantState);

      lock (_codeGenerationLock)
        return _typeAssembler.RetrieveAdditionalType (additionalTypeID, participantState);
    }
  }
}