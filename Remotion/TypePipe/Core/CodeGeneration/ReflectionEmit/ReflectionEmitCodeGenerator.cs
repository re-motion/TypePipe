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
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Configuration;
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
      private readonly Func<bool> _forceStrongNamingFunc;
      private bool? _forceStrongNaming;

      // TODO 5057: Use Lazy<T>.
      public ModuleContext (Func<bool> forceStrongNamingFunc)
      {
        _forceStrongNamingFunc = forceStrongNamingFunc;
      }

      public bool ForceStrongNaming
      {
        get
        {
          if (!_forceStrongNaming.HasValue)
            _forceStrongNaming = _forceStrongNamingFunc();

          return _forceStrongNaming.Value;
        }
      }

      public IModuleBuilder ModuleBuilder { get; set; }
      public DebugInfoGenerator DebugInfoGenerator { get; set; }
    }

    private const string c_defaultAssemblyNamePattern = "TypePipe_GeneratedAssembly_{counter}";

    // This field is static to avoid that different pipeline instances override each other's assemblies.
    private static int s_counter;

    private readonly IModuleBuilderFactory _moduleBuilderFactory;
    private readonly IConfigurationProvider _configurationProvider;

    private string _assemblyDirectory;
    private string _assemblyNamePattern = c_defaultAssemblyNamePattern;
    private ModuleContext _moduleContext;

    [CLSCompliant (false)]
    public ReflectionEmitCodeGenerator (IModuleBuilderFactory moduleBuilderFactory, IConfigurationProvider configurationProvider)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilderFactory", moduleBuilderFactory);

      _moduleBuilderFactory = moduleBuilderFactory;
      _configurationProvider = configurationProvider;

      ResetContext();
    }

    private void ResetContext ()
    {
      _moduleContext = new ModuleContext (() => _configurationProvider.ForceStrongNaming);
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
      // assemblyDirectory may be null.
      EnsureNoCurrentModuleBuilder ("assembly directory");

      _assemblyDirectory = assemblyDirectoryOrNull;
    }

    public void SetAssemblyNamePattern (string assemblyNamePattern)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyNamePattern", assemblyNamePattern);
      EnsureNoCurrentModuleBuilder ("assembly name pattern");

      _assemblyNamePattern = assemblyNamePattern;
    }

    public string FlushCodeToDisk (CustomAttributeDeclaration assemblyAttribute)
    {
      ArgumentUtility.CheckNotNull ("assemblyAttribute", assemblyAttribute);

      var moduleBuilder = _moduleContext.ModuleBuilder;
      if (moduleBuilder == null)
        return null;

      var assemblyBuilder = moduleBuilder.AssemblyBuilder;
      ResetContext();

      assemblyBuilder.SetCustomAttribute (assemblyAttribute);
      return assemblyBuilder.SaveToDisk();
    }

    public IEmittableOperandProvider CreateEmittableOperandProvider ()
    {
      IEmittableOperandProvider emittableOperandProvider = new EmittableOperandProvider (new DelegateProvider());
      return _moduleContext.ForceStrongNaming
                 ? new StrongNameCheckingEmittableOperandProviderDecorator (emittableOperandProvider, new TypeAnalyzer (new AssemblyAnalyzer()))
                 : emittableOperandProvider;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attributes, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      if (_moduleContext.ModuleBuilder == null)
      {
        var assemblyName = GetNextAssemblyName();
        var strongName = _moduleContext.ForceStrongNaming;
        var keyFilePathOrNull = _configurationProvider.KeyFilePath;

        _moduleContext.ModuleBuilder = _moduleBuilderFactory.CreateModuleBuilder (assemblyName, _assemblyDirectory, strongName, keyFilePathOrNull);
      }

      var typeBuilder = _moduleContext.ModuleBuilder.DefineType (name, attributes);
      var typeBuilderDecorator = new TypeBuilderDecorator (typeBuilder, emittableOperandProvider);

      return typeBuilderDecorator;
    }

    private string GetNextAssemblyName ()
    {
      var uniqueCounterValue = Interlocked.Increment (ref s_counter);
      return _assemblyNamePattern.Replace ("{counter}", uniqueCounterValue.ToString (CultureInfo.InvariantCulture));
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