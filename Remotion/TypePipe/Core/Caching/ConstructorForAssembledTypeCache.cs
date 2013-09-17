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
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Retrieves construction delegates for assembled types from a cache or creates a new one.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is implemented as a dedicated cache (rather than going from assembled type to requested type and then
  /// getting the constructor delegate) because we want to ensure that if someone requests a constructor for
  /// an assembled type, that ctor should instantiate <b>exactly</b> that type. If we went via requested type, the
  /// pipeline might create a new assembled type for that requested type, and then we'd return an object of a similar, but incompatible
  /// assembled type from what the user specified.
  /// </para>
  /// <para>
  /// (Required for re-mix's feature of saying ObjectFactory.Create (assembledType).)
  /// </para>
  /// </remarks>
  /// <threadsafety static="true" instance="true"/>
  public class ConstructorForAssembledTypeCache : IConstructorForAssembledTypeCache
  {
    private readonly ITypeAssembler _typeAssembler;
    private readonly IConstructorDelegateFactory _constructorDelegateFactory;

    private readonly ConcurrentDictionary<ConstructorForAssembledTypeCacheKey, Delegate> _constructorCalls =
        new ConcurrentDictionary<ConstructorForAssembledTypeCacheKey, Delegate>();

    private readonly Func<ConstructorForAssembledTypeCacheKey, Delegate> _createConstructorCallFunc;

    public ConstructorForAssembledTypeCache (ITypeAssembler typeAssembler, IConstructorDelegateFactory constructorDelegateFactory)
    {
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);
      ArgumentUtility.CheckNotNull ("constructorDelegateFactory", constructorDelegateFactory);

      _typeAssembler = typeAssembler;
      _constructorDelegateFactory = constructorDelegateFactory;
      _createConstructorCallFunc = CreateConstructorCall;
    }

    public Delegate GetOrCreateConstructorCall (Type assembledType, Type delegateType, bool allowNonPublic)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var reverseConstructionKey = new ConstructorForAssembledTypeCacheKey (assembledType, delegateType, allowNonPublic);

      return _constructorCalls.GetOrAdd (reverseConstructionKey, _createConstructorCallFunc);
    }

    private Delegate CreateConstructorCall (ConstructorForAssembledTypeCacheKey key)
    {
      var requestedType = _typeAssembler.GetRequestedType (key.AssembledType);
      return _constructorDelegateFactory.CreateConstructorCall (requestedType, key.AssembledType, key.DelegateType, key.AllowNonPublic);
    }
  }
}