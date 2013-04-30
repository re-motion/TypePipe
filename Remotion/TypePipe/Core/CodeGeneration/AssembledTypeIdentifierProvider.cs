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
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Provides identifiers for assembled types.
  /// </summary>
  public class AssembledTypeIdentifierProvider : IAssembledTypeIdentifierProvider
  {
    private static readonly ConstructorInfo s_assembledTypeIDConstructor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeID (typeof (object), new object[0]));

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

    public AssembledTypeID GetTypeID (Type requestedType)
    {
      // Using Debug.Assert because it will be compiled away.
      Debug.Assert (requestedType != null);

      var parts = new object[_identifierProviders.Length];

      // No LINQ for performance reasons.
      for (int i = 0; i < _identifierProviders.Length; i++)
        parts[i] = _identifierProviders[i].GetID (requestedType);

      return new AssembledTypeID (requestedType, parts);
    }

    public Expression GetExpression (AssembledTypeID typeID)
    {
      ArgumentUtility.CheckNotNull ("typeID", typeID);

      var requestedType = Expression.Constant (typeID.RequestedType);
      var parts = typeID.Parts.Select ((idPart, i) => GetNonNullExpressionForID (i, idPart));
      var partsArray = Expression.NewArrayInit (typeof (object), parts);

      return Expression.New (s_assembledTypeIDConstructor, requestedType, partsArray);
    }

    public object GetPart (AssembledTypeID typeID, IParticipant participant)
    {
      ArgumentUtility.CheckNotNull ("participant", participant);

      int index;
      if (_identifierProviderIndexes.TryGetValue (participant, out index))
        return typeID.Parts[index];

      return null;
    }

    private Expression GetNonNullExpressionForID (int index, object id)
    {
      return _identifierProviders[index].GetExpressionForID (id) ?? Expression.Constant (null);
    }
  }
}