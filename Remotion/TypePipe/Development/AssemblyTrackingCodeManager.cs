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
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.Development.TypePipe
{
  /// <summary>
  /// Decorates a <see cref="ICodeManager"/> to track the generated assemblies.
  /// Enables saving, verification and cleanup of generated assemblies, which is useful for testing.
  /// <para>
  /// To use assembly tracking register <see cref="AssemblyTrackingPipelineFactory"/> for <see cref="IPipelineFactory"/> in your IoC container.
  /// </para>
  /// </summary>
  public class AssemblyTrackingCodeManager : ICodeManager
  {
    private readonly object _lockObject = new object();
    private readonly List<string> _savedAssemblies = new List<string>();
    private readonly ICodeManager _codeManager;

    public AssemblyTrackingCodeManager (ICodeManager codeManager)
    {
      ArgumentUtility.CheckNotNull ("codeManager", codeManager);

      _codeManager = codeManager;
    }

    public ReadOnlyCollection<string> SavedAssemblies
    {
      get
      {
        lock (_lockObject)
        {
          return _savedAssemblies.AsReadOnly();
        }
      }
    }

    public void AddSavedAssembly (string assemblyPath)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyPath", assemblyPath);
      lock (_lockObject)
      {
        _savedAssemblies.Add (assemblyPath);
      }
    }

    public void PeVerifySavedAssemblies ()
    {
      lock (_lockObject)
      {
        foreach (var assemblyPath in _savedAssemblies)
          PEVerifier.CreateDefault().VerifyPEFile (assemblyPath);
      }
    }

    public void DeleteSavedAssemblies ()
    {
      lock (_lockObject)
      {
        foreach (var assemblyPath in _savedAssemblies)
        {
          FileUtility.DeleteAndWaitForCompletion (assemblyPath);
          FileUtility.DeleteAndWaitForCompletion (Path.ChangeExtension (assemblyPath, "pdb"));
        }

        _savedAssemblies.Clear();
      }
    }

    public string[] FlushCodeToDisk (params CustomAttributeDeclaration[] assemblyAttributes)
    {
      ArgumentUtility.CheckNotNull ("assemblyAttributes", assemblyAttributes);

      lock (_lockObject)
      {
        var assemblyPaths = _codeManager.FlushCodeToDisk (assemblyAttributes);

        _savedAssemblies.AddRange (assemblyPaths);

        return assemblyPaths;
      }
    }

    public void LoadFlushedCode (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);

      lock (_lockObject)
      {
        _codeManager.LoadFlushedCode (assembly);
      }
    }
  }
}