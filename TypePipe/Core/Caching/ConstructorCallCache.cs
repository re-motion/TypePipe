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
using Remotion.TypePipe.CodeGeneration;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves the generated type's constructors for the requested type from the cache.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class ConstructorCallCache : IConstructorCallCache
  {
    private readonly ConcurrentDictionary<ConstructionKey, Delegate> _constructorCalls = new ConcurrentDictionary<ConstructionKey, Delegate>();
    private readonly Func<ConstructionKey, Delegate> _createConstructorCallFunc;
    private readonly ITypeCache _typeCache;
    private readonly IConstructorDelegateFactory _constructorDelegateFactory;

    public ConstructorCallCache (ITypeCache typeCache, IConstructorDelegateFactory constructorDelegateFactory)
    {
      ArgumentUtility.CheckNotNull ("typeCache", typeCache);
      ArgumentUtility.CheckNotNull ("constructorDelegateFactory", constructorDelegateFactory);

      _typeCache = typeCache;
      _constructorDelegateFactory = constructorDelegateFactory;
      _createConstructorCallFunc = CreateConstructorCall;
    }

    public Delegate GetOrCreateConstructorCall (AssembledTypeID typeID, Type delegateType, bool allowNonPublic)
    {
      // Using Assertion.DebugAssert because it will be compiled away.
      Assertion.DebugAssert (delegateType != null && typeof (Delegate).IsAssignableFrom (delegateType));

      var constructionKey = new ConstructionKey (typeID, delegateType, allowNonPublic);
      return _constructorCalls.GetOrAdd (constructionKey, _createConstructorCallFunc);
    }

    private Delegate CreateConstructorCall (ConstructionKey key)
    {
      var assembledType = _typeCache.GetOrCreateType (key.TypeID);
      return _constructorDelegateFactory.CreateConstructorCall (key.TypeID.RequestedType, assembledType, key.DelegateType, key.AllowNonPublic);
    }
  }
}