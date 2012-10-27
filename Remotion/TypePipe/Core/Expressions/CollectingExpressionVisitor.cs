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
using System.Collections.ObjectModel;
using Microsoft.Scripting.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Collects all <see cref="Expression"/>s in an expression tree that match the given <see cref="Predicate{Expression}"/>.
  /// The matching nodes can be retrieved via the property <see cref="MatchingNodes"/>.
  /// </summary>
  public class CollectingExpressionVisitor : ExpressionVisitor
  {
    private readonly Predicate<Expression> _predicate;
    private readonly List<Expression> _matchingNodes = new List<Expression>();

    public CollectingExpressionVisitor (Predicate<Expression> predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      _predicate = predicate;
    }

    public ReadOnlyCollection<Expression> MatchingNodes
    {
      get { return _matchingNodes.AsReadOnly(); }
    }

    public override Expression Visit (Expression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      if (_predicate (node))
        _matchingNodes.Add (node);

      return base.Visit (node);
    }
  }
}