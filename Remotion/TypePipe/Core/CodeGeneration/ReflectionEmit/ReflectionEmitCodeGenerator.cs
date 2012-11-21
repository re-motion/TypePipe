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
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// This class represents the Reflection.Emit-based <see cref="ICodeGenerator"/> of the pipeline.
  /// </summary>
  public class ReflectionEmitCodeGenerator : IReflectionEmitCodeGenerator
  {
    private const string c_assemblyNamePattern = "TypePipe_GeneratedAssembly_{0}.dll";

    private readonly IModuleBuilderFactory _moduleBuilderFactory;

    private int _counter;
    private string _assemblyName;

    [CLSCompliant (false)]
    public ReflectionEmitCodeGenerator (IModuleBuilderFactory moduleBuilderFactory)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilderFactory", moduleBuilderFactory);

      _moduleBuilderFactory = moduleBuilderFactory;
    }

    [CLSCompliant (false)]
    public IModuleBuilder CurrentModuleBuilder { get; private set; }

    public string AssemblyName
    {
      get { return _assemblyName = _assemblyName ?? string.Format (c_assemblyNamePattern, ++_counter); }
    }

    public void SetAssemblyName (string assemblyName)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyName", assemblyName);

      if (CurrentModuleBuilder != null)
      {
        var flushMethod = MemberInfoFromExpressionUtility.GetMethod (() => FlushCodeToDisk());
        var message = string.Format ("Cannot set assembly name after a type has been defined (use {0}() to start a new assembly).", flushMethod.Name);
        throw new InvalidOperationException (message);
      }

      _assemblyName = assemblyName;
    }

    public string FlushCodeToDisk ()
    {
      if (CurrentModuleBuilder == null)
        throw new InvalidOperationException ("Cannot flush to disk if no type was defined.");

      var assemblyPath = CurrentModuleBuilder.SaveToDisk();
      _assemblyName = null;
      CurrentModuleBuilder = null;

      return assemblyPath;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attributes, Type parent)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("parent", parent);

      if (CurrentModuleBuilder == null)
        CurrentModuleBuilder = _moduleBuilderFactory.CreateModuleBuilder (AssemblyName);

      return CurrentModuleBuilder.DefineType (name, attributes, parent);
    }
  }
}