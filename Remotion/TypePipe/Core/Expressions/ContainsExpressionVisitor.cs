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
using Microsoft.Scripting.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Checks if an <see cref="Expression"/> tree contains a node that matches the given <see cref="Predicate{Expression}"/>.
  /// The result can be accessed via the property <see cref="Result"/>.
  /// </summary>
  public class ContainsExpressionVisitor : ExpressionVisitor
  {
    private readonly Predicate<Expression> _predicate;

    public ContainsExpressionVisitor (Predicate<Expression> predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      _predicate = predicate;
    }

    public bool Result { get; private set; }

    public override Expression Visit (Expression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      if (Result)
        return node;

      if (_predicate (node))
      {
        Result = true;
        return node;
      }

      return base.Visit (node);
    }
  }
}