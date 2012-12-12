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
    public const string UnderlyingTypeKey = SerializationKeyPrefix + "underlyingType";
    public const string FactoryIdentifierKey = SerializationKeyPrefix + "factoryIdentifier";

    private static readonly MethodInfo s_getObjectDataMethod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));
    private static readonly MethodInfo s_addFieldValueMethod =
        MemberInfoFromExpressionUtility.GetMethod (() => ReflectionSerializationHelper.AddFieldValues (null, null));

    private readonly string _factoryIdentifier;

    public SerializationParticipant (string factoryIdentifier)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("factoryIdentifier", factoryIdentifier);

      _factoryIdentifier = factoryIdentifier;
    }

    public ICacheKeyProvider PartialCacheKeyProvider
    {
      get { return null; }
    }

    public void ModifyType (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      if (!mutableType.IsSerializable)
        return;

      if (mutableType.IsAssignableTo (typeof (ISerializable)))
      {
        // If the mutable type already implements ISerializable, we only need to extend the implementation to include the metadata required for 
        // deserialization. Existing fields will be serialized by the base ISerialization implementation. Added fields will be serialized by
        // the TypePipe (ProxySerializationEnabler).
        mutableType
            .GetOrAddMutableMethod (s_getObjectDataMethod)
            .SetBody (
                ctx => Expression.Block (
                    new[] { ctx.PreviousBody }.Concat (CreateMetaDataSerializationExpressions (ctx, typeof (ObjectWithDeserializationConstructorProxy)))));
      }
      else
      {
        // If the mutable type does not implement ISerializable, we need to add the interface and then also serialize all the fields on the object.
        // We cannot add a deserialization constructor because there is no base constructor that we could call. Therefore, ProxySerializationEnabler
        // cannot take care of serializing the added fields, and we thus have to serialize both existing and added fields ourselves via 
        // ReflectionHelper.AddFieldValues.

        mutableType.AddInterface (typeof (ISerializable));

        mutableType.AddExplicitOverride (
            s_getObjectDataMethod,
            ctx => Expression.Block (
                typeof (void),
                CreateMetaDataSerializationExpressions (ctx, typeof (ObjectWithoutDeserializationConstructorProxy))
                    .Concat (Expression.Call (s_addFieldValueMethod, ctx.Parameters[0], ctx.This))));
      }
    }

    private IEnumerable<Expression> CreateMetaDataSerializationExpressions (MethodBodyContextBase context, Type serializationSurrogateType)
    {
      var serializationInfo = context.Parameters[0];
      return new Expression[]
             {
                 Expression.Call (serializationInfo, "SetType", Type.EmptyTypes, Expression.Constant (serializationSurrogateType)),
                 CreateAddValueExpression (serializationInfo, UnderlyingTypeKey, context.DeclaringType.UnderlyingSystemType.AssemblyQualifiedName),
                 CreateAddValueExpression (serializationInfo, FactoryIdentifierKey, _factoryIdentifier)
             };
    }

    private MethodCallExpression CreateAddValueExpression (ParameterExpression serializationInfo, string key, string value)
    {
      return Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant (key), Expression.Constant (value));
    }
  }
}