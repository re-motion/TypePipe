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

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Instances of this interface represent the code generator of the pipeline.
  /// This interface is an implementation detail of <see cref="TypeCache"/> to enable managing a single code generation lock in one place,
  /// that is, the <see cref="CodeManager"/>.
  /// </summary>
  public interface ICodeGenerator
  {
    Type GetOrGenerateType (
        ConcurrentDictionary<object[], Type> types,
        object[] typeKey,
        ITypeAssembler typeAssembler,
        Type requestedType,
        IDictionary<string, object> participantState,
        IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator);

    Delegate GetOrGenerateConstructorCall (ConcurrentDictionary<object[], Delegate> constructorCalls, object[] constructorKey, ConcurrentDictionary<object[], Type> types, object[] typeKey, ITypeAssembler typeAssembler, Type requestedType, Type delegateType, bool allowNonPublic, IDictionary<string, object> participantState, IMutableTypeBatchCodeGenerator mutableTypeBatchCodeGenerator);
  }
}