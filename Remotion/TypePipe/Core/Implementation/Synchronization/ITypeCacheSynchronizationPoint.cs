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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation.Synchronization
{
  /// <summary>
  /// This interface is an implementation detail of <see cref="TypeCache"/> to enable synchronization of code generation functionalities in one place.
  /// </summary>
  public interface ITypeCacheSynchronizationPoint
  {
    bool IsAssembledType (Type type);

    AssembledTypeID ExtractTypeID (Type assembledType);

    Type GetOrGenerateType (
        ConcurrentDictionary<AssembledTypeID, Type> types,
        AssembledTypeID typeID,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator);

    Delegate GetOrGenerateConstructorCall (
        ConcurrentDictionary<ConstructionKey, Delegate> constructorCalls,
        ConstructionKey constructionKey,
        ConcurrentDictionary<AssembledTypeID, Type> types,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator);

    void RebuildParticipantState (
        ConcurrentDictionary<AssembledTypeID, Type> types,
        IEnumerable<KeyValuePair<AssembledTypeID, Type>> keysToAssembledTypes,
        IEnumerable<Type> additionalTypes,
        IDictionary<string, object> participantState);

    Type GetOrGenerateAdditionalType (
        object additionalTypeID, IDictionary<string, object> participantState, IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator);
  }
}