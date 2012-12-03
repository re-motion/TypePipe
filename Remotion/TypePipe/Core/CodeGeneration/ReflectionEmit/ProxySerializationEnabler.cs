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
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IProxySerializationEnabler"/>.
  /// </summary>
  public class ProxySerializationEnabler : IProxySerializationEnabler
  {
    private const string c_serializationKeyPrefix = "<tp>";

    public void MakeSerializable (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      var implementsSerializable = mutableType.GetInterfaces().Contains (typeof (ISerializable));
      if (implementsSerializable)
      {
        OverrideGetObjectData (mutableType);
        AdaptDeserializationConstructor (mutableType);
      }
    }

    private void OverrideGetObjectData (MutableType mutableType)
    {
      var interfaceMethod = MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));
      var getObjectDataOverride = mutableType.GetOrAddMutableMethod (interfaceMethod);
      getObjectDataOverride.SetBody (
          ctx =>
          {
            var fieldSerializations = EnumerateSerializableFields (ctx, SerializeField);
            var expressions = EnumerableUtility.Singleton (ctx.PreviousBody).Concat (fieldSerializations);
            return Expression.Block (expressions);
          });
    }

    private void AdaptDeserializationConstructor (MutableType mutableType)
    {
      var deserializationConstructor = mutableType.GetConstructor (
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
          null,
          new[] { typeof (SerializationInfo), typeof (StreamingContext) },
          null);
      if (deserializationConstructor == null)
        throw new InvalidOperationException ("The modified type implements 'ISerializable' but does not define a deserialization constructor.");

      var mutableConstructor = mutableType.GetMutableConstructor (deserializationConstructor);
      mutableConstructor.SetBody (
          ctx =>
          {
            var fieldDeserializations = EnumerateSerializableFields (ctx, DeserializeField);
            var expressions = EnumerableUtility.Singleton (ctx.PreviousBody).Concat (fieldDeserializations);
            return Expression.Block (expressions);
          });
    }

    private IEnumerable<Expression> EnumerateSerializableFields (
        MethodBaseBodyContextBase ctx, Func<Expression, Expression, string, FieldInfo, Expression> expressionProvider)
    {
      return ctx
          .DeclaringType.AddedFields
          .Where (f => !f.IsStatic)
          .ToLookup (f => f.Name)
          .SelectMany (
              fieldsByName =>
              {
                var fields = fieldsByName.ToArray();
                if (fields.Length == 1)
                  return EnumerableUtility.Singleton (
                      expressionProvider (ctx.This, ctx.Parameters[0], c_serializationKeyPrefix + fields[0].Name, fields[0]));

                return from field in fields
                       let serializationKey = string.Format ("{0}{1}@{2}", c_serializationKeyPrefix, field.Name, field.FieldType.FullName)
                       select expressionProvider (ctx.This, ctx.Parameters[0], serializationKey, field);
              });
    }

    private Expression SerializeField (Expression @this, Expression serializationInfo, string serializationKey, FieldInfo field)
    {
      return Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant (serializationKey), Expression.Field (@this, field));
    }

    private Expression DeserializeField (Expression @this, Expression serializationInfo, string serializationKey, FieldInfo field)
    {
      var getValueMethod = MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));
      var type = field.FieldType;
      return Expression.Assign (
          Expression.Field (@this, field),
          Expression.Convert (
              Expression.Call (serializationInfo, getValueMethod, Expression.Constant (serializationKey), Expression.Constant (type)), type));
    }
  }
}