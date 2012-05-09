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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Defines an API for classes registering emittable operands for Reflection objects. This is used to allow mutable Reflection objects to be
  /// used within expression trees - when generating code, the mutable objects are replaced with emittable counterparts using this interface.
  /// </summary>
  // TODO 4813: Remove attribute here and at all usage sites
  [CLSCompliant (false)]
  public interface IEmittableOperandProvider
  {
    void AddMapping (Type mappedType, IEmittableOperand typeOperand);
    void AddMapping (FieldInfo mappedFieldInfo, IEmittableOperand fieldOperand);
    void AddMapping (ConstructorInfo mappedConstructorInfo, IEmittableOperand constructorOperand);
    void AddMapping (MethodInfo mappedMethodInfo, IEmittableMethodOperand methodOperand);
    IEmittableOperand GetEmittableType (Type type);
    IEmittableOperand GetEmittableField (FieldInfo fieldInfo);
    IEmittableOperand GetEmittableConstructor (ConstructorInfo constructorInfo);
    IEmittableMethodOperand GetEmittableMethod (MethodInfo methodInfo);
  }
}