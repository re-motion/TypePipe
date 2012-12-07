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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// Acts as a helper for the .NET deserialization process of modified types that implement <see cref="ISerializable"/> but do not declare a
  /// deserialization constructor.
  /// </summary>
  public class ReflectionSerializationSurrogate : SerializationSurrogateBase
  {
    private readonly IFieldSerializationExpressionBuilder _fieldSerializationExpressionBuilder = new FieldSerializationExpressionBuilder();

    public ReflectionSerializationSurrogate (SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base (serializationInfo, streamingContext)
    {
    }

    protected override object CreateRealObject (IObjectFactory objectFactory, Type underlyingType, StreamingContext context)
    {
      var instance = objectFactory.GetUninitializedObject (underlyingType);
      var type = instance.GetType();

      var mapping = _fieldSerializationExpressionBuilder.GetSerializedFieldMapping (type).ToArray();
      var fields = mapping.Select (m => m.Item2).Cast<MemberInfo>().ToArray();
      var data = mapping.Select (m => SerializationInfo.GetValue (m.Item1, m.Item2.FieldType)).ToArray();

      FormatterServices.PopulateObjectMembers (instance, fields, data);

      return instance;
    }
  }
}