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
using Remotion.Utilities;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  public class TypeCache : ITypeCache
  {
    private readonly Dictionary<object[], Type> _types = new Dictionary<object[], Type> (new CompoundCacheKeyEqualityComparer());
    private readonly Dictionary<object[], Delegate> _constructorCalls = new Dictionary<object[], Delegate> (new CompoundCacheKeyEqualityComparer());

    private readonly ITypeAssembler _typeAssembler;
    private readonly IDelegateFactory _delegateFactory;

    public TypeCache (ITypeAssembler typeAssembler, IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);

      _typeAssembler = typeAssembler;
      _delegateFactory = delegateFactory;
    }

    public Type GetOrCreateType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      var cacheKey = _typeAssembler.GetCompoundCacheKey (requestedType, freeSlotsAtStart: 0);

      return GetOrCreateType (requestedType, cacheKey);
    }

    public Delegate GetOrCreateConstructorCall (Type requestedType, Type[] parameterTypes, bool allowNonPublic, Type delegateType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      const int additionalCacheKeyElements = 2;
      var key = _typeAssembler.GetCompoundCacheKey (requestedType, freeSlotsAtStart: additionalCacheKeyElements);
      key[0] = delegateType;
      key[1] = allowNonPublic;

      // No locking!
      //if (_constructorCalls.TryGetValue (key, out constructorCall))
      //  return constructorCall;

      Delegate constructorCall;
      lock (_constructorCalls)
      {
        if (!_constructorCalls.TryGetValue (key, out constructorCall))
        {
          // TODO: better option than copying the array?
          int typeKeyLength = key.Length - additionalCacheKeyElements;
          var typeKey = new object[typeKeyLength];
          Array.Copy (key, additionalCacheKeyElements, typeKey, 0, typeKeyLength);
          var generatedType = GetOrCreateType (requestedType, typeKey);

          constructorCall = _delegateFactory.CreateConstructorCall (generatedType, parameterTypes, allowNonPublic, delegateType);
          _constructorCalls.Add (key, constructorCall);
        }
      }
      return constructorCall;
    }

    private Type GetOrCreateType (Type requestedType, object[] key)
    {
      // No locking!
      //if (_types.TryGetValue (key, out generatedType))
      //  return generatedType;

      Type generatedType;
      lock (_types)
      {
        if (!_types.TryGetValue (key, out generatedType))
        {
          generatedType = _typeAssembler.AssembleType (requestedType);
          _types.Add (key, generatedType);
        }
      }

      return generatedType;
    }
  }
}