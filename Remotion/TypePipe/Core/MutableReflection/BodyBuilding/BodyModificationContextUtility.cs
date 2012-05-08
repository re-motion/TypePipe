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
using System.Linq;
using Microsoft.Scripting.Ast;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Holds utility functions used by <see cref="MethodBodyModificationContext"/> and <see cref="ConstructorBodyModificationContext"/>.
  /// </summary>
  public static class BodyModificationContextUtility
  {
    public static Expression PreparePreviousBody (
        ReadOnlyCollection<ParameterExpression> parameters, Expression previousBody, IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("previousBody", previousBody);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      var argumentCollection = arguments.ConvertToCollection();
      if (argumentCollection.Count != parameters.Count)
      {
        var message = String.Format ("The argument count ({0}) does not match the parameter count ({1}).", argumentCollection.Count, parameters.Count);
        throw new ArgumentException (message, "arguments");
      }

      var replacements = parameters
          .Select ((p, i) => new { Parameter = p, Index = i })
          .Zip (argumentCollection, (t, a) => new { t.Parameter, Argument = EnsureCorrectType (a, t.Parameter.Type, t.Index, "arguments") })
          .ToDictionary (t => (Expression) t.Parameter, t => t.Argument);
      var visitor = new ReplacingExpressionVisitor (replacements);
      return visitor.Visit (previousBody);
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