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
using Remotion.Collections;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  public static class ReflectionEmitBackendFactory
  {
    // TODO Review: Unit test.
    /// <summary>
    /// Creates a standard <see cref="IModuleBuilder"/>, decorated with <see cref="UniqueNamingModuleBuilderDecorator"/> and backed by a
    /// real Reflection.Emit <see cref="ModuleBuilder"/>.
    /// The <see cref="AssemblyBuilder"/> created in this process is also returned as the second item of the <see cref="Tuple{T1,T2}"/>.
    /// </summary>
    /// <remarks> The module will be created with <see cref="AssemblyBuilderAccess.RunAndSave"/> and the <c>emitSymbolInfo</c> flag set to
    /// <see langword="true"/>.
    /// </remarks>
    /// <param name="assemblyName">The assembly name (without '.dll').</param>
    /// <param name="assemblyDirectory">The name of the directory where the assembly is potentially saved.</param>
    /// <returns>The created module and assembly builder.</returns>
    [CLSCompliant (false)]
    // TODO Review: Remove assemblyBuilderAccess, make RunAndSave by default.
    public static Tuple<IModuleBuilder, AssemblyBuilder> CreateModuleBuilder (string assemblyName, string assemblyDirectory = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyName", assemblyName);

      // TODO Review: Refactor existing DefaultSubclassProxyBuilderFactoryTest into two tests - one for this method. 
      // Check assemblyName, assemblyDirectory, module name, concrete wiring of module builder.
      // Check assemblyBuilderAccess, emitSymbolInfo by trying to save/check whether PDB is generated.
      var name = new AssemblyName (assemblyName);
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (name, AssemblyBuilderAccess.RunAndSave, assemblyDirectory);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (name + ".dll", emitSymbolInfo: true);
      var moduleBuilderAdapter = new ModuleBuilderAdapter (moduleBuilder);
      var uniqueNameModuleBuilderDecorator = (IModuleBuilder) new UniqueNamingModuleBuilderDecorator (moduleBuilderAdapter);

      return Tuple.Create (uniqueNameModuleBuilderDecorator, assemblyBuilder);
    }
  }
}