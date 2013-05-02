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
using System.Reflection;
using System.Runtime.Serialization;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// Enables the serialization of assembled type instances without the need of saving the generated assembly to disk.
  /// </summary>
  /// <remarks>
  /// Serialization is enabled by adding additional metadata to the serialization payload and using a serialization helper for deserialization.
  /// The helper uses the metadata to regenerate a compatible type.
  /// The metadata includes the following items:
  /// <list type="bullet">
  ///   <item>Type of the serialization helper which should be used for deserialization.</item>
  ///   <item>Participant configuration identifier that will be used to retrieve the pipeline during deserialization.</item>
  ///   <item>An instance of <see cref="AssembledTypeIDData"/>.</item>
  /// </list>
  /// </remarks>
  public class ComplexSerializationEnabler : IComplexSerializationEnabler
  {
    public const string SerializationKeyPrefix = "<tp>";
    public const string ParticipantConfigurationID = SerializationKeyPrefix + "participantConfigurationID";
    public const string AssembledTypeIDData = SerializationKeyPrefix + "assembledTypeIDData";

    private static readonly MethodInfo s_getObjectDataMethod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));

    private static readonly MethodInfo s_addFieldValuesMethod =
        MemberInfoFromExpressionUtility.GetMethod (() => ReflectionSerializationHelper.AddFieldValues (null, null));

    private static readonly MethodInfo s_setTypeMethod =
        MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo o) => o.SetType (typeof (int)));
    private static readonly MethodInfo s_addValueMethod =
        MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo o) => o.AddValue ("name", new object()));

    public void MakeSerializable (MutableType proxyType, string participantConfigurationID, Expression assembledTypeIDData)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("assembledTypeIDData", assembledTypeIDData);

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
                      ctx.PreviousBody,
                      CreateMetaDataSerializationExpression (
                          ctx.Parameters[0], typeof (ObjectWithDeserializationConstructorProxy), participantConfigurationID, assembledTypeIDData)));
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
                CreateMetaDataSerializationExpression (
                    ctx.Parameters[0], typeof (ObjectWithoutDeserializationConstructorProxy), participantConfigurationID, assembledTypeIDData),
                Expression.Call (s_addFieldValuesMethod, ctx.Parameters[0], ctx.This)));
      }
    }

    private Expression CreateMetaDataSerializationExpression (
        Expression serializationInfo, Type serializationSurrogateType, string participantConfigurationID, Expression assembledTypeIDData)
    {
      return Expression.Block (
          Expression.Call (serializationInfo, s_setTypeMethod, Expression.Constant (serializationSurrogateType)),
          CreateAddValueExpression (serializationInfo, ParticipantConfigurationID, Expression.Constant (participantConfigurationID)),
          CreateAddValueExpression (serializationInfo, AssembledTypeIDData, assembledTypeIDData));
    }

    private MethodCallExpression CreateAddValueExpression (Expression serializationInfo, string key, Expression value)
    {
      return Expression.Call (serializationInfo, s_addValueMethod, Expression.Constant (key), value);
    }
  }
}