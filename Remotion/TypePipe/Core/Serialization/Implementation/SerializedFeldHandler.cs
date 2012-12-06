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
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// Implements <see cref="ISerializedFieldHandler"/>, e.g. returns field by names mappings and retuby checking if a field should be seby applying a LINQ query to the input fields.
  /// </summary>
  public class SerializedFeldHandler : ISerializedFieldHandler
  {
    private const string c_serializationKeyPrefix = "<tp>";

    public IEnumerable<Tuple<string, FieldInfo>> GetSerializedFieldMapping (IEnumerable<FieldInfo> fields)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);

      return fields
          .Where (f => !f.IsStatic && !f.GetCustomAttributes (typeof (NonSerializedAttribute), false).Any())
          .ToLookup (f => f.Name)
          .SelectMany (
              fieldsByName =>
              {
                var fields1 = fieldsByName.ToArray();

                var serializationKeyProvider =
                    fields1.Length == 1
                        ? (Func<FieldInfo, string>) (f => c_serializationKeyPrefix + f.Name)
                        : (f => string.Format ("{0}{1}@{2}", c_serializationKeyPrefix, f.Name, f.FieldType.FullName));

                return fields1.Select (f => Tuple.Create (serializationKeyProvider (f), f));
              });
    }
  }
}