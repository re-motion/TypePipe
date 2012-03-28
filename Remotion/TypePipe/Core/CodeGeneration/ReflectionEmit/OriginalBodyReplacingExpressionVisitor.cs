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
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Replaces all occurences of <see cref="OriginalBodyExpression"/> with a base call to the original implementation of the respective method.
  /// </summary>
  public class OriginalBodyReplacingExpressionVisitor : TypePipeExpressionVisitorBase
  {
    private readonly MethodInfo _baseMethod;

    public OriginalBodyReplacingExpressionVisitor (MethodInfo baseMethod)
    {
      ArgumentUtility.CheckNotNull ("baseMethod", baseMethod);

      if (baseMethod.IsStatic)
        throw new ArgumentException ("Method must not be static.", "baseMethod");

      _baseMethod = baseMethod;
    }

    protected override Expression VisitOriginalBody (OriginalBodyExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return Visit (Expression.Call (new ThisExpression (_baseMethod.DeclaringType), _baseMethod, expression.Arguments));
    }
  }
}