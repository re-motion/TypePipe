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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Behaves like <see cref="MutableTypeCodeGenerator"/> but for nested types.
  /// </summary>
  public class MutableNestedTypeCodeGenerator : MutableTypeCodeGenerator
  {
    private readonly ITypeBuilder _enclosingTypeBuilder;

    [CLSCompliant (false)]
    public MutableNestedTypeCodeGenerator (
        ITypeBuilder enclosingTypeBuilder,
        MutableType mutableType,
        IMutableNestedTypeCodeGeneratorFactory nestedTypeCodeGeneratorFactory,
        IReflectionEmitCodeGenerator codeGenerator,
        IEmittableOperandProvider emittableOperandProvider,
        IMemberEmitter memberEmitter,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
        : base (
            mutableType,
            nestedTypeCodeGeneratorFactory,
            codeGenerator,
            emittableOperandProvider,
            memberEmitter,
            initializationBuilder,
            proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("enclosingTypeBuilder", enclosingTypeBuilder);

      _enclosingTypeBuilder = enclosingTypeBuilder;
    }

    [CLSCompliant (false)]
    protected override ITypeBuilder DefineType (IReflectionEmitCodeGenerator codeGenerator, IEmittableOperandProvider emittableOperandProvider)
    {
      return _enclosingTypeBuilder.DefineNestedType (MutableType.Name, MutableType.Attributes);
    }
  }
}