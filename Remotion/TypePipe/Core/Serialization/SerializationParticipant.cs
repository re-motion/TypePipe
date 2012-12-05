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
    private static readonly MethodInfo s_getObjectDataMethod =
        MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));

    private readonly string _configurationKey;

    public SerializationParticipant (string configurationKey)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("configurationKey", configurationKey);

      _configurationKey = configurationKey;
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
                   .SetBody (ctx => Expression.Block (new[] { ctx.PreviousBody }.Concat (CreateSerializationExpressions (ctx))));
      }
      else
      {
        mutableType.AddInterface (typeof (ISerializable));
        mutableType.AddExplicitOverride (s_getObjectDataMethod, ctx => Expression.Block (CreateSerializationExpressions (ctx)));
      }
    }

    private IEnumerable<Expression> CreateSerializationExpressions (MethodBodyContextBase context)
    {
      return new Expression[]
             {
                 Expression.Call (context.Parameters[0], "SetType", Type.EmptyTypes, Expression.Constant (typeof (SerializationSurrogate))),
                 Expression.Call (
                     context.Parameters[0],
                     "AddValue",
                     Type.EmptyTypes,
                     Expression.Constant ("<tp>underlyingType"),
                     Expression.Constant (context.DeclaringType.UnderlyingSystemType.AssemblyQualifiedName)),
                 Expression.Call (
                     context.Parameters[0], "AddValue", Type.EmptyTypes, Expression.Constant ("<tp>configurationKey"), Expression.Constant (_configurationKey))
             }
          ;
    }
  }
}