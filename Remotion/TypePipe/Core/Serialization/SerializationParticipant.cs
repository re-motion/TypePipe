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
  public class SerializationParticipant : IParticipant
  {
    public const string SerializationKeyPrefix = "<tp>";
    public const string UnderlyingTypeKey = SerializationKeyPrefix + "underlyingType";
    public const string FactoryIdentifierKey = SerializationKeyPrefix + "factoryIdentifier";

    private static readonly MethodInfo s_getObjectDataMethod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));

    private readonly string _factoryIdentifier;
    private readonly IFieldSerializationExpressionBuilder _fieldSerializationExpressionBuilder;

    public SerializationParticipant (string factoryIdentifier, IFieldSerializationExpressionBuilder fieldSerializationExpressionBuilder)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("factoryIdentifier", factoryIdentifier);
      ArgumentUtility.CheckNotNull ("fieldSerializationExpressionBuilder", fieldSerializationExpressionBuilder);

      _factoryIdentifier = factoryIdentifier;
      _fieldSerializationExpressionBuilder = fieldSerializationExpressionBuilder;
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
        mutableType.GetOrAddMutableMethod (s_getObjectDataMethod)
                   .SetBody (ctx => Expression.Block (new[] { ctx.PreviousBody }.Concat (CreateMetaDataSerializationExpressions (ctx))));
      }
      else
      {
        var serializedFields = _fieldSerializationExpressionBuilder.GetSerializedFieldMapping (mutableType.ExistingMutableFields.Cast<FieldInfo>());

        mutableType.AddInterface (typeof (ISerializable));

        mutableType.AddExplicitOverride (
            s_getObjectDataMethod,
            ctx => Expression.Block (
                typeof (void),
                CreateMetaDataSerializationExpressions (ctx)
                    .Concat (_fieldSerializationExpressionBuilder.BuildFieldSerializationExpressions (ctx.This, ctx.Parameters[0], serializedFields))));

        // TODO 5222: Modify for existing deserialization constructor -> throw exception
        var parameters =
            new[] { new ParameterDeclaration (typeof (SerializationInfo), "info"), new ParameterDeclaration (typeof (StreamingContext), "context") };
        mutableType.AddConstructor (
            MethodAttributes.Family,
            parameters,
            ctx => Expression.Block (
                typeof (void),
                _fieldSerializationExpressionBuilder.BuildFieldDeserializationExpressions (ctx.This, ctx.Parameters[0], serializedFields)));
      }
    }

    private IEnumerable<Expression> CreateMetaDataSerializationExpressions (MethodBodyContextBase context)
    {
      var serializationInfo = context.Parameters[0];
      return new Expression[]
             {
                 Expression.Call (serializationInfo, "SetType", Type.EmptyTypes, Expression.Constant (typeof (SerializationSurrogate))),
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