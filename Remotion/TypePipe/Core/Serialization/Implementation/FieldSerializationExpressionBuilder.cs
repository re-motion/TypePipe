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
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.Serialization.Implementation
{
  /// <summary>
  /// This class implements <see cref="IFieldSerializationExpressionBuilder" />.
  /// It filters filters fields for those that are serializable and creates a mapping so that the field values can be stored or retrieved from a
  /// <see cref="SerializationInfo"/> instance.
  /// The expressions that represent those actions can also be created by this class.
  /// </summary>
  public class FieldSerializationExpressionBuilder : IFieldSerializationExpressionBuilder
  {
    private static readonly MethodInfo s_getValueMethod =
        MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));

    public IEnumerable<Tuple<string, FieldInfo>> GetSerializedFieldMapping (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      var fields = type
          .CreateSequence (t => t.BaseType)
          .SelectMany (t => t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

      return GetSerializedFieldMapping (fields);
    }

    public IEnumerable<Tuple<string, FieldInfo>> GetSerializedFieldMapping (IEnumerable<FieldInfo> fields)
    {
      ArgumentUtility.CheckNotNull ("fields", fields);

      return fields
          .Where (f => !f.IsStatic && (f.Attributes & FieldAttributes.NotSerialized) == 0)
          .Where (f => !f.GetCustomAttributes (typeof (NonSerializedAttribute), false).Any())
          .ToLookup (f => f.Name)
          .SelectMany (
              fieldsByName =>
              {
                var fieldArray = fieldsByName.ToArray();

                var prefix = SerializationParticipant.SerializationKeyPrefix;
                var serializationKeyProvider =
                    fieldArray.Length == 1
                        ? (Func<FieldInfo, string>) (f => prefix + f.Name)
                        : (f => string.Format ("{0}{1}::{2}@{3}", prefix, f.DeclaringType.FullName, f.Name, f.FieldType.FullName));

                return fieldArray.Select (f => Tuple.Create (serializationKeyProvider (f), f));
              });
    }

    public IEnumerable<Expression> BuildFieldSerializationExpressions (
        Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> fieldMapping)
    {
      ArgumentUtility.CheckNotNull ("fieldMapping", fieldMapping);

      return fieldMapping
          .Select (
              entry => (Expression) Expression.Call (
                  serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant (entry.Item1), Expression.Field (@this, entry.Item2)));
    }

    public IEnumerable<Expression> BuildFieldDeserializationExpressions (
        Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> fieldMapping)
    {
      ArgumentUtility.CheckNotNull ("fieldMapping", fieldMapping);

      return fieldMapping
          .Select (
              entry => (Expression) Expression.Assign (
                  Expression.Field (@this, entry.Item2),
                  Expression.Convert (
                      Expression.Call (
                          serializationInfo, s_getValueMethod, Expression.Constant (entry.Item1), Expression.Constant (entry.Item2.FieldType)),
                      entry.Item2.FieldType)));
    }
  }
}