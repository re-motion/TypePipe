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
using JetBrains.Annotations;
using Remotion.TypePipe.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Configuration
{
  /// <summary>
  /// Holds configuration options for pipelines. This class is immutable.
  /// </summary>
  /// <seealso cref="PipelineFactory.Create(string,Remotion.TypePipe.Configuration.PipelineSettings,Remotion.TypePipe.IParticipant[])"/>
  /// <seealso cref="IPipeline.Settings"/>
  public class PipelineSettings
  {
    //TODO 5840: Test
    private static readonly PipelineSettings s_defaults = New().Build();

    public static PipelineSettings Defaults
    {
      get { return s_defaults; }
    }

    public static Builder New ()
    {
      return new Builder();
    }

    public static Builder From (PipelineSettings settings)
    {
      ArgumentUtility.CheckNotNull ("settings", settings);

      return New()
          .SetForceStrongNaming (settings.ForceStrongNaming)
          .SetKeyFilePath (settings.KeyFilePath)
          .SetEnableSerializationWithoutAssemblySaving (settings.EnableSerializationWithoutAssemblySaving)
          .SetAssemblyDirectory (settings.AssemblyDirectory)
          .SetAssemblyNamePattern (settings.AssemblyNamePattern)
          .SetParallelAssemblyCount (settings.ParallelAssemblyCount);
    }

    private readonly bool _forceStrongNaming;

    [CanBeNull]
    private readonly string _keyFilePath;

    private readonly bool _enableSerializationWithoutAssemblySaving;

    [CanBeNull]
    private readonly string _assemblyDirectory;

    private readonly string _assemblyNamePattern;

    private readonly int _parallelAssemblyCount;

    public PipelineSettings (
        bool forceStrongNaming,
        [CanBeNull] string keyFilePath,
        bool enableSerializationWithoutAssemblySaving,
        [CanBeNull] string assemblyDirectory,
        string assemblyNamePattern,
        int parallelAssemblyCount)
    {
      ArgumentUtility.CheckNotNull ("assemblyNamePattern", assemblyNamePattern);

      _forceStrongNaming = forceStrongNaming;
      _keyFilePath = keyFilePath;
      _enableSerializationWithoutAssemblySaving = enableSerializationWithoutAssemblySaving;
      _assemblyDirectory = assemblyDirectory;
      _assemblyNamePattern = assemblyNamePattern;
      _parallelAssemblyCount = parallelAssemblyCount;
    }

    /// <summary>
    /// Gets the directory in which assemblies will be saved when <see cref="ICodeManager.FlushCodeToDisk"/> is invoked.
    /// <see langword="null"/> means the current working directory.
    /// </summary>
    /// <value>The assembly directory path or <see langword="null"/>.</value>
    [CanBeNull]
    public string AssemblyDirectory
    {
      get { return _assemblyDirectory; }
    }

    /// <summary>
    /// Gets the assembly name pattern, that is, a pattern used to determine the assembly name when <see cref="ICodeManager.FlushCodeToDisk"/> is
    /// invoked. To ensure unique assembly file names, use the placeholder <c>{counter}</c>, which will be replaced with a unique number.
    /// If the name pattern does not contain the placeholder, calls to <see cref="ICodeManager.FlushCodeToDisk"/> will overwrite previously saved assemblies.
    /// </summary>
    /// <value>The assembly name pattern; the default is <c>TypePipe_GeneratedAssembly_{counter}</c>.</value>
    [NotNull]
    public string AssemblyNamePattern
    {
      get { return _assemblyNamePattern; }
    }

    /// <summary>
    /// If <see langword="true"/>, the pipeline signs all generated assemblies or throws an <see cref="InvalidOperationException"/> if that is not
    /// possible.
    /// </summary>
    public bool ForceStrongNaming
    {
      get { return _forceStrongNaming; }
    }

    /// <summary>
    /// When <see cref="ForceStrongNaming"/> is enabled, the key file (<c>*.snk</c>) denoted by this property is used to sign generated assemblies.
    /// If this property is <see langword="null"/> a pipeline-provided default key file is used instead.
    /// </summary>
    [CanBeNull]
    public string KeyFilePath
    {
      get { return _keyFilePath; }
    }

    /// <summary>
    /// Enables the serialization of assembled type instances without the need of saving the generated assembly to disk.
    /// </summary>
    public bool EnableSerializationWithoutAssemblySaving
    {
      get { return _enableSerializationWithoutAssemblySaving; }
    }

    public int ParallelAssemblyCount
    {
      get { return _parallelAssemblyCount; }
    }

    public class Builder
    {
      private const string c_defaultAssemblyNamePattern = "TypePipe_GeneratedAssembly_{counter}";

      private bool _forceStrongNaming;

      [CanBeNull]
      private string _keyFilePath;

      private bool _enableSerializationWithoutAssemblySaving;

      [CanBeNull]
      private string _assemblyDirectory;

      [CanBeNull]
      private string _assemblyNamePattern;

      private int _parallelAssemblyCount;

      public Builder SetForceStrongNaming (bool value)
      {
        _forceStrongNaming = value;
        return this;
      }

      public Builder SetKeyFilePath ([CanBeNull] string value)
      {
        _keyFilePath = value;
        return this;
      }

      public Builder SetEnableSerializationWithoutAssemblySaving (bool enableSerializationWithoutAssemblySaving)
      {
        _enableSerializationWithoutAssemblySaving = enableSerializationWithoutAssemblySaving;
        return this;
      }

      public Builder SetAssemblyDirectory ([CanBeNull]string value)
      {
        _assemblyDirectory = value;
        return this;
      }
      
      public Builder SetAssemblyNamePattern ([CanBeNull]string value)
      {
        _assemblyNamePattern = value;
        return this;
      }

      public Builder SetParallelAssemblyCount (int value)
      {
        _parallelAssemblyCount = value;
        return this;
      }

      public PipelineSettings Build ()
      {
        return new PipelineSettings (
            _forceStrongNaming,
            _keyFilePath,
            _enableSerializationWithoutAssemblySaving,
            _assemblyDirectory,
            _assemblyNamePattern ?? c_defaultAssemblyNamePattern,
            _parallelAssemblyCount);
      }
    }
  }
}