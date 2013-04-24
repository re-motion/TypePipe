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
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// Enables the serialization of modified type instances without the need of saving the generated assembly to disk.
  /// </summary>
  /// <remarks>
  /// Serialization is enabled by adding additional metadata to the serialization payload and using a serialization helper for deserialization.
  /// The helper uses the metadata to regenerate a compatible type.
  /// The metadata includes the following items:
  /// <list type="bullet">
  ///   <item>Type of the serialization helper which should be used for deserialization.</item>
  ///   <item>The assembly-qualified type name of the underlying type.</item>
  ///   <item>Object factory identifier that will be used to retrieve the object factory during deserialization.</item>
  /// </list>
  /// </remarks>
  public class SerializationParticipant : IParticipant
  {
    public const string SerializationKeyPrefix = "<tp>";
    public const string RequestedTypeKey = SerializationKeyPrefix + "requestedType";
    public const string ParticipantConfigurationID = SerializationKeyPrefix + "participantConfigurationID";

    private static readonly MethodInfo s_getObjectDataMethod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));
    private static readonly MethodInfo s_addFieldValuesMethod =
        MemberInfoFromExpressionUtility.GetMethod (() => ReflectionSerializationHelper.AddFieldValues (null, null));

    public ICacheKeyProvider PartialCacheKeyProvider
    {
      get { return null; }
    }

    public void Participate (ITypeAssemblyContext typeAssemblyContext)
    {
      ArgumentUtility.CheckNotNull ("typeAssemblyContext", typeAssemblyContext);
      var proxyType = typeAssemblyContext.ProxyType;

      if (!proxyType.IsTypePipeSerializable())
        return;

      if (typeof (ISerializable).IsTypePipeAssignableFrom (proxyType))
      {
        // If the mutable type already implements ISerializable, we only need to extend the implementation to include the metadata required for 
        // deserialization. Existing fields will be serialized by the base ISerialization implementation. Added fields will be serialized by
        // the TypePipe (ProxySerializationEnabler).
        try
        {
          proxyType
              .GetOrAddOverride (s_getObjectDataMethod)
              .SetBody (
                  ctx => Expression.Block (
                      new[] { ctx.PreviousBody }.Concat (
                          CreateMetaDataSerializationExpressions (
                              ctx, typeof (ObjectWithDeserializationConstructorProxy), typeAssemblyContext.ParticipantConfigurationID))));
        }
        catch (NotSupportedException exception)
        {
          throw new NotSupportedException (
              "The proxy type implements ISerializable but GetObjectData cannot be overridden. "
              + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.",
              exception);
        }
      }
      else
      {
        // If the mutable type does not implement ISerializable, we need to add the interface and then also serialize all the fields on the object.
        // We cannot add a deserialization constructor because there is no base constructor that we could call. Therefore, ProxySerializationEnabler
        // cannot take care of serializing the added fields, and we thus have to serialize both existing and added fields ourselves via 
        // ReflectionHelper.AddFieldValues.

        proxyType.AddInterface (typeof (ISerializable));

        proxyType.AddExplicitOverride (
            s_getObjectDataMethod,
            ctx => Expression.Block (
                typeof (void),
                CreateMetaDataSerializationExpressions (
                    ctx, typeof (ObjectWithoutDeserializationConstructorProxy), typeAssemblyContext.ParticipantConfigurationID)
                    .Concat (Expression.Call (s_addFieldValuesMethod, ctx.Parameters[0], ctx.This))));
      }
    }

    public void RebuildState (LoadedTypesContext loadedTypesContext)
    {
      // Does nothing.
    }

    public void HandleNonSubclassableType (Type requestedType)
    {
      // Does nothing.
    }

    private IEnumerable<Expression> CreateMetaDataSerializationExpressions (
        MethodBodyContextBase context, Type serializationSurrogateType, string participantConfigurationID)
    {
      var serializationInfo = context.Parameters[0];
      return new Expression[]
             {
                 Expression.Call (serializationInfo, "SetType", Type.EmptyTypes, Expression.Constant (serializationSurrogateType)),
                 CreateAddValueExpression (serializationInfo, RequestedTypeKey, context.DeclaringType.BaseType.AssemblyQualifiedName),
                 CreateAddValueExpression (serializationInfo, ParticipantConfigurationID, participantConfigurationID)
             };
    }

    private MethodCallExpression CreateAddValueExpression (ParameterExpression serializationInfo, string key, string value)
    {
      return Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant (key), Expression.Constant (value));
    }
  }
}