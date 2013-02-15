/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast.Compiler;

// ReSharper disable CheckNamespace
namespace Microsoft.Scripting.Ast
// ReSharper restore CheckNamespace
{
  public partial class LambdaExpression
  {
    /// <summary>
    /// Creates a new expression that is like this one, but using the
    /// supplied children. If all of the children are the same, it will
    /// return this expression.
    /// </summary>
    /// <param name="body">The <see cref="LambdaExpression.Body">Body</see> property of the result.</param>
    /// <param name="parameters">The <see cref="LambdaExpression.Parameters">Parameters</see> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual LambdaExpression Update (Expression body, IEnumerable<ParameterExpression> parameters)
    {
      if (body == Body && parameters == Parameters)
        return this;

      return Expression.Lambda (Type, body, Name, TailCall, parameters);
    }

    protected internal override Expression Accept (ExpressionVisitor visitor)
    {
      return visitor.VisitLambda (this);
    }

    internal LambdaExpression Accept (StackSpiller spiller)
    {
      return spiller.Rewrite (this);
    }
  }
}