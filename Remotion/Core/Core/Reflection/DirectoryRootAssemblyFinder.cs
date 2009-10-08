// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.IO;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Finds the root assemblies by looking up and loading all DLL and EXE files in a specified directory.
  /// </summary>
  public class DirectoryRootAssemblyFinder : IRootAssemblyFinder
  {
    private readonly string _searchPath;

    public DirectoryRootAssemblyFinder (string searchPath)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("searchPath", searchPath);

      _searchPath = searchPath;
    }

    public string SearchPath
    {
      get { return _searchPath; }
    }

    public Assembly[] FindRootAssemblies (IAssemblyLoader loader)
    {
      var dllFiles = Directory.GetFiles (_searchPath, "*.dll", SearchOption.TopDirectoryOnly);
      var exeFiles = Directory.GetFiles (_searchPath, "*.exe", SearchOption.TopDirectoryOnly);

      var allAssemblies = new List<Assembly> ();
      if (dllFiles.Length > 0)
        allAssemblies.AddRange (loader.LoadAssemblies (dllFiles));
      if (exeFiles.Length > 0)
        allAssemblies.AddRange (loader.LoadAssemblies (exeFiles));
      return allAssemblies.ToArray ();
    }
  }
}