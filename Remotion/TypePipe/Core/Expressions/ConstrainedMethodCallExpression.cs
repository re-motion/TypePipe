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
using System.Reflection.Emit;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Represents a <see cref="OpCodes.Constrained"/> virtual method call (<see cref="OpCodes.Callvirt"/>).
  /// </summary>
  public class ConstrainedMethodCallExpression : PrimitiveTypePipeExpressionBase
  {
    private readonly MethodCallExpression _methodCall;

    public ConstrainedMethodCallExpression (MethodCallExpression methodCall)
        : base (ArgumentUtility.CheckNotNull ("methodCall", methodCall).Type)
    {
      _methodCall = methodCall;
    }

    public MethodCallExpression MethodCall
    {
      get { return _methodCall; }
    }

    public Type ConstrainingType
    {
      get { return _methodCall.Method.DeclaringType; }
    }

    public override Expression Accept (IPrimitiveTypePipeExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return visitor.VisitConstrainedMethodCall (this);
    }

    protected internal override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newMethodCall = visitor.VisitAndConvert (_methodCall, "VisitChildren");
      if (newMethodCall == _methodCall)
        return this;

      return new ConstrainedMethodCallExpression (newMethodCall);
    }
  }
}