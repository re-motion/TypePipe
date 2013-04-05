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
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  public class TypeCache : ITypeCache
  {
    // Storing the delegates as static readonly fields has two advantages for performance:
    // 1) It ensures that no closure is created.
    // 2) We do not create new delegate instances every time a cache key is computed.
    private static readonly Func<ICacheKeyProvider, Type, object> s_fromRequestedType = (ckp, t) => ckp.GetCacheKey (t);
    private static readonly Func<ICacheKeyProvider, Type, object> s_fromGeneratedType = (ckp, t) => ckp.RebuildCacheKey (t);

    private readonly object _typeLock = new object();
    private readonly object _ctorLock = new object();
    private readonly object _codeLock = new object();
    private readonly Dictionary<object[], Type> _types = new Dictionary<object[], Type> (new CompoundCacheKeyEqualityComparer());
    private readonly Dictionary<object[], Delegate> _constructorCalls = new Dictionary<object[], Delegate> (new CompoundCacheKeyEqualityComparer());
    private readonly Dictionary<string, object> _participantState = new Dictionary<string, object>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly ITypeAssemblyContextCodeGenerator _typeAssemblyContextCodeGenerator;
    private readonly IConstructorFinder _constructorFinder;
    private readonly IDelegateFactory _delegateFactory;

    public TypeCache (
        ITypeAssembler typeAssembler,
        ITypeAssemblyContextCodeGenerator typeAssemblyContextCodeGenerator,
        IConstructorFinder constructorFinder,
        IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("typeAssemblyContextCodeGenerator", typeAssemblyContextCodeGenerator);
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);

      _typeAssembler = typeAssembler;
      _typeAssemblyContextCodeGenerator = typeAssemblyContextCodeGenerator;
      _constructorFinder = constructorFinder;
      _delegateFactory = delegateFactory;
    }

    public string ParticipantConfigurationID
    {
      get { return _typeAssembler.ParticipantConfigurationID; }
    }

    public Type GetOrCreateType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      var key = GetTypeKey (requestedType, s_fromRequestedType, requestedType);
      return GetOrCreateType (key, requestedType);
    }

    public Delegate GetOrCreateConstructorCall (Type requestedType, Type delegateType, bool allowNonPublic)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var key = GetConstructorKey (requestedType, delegateType, allowNonPublic);

      Delegate constructorCall;
      lock (_ctorLock)
      {
        if (_constructorCalls.TryGetValue (key, out constructorCall))
          return constructorCall;
      }

      var typeKey = GetTypeKeyFromConstructorKey (key);
      // TODO Review2: This potentially redoes a lot of work, maybe better work with
      // Monitor.Enter() try{ GetOrCreateType() }finally{Monitor.Release()}; ??
      var generatedType = GetOrCreateType (typeKey, requestedType);
      var ctorSignature = _delegateFactory.GetSignature (delegateType);
      var constructor = _constructorFinder.GetConstructor (generatedType, ctorSignature.Item1, allowNonPublic, requestedType, ctorSignature.Item1);
      constructorCall = _delegateFactory.CreateConstructorCall (constructor, delegateType);

      lock (_ctorLock)
      {
        if (!_constructorCalls.ContainsKey (key))
          _constructorCalls.Add (key, constructorCall);
      }

      return constructorCall;
    }

    public void LoadTypes (IEnumerable<Type> generatedTypes)
    {
      ArgumentUtility.CheckNotNull ("generatedTypes", generatedTypes);

      var proxyTypes = new HashSet<Type>();
      var additionalTypes = new HashSet<Type>();

      foreach (var type in generatedTypes)
      {
        if (type.IsDefined (typeof (ProxyTypeAttribute), inherit: false))
          proxyTypes.Add (type);
        else
          additionalTypes.Add (type);
      }

      var keysAndTypes = proxyTypes.Select (t => new { Key = GetTypeKey (t.BaseType, s_fromGeneratedType, t), Type = t }).ToList();

      lock (_typeLock)
      {
        foreach (var p in keysAndTypes)
        {
          if (_types.ContainsKey (p.Key))
            proxyTypes.Remove (p.Type);
          else
            _types.Add (p.Key, p.Type);
        }

        // TODO Review: This must be inside _typeLock so that, _participantState is also guarded?!
        var loadedTypesContext = new LoadedTypesContext (proxyTypes, additionalTypes, _participantState);
        _typeAssembler.RebuildParticipantState (loadedTypesContext);
      }
    }

    private Type GetOrCreateType (object[] typeKey, Type requestedType)
    {
      Type generatedType;
      lock (_typeLock)
      {
        if (!_types.TryGetValue (typeKey, out generatedType))
        {
          generatedType = _typeAssembler.AssembleType (requestedType, _participantState, _typeAssemblyContextCodeGenerator);
          _types.Add (typeKey, generatedType);
        }
      }

      return generatedType;
    }

    private object[] GetTypeKey (Type requestedType, Func<ICacheKeyProvider, Type, object> cacheKeyProviderMethod, Type fromType)
    {
      var key = _typeAssembler.GetCompoundCacheKey (cacheKeyProviderMethod, fromType, freeSlotsAtStart: 1);
      key[0] = requestedType;

      return key;
    }

    private object[] GetConstructorKey (Type requestedType, Type delegateType, bool allowNonPublic)
    {
      var key = _typeAssembler.GetCompoundCacheKey (s_fromRequestedType, requestedType, freeSlotsAtStart: 3);
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