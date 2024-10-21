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
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="ModuleBuilder"/> with the <see cref="IModuleBuilder"/> interface.
  /// </summary>
  public class ModuleBuilderAdapter : BuilderAdapterBase, IModuleBuilder
  {
    private readonly ModuleBuilder _moduleBuilder;
    private readonly IAssemblyBuilder _assemblyBuilder;

    public string ScopeName
    {
      get { return _moduleBuilder.ScopeName; }
    }

    public IAssemblyBuilder AssemblyBuilder
    {
      get { return _assemblyBuilder; }
    }

    public ModuleBuilderAdapter (ModuleBuilder moduleBuilder)
        : base (ArgumentUtility.CheckNotNull ("moduleBuilder", moduleBuilder).SetCustomAttribute)
    {
#if NET9_0_OR_GREATER
      Assertion.IsTrue (moduleBuilder.Assembly is PersistedAssemblyBuilder);
#else
      Assertion.IsTrue (moduleBuilder.Assembly is AssemblyBuilder);
#endif

      _moduleBuilder = moduleBuilder;
#if NET9_0_OR_GREATER
      _assemblyBuilder = new AssemblyBuilderAdapter ((PersistedAssemblyBuilder) moduleBuilder.Assembly, moduleBuilder);
#else
      _assemblyBuilder = new AssemblyBuilderAdapter (((AssemblyBuilder) moduleBuilder.Assembly), moduleBuilder);
#endif
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var typeBuilder = _moduleBuilder.DefineType (name, attr);
      return new TypeBuilderAdapter (typeBuilder);
    }
  }
}