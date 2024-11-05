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
using JetBrains.Annotations;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// The Reflection.Emit-based code generator of the pipeline.
  /// </summary>
  /// <remarks>
  /// This class is not thread-safe. Thread-safety will be enforced by the <see cref="TypeCache"/>.
  /// </remarks>
  /// <threadsafety static="true" instance="false"/>
  public class ReflectionEmitCodeGenerator : IReflectionEmitCodeGenerator
  {
    private class ModuleContext
    {
      public IModuleBuilder ModuleBuilder { get; set; }
      public DebugInfoGenerator DebugInfoGenerator { get; set; }
    }

    // This field is static to avoid that different pipeline instances override each other's assemblies.
    private static int s_counter;

    private readonly IModuleBuilderFactory _moduleBuilderFactory;
    private readonly bool _forceStrongNaming;
    private readonly string _keyFilePath;

    private readonly string _assemblyDirectory;
    private readonly string _assemblyNamePattern;
    private ModuleContext _moduleContext;

    [CLSCompliant (false)]
    public ReflectionEmitCodeGenerator (
        IModuleBuilderFactory moduleBuilderFactory,
        bool forceStrongNaming,
        [CanBeNull] string keyFilePath,
        [CanBeNull] string assemblyDirectory,
        [NotNull] string assemblyNamePattern)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilderFactory", moduleBuilderFactory);
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyNamePattern", assemblyNamePattern);

      _moduleBuilderFactory = moduleBuilderFactory;
      _forceStrongNaming = forceStrongNaming;
      _keyFilePath = keyFilePath;
      _assemblyDirectory = assemblyDirectory;
      _assemblyNamePattern = assemblyNamePattern;

      ResetModuleContext();
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get
      {
        return _moduleContext.DebugInfoGenerator ??
#if FEATURE_PDBEMIT
           (_moduleContext.DebugInfoGenerator = DebugInfoGenerator.CreatePdbGenerator());
#else
           (_moduleContext.DebugInfoGenerator = NullDebugInfoGenerator.Instance);
#endif
      }
    }

    public string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
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
    public ITypeBuilder DefineType (string name, TypeAttributes attributes, IEmittableOperandProvider emittableOperandProvider)
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
      return _assemblyNamePattern.Replace (PipelineSettings.CounterPattern, uniqueCounterValue.ToString (CultureInfo.InvariantCulture));
    }

    private string SaveToDisk (IAssemblyBuilder assemblyBuilder, IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
    {
      foreach (var attribute in assemblyAttributes)
        assemblyBuilder.SetCustomAttribute (attribute);

      return assemblyBuilder.SaveToDisk();
    }
  }
}