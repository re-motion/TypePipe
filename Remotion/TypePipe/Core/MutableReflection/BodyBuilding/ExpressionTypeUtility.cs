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

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Provides functionality to check and adapt the types of <see cref="Expression"/> trees.
  /// </summary>
  public static class ExpressionTypeUtility
  {
    public static Expression EnsureCorrectType (Expression expression, Type expectedType)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("expectedType", expectedType);

      if (expression.Type == expectedType)
        return expression;

      if (expectedType == typeof (void))
        return Expression.Block (typeof (void), expression);

      if (expectedType.IsTypePipeAssignableFrom (expression.Type))
      {
        if (expression.Type.IsValueType)
          return Expression.Convert (expression, expectedType);
        else
          return expression;
      }

      var message = string.Format (
          "Type '{0}' cannot be implicitly converted to type '{1}'. "
          + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.", 
          expression.Type, 
          expectedType);
      throw new InvalidOperationException (message);
    }
  }
}