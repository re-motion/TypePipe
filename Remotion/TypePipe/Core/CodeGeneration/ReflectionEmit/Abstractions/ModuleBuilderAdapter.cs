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
  public class ModuleBuilderAdapter : IModuleBuilder
  {
    private readonly ModuleBuilder _moduleBuilder;
    private readonly string _moduleName;
    private readonly AssemblyBuilder _assemblyBuilder;

    public string ScopeName
    {
      get { return _moduleBuilder.ScopeName; }
    }

    public string AssemblyName
    {
      get { return _assemblyBuilder.GetName().Name; }
    }

    public byte[] PublicKey
    {
      get { return _assemblyBuilder.GetName().GetPublicKey(); }
    }

    public ModuleBuilderAdapter (ModuleBuilder moduleBuilder, string moduleName)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilder", moduleBuilder);
      ArgumentUtility.CheckNotNullOrEmpty ("moduleName", moduleName);
      Assertion.IsTrue (moduleBuilder.Assembly is AssemblyBuilder);

      _moduleBuilder = moduleBuilder;
      _moduleName = moduleName;
      _assemblyBuilder = (AssemblyBuilder) moduleBuilder.Assembly;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attr, Type parent)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("parent", parent);

      var typeBuilder = _moduleBuilder.DefineType (name, attr, parent);
      return new TypeBuilderAdapter (typeBuilder);
    }

    public string SaveToDisk ()
    {
      _assemblyBuilder.Save (_moduleName);

      // This is the absolute path to the module, which is also the assembly file path for single-module assemblies.
      return _moduleBuilder.FullyQualifiedName;
    }
  }
}