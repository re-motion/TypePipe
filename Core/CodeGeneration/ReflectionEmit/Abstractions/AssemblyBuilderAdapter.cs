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
using System.IO;
using System.Reflection.Emit;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
#if NET9_0_OR_GREATER
  /// <summary>
  /// Adapts <see cref="PersistedAssemblyBuilder"/> with the <see cref="IAssemblyBuilder"/> interface.
  /// </summary>
#else
  /// <summary>
  /// Adapts <see cref="AssemblyBuilder"/> with the <see cref="IAssemblyBuilder"/> interface.
  /// </summary>
#endif
  public class AssemblyBuilderAdapter : BuilderAdapterBase, IAssemblyBuilder
  {
#if NET9_0_OR_GREATER
    private readonly PersistedAssemblyBuilder _assemblyBuilder;
    private readonly string _assemblyDirectory;
#else
    private readonly AssemblyBuilder _assemblyBuilder;
#endif
    private readonly ModuleBuilder _moduleBuilder;

#if NET9_0_OR_GREATER
    public AssemblyBuilderAdapter (PersistedAssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder, string assemblyDirectory)
#else
    public AssemblyBuilderAdapter (AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder)
#endif
        : base (ArgumentUtility.CheckNotNull ("assemblyBuilder", assemblyBuilder).SetCustomAttribute)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilder", moduleBuilder);
#if NET9_0_OR_GREATER
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyDirectory", assemblyDirectory);
#endif

      _assemblyBuilder = assemblyBuilder;
      _moduleBuilder = moduleBuilder;
#if NET9_0_OR_GREATER
      _assemblyDirectory = assemblyDirectory;
#endif
    }

    public string AssemblyName
    {
      get { return _assemblyBuilder.GetName().Name; }
    }

    public byte[] PublicKey
    {
      get { return _assemblyBuilder.GetName ().GetPublicKey (); }
    }

#if NET9_0_OR_GREATER
    public string AssemblyDirectory
    {
      get { return _assemblyDirectory; }
    }
#endif

    public string SaveToDisk ()
    {
#if NETFRAMEWORK
      // Scope name is the module name or file name, i.e., assembly name + '.dll'.
      _assemblyBuilder.Save (_moduleBuilder.ScopeName);

      // This is the absolute path to the module, which is also the assembly file path for single-module assemblies.
      return _moduleBuilder.FullyQualifiedName;
#elif NET9_0_OR_GREATER
      // Scope name is the module name or file name, i.e., assembly name + '.dll'.
      var modulePath = Path.Combine (_assemblyDirectory, _moduleBuilder.ScopeName);
      using (var stream = new FileStream (modulePath, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        _assemblyBuilder.Save (stream);
      }

      return modulePath;
#else
      throw new PlatformNotSupportedException ("Assembly persistence is not supported.");
#endif
    }
  }
}
