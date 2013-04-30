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
using System.Text;
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

    public AssembledTypeIdentifierProvider (IEnumerable<IParticipant> participants)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);

      _identifierProviders = participants.Select (p => p.PartialTypeIdentifierProvider).Where (p => p != null).ToArray();
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

    public Expression GetTypeIDExpression (AssembledTypeID typeID)
    {
      ArgumentUtility.CheckNotNull ("typeID", typeID);

      var requestedType = Expression.Constant (typeID.RequestedType);
      var individualParts = typeID.Parts.Select ((p, i) => _identifierProviders[i].GetExpressionForID (p));
      var parts = Expression.NewArrayInit (typeof (object), individualParts);

      return Expression.New (s_assembledTypeIDConstructor, requestedType, parts);
    }
  }
}