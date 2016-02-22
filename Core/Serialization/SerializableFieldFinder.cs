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
using System.Reflection;
using System.Runtime.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// This class filters fields, retains the serializable ones and creates a mapping so that the field values can be stored or retrieved from a
  /// <see cref="SerializationInfo"/> instance.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class SerializableFieldFinder : ISerializableFieldFinder
  {
    public IEnumerable<Tuple<string, FieldInfo>> GetSerializableFieldMapping (IEnumerable<FieldInfo> fields)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);

      return fields
          .Where (f => !f.IsStatic && (f.Attributes & FieldAttributes.NotSerialized) == 0)
          .ToLookup (f => f.Name)
          .SelectMany (
              fieldsByName =>
              {
                var fieldArray = fieldsByName.ToArray();

                var prefix = ComplexSerializationEnabler.SerializationKeyPrefix;
                var serializationKeyProvider =
                    fieldArray.Length == 1
                        ? (Func<FieldInfo, string>) (f => prefix + f.Name)
                        : (f => string.Format ("{0}{1}::{2}@{3}", prefix, f.DeclaringType.FullName, f.Name, f.FieldType.FullName));

                return fieldArray.Select (f => Tuple.Create (serializationKeyProvider (f), f));
              });
    }
  }
}