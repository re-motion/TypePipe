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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Implements <see cref="IReflectionService"/> by delegating to the contained <see cref="ITypeAssembler"/> and <see cref="ITypeCache"/> instances.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class ReflectionService : IReflectionService
  {
    private readonly ITypeAssembler _typeAssembler;
    private readonly ITypeCache _typeCache;
    private readonly IConstructorCallCache _constructorCallCache;
    private readonly IConstructorForAssembledTypeCache _constructorForAssembledTypeCache;

    public ReflectionService (
        ITypeAssembler typeAssembler,
        ITypeCache typeCache,
        IConstructorCallCache constructorCallCache,
        IConstructorForAssembledTypeCache constructorForAssembledTypeCache)
    {
      ArgumentUtility.CheckNotNull (nameof(typeAssembler), typeAssembler);
      ArgumentUtility.CheckNotNull (nameof(typeCache), typeCache);
      ArgumentUtility.CheckNotNull (nameof(constructorCallCache), constructorCallCache);
      ArgumentUtility.CheckNotNull (nameof(constructorForAssembledTypeCache), constructorForAssembledTypeCache);

      _typeAssembler = typeAssembler;
      _typeCache = typeCache;
      _constructorCallCache = constructorCallCache;
      _constructorForAssembledTypeCache = constructorForAssembledTypeCache;
    }

    public bool IsAssembledType (Type type)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);

      return _typeAssembler.IsAssembledType (type);
    }

    public Type GetRequestedType (Type assembledType)
    {
      ArgumentUtility.CheckNotNull (nameof(assembledType), assembledType);

      return _typeAssembler.GetRequestedType (assembledType);
    }

    public AssembledTypeID GetTypeIDForRequestedType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull (nameof(requestedType), requestedType);

      return _typeAssembler.ComputeTypeID (requestedType);
    }

    public AssembledTypeID GetTypeIDForAssembledType (Type assembledType)
    {
      ArgumentUtility.CheckNotNull (nameof(assembledType), assembledType);

      return _typeAssembler.ExtractTypeID (assembledType);
    }

    public Type GetAssembledType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull (nameof(requestedType), requestedType);

      var typeID = _typeAssembler.ComputeTypeID (requestedType);
      return _typeCache.GetOrCreateType (typeID);
    }

    public Type GetAssembledType (AssembledTypeID typeID)
    {
      return _typeCache.GetOrCreateType (typeID);
    }

    public Type GetAdditionalType (object additionalTypeID)
    {
      ArgumentUtility.CheckNotNull (nameof(additionalTypeID), additionalTypeID);

      return _typeCache.GetOrCreateAdditionalType (additionalTypeID);
    }

    public object InstantiateAssembledType (AssembledTypeID typeID, ParamList constructorArguments, bool allowNonPublicConstructor)
    {
      ArgumentUtility.CheckNotNull (nameof(constructorArguments), constructorArguments);

      var constructorCall = _constructorCallCache.GetOrCreateConstructorCall (typeID, constructorArguments.FuncType, allowNonPublicConstructor);
      var instance = constructorArguments.InvokeFunc (constructorCall);

      return instance;
    }

    public object InstantiateAssembledType (Type assembledType, ParamList constructorArguments, bool allowNonPublicConstructor)
    {
      ArgumentUtility.CheckNotNull (nameof(assembledType), assembledType);
      ArgumentUtility.CheckNotNull (nameof(constructorArguments), constructorArguments);

      var constructorCall = _constructorForAssembledTypeCache.GetOrCreateConstructorCall (
          assembledType,
          constructorArguments.FuncType,
          allowNonPublicConstructor);

      var instance = constructorArguments.InvokeFunc (constructorCall);

      return instance;
    }

    public void PrepareExternalUninitializedObject (object instance, InitializationSemantics initializationSemantics)
    {
      ArgumentUtility.CheckNotNull (nameof(instance), instance);

      var initializableInstance = instance as IInitializableObject;
      if (initializableInstance != null)
        initializableInstance.Initialize (initializationSemantics);
    }
  }
}