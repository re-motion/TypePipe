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
using Remotion.Collections;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IProxySerializationEnabler"/>.
  /// </summary>
  public class ProxySerializationEnabler : IProxySerializationEnabler
  {
    private static readonly MethodInfo s_getObjectDataMetod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));
    private static readonly MethodInfo s_getValueMethod =
        MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));
    private static readonly MethodInfo s_onDeserializationMethod =
        MemberInfoFromExpressionUtility.GetMethod ((IDeserializationCallback obj) => obj.OnDeserialization (null));

    private readonly ISerializableFieldFinder _serializableFieldFinder;

    public ProxySerializationEnabler (ISerializableFieldFinder serializableFieldFinder)
    {
      ArgumentUtility.CheckNotNull ("serializableFieldFinder", serializableFieldFinder);

      _serializableFieldFinder = serializableFieldFinder;
    }

    public void MakeSerializable (ProxyType proxyType, MethodInfo initializationMethod)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      // initializationMethod may be null

      // Base fields are always serialized by the standard .NET serialization or by an implementation of ISerializable on the base type.
      // Added fields are also serialized by the standard .NET serialization, unless the proxy type implements ISerializable. In that case,
      // we need to extend the ISerializable implementation to include the added fields.

      var serializedFieldMapping = _serializableFieldFinder.GetSerializableFieldMapping (proxyType.AddedFields.Cast<FieldInfo>()).ToArray();
      var deserializationConstructor = GetDeserializationConstructor (proxyType);

      // If the base type implements ISerializable but has no deserialization constructor, we can't implement ISerializable correctly, so
      // we don't even try. (SerializationParticipant relies on this behavior.)
      var needsCustomFieldSerialization =
          serializedFieldMapping.Length != 0 && typeof (ISerializable).IsAssignableFromFast (proxyType) && deserializationConstructor != null;

      if (needsCustomFieldSerialization)
      {
        OverrideGetObjectData (proxyType, serializedFieldMapping);
        AdaptDeserializationConstructor (deserializationConstructor, serializedFieldMapping);
      }

      if (initializationMethod != null)
      {
        if (typeof (IDeserializationCallback).IsAssignableFromFast (proxyType))
          OverrideOnDeserialization (proxyType, initializationMethod);
        else if (proxyType.IsSerializableFast())
          ExplicitlyImplementOnDeserialization (proxyType, initializationMethod);
      }
    }

    public bool IsDeserializationConstructor (ConstructorInfo constructor)
    {
      return constructor.GetParameters ().Select (x => x.ParameterType).SequenceEqual (new[] { typeof (SerializationInfo), typeof (StreamingContext) });
    }

    private void OverrideGetObjectData (ProxyType proxyType, Tuple<string, FieldInfo>[] serializedFieldMapping)
    {
      try
      {
        proxyType
            .GetOrAddOverride (s_getObjectDataMetod)
            .SetBody (
                ctx => Expression.Block (
                    typeof (void),
                    new[] { ctx.PreviousBody }.Concat (BuildFieldSerializationExpressions (ctx.This, ctx.Parameters[0], serializedFieldMapping))));
      }
      catch (NotSupportedException exception)
      {
        throw new NotSupportedException (
            "The proxy type implements ISerializable but GetObjectData cannot be overridden. "
            + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.",
            exception);
      }
    }

    private MutableConstructorInfo GetDeserializationConstructor (ProxyType type)
    {
      var parameterTypes = new[] { typeof (SerializationInfo), typeof (StreamingContext) };
      return type.AddedConstructors
                 .SingleOrDefault (c => !c.IsStatic && c.GetParameters().Select (p => p.ParameterType).SequenceEqual (parameterTypes));
    }

    private void AdaptDeserializationConstructor (MutableConstructorInfo constructor, Tuple<string, FieldInfo>[] serializedFieldMapping)
    {
      constructor
          .SetBody (
              ctx => Expression.Block (
                  typeof (void),
                  new[] { ctx.PreviousBody }.Concat (BuildFieldDeserializationExpressions (ctx.This, ctx.Parameters[0], serializedFieldMapping))));
    }

    private static void OverrideOnDeserialization (ProxyType proxyType, MethodInfo initializationMethod)
    {
      try
      {
        proxyType.GetOrAddOverride (s_onDeserializationMethod)
                   .SetBody (ctx => Expression.Block (typeof (void), ctx.PreviousBody, Expression.Call (ctx.This, initializationMethod)));
      }
      catch (NotSupportedException exception)
      {
        throw new NotSupportedException (
            "The proxy type implements IDeserializationCallback but OnDeserialization cannot be overridden. "
            + "Make sure that OnDeserialization is implemented implicitly (not explicitly) and virtual.",
            exception);
      }
    }

    private static void ExplicitlyImplementOnDeserialization (ProxyType proxyType, MethodInfo initializationMethod)
    {
      proxyType.AddInterface (typeof (IDeserializationCallback));
      proxyType.AddExplicitOverride (s_onDeserializationMethod, ctx => Expression.Call (ctx.This, initializationMethod));
    }

    private IEnumerable<Expression> BuildFieldSerializationExpressions (
        Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> fieldMapping)
    {
      return fieldMapping
          .Select (
              entry => (Expression) Expression.Call (
                  serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant (entry.Item1), Expression.Field (@this, entry.Item2)));
    }

    private IEnumerable<Expression> BuildFieldDeserializationExpressions (
        Expression @this, Expression serializationInfo, IEnumerable<Tuple<string, FieldInfo>> fieldMapping)
    {
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