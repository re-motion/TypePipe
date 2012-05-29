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
using Microsoft.Scripting.Ast.Compiler;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Replaces all occurences of <see cref="OriginalBodyExpression"/> with <see cref="MethodCallExpression"/>s that can be processed by the
  /// <see cref="LambdaCompiler"/>.
  /// </summary>
  public class OriginalBodyReplacingExpressionVisitor : TypePipeExpressionVisitorBase
  {
    private readonly IMutableMethodBase _mutableMethodBase;

    public OriginalBodyReplacingExpressionVisitor (IMutableMethodBase mutableMethodBase)
    {
      ArgumentUtility.CheckNotNull ("mutableMethodBase", mutableMethodBase);

      _mutableMethodBase = mutableMethodBase;
    }

    protected override Expression VisitOriginalBody (OriginalBodyExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (_mutableMethodBase.IsNew || _mutableMethodBase.IsStatic)
      {
        var message = string.Format (
            "The body of an added or static member ('{0}', declared for mutable type '{1}') must not contain an OriginalBodyExpression.", 
            _mutableMethodBase, 
            _mutableMethodBase.DeclaringType.Name);
        throw new NotSupportedException (message);
      }

      var thisExpression = new ThisExpression (_mutableMethodBase.DeclaringType);
      // Since _mutableMethodBase.DeclaringType is a MutableType, we need to convert the ThisExpression to its underlying
      // system type. This is the only way we can be sure that all type checks within the Expression.Call factory method succeed. (We cannot rely
      // on System.RuntimeType.IsAssignableFrom working with MutableTypes.)
      var convertedThisExpression = new TypeAsUnderlyingSystemTypeExpression (thisExpression);
      var methodRepresentingOriginalBody = AdaptOriginalMethodBase (expression.MethodBase);

      var baseCall = Expression.Call (convertedThisExpression, methodRepresentingOriginalBody, expression.Arguments);

      return Visit (baseCall);
    }

    private MethodInfo AdaptOriginalMethodBase (MethodBase methodBase)
    {
      Assertion.IsTrue (methodBase is MethodInfo || methodBase is ConstructorInfo);

      var method = methodBase as MethodInfo;
      if (method == null)
        method = new ConstructorAsMethodInfoAdapter ((ConstructorInfo) methodBase);
      
      return new BaseCallMethodInfoAdapter (method);
    }
  }
}