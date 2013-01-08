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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Decorates an instance of <see cref="IMethodBuilder"/> to allow <see cref="MutableType"/>s to be used in signatures and 
  /// for checking strong-name compatibility.
  /// </summary>
  public class MethodBuilderDecorator : BuilderDecoratorBase, IMethodBuilder
  {
    private readonly IMethodBuilder _methodBuilder;
    private readonly IEmittableOperandProvider _emittableOperandProvider;

    [CLSCompliant (false)]
    public MethodBuilderDecorator (IMethodBuilder methodBuilder, IEmittableOperandProvider emittableOperandProvider)
        : base (methodBuilder, emittableOperandProvider)
    {
      _methodBuilder = methodBuilder;
      _emittableOperandProvider = emittableOperandProvider;
    }

    public IParameterBuilder DefineParameter (int iSequence, ParameterAttributes attributes, string strParamName)
    {
      throw new System.NotImplementedException();
    }

    [CLSCompliant (false)]
    public void SetBody (LambdaExpression body, IILGeneratorFactory ilGeneratorFactory, DebugInfoGenerator debugInfoGeneratorOrNull)
    {
      throw new System.NotImplementedException();
    }

    public void RegisterWith (IEmittableOperandProvider emittableOperandProvider, MutableMethodInfo method)
    {
      throw new System.NotImplementedException();
    }
  }
}