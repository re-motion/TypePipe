﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
  /// Serves as a factory for instances of <see cref="IMutableTypeCodeGenerator"/> for nested types.
  /// </summary>
  public class MutableNestedTypeCodeGeneratorFactory : IMutableNestedTypeCodeGeneratorFactory
  {
    private readonly IReflectionEmitCodeGenerator _reflectionEmitCodeGenerator;
    private readonly IEmittableOperandProvider _emittableOperandProvider;
    private readonly IMemberEmitter _memberEmitter;
    private readonly IInitializationBuilder _initializationBuilder;

    [CLSCompliant (false)]
    public MutableNestedTypeCodeGeneratorFactory (
        IReflectionEmitCodeGenerator reflectionEmitCodeGenerator,
        IEmittableOperandProvider emittableOperandProvider,
        IMemberEmitter memberEmitter,
        IInitializationBuilder initializationBuilder)
    {
      ArgumentUtility.CheckNotNull ("reflectionEmitCodeGenerator", reflectionEmitCodeGenerator);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);

      _reflectionEmitCodeGenerator = reflectionEmitCodeGenerator;
      _emittableOperandProvider = emittableOperandProvider;
      _memberEmitter = memberEmitter;
      _initializationBuilder = initializationBuilder;
    }

    [CLSCompliant (false)]
    public IMutableTypeCodeGenerator Create (ITypeBuilder enclosingTypeBuilder, MutableType nestedType)
    {
      ArgumentUtility.CheckNotNull ("enclosingTypeBuilder", enclosingTypeBuilder);
      ArgumentUtility.CheckNotNull ("nestedType", nestedType);

      return new MutableNestedTypeCodeGenerator (
          enclosingTypeBuilder,
          nestedType,
          this,
          _reflectionEmitCodeGenerator,
          _emittableOperandProvider,
          _memberEmitter,
          _initializationBuilder);
    }
  }
}