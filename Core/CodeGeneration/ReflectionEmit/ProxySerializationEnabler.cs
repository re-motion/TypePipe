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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IProxySerializationEnabler"/>.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class ProxySerializationEnabler : IProxySerializationEnabler
  {
    private static readonly MethodInfo s_getObjectDataMethod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));

    private static readonly MethodInfo s_getValueMethod =
        MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));

    private static readonly MethodInfo s_onDeserializationMethod =
        MemberInfoFromExpressionUtility.GetMethod ((IDeserializationCallback obj) => obj.OnDeserialization (null));

    private static readonly ConstructorInfo s_serializationExceptionConstructor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new SerializationException ("message"));

    private readonly ISerializableFieldFinder _serializableFieldFinder;

    public ProxySerializationEnabler (ISerializableFieldFinder serializableFieldFinder)
    {
      ArgumentUtility.CheckNotNull ("serializableFieldFinder", serializableFieldFinder);

      _serializableFieldFinder = serializableFieldFinder;
    }

    public void MakeSerializable (MutableType proxyType, MethodInfo initializationMethod)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      // initializationMethod may be null

      // Base fields are always serialized by the standard .NET serialization or by an implementation of ISerializable on the base type.
      // Added fields are also serialized by the standard .NET serialization, unless the proxy type implements ISerializable. In that case,
      // we need to extend the ISerializable implementation to include the added fields.

      var serializedFieldMapping = _serializableFieldFinder.GetSerializableFieldMapping (proxyType.AddedFields.Cast<FieldInfo>()).ToArray();
      var deserializationConstructor = GetDeserializationConstructor (proxyType);

      // If the base type implements ISerializable but has no deserialization constructor, we can't implement ISerializable correctly, so
      // we don't even try. (ComplexSerializationEnabler relies on this behavior.)
      var needsCustomFieldSerialization =
          serializedFieldMapping.Length != 0 && typeof (ISerializable).IsTypePipeAssignableFrom (proxyType) && deserializationConstructor != null;

      if (needsCustomFieldSerialization)
      {
        OverrideGetObjectData (proxyType, serializedFieldMapping);
        AdaptDeserializationConstructor (deserializationConstructor, serializedFieldMapping);
      }

      if (initializationMethod != null)
      {
        if (typeof (IDeserializationCallback).IsTypePipeAssignableFrom (proxyType))
          OverrideOnDeserialization (proxyType, initializationMethod);
        else if (proxyType.IsTypePipeSerializable())
          ExplicitlyImplementOnDeserialization (proxyType, initializationMethod);
      }
    }

    public bool IsDeserializationConstructor (ConstructorInfo constructor)
    {
      return constructor.GetParameters ().Select (x => x.ParameterType).SequenceEqual (new[] { typeof (SerializationInfo), typeof (StreamingContext) });
    }

    private void OverrideGetObjectData (MutableType proxyType, Tuple<string, FieldInfo>[] serializedFieldMapping)
    {
      try
      {
        proxyType
            .GetOrAddImplementation (s_getObjectDataMethod)
            .SetBody (
                ctx => Expression.Block (
                    typeof (void),
                    new[] { ctx.PreviousBody }.Concat (BuildFieldSerializationExpressions (ctx.This, ctx.Parameters[0], serializedFieldMapping))));
      }
      catch (NotSupportedException)
      {
        // Overriding and re-implementation failed because the base implementation is not accessible from the proxy.
        // Do nothing here; error reporting code will be generated in the ProxySerializationEnabler.
        // Add an explicit re-implementation that throws exception (instead of simply throwing an exception here).
        // Reasoning: Users often cannot influence the requested type and do not care about any serialization problem.

        proxyType.AddInterface (typeof (ISerializable), throwIfAlreadyImplemented: false);

        var message = "The requested type implements ISerializable but GetObjectData is not accessible from the proxy. "
                      + "Make sure that GetObjectData is implemented implicitly (not explicitly).";
        proxyType.AddExplicitOverride (
            s_getObjectDataMethod,
            ctx => Expression.Throw (Expression.New (s_serializationExceptionConstructor, Expression.Constant (message))));
      }
    }

    private MutableConstructorInfo GetDeserializationConstructor (MutableType type)
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

    private void OverrideOnDeserialization (MutableType proxyType, MethodInfo initializationMethod)
    {
      try
      {
        proxyType.GetOrAddImplementation (s_onDeserializationMethod)
                 .SetBody (
                     ctx => Expression.Block (
                         typeof (void),
                         ctx.PreviousBody,
                         CallInitializationMethod (ctx.This, initializationMethod)));
      }
      catch (NotSupportedException)
      {
        // Overriding and re-implementation failed because the base implementation is not accessible from the proxy.
        // Add an explicit re-implementation that throws exception (instead of simply throwing an exception here).
        // Reasoning: Users often cannot influence the requested type and do not care about any serialization problem.

        proxyType.AddInterface (typeof (IDeserializationCallback), throwIfAlreadyImplemented: false);

        var message = "The requested type implements IDeserializationCallback but OnDeserialization is not accessible from the proxy. "
                      + "Make sure that OnDeserialization is implemented implicitly (not explicitly).";
        proxyType.AddExplicitOverride (
            s_onDeserializationMethod,
            ctx => Expression.Throw (Expression.New (s_serializationExceptionConstructor, Expression.Constant (message))));
      }
    }

    private void ExplicitlyImplementOnDeserialization (MutableType proxyType, MethodInfo initializationMethod)
    {
      proxyType.AddInterface (typeof (IDeserializationCallback));
      proxyType.AddExplicitOverride (s_onDeserializationMethod, ctx => CallInitializationMethod (ctx.This, initializationMethod));
    }

    private static MethodCallExpression CallInitializationMethod (Expression @this, MethodInfo initializationMethod)
    {
      return Expression.Call (@this, initializationMethod, Expression.Constant (InitializationSemantics.Deserialization));
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