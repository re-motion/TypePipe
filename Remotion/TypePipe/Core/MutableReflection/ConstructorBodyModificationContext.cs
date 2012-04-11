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
using Microsoft.Scripting.Ast;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Provides access to parameters and custom expression for building the bodies of modified constructors. 
  /// See also <see cref="MutableConstructorInfo.SetBody"/>.
  /// </summary>
  public class ConstructorBodyModificationContext : MethodBodyContextBase
  {
    private readonly Expression _previousBody;

    public ConstructorBodyModificationContext (
        MutableType declaringType,
        IEnumerable<ParameterExpression> parameterExpressions,
        Expression previousBody)
        : base (declaringType, parameterExpressions, false)
    {
      ArgumentUtility.CheckNotNull ("previousBody", previousBody);
      _previousBody = previousBody;
    }

    public Expression GetPreviousBody ()
    {
      return _previousBody;
    }

    public Expression GetPreviousBody (params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return GetPreviousBody ((IEnumerable<Expression>) arguments);
    }

    public Expression GetPreviousBody (IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      var argumentCollection = arguments.ConvertToCollection();
      if (argumentCollection.Count != Parameters.Count)
      {
        var message = string.Format ("The argument count ({0}) does not match the parameter count ({1}).", argumentCollection.Count, Parameters.Count);
        throw new ArgumentException (message, "arguments");
      }

      var replacements = Parameters
          .Select ((p, i) => new { Parameter = p, Index = i })
          .Zip (argumentCollection, (t, a) => new { t.Parameter, Argument = EnsureCorrectType (a, t.Parameter.Type, t.Index, "arguments") })
          .ToDictionary (t => (Expression) t.Parameter, t => t.Argument);
      var visitor = new ReplacingExpressionVisitor (replacements);
      return visitor.Visit (_previousBody);
    }

    private Expression EnsureCorrectType (Expression expression, Type type, int argumentIndex, string parameterName)
    {
      if (expression.Type != type)
      {
        try
        {
          return Expression.Convert (expression, type);
        }
        catch (InvalidOperationException ex)
        {
          var message = string.Format ("The argument at index {0} has an invalid type: {1}", argumentIndex, ex.Message);
          throw new ArgumentException (message, parameterName, ex);
        }
      }

      return expression;
    }

    public Expression GetConstructorCall (params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return GetConstructorCall (((IEnumerable<Expression>) arguments));
    }

    public Expression GetConstructorCall (IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return ConstructorBodyContextUtility.GetConstructorCallExpression (This, arguments);
    }
  }
}