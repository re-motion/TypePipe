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
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Replaces all occurences of <see cref="OriginalBodyExpression"/> with a base call to the underlying constructor.
  /// </summary>
  public class OriginalConstructorBodyReplacingExpressionVisitor : TypePipeExpressionVisitorBase
  {
    private readonly MutableConstructorInfo _mutableConstructorInfo;

    public OriginalConstructorBodyReplacingExpressionVisitor (MutableConstructorInfo mutableConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("mutableConstructorInfo", mutableConstructorInfo);

      Assertion.IsFalse (mutableConstructorInfo.IsStatic, "Static constructors are not (yet) supported.");

      _mutableConstructorInfo = mutableConstructorInfo;
    }

    protected override Expression VisitOriginalBody (OriginalBodyExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // TODO 4686: Check that _mutableConstructorInfo.UnderlyingSystemConstructorInfo is different from _mutableConstructorInfo.
      var baseMethod = new ConstructorAsMethodInfoAdapter (_mutableConstructorInfo.UnderlyingSystemConstructorInfo);
      var thisExpression = new ThisExpression (_mutableConstructorInfo.DeclaringType);
      // Since _mutableConstructorInfo.DeclaringType is a MutableType, we need to convert the ThisExpression to its underlying
      // system type. This is the only way we can be sure that all type checks within the Expression.Call factory method succeed. (We cannot rely
      // on System.RuntimeType.IsAssignableFrom working with MutableTypes.)
      var convertedThisExpression = new TypeAsUnderlyingSystemTypeExpression (thisExpression);
      var baseCall = Expression.Call (convertedThisExpression, baseMethod, expression.Arguments);
      return Visit (baseCall);
    }
  }
}