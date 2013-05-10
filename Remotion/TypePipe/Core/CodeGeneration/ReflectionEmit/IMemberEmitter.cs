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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Defines an interface for classes emitting members for mutable reflection objects. Used by <see cref="MutableTypeCodeGenerator"/>.
  /// </summary>
  public interface IMemberEmitter
  {
    void AddNestedType (CodeGenerationContext context, MutableType nestedType);
    void AddField (CodeGenerationContext context, MutableFieldInfo field);
    void AddConstructor (CodeGenerationContext context, MutableConstructorInfo constructor);
    void AddMethod (CodeGenerationContext context, MutableMethodInfo method);
    void AddProperty (CodeGenerationContext context, MutablePropertyInfo property);
    void AddEvent (CodeGenerationContext context, MutableEventInfo event_);
  }
}