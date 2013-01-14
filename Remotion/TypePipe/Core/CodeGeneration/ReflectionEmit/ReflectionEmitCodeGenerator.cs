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
using System.Runtime.CompilerServices;
using System.Threading;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Configuration;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// The Reflection.Emit-based <see cref="ICodeGenerator"/> of the pipeline.
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
      public IEmittableOperandProvider EmittableOperandProvider { get; set; }
      public string AssemblyName { get; set; }
    }

    private const string c_assemblyNamePattern = "TypePipe_GeneratedAssembly_{0}";

    private static int s_counter;

    private readonly IModuleBuilderFactory _moduleBuilderFactory;
    private readonly ITypePipeConfigurationProvider _configurationProvider;
    private readonly DebugInfoGenerator _debugInfoGenerator = DebugInfoGenerator.CreatePdbGenerator();

    private string _assemblyDirectory;
    private ModuleContext _moduleContext;

    [CLSCompliant (false)]
    public ReflectionEmitCodeGenerator (IModuleBuilderFactory moduleBuilderFactory, ITypePipeConfigurationProvider configurationProvider)
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

    public string AssemblyName
    {
      get
      {
        if (_moduleContext.AssemblyName == null)
        {
          var uniqueCounterValue = Interlocked.Increment (ref s_counter);
          _moduleContext.AssemblyName = string.Format (c_assemblyNamePattern, uniqueCounterValue);
        }

        return _moduleContext.AssemblyName;
      }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    public IEmittableOperandProvider EmittableOperandProvider
    {
      get
      {
        if (_moduleContext.EmittableOperandProvider == null)
        {
          IEmittableOperandProvider provider = new EmittableOperandProvider();
          _moduleContext.EmittableOperandProvider =
              _moduleContext.ForceStrongNaming ? new StrongNameCheckingEmittableOperandProviderDecorator (provider) : provider;
        }

        return _moduleContext.EmittableOperandProvider;
      }
    }

    public void SetAssemblyDirectory (string assemblyDirectoryOrNull)
    {
      // assemblyDirectory may be null.
      EnsureNoCurrentModuleBuilder ("assembly directory");

      _assemblyDirectory = assemblyDirectoryOrNull;
    }

    public void SetAssemblyName (string assemblyName)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyName", assemblyName);
      EnsureNoCurrentModuleBuilder ("assembly name");

      _moduleContext.AssemblyName = assemblyName;
    }

    public string FlushCodeToDisk ()
    {
      if (_moduleContext.ModuleBuilder == null)
        return null;

      var assemblyPath = _moduleContext.ModuleBuilder.SaveToDisk();
      ResetContext();

      return assemblyPath;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attributes, Type parent)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("parent", parent);

      if (_moduleContext.ModuleBuilder == null)
      {
        var strongName = _moduleContext.ForceStrongNaming;
        var keyFilePathOrNull = _configurationProvider.KeyFilePath;

        _moduleContext.ModuleBuilder = _moduleBuilderFactory.CreateModuleBuilder (
            AssemblyName, _assemblyDirectory, strongName, keyFilePathOrNull, EmittableOperandProvider);
      }

      return _moduleContext.ModuleBuilder.DefineType (name, attributes, parent);
    }

    private void EnsureNoCurrentModuleBuilder (string propertyDescription)
    {
      if (_moduleContext.ModuleBuilder != null)
      {
        var flushMethod = MemberInfoFromExpressionUtility.GetMethod (() => FlushCodeToDisk());
        var message = string.Format (
            "Cannot set {0} after a type has been defined (use {1}() to start a new assembly).", propertyDescription, flushMethod.Name);
        throw new InvalidOperationException (message);
      }
    }
  }
}