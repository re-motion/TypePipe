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
using JetBrains.Annotations;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Helps with building initializations for a <see cref="MutableType"/>.
  /// </summary>
  /// <remarks>This interface is an implementation detail of <see cref="MutableTypeCodeGenerator"/>.</remarks>
  /// <threadsafety static="true" instance="true"/>
  public interface IInitializationBuilder
  {
    [CanBeNull] Tuple<FieldInfo, MethodInfo> CreateInitializationMembers ([NotNull] MutableType mutableType);

    void WireConstructorWithInitialization (
        [NotNull] MutableConstructorInfo constructor,
        [CanBeNull] Tuple<FieldInfo, MethodInfo> initializationMembers);
  }
}