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

    private static readonly CompoundCacheKeyEqualityComparer s_comparer = new CompoundCacheKeyEqualityComparer();

    /// <summary>Guards access to<see cref="_participantState"/> and serializes execution of code generation and state rebuilding.</summary>
    private readonly object _codeGenerationLock;

    private readonly ConcurrentDictionary<object[], Type> _types = new ConcurrentDictionary<object[], Type> (s_comparer);
    private readonly ConcurrentDictionary<object[], Delegate> _constructorCalls = new ConcurrentDictionary<object[], Delegate> (s_comparer);
    private readonly Dictionary<string, object> _participantState = new Dictionary<string, object>();

    private readonly ITypeAssembler _typeAssembler;
    private readonly ITypeAssemblyContextCodeGenerator _typeAssemblyContextCodeGenerator;
    private readonly IConstructorFinder _constructorFinder;
    private readonly IDelegateFactory _delegateFactory;

    public TypeCache (
        ITypeAssembler typeAssembler,
        object codeGenerationLock,
        ITypeAssemblyContextCodeGenerator typeAssemblyContextCodeGenerator,
        IConstructorFinder constructorFinder,
        IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("codeGenerationLock", codeGenerationLock);
      ArgumentUtility.CheckNotNull ("typeAssemblyContextCodeGenerator", typeAssemblyContextCodeGenerator);
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);

      _typeAssembler = typeAssembler;
      _codeGenerationLock = codeGenerationLock;
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
      return GetOrCreateConstructorCall (key, requestedType, delegateType, allowNonPublic);
    }

    public void LoadTypes (IEnumerable<Type> generatedTypes)
    {
      ArgumentUtility.CheckNotNull ("generatedTypes", generatedTypes);

      var assembledTypes = new HashSet<Type>();
      var additionalTypes = new HashSet<Type>();

      foreach (var type in generatedTypes)
      {
        // TODO Review: Move to ITypeAssembler.IsAssembledType
        if (type.IsDefined (typeof (ProxyTypeAttribute), inherit: false))
          assembledTypes.Add (type);
        else
          additionalTypes.Add (type);
      }

      var keysAndTypes = assembledTypes.Select (t => new { Key = GetTypeKey (t.BaseType, s_fromGeneratedType, t), Type = t }).ToList();

      lock (_codeGenerationLock)
      {
        foreach (var p in keysAndTypes)
        {
          if (_types.ContainsKey (p.Key))
            assembledTypes.Remove (p.Type);
          else
            _types.Add (p.Key, p.Type);
        }

        var loadedTypesContext = new LoadedTypesContext (assembledTypes, additionalTypes, _participantState);
        _typeAssembler.RebuildParticipantState (loadedTypesContext);
      }
    }

    private Type GetOrCreateType (object[] key, Type requestedType)
    {
      Type generatedType;
      if (_types.TryGetValue (key, out generatedType))
        return generatedType;

      // TODO Review: Refactor
      // return _codeManager.GenerateCodeForTypeCache (_types, _typeAssembler, _participantState, _typeAssemblyContextCodeGenerator, requestedType);

      lock (_codeGenerationLock)
      {
        if (_types.TryGetValue (key, out generatedType))
          return generatedType;

        generatedType = _typeAssembler.AssembleType (requestedType, _participantState, _typeAssemblyContextCodeGenerator);
        _types.Add (key, generatedType);
      }

      return generatedType;
    }

    private Delegate GetOrCreateConstructorCall (object[] key, Type requestedType, Type delegateType, bool allowNonPublic)
    {
      Delegate constructorCall;
      if (_constructorCalls.TryGetValue (key, out constructorCall))
        return constructorCall;

      lock (_codeGenerationLock)
      {
        if (_constructorCalls.TryGetValue (key, out constructorCall))
          return constructorCall;

        var typeKey = GetTypeKeyFromConstructorKey (key);
        var generatedType = GetOrCreateType (typeKey, requestedType);
        var ctorSignature = _delegateFactory.GetSignature (delegateType);
        var constructor = _constructorFinder.GetConstructor (generatedType, ctorSignature.Item1, allowNonPublic, requestedType, ctorSignature.Item1);

        constructorCall = _delegateFactory.CreateConstructorCall (constructor, delegateType);
        _constructorCalls.Add (key, constructorCall);
      }

      return constructorCall;
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