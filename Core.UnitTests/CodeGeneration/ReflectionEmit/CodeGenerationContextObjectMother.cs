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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  public static class CodeGenerationContextObjectMother
  {
    public static CodeGenerationContext GetSomeContext (
        MutableType mutableType = null,
        ITypeBuilder typeBuilder = null,
        DebugInfoGenerator debugInfoGenerator = null,
        IEmittableOperandProvider emittableOperandProvider = null)
    {
      return new CodeGenerationContext (
          mutableType ?? MutableTypeObjectMother.Create (),
          typeBuilder ?? new Mock<ITypeBuilder>().Object,
          debugInfoGenerator ?? new Mock<DebugInfoGenerator>().Object,
          emittableOperandProvider ?? new Mock<IEmittableOperandProvider>().Object);
    }
  }
}