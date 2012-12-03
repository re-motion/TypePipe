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
        OverrideGetObjectData (mutableType);
    }

    private void OverrideGetObjectData (MutableType mutableType)
    {
      var interfaceMethod = MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));
      var getObjectDataOverride = mutableType.GetOrAddMutableMethod (interfaceMethod);
      var serializedFields = mutableType.AddedFields.Where (f => !f.IsStatic);
      getObjectDataOverride.SetBody (
          ctx =>
          {
            var fieldSerializations = SerializeFields (ctx, serializedFields);
            var expressions = EnumerableUtility.Singleton (ctx.PreviousBody).Concat (fieldSerializations);
            return Expression.Block (expressions);
          });
    }

    private IEnumerable<Expression> SerializeFields (MethodBodyModificationContext ctx, IEnumerable<MutableFieldInfo> serializedFields)
    {
      return serializedFields
          .ToLookup (f => f.Name)
          .SelectMany (
              fieldsByName =>
              {
                var fields = fieldsByName.ToArray();
                if (fields.Length == 1)
                  return EnumerableUtility.Singleton (SerializeField (ctx, fields[0].Name, fields[0]));

                return from field in fields
                       let serializationKey = string.Format ("{0}@{1}", field.Name, field.FieldType.FullName)
                       select SerializeField (ctx, serializationKey, field);
              });
    }

    private Expression SerializeField (MethodBodyModificationContext ctx, string serializationKey, MutableFieldInfo field)
    {
      var key = c_serializationKeyPrefix + serializationKey;
      return Expression.Call (ctx.Parameters[0], "AddValue", Type.EmptyTypes, Expression.Constant (key), Expression.Field (ctx.This, field));
    }
  }
}