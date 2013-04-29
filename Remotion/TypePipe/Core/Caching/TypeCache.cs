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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  public class TypeCache : ITypeCache
  {
    private static readonly CompoundCacheKeyEqualityComparer s_comparer = new CompoundCacheKeyEqualityComparer();

    private readonly ConcurrentDictionary<object[], Type> _types = new ConcurrentDictionary<object[], Type> (s_comparer);
    private readonly ConcurrentDictionary<object[], Delegate> _constructorCalls = new ConcurrentDictionary<object[], Delegate> (s_comparer);
    private readonly Dictionary<string, object> _participantState = new Dictionary<string, object>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly ITypeCacheSynchronizationPoint _typeCacheSynchronizationPoint;
    private readonly IMutableTypeBatchCodeGenerator _mutableTypeBatchCodeGenerator;

    public TypeCache (
        ITypeAssembler typeAssembler,
        ITypeCacheSynchronizationPoint typeCacheSynchronizationPoint,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("typeCacheSynchronizationPoint", typeCacheSynchronizationPoint);
      ArgumentUtility.CheckNotNull ("mutableTypeBatchCodeGenerator", mutableTypeBatchCodeGenerator);

      _typeAssembler = typeAssembler;
      _typeCacheSynchronizationPoint = typeCacheSynchronizationPoint;
      _mutableTypeBatchCodeGenerator = mutableTypeBatchCodeGenerator;
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
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (requestedType != null);

      var key = GetTypeKey (requestedType);
      return GetOrCreateType (key, requestedType);
    }

    public Delegate GetOrCreateConstructorCall (Type requestedType, Type delegateType, bool allowNonPublic)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (requestedType != null);
      Debug.Assert (delegateType != null && typeof (Delegate).IsAssignableFrom (delegateType));

      var key = GetConstructorKey (requestedType, delegateType, allowNonPublic);
      return GetOrCreateConstructorCall (key, requestedType, delegateType, allowNonPublic);
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

      var keysToAssembledTypes = assembledTypes.Select (CreateKeyValuePair);
      _typeCacheSynchronizationPoint.RebuildParticipantState (_types, keysToAssembledTypes, additionalTypes, _participantState);
    }

    private KeyValuePair<object[], Type> CreateKeyValuePair (Type assembledType)
    {
      var requestedType = _typeAssembler.GetRequestedType (assembledType);
      var compoundID = _typeAssembler.ExtractCompoundID (assembledType);
      var key = new[] { requestedType }.Concat (compoundID).ToArray();

      return new KeyValuePair<object[], Type> (key, assembledType);
    }

    private Type GetOrCreateType (object[] key, Type requestedType)
    {
      Type generatedType;
      if (_types.TryGetValue (key, out generatedType))
        return generatedType;

      return _typeCacheSynchronizationPoint.GetOrGenerateType (_types, key, requestedType, _participantState, _mutableTypeBatchCodeGenerator);
    }

    private Delegate GetOrCreateConstructorCall (object[] key, Type requestedType, Type delegateType, bool allowNonPublic)
    {
      Delegate constructorCall;
      if (_constructorCalls.TryGetValue (key, out constructorCall))
        return constructorCall;

      var typeKey = GetTypeKeyFromConstructorKey (key);
      return _typeCacheSynchronizationPoint.GetOrGenerateConstructorCall (
          _constructorCalls,
          key,
          delegateType,
          allowNonPublic,
          _types,
          typeKey,
          requestedType,
          _participantState,
          _mutableTypeBatchCodeGenerator);
    }

    private object[] GetTypeKey (Type requestedType)
    {
      var key = _typeAssembler.GetCompoundID (requestedType, freeSlotsAtStart: 1);
      key[0] = requestedType;

      return key;
    }

    private object[] GetConstructorKey (Type requestedType, Type delegateType, bool allowNonPublic)
    {
      var key = _typeAssembler.GetCompoundID (requestedType, freeSlotsAtStart: 3);
      key[0] = requestedType;
      key[1] = delegateType;
      key[2] = allowNonPublic;

      return key;
    }

    private object[] GetTypeKeyFromConstructorKey (object[] constructorKey)
    {
      var typeKey = new object[constructorKey.Length - 2];
      typeKey[0] = constructorKey[0];
      Array.Copy (constructorKey, 3, typeKey, 1, constructorKey.Length - 3);

      return typeKey;
    }
  }
}