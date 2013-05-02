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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// The Reflection.Emit-based <see cref="IGeneratedCodeFlusher"/> of the pipeline.
  /// </summary>
  /// <remarks>
  /// This class is not thread-safe. Thread-safety will be enforced by the <see cref="TypeCache"/>.
  /// </remarks>
  public class ReflectionEmitCodeGenerator : IReflectionEmitCodeGenerator
  {
    private class ModuleContext
    {
      public IModuleBuilder ModuleBuilder { get; set; }
      public DebugInfoGenerator DebugInfoGenerator { get; set; }
    }

    private const string c_defaultAssemblyNamePattern = "TypePipe_GeneratedAssembly_{counter}";

    // This field is static to avoid that different pipeline instances override each other's assemblies.
    private static int s_counter;

    private readonly IModuleBuilderFactory _moduleBuilderFactory;
    private readonly bool _forceStrongNaming;
    private readonly string _keyFilePath;

    private string _assemblyDirectory;
    private string _assemblyNamePattern = c_defaultAssemblyNamePattern;
    private ModuleContext _moduleContext;

    [CLSCompliant (false)]
    public ReflectionEmitCodeGenerator (IModuleBuilderFactory moduleBuilderFactory, bool forceStrongNaming, string keyFilePath)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilderFactory", moduleBuilderFactory);
      // Key file path may be null.

      _moduleBuilderFactory = moduleBuilderFactory;
      _forceStrongNaming = forceStrongNaming;
      _keyFilePath = keyFilePath;

      ResetModuleContext();
    }

    public string AssemblyDirectory
    {
      get { return _assemblyDirectory; }
    }

    public string AssemblyNamePattern
    {
      get { return _assemblyNamePattern; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _moduleContext.DebugInfoGenerator ?? (_moduleContext.DebugInfoGenerator = DebugInfoGenerator.CreatePdbGenerator()); }
    }

    public void SetAssemblyDirectory (string assemblyDirectoryOrNull)
    {
      // Assembly directory may be null (to use the current directory).
      EnsureNoCurrentModuleBuilder ("assembly directory");

      _assemblyDirectory = assemblyDirectoryOrNull;
    }

    public void SetAssemblyNamePattern (string assemblyNamePattern)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyNamePattern", assemblyNamePattern);
      EnsureNoCurrentModuleBuilder ("assembly name pattern");

      _assemblyNamePattern = assemblyNamePattern;
    }

    public virtual string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      ArgumentUtility.CheckNotNull ("assemblyAttributes", assemblyAttributes);

      var moduleBuilder = _moduleContext.ModuleBuilder;
      if (moduleBuilder == null)
        return null;

      var assemblyPath = SaveToDisk (moduleBuilder.AssemblyBuilder, assemblyAttributes);
      ResetModuleContext();

      return assemblyPath;
    }

    public IEmittableOperandProvider CreateEmittableOperandProvider ()
    {
      IEmittableOperandProvider emittableOperandProvider = new EmittableOperandProvider (new DelegateProvider());
      return _forceStrongNaming
                 ? new StrongNameCheckingEmittableOperandProviderDecorator (emittableOperandProvider, new TypeAnalyzer (new AssemblyAnalyzer()))
                 : emittableOperandProvider;
    }

    [CLSCompliant (false)]
    public virtual ITypeBuilder DefineType (string name, TypeAttributes attributes, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      if (_moduleContext.ModuleBuilder == null)
      {
        var assemblyName = GetNextAssemblyName();
        _moduleContext.ModuleBuilder = _moduleBuilderFactory.CreateModuleBuilder (assemblyName, _assemblyDirectory, _forceStrongNaming, _keyFilePath);
      }

      var typeBuilder = _moduleContext.ModuleBuilder.DefineType (name, attributes);
      return new TypeBuilderDecorator (typeBuilder, emittableOperandProvider);
    }

    // Used by DebuggerWorkaroundCodeGenerator.
    protected void ResetModuleContext ()
    {
      _moduleContext = new ModuleContext();
    }

    private string GetNextAssemblyName ()
    {
      var uniqueCounterValue = Interlocked.Increment (ref s_counter);
      return _assemblyNamePattern.Replace ("{counter}", uniqueCounterValue.ToString (CultureInfo.InvariantCulture));
    }

    private string SaveToDisk (IAssemblyBuilder assemblyBuilder, IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      foreach (var attribute in assemblyAttributes)
        assemblyBuilder.SetCustomAttribute (attribute);

      return assemblyBuilder.SaveToDisk();
    }

    private void EnsureNoCurrentModuleBuilder (string propertyDescription)
    {
      if (_moduleContext.ModuleBuilder != null)
      {
        var flushMethod = MemberInfoFromExpressionUtility.GetMethod ((ICodeManager o) => o.FlushCodeToDisk());
        var message = string.Format (
            "Cannot set {0} after a type has been defined (use {1}() to start a new assembly).", propertyDescription, flushMethod.Name);
        throw new InvalidOperationException (message);
      }
    }
  }
}