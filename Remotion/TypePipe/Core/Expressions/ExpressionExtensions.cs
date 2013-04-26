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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.Collections;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Provides extension methods for working with <see cref="Expression"/> trees.
  /// </summary>
  public static class ExpressionExtensions
  {
    public static Expression Replace (this Expression expression, IDictionary<Expression, Expression> replacements)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("replacements", replacements);

      return new ReplacingExpressionVisitor (replacements).Visit (expression);
    }

    public static Expression InlinedVisit (this Expression expression, Func<Expression, Expression> expressionVisitorDelegate)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("expressionVisitorDelegate", expressionVisitorDelegate);

      return new DelegateBasedExpressionVisitor (expressionVisitorDelegate).Visit (expression);
    }

    public static ReadOnlyCollectionDecorator<Expression> Collect (this Expression expression, Predicate<Expression> predicate)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("predicate", predicate);

      var matchingNodes = new HashSet<Expression>();
      Func<Expression, Expression> collectingDelegate = expr =>
      {
        if (predicate (expr))
          matchingNodes.Add (expr);
        return expr;
      };

      new DelegateBasedExpressionVisitor (collectingDelegate).Visit (expression);

      return matchingNodes.AsReadOnly();
    }

    public static ReadOnlyCollectionDecorator<T> Collect<T> (this Expression expression, Predicate<T> predicate = null)
        where T : Expression
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var matchingExpressions = predicate == null
                            ? Collect (expression, expr => expr is T)
                            : Collect (expression, expr => expr is T && predicate ((T) expr));

      return matchingExpressions.Cast<T>().ConvertToCollection().AsReadOnly();
    }
  }
}