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
using System.Reflection.Emit;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="AssemblyBuilder"/> with the <see cref="IAssemblyBuilder"/> interface.
  /// </summary>
  public class AssemblyBuilderAdapter : BuilderAdapterBase, IAssemblyBuilder
  {
    private readonly AssemblyBuilder _assemblyBuilder;
    private readonly ModuleBuilder _moduleBuilder;

    public AssemblyBuilderAdapter (AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder)
        : base (ArgumentUtility.CheckNotNull ("assemblyBuilder", assemblyBuilder).SetCustomAttribute)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilder", moduleBuilder);

      _assemblyBuilder = assemblyBuilder;
      _moduleBuilder = moduleBuilder;
    }

    public string SaveToDisk ()
    {
      // Scope name is the module name or file name, i.e., assembly name + '.dll'.
      _assemblyBuilder.Save (_moduleBuilder.ScopeName);

      // This is the absolute path to the module, which is also the assembly file path for single-module assemblies.
      return _moduleBuilder.FullyQualifiedName;
    }
  }
}