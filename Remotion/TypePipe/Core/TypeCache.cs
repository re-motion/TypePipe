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
using Remotion.Collections;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Retrieves the generated type or its constructors for the requested type from the cache or delegates to the contained
  /// <see cref="ITypeAssembler"/> instance.
  /// </summary>
  public class TypeCache : ITypeCache
  {
    private readonly ICache<object[], Type> _types = CacheFactory.CreateWithLocking<object[], Type> (new CompoundCacheKeyEqualityComparer());
    private readonly ICache<object[], IConstructorLookupInfo> _constructors =
        CacheFactory.CreateWithLocking<object[], IConstructorLookupInfo> (new CompoundCacheKeyEqualityComparer());

    private readonly ITypeAssembler _typeAssembler;

    public TypeCache (ITypeAssembler typeAssembler)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);

      _typeAssembler = typeAssembler;
    }

    public Type GetOrCreateType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      var cacheKey = _typeAssembler.GetCompoundCacheKey (requestedType);

      return GetOrCreateType (requestedType, cacheKey);
    }

    public IConstructorLookupInfo GetOrCreateConstructorLookup (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      var cacheKey = _typeAssembler.GetCompoundCacheKey (requestedType);

      // Avoid creation of lambda closure for performance reasons.
      IConstructorLookupInfo constructorLookup;
      if (_constructors.TryGetValue (cacheKey, out constructorLookup))
        return constructorLookup;

      return _constructors.GetOrCreateValue (cacheKey, _ => new ConstructorLookupInfo (GetOrCreateType (requestedType, cacheKey)));
    }

    private Type GetOrCreateType (Type requestedType, object[] cacheKey)
    {
      // Avoid creation of lambda closure for performance reasons.
      Type generatedType;
      if (_types.TryGetValue (cacheKey, out generatedType))
        return generatedType;

      return _types.GetOrCreateValue (cacheKey, _ => _typeAssembler.AssembleType (requestedType));
    }
  }
}