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

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Holds utility functions used by <see cref="BodyContextBase"/> and its descendants.
  /// </summary>
  public static class BodyContextUtility
  {
    /// <summary>
    /// Prepares a new body by replacing the <paramref name="parameters"/> contained in <paramref name="body"/> with the supplied
    /// <paramref name="arguments"/>.
    /// </summary>
    /// <param name="parameters">The parameters to be replaced.</param>
    /// <param name="body">The original body.</param>
    /// <param name="arguments">The arguments replacing the parameters.</param>
    /// <returns></returns>
    public static Expression PrepareNewBody (IEnumerable<ParameterExpression> parameters, Expression body, IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("body", body);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      var parameterCollection = parameters.ConvertToCollection();
      var argumentCollection = arguments.ConvertToCollection();

      if (parameterCollection.Count != argumentCollection.Count)
      {
        var message = string.Format (
            "The argument count ({0}) does not match the parameter count ({1}).", argumentCollection.Count, parameterCollection.Count);
        throw new ArgumentException (message, "arguments");
      }

      var replacements = parameterCollection
          .Select ((p, i) => new { Parameter = p, Index = i })
          .Zip (argumentCollection, (t, a) => new { t.Parameter, Argument = EnsureCorrectType (a, t.Parameter.Type, t.Index, "arguments") })
          .ToDictionary (t => (Expression) t.Parameter, t => t.Argument);

      var visitor = new ReplacingExpressionVisitor (replacements);
      return visitor.Visit (body);
    }

    private static Expression EnsureCorrectType (Expression expression, Type type, int argumentIndex, string parameterName)
    {
      try
      {
        return ExpressionTypeUtility.EnsureCorrectType (expression, type);
      }
      catch (InvalidOperationException ex)
      {
        var message = String.Format ("The argument at index {0} has an invalid type: {1}", argumentIndex, ex.Message);
        throw new ArgumentException (message, parameterName, ex);
      }
    }
  }
}