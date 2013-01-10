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
using Remotion.TypePipe.StrongNaming;
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
    private const string c_assemblyNamePattern = "TypePipe_GeneratedAssembly_{0}";

    private static int s_counter;

    private readonly IModuleBuilderFactory _moduleBuilderFactory;
    private readonly ITypePipeConfigurationProvider _configurationProvider;
    private readonly DebugInfoGenerator _debugInfoGenerator = DebugInfoGenerator.CreatePdbGenerator();

    private IModuleBuilder _currentModuleBuilder;
    private IEmittableOperandProvider _emittableOperandProvider;
    private string _assemblyDirectory;
    private string _assemblyName;

    [CLSCompliant (false)]
    public ReflectionEmitCodeGenerator (IModuleBuilderFactory moduleBuilderFactory, ITypePipeConfigurationProvider configurationProvider)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilderFactory", moduleBuilderFactory);

      _moduleBuilderFactory = moduleBuilderFactory;
      _configurationProvider = configurationProvider;
    }

    public string AssemblyDirectory
    {
      get { return _assemblyDirectory; }
    }

    public string AssemblyName
    {
      get
      {
        if (_assemblyName == null)
        {
          var uniqueCounterValue = Interlocked.Increment (ref s_counter);
          _assemblyName = string.Format (c_assemblyNamePattern, uniqueCounterValue);
        }

        return _assemblyName;
      }
    }

    public bool IsStrongNamingEnabled
    {
      get { return _configurationProvider.ForceStrongNaming; }
    }

    public DebugInfoGenerator DebugInfoGenerator
    {
      get { return _debugInfoGenerator; }
    }

    public IEmittableOperandProvider EmittableOperandProvider
    {
      get
      {
        if (_emittableOperandProvider == null)
        {
          IEmittableOperandProvider provider = new EmittableOperandProvider();
          _emittableOperandProvider = IsStrongNamingEnabled ? new StrongNamingEmittableOperandProviderDecorator (provider) : provider;
        }

        return _emittableOperandProvider;
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

      _assemblyName = assemblyName;
    }

    public string FlushCodeToDisk ()
    {
      if (_currentModuleBuilder == null)
        return null;

      var assemblyPath = _currentModuleBuilder.SaveToDisk();

      // TODO Review: Move these three items to a single class to ensure they are always nulled out together. Module needs to stay lazy. Pass in required information via ctor (e.g., ForceStrongNaming).
      // TODO Review: Add unit test proving that module creation uses the same ForceStrongNaming value that was read when the emittable operand provider was created.
      _currentModuleBuilder = null;
      _emittableOperandProvider = null;
      _assemblyName = null;

      return assemblyPath;
    }

    [CLSCompliant (false)]
    public ITypeBuilder DefineType (string name, TypeAttributes attributes, Type parent)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("parent", parent);

      if (_currentModuleBuilder == null)
      {
        var strongNamed = _configurationProvider.ForceStrongNaming;
        var keyFilePathOrNull = _configurationProvider.KeyFilePath;
        _currentModuleBuilder = _moduleBuilderFactory.CreateModuleBuilder (
            AssemblyName, _assemblyDirectory, strongNamed, keyFilePathOrNull, EmittableOperandProvider);
      }

      return _currentModuleBuilder.DefineType (name, attributes, parent);
    }

    private void EnsureNoCurrentModuleBuilder (string propertyDescription)
    {
      if (_currentModuleBuilder != null)
      {
        var flushMethod = MemberInfoFromExpressionUtility.GetMethod (() => FlushCodeToDisk());
        var message = string.Format (
            "Cannot set {0} after a type has been defined (use {1}() to start a new assembly).", propertyDescription, flushMethod.Name);
        throw new InvalidOperationException (message);
      }
    }
  }
}