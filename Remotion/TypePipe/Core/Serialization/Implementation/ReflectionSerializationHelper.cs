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
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// A static helper class for serializing object state.
  /// This class is used by <see cref="SerializationParticipant"/> and <see cref="ReflectionDeserializationSurrogate"/>.
  /// </summary>
  public static class ReflectionSerializationHelper
  {
    private static readonly ISerializableFieldFinder s_serializableFieldFinder = new SerializableFieldFinder();

    public static void AddFieldValues (SerializationInfo serializationInfo, object instance)
    {
      ArgumentUtility.CheckNotNull ("serializationInfo", serializationInfo);
      ArgumentUtility.CheckNotNull ("instance", instance);

      var members = FormatterServices.GetSerializableMembers (instance.GetType());
      var mapping = s_serializableFieldFinder.GetSerializableFieldMapping (members.Cast<FieldInfo>()).ToArray();
      var data = FormatterServices.GetObjectData (instance, members);

      for (int i = 0; i < mapping.Length; i++)
        serializationInfo.AddValue (mapping[i].Item1, data[i]);
    }

    public static void PopulateFields (SerializationInfo serializationInfo, object instance)
    {
      ArgumentUtility.CheckNotNull ("serializationInfo", serializationInfo);
      ArgumentUtility.CheckNotNull ("instance", instance);

      var members = FormatterServices.GetSerializableMembers (instance.GetType());
      var mapping = s_serializableFieldFinder.GetSerializableFieldMapping (members.Cast<FieldInfo>());
      var data = mapping.Select (m => serializationInfo.GetValue (m.Item1, m.Item2.FieldType)).ToArray();

      FormatterServices.PopulateObjectMembers (instance, members, data);
    }
  }
}