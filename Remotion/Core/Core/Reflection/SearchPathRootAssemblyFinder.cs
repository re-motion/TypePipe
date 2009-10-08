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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Finds the root assemblies by looking up and loading all DLL and EXE files in the assembly search path.
  /// </summary>
  public class SearchPathRootAssemblyFinder : IRootAssemblyFinder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchPathRootAssemblyFinder"/> type to look for assemblies within the current
    /// <see cref="AppDomain"/>'s <see cref="AppDomain.BaseDirectory"/>.
    /// </summary>
    /// <param name="considerDynamicDirectory">Specifies whether to search the <see cref="AppDomain.DynamicDirectory"/> as well as the base
    /// directory.</param>
    public static SearchPathRootAssemblyFinder CreateForCurrentAppDomain (bool considerDynamicDirectory)
    {
      var searchPathRootAssemblyFinder = new SearchPathRootAssemblyFinder (
          AppDomain.CurrentDomain.BaseDirectory,
          AppDomain.CurrentDomain.RelativeSearchPath,
          considerDynamicDirectory,
          AppDomain.CurrentDomain.DynamicDirectory);
      return searchPathRootAssemblyFinder;
    }

    private readonly string _baseDirectory;
    private readonly string _relativeSearchPath;
    private readonly bool _considerDynamicDirectory;
    private readonly string _dynamicDirectory;

    public SearchPathRootAssemblyFinder (
        string baseDirectory, 
        string relativeSearchPath, 
        bool considerDynamicDirectory, 
        string dynamicDirectory)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("baseDirectory", baseDirectory);

      _baseDirectory = baseDirectory;
      _relativeSearchPath = relativeSearchPath;
      _considerDynamicDirectory = considerDynamicDirectory;
      _dynamicDirectory = dynamicDirectory;
    }

    public string BaseDirectory
    {
      get { return _baseDirectory; }
    }

    public string RelativeSearchPath
    {
      get { return _relativeSearchPath; }
    }

    public bool ConsiderDynamicDirectory
    {
      get { return _considerDynamicDirectory; }
    }

    public string DynamicDirectory
    {
      get { return _dynamicDirectory; }
    }

    public Assembly[] FindRootAssemblies (IAssemblyLoader loader)
    {
      ArgumentUtility.CheckNotNull ("loader", loader);
      var combinedFinder = CreateCombinedFinder ();
      return combinedFinder.FindRootAssemblies (loader);
    }

    public virtual CompositeRootAssemblyFinder CreateCombinedFinder ()
    {
      var finders = new List<IRootAssemblyFinder> { new DirectoryRootAssemblyFinder (_baseDirectory) };

      if (!string.IsNullOrEmpty (_relativeSearchPath))
      {
        foreach (string privateBinPath in _relativeSearchPath.Split (';'))
          finders.Add (new DirectoryRootAssemblyFinder (privateBinPath));
      }

      if (_considerDynamicDirectory && !string.IsNullOrEmpty (_dynamicDirectory))
        finders.Add (new DirectoryRootAssemblyFinder (_dynamicDirectory));

      return new CompositeRootAssemblyFinder (finders);
    }
  }
}