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
  /// A visitor that implements <see cref="Visit"/> by using the provided <see cref="Func{T,TResult}"/> delegate.
  /// </summary>
  public class DelegateBasedExpressionVisitor : ExpressionVisitor
  {
    private readonly Func<Expression, Expression> _replacingExpressionDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateBasedExpressionVisitor" /> class.
    /// </summary>
    /// <param name="replacingExpressionDelegate">The expression delegate.</param>
    public DelegateBasedExpressionVisitor (Func<Expression, Expression> replacingExpressionDelegate)
    {
      ArgumentUtility.CheckNotNull ("replacingExpressionDelegate", replacingExpressionDelegate);

      _replacingExpressionDelegate = replacingExpressionDelegate;
    }

    public override Expression Visit (Expression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      var newNode = base.Visit (node);
      return _replacingExpressionDelegate (newNode);
    }
  }
}