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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// This class creates instances of <see cref="IModuleBuilder"/>.
  /// </summary>
  /// <remarks> The module will be created with <see cref="AssemblyBuilderAccess.RunAndSave"/> and the <c>emitSymbolInfo</c> flag set to
  /// <see langword="true"/>.
  /// </remarks>
  public class ModuleBuilderFactory : IModuleBuilderFactory
  {
    [CLSCompliant (false)]
    public IModuleBuilder CreateModuleBuilder (string assemblyName, string assemblyDirectoryOrNull)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyName", assemblyName);
      // assemblyDirectory may be null.

      var name = new AssemblyName (assemblyName);
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (name, AssemblyBuilderAccess.RunAndSave, assemblyDirectoryOrNull);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (name + ".dll", emitSymbolInfo: true);
      // TODO Review: Pass in module file name
      var moduleBuilderAdapter = new ModuleBuilderAdapter (moduleBuilder);
      var uniqueNamingModuleBuilder = new UniqueNamingModuleBuilderDecorator (moduleBuilderAdapter);

      return uniqueNamingModuleBuilder;
    }
  }
}