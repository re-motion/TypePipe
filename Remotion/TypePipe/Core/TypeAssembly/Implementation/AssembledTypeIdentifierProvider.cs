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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  /// <summary>
  /// Provides identifiers for assembled types.
  /// </summary>
  public class AssembledTypeIdentifierProvider : IAssembledTypeIdentifierProvider
  {
    private const string c_typeIDFieldName = "__typeID";

    private static readonly ConstructorInfo s_assembledTypeIDConstructor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeID (typeof (object), new object[0]));

    private static readonly ConstructorInfo s_assembledTypeIDDataConstructor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeIDData ("type name", new IFlatValue[0]));

    // Array for performance reasons.
    private readonly ITypeIdentifierProvider[] _identifierProviders;
    private readonly Dictionary<IParticipant, int> _identifierProviderIndexes;

    public AssembledTypeIdentifierProvider (IEnumerable<IParticipant> participants)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);

      var providersWithIndex = participants
          .Select (p => new { Participant = p, IdentifierProvider = p.PartialTypeIdentifierProvider })
          .Where (t => t.IdentifierProvider != null)
          .Select ((t, i) => new { t.Participant, t.IdentifierProvider, Index = i }).ToList();

      _identifierProviders = providersWithIndex.Select (t => t.IdentifierProvider).ToArray();
      _identifierProviderIndexes = providersWithIndex.ToDictionary (t => t.Participant, t => t.Index);
    }

    public AssembledTypeID ComputeTypeID (Type requestedType)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (requestedType != null);

      var parts = new object[_identifierProviders.Length];

      // No LINQ for performance reasons.
      for (int i = 0; i < _identifierProviders.Length; i++)
        parts[i] = _identifierProviders[i].GetID (requestedType);

      return new AssembledTypeID (requestedType, parts);
    }

    public object GetPart (AssembledTypeID typeID, IParticipant participant)
    {
      ArgumentUtility.CheckNotNull ("participant", participant);

      int index;
      if (_identifierProviderIndexes.TryGetValue (participant, out index))
        return typeID.Parts[index];

      return null;
    }

    public void AddTypeID (MutableType proxyType, AssembledTypeID typeID)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNull ("typeID", typeID);

      var typeIDField = proxyType.AddField (c_typeIDFieldName, FieldAttributes.Private | FieldAttributes.Static, typeof (AssembledTypeID));
      var typeIDExpression = CreateNewTypeIDExpression (
          s_assembledTypeIDConstructor, typeID.RequestedType, typeID.Parts, typeof (object), (p, id) => p.GetExpression (id), "GetExpression");
      var typeIDFieldInitialization = Expression.Assign (Expression.Field (null, typeIDField), typeIDExpression);

      proxyType.AddTypeInitialization (typeIDFieldInitialization);
    }

    public AssembledTypeID ExtractTypeID (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      var typeIDField = assembledType.GetField (c_typeIDFieldName, BindingFlags.NonPublic | BindingFlags.Static);
      Assertion.IsNotNull (typeIDField);

      return (AssembledTypeID) typeIDField.GetValue (null);
    }

    public Expression GetAssembledTypeIDDataExpression (AssembledTypeID typeID)
    {
      return CreateNewTypeIDExpression (
          s_assembledTypeIDDataConstructor,
          typeID.RequestedType.AssemblyQualifiedName,
          typeID.Parts,
          typeof (IFlatValue),
          (p, id) => p.GetFlatValueExpressionForSerialization (id),
          "GetFlatValueExpressionForSerialization");
    }

    private Expression CreateNewTypeIDExpression (
        ConstructorInfo constructor,
        object requestedTypeValue,
        object[] idParts,
        Type idPartType,
        Func<ITypeIdentifierProvider, object, Expression> expressionProvider,
        string methodName)
    {
      var requestedTypeExpression = Expression.Constant (requestedTypeValue);
      var parts = idParts.Select ((idPart, i) => GetNonNullExpressionForID (expressionProvider, i, idPart, idPartType, methodName));
      var partsArray = Expression.NewArrayInit (idPartType, parts);

      return Expression.New (constructor, requestedTypeExpression, partsArray);
    }

    private Expression GetNonNullExpressionForID (
        Func<ITypeIdentifierProvider, object, Expression> expressionProvider, int index, object idPart, Type idPartType, string methodName)
    {
      if (idPart == null)
        return Expression.Constant (null, idPartType);

      var flatValue = expressionProvider (_identifierProviders[index], idPart);
      if (flatValue == null)
        return Expression.Constant (null, idPartType);

      if (!idPartType.IsTypePipeAssignableFrom (flatValue.Type))
      {
        var message = string.Format ("The expression returned from '{0}' must build an serializable instance of '{1}'.", methodName, idPartType.Name);
        throw new InvalidOperationException (message);
      }

      return flatValue;
    }
  }
}