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
using Microsoft.Scripting.Ast.Compiler;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// An <see cref="IPrimitiveTypePipeExpressionVisitor"/> that emits code for <see cref="IPrimitiveTypePipeExpression"/> instances.
  /// </summary>
  /// <remarks>
  /// This class participates in code generation via <see cref="LambdaCompiler.EmitPrimitiveTypePipeExpression"/> in the <see cref="LambdaCompiler"/>.
  /// </remarks>
  public class ILGeneratingExpressionVisitor : IPrimitiveTypePipeExpressionVisitor
  {
    private readonly IILGenerator _ilGenerator;
    private readonly Action<Expression> _childExpressionEmitter;

    [CLSCompliant (false)]
    public ILGeneratingExpressionVisitor (IILGenerator ilGenerator, Action<Expression> childExpressionEmitter)
    {
      ArgumentUtility.CheckNotNull ("ilGenerator", ilGenerator);
      ArgumentUtility.CheckNotNull ("childExpressionEmitter", childExpressionEmitter);

      _ilGenerator = ilGenerator;
      _childExpressionEmitter = childExpressionEmitter;
    }

    public Expression VisitThis (ThisExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      _ilGenerator.Emit (OpCodes.Ldarg_0);
      return node;
    }

    public Expression VisitNewDelegate (NewDelegateExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      var constructorInfo = node.Type.GetConstructor (new[] { typeof (object), typeof (IntPtr) });

      if (node.Method.IsStatic)
        _ilGenerator.Emit (OpCodes.Ldnull);
      else
        _childExpressionEmitter (node.Target);

      if (node.Method.IsVirtual)
      {
        _ilGenerator.Emit (OpCodes.Dup);
        _ilGenerator.Emit (OpCodes.Ldvirtftn, node.Method);
      }
      else
        _ilGenerator.Emit (OpCodes.Ldftn, node.Method);

      _ilGenerator.Emit (OpCodes.Newobj, constructorInfo);

      return node;
    }
  }
}