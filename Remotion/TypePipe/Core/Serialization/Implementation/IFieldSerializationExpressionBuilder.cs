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

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Collections;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// This interfaces encapsulates getting a serialized field mapping from a greater list of fields and building serialization and deserialization 
  /// expressions.
  /// </summary>
  public interface IFieldSerializationExpressionBuilder
  {
    IEnumerable<Tuple<string, FieldInfo>> GetSerializedFieldMapping (IEnumerable<FieldInfo> fields);

    IEnumerable<Expression> BuildFieldSerializationExpressions (
        Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> fieldMapping);

    IEnumerable<Expression> BuildFieldDeserializationExpressions (
        Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> fieldMapping);
  }
}