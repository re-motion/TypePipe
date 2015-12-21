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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Dlr.Ast.Compiler;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Prepares method (and constructor) bodies so that code can be generated for them by replacing all nodes not
  /// understood by <see cref="LambdaCompiler"/> and <see cref="CodeGenerationExpressionEmitter"/>.
  /// </summary>
  public class ExpressionPreparer : IExpressionPreparer
  {
    private readonly IMethodTrampolineProvider _methodTrampolineProvider;

    public ExpressionPreparer (IMethodTrampolineProvider methodTrampolineProvider)
    {
      ArgumentUtility.CheckNotNull ("methodTrampolineProvider", methodTrampolineProvider);

      _methodTrampolineProvider = methodTrampolineProvider;
    }

    public IMethodTrampolineProvider MethodTrampolineProvider
    {
      get { return _methodTrampolineProvider; }
    }

    public Expression PrepareBody (CodeGenerationContext context, Expression body)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("body", body);

      return new UnemittableExpressionVisitor (context, _methodTrampolineProvider).Visit (body);
    }
  }
}