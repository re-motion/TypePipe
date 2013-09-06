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
using System.Linq;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  public class TypeCache : ITypeCache
  {
    private readonly ConcurrentDictionary<AssembledTypeID, Type> _types = new ConcurrentDictionary<AssembledTypeID, Type>();
    private readonly ConcurrentDictionary<ConstructionKey, Delegate> _constructorCalls = new ConcurrentDictionary<ConstructionKey, Delegate>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly ITypeCacheSynchronizationPoint _typeCacheSynchronizationPoint;

    public TypeCache (ITypeAssembler typeAssembler, ITypeCacheSynchronizationPoint typeCacheSynchronizationPoint)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("typeCacheSynchronizationPoint", typeCacheSynchronizationPoint);

      _typeAssembler = typeAssembler;
      _typeCacheSynchronizationPoint = typeCacheSynchronizationPoint;
    }

    public string ParticipantConfigurationID
    {
      get { return _typeAssembler.ParticipantConfigurationID; }
    }

    public ReadOnlyCollection<IParticipant> Participants
    {
      get { return _typeAssembler.Participants; }
    }

    public Type GetOrCreateType (Type requestedType)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert(requestedType != null);

      var typeID = _typeAssembler.ComputeTypeID (requestedType);

      return GetOrCreateType (typeID);
    }

    public Type GetOrCreateType (AssembledTypeID typeID)
    {
      Type assembledType;
      if (_types.TryGetValue (typeID, out assembledType))
        return assembledType;

      return _typeCacheSynchronizationPoint.GetOrGenerateType (_types, typeID);
    }

    public Delegate GetOrCreateConstructorCall (Type requestedType, Type delegateType, bool allowNonPublic)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert (requestedType != null);
      Assertion.DebugAssert (delegateType != null && typeof(Delegate).IsAssignableFrom(delegateType));

      var typeID = _typeAssembler.ComputeTypeID (requestedType);

      return GetOrCreateConstructorCall (typeID, delegateType, allowNonPublic);
    }

    public Delegate GetOrCreateConstructorCall (AssembledTypeID typeID, Type delegateType, bool allowNonPublic)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert (delegateType != null && typeof(Delegate).IsAssignableFrom(delegateType));

      var constructionKey = new ConstructionKey (typeID, delegateType, allowNonPublic);

      Delegate constructorCall;
      if (_constructorCalls.TryGetValue (constructionKey, out constructorCall))
        return constructorCall;

      return _typeCacheSynchronizationPoint.GetOrGenerateConstructorCall (_constructorCalls, constructionKey, _types);
    }

    public void LoadTypes (IEnumerable<Type> generatedTypes)
    {
      ArgumentUtility.CheckNotNull ("generatedTypes", generatedTypes);

      var assembledTypes = new List<Type>();
      var additionalTypes = new List<Type>();

      foreach (var type in generatedTypes)
      {
        if (_typeCacheSynchronizationPoint.IsAssembledType (type))
          assembledTypes.Add (type);
        else
          additionalTypes.Add (type);
      }

      var keysToAssembledTypes = assembledTypes
          .Select (t => new KeyValuePair<AssembledTypeID, Type> (_typeCacheSynchronizationPoint.ExtractTypeID (t), t));
      _typeCacheSynchronizationPoint.RebuildParticipantState (_types, keysToAssembledTypes, additionalTypes);
    }

    public Type GetOrCreateAdditionalType (object additionalTypeID)
    {
      ArgumentUtility.CheckNotNull ("additionalTypeID", additionalTypeID);

      return _typeCacheSynchronizationPoint.GetOrGenerateAdditionalType (additionalTypeID);
    }
  }
}