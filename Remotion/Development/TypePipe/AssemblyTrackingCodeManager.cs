// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
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