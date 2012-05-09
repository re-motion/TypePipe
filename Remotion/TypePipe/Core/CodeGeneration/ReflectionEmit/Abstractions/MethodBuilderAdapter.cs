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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Ast.Compiler;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="MethodBuilder"/> with the <see cref="IMethodBaseBuilder"/> interface.
  /// </summary>
  public class MethodBuilderAdapter : IMethodBuilder
  {
    private readonly MethodBuilder _methodBuilder;

    public MethodBuilderAdapter (MethodBuilder methodBuilder)
    {
      ArgumentUtility.CheckNotNull ("methodBuilder", methodBuilder);

      _methodBuilder = methodBuilder;
    }

    public MethodBuilder MethodBuilder
    {
      get { return _methodBuilder; }
    }

    // TODO 4813
    [CLSCompliant (false)]
    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("method", method);

      emittableOperandProvider.AddMapping (method, new EmittableMethod (_methodBuilder));
    }

    public void DefineParameter (int iSequence, ParameterAttributes attributes, string strParamName)
    {
      _methodBuilder.DefineParameter (iSequence, attributes, strParamName);
    }

    [CLSCompliant (false)]
    public void SetBody (LambdaExpression body, IILGeneratorFactory ilGeneratorFactory, DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("body", body);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      var builderForLambdaCompiler = new MethodBuilderForLambdaCompiler (_methodBuilder, ilGeneratorFactory, true);
      LambdaCompiler.Compile (body, builderForLambdaCompiler, debugInfoGeneratorOrNull);
    }

    [CLSCompliant (false)]
    public void Emit (IILGenerator ilGenerator, OpCode opCode)
    {
      ArgumentUtility.CheckNotNull ("ilGenerator", ilGenerator);

      ilGenerator.Emit (opCode, _methodBuilder);
    }

    [CLSCompliant (false)]
    public void EmitCall (IILGenerator ilGenerator, OpCode opCode, Type[] optionalParameterTypes)
    {
      ArgumentUtility.CheckNotNull ("ilGenerator", ilGenerator);
      // optionalParameterTypes may be null

      ilGenerator.EmitCall (opCode, _methodBuilder, optionalParameterTypes);
    }

    public void DefineOverride (MethodInfo methodInfoDeclaration)
    {
      ArgumentUtility.CheckNotNull ("methodInfoDeclaration", methodInfoDeclaration);

      var typeBuilder = (TypeBuilder) _methodBuilder.DeclaringType;
      typeBuilder.DefineMethodOverride (_methodBuilder, methodInfoDeclaration);
    }
  }
}