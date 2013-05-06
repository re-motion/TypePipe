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
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.LambdaCompilation;

namespace Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit
{
  /// <summary>
  /// Creates <see cref="MemberEmitter"/> instances.
  /// </summary>
  public class MemberEmitterFactory : IMemberEmitterFactory
  {
    public IMemberEmitter CreateMemberEmitter (IEmittableOperandProvider emittableOperandProvider)
    {
      var ilGeneratorFactory = new ILGeneratorDecoratorFactory (new OffsetTrackingILGeneratorFactory(), emittableOperandProvider);

      // The trampoline provider is part of the preparation stage and as such only generates expressions that are already prepared.
      // Therefore, it can use a MemberEmitter that does not prepare its expressions.
      // Should this ever change, use Method Injection to inject the MemberEmitter into the ExpressionPreparer (to solve the circular dependency).
      var nonPreparingMemberEmitter = new MemberEmitter (new NullExpressionPreparer(), ilGeneratorFactory);
      var trampolineProvider = new MethodTrampolineProvider (nonPreparingMemberEmitter);
      var expressionPreparer = new ExpressionPreparer (trampolineProvider);

      return new MemberEmitter (expressionPreparer, ilGeneratorFactory);
    }
  }
}