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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Implements <see cref="IReflectionService"/> by delegating to the contained <see cref="ITypeAssembler"/> and <see cref="ITypeCache"/> instances.
  /// </summary>
  public class ReflectionService : IReflectionService
  {
    private readonly IReflectionServiceSynchronizationPoint _reflectionServiceSynchronizationPoint;
    private readonly ITypeCache _typeCache;

    public ReflectionService (IReflectionServiceSynchronizationPoint reflectionServiceSynchronizationPoint, ITypeCache typeCache)
    {
      ArgumentUtility.CheckNotNull ("reflectionServiceSynchronizationPoint", reflectionServiceSynchronizationPoint);
      ArgumentUtility.CheckNotNull ("typeCache", typeCache);

      _reflectionServiceSynchronizationPoint = reflectionServiceSynchronizationPoint;
      _typeCache = typeCache;
    }

    public bool IsAssembledType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return _reflectionServiceSynchronizationPoint.IsAssembledType (type);
    }

    public Type GetRequestedType (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      return _reflectionServiceSynchronizationPoint.GetRequestedType (assembledType);
    }

    public Type GetAssembledType (Type requestedType)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      return _typeCache.GetOrCreateType (requestedType);
    }

    public Type GetAssembledType (Type requestedType, object typeID)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      var castedTypeID = ArgumentUtility.CheckNotNullAndType<object[]> ("typeID", typeID);

      return _typeCache.GetOrCreateType (requestedType, castedTypeID);
    }
  }
}