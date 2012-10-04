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
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  public class ILGeneratingTypePipeExpressionVisitor : ITypePipeExpressionVisitor
  {
    private readonly IILGenerator _ilGenerator;
    private readonly Action<Expression> _childExpressionEmitter;

    [CLSCompliant (false)]
    public ILGeneratingTypePipeExpressionVisitor (IILGenerator ilGenerator, Action<Expression> childExpressionEmitter)
    {
      ArgumentUtility.CheckNotNull ("ilGenerator", ilGenerator);
      ArgumentUtility.CheckNotNull ("childExpressionEmitter", childExpressionEmitter);

      _ilGenerator = ilGenerator;
      _childExpressionEmitter = childExpressionEmitter;
    }

    public Expression VisitThis (ThisExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _ilGenerator.Emit (OpCodes.Ldarg_0);
      return expression;
    }

    public Expression VisitOriginalBody (OriginalBodyExpression expression)
    {
      var message = string.Format ("{0} must be replaced before code generation.", typeof(OriginalBodyExpression).Name);
      throw new NotSupportedException (message);
    }

    public Expression VisitMethodAddress (MethodAddressExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _ilGenerator.Emit (OpCodes.Ldftn, expression.Method);
      return expression;
    }

    public Expression VisitVirtualMethodAddress (VirtualMethodAddressExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _childExpressionEmitter (expression.Instance);
      _ilGenerator.Emit (OpCodes.Ldvirtftn, expression.Method);
      return expression;
    }
  }
}