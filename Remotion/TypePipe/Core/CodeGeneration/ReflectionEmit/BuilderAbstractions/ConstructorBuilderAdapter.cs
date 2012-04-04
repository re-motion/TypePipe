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

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions
{
  /// <summary>
  /// Adapts <see cref="System.Reflection.Emit.ConstructorBuilder"/> with the <see cref="IConstructorBuilder"/> interface.
  /// </summary>
  public class ConstructorBuilderAdapter : IConstructorBuilder
  {
    private readonly ConstructorBuilder _constructorBuilder;

    public ConstructorBuilderAdapter (ConstructorBuilder constructorBuilder)
    {
      ArgumentUtility.CheckNotNull ("constructorBuilder", constructorBuilder);

      _constructorBuilder = constructorBuilder;
    }

    public ConstructorBuilder ConstructorBuilder
    {
      get { return _constructorBuilder; }
    }

    public void DefineParameter (int iSequence, ParameterAttributes attributes, string strParamName)
    {
      _constructorBuilder.DefineParameter (iSequence, attributes, strParamName);
    }

    [CLSCompliant (false)]
    public void SetBody (LambdaExpression body, IILGeneratorFactory ilGeneratorFactory, DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      ArgumentUtility.CheckNotNull ("body", body);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      // TODO 4743: Covnert to argument check
      Assertion.IsTrue (body.ReturnType == typeof (void));

      var builderForLambdaCompiler = new ConstructorBuilderForLambdaCompiler(_constructorBuilder, ilGeneratorFactory);
      LambdaCompiler.Compile (body, builderForLambdaCompiler, debugInfoGeneratorOrNull);
    }

    [CLSCompliant (false)]
    public void Emit (IILGenerator ilGenerator, OpCode opCode)
    {
      ArgumentUtility.CheckNotNull ("ilGenerator", ilGenerator);

      ilGenerator.Emit (opCode, _constructorBuilder);
    }
  }
}