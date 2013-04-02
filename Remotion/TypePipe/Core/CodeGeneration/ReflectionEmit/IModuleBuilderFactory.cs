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
using Remotion.ServiceLocation;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implementations of this interface create instances of <see cref="IModuleBuilder"/>.
  /// </summary>
  [ConcreteImplementation (typeof (ModuleBuilderFactory))]
  [CLSCompliant (false)]
  public interface IModuleBuilderFactory
  {
    /// <summary>
    /// Creates a module builder.
    /// </summary>
    /// <param name="assemblyName">The assembly name (without the file ending '.dll').</param>
    /// <param name="assemblyDirectoryOrNull">The directory in which the assembly will be saved when <see cref="IAssemblyBuilder.SaveToDisk" /> is
    /// called on <see cref="IModuleBuilder.AssemblyBuilder"/> of the returned <see cref="IModuleBuilder" />.</param>
    /// <param name="strongNamed">Whether or not to strong-name the generated assembly.</param>
    /// <param name="keyFilePathOrNull">The file path to the strong-name key, or <see langword="null"/> if the factory should create a temporary key.</param>
    /// <returns>
    /// The created module builder.
    /// </returns>
    IModuleBuilder CreateModuleBuilder (string assemblyName, string assemblyDirectoryOrNull, bool strongNamed, string keyFilePathOrNull);
  }
}