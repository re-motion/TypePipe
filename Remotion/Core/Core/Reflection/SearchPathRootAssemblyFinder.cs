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
using Remotion.Logging;
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
    /// <param name="loader">The <see cref="IAssemblyLoader"/> to use to load the root assemblies.</param>
    /// <param name="considerDynamicDirectory">Specifies whether to search the <see cref="AppDomain.DynamicDirectory"/> as well as the base
    /// directory.</param>
    public static SearchPathRootAssemblyFinder CreateForCurrentAppDomain (IAssemblyLoader loader, bool considerDynamicDirectory)
    {
      ArgumentUtility.CheckNotNull ("loader", loader);

      var searchPathRootAssemblyFinder = new SearchPathRootAssemblyFinder (
          loader,
          AppDomain.CurrentDomain.BaseDirectory,
          AppDomain.CurrentDomain.RelativeSearchPath,
          considerDynamicDirectory,
          AppDomain.CurrentDomain.DynamicDirectory);
      return searchPathRootAssemblyFinder;
    }

    private readonly IAssemblyLoader _loader;
    private readonly string _baseDirectory;
    private readonly string _relativeSearchPath;
    private readonly bool _considerDynamicDirectory;
    private readonly string _dynamicDirectory;
    private static readonly ILog s_log = LogManager.GetLogger (typeof (SearchPathRootAssemblyFinder));

    public SearchPathRootAssemblyFinder (
        IAssemblyLoader loader, 
        string baseDirectory, 
        string relativeSearchPath, 
        bool considerDynamicDirectory, 
        string dynamicDirectory)
    {
      ArgumentUtility.CheckNotNull ("loader", loader);
      ArgumentUtility.CheckNotNullOrEmpty ("baseDirectory", baseDirectory);

      _loader = loader;
      _baseDirectory = baseDirectory;
      _relativeSearchPath = relativeSearchPath;
      _considerDynamicDirectory = considerDynamicDirectory;
      _dynamicDirectory = dynamicDirectory;
    }

    public IAssemblyLoader Loader
    {
      get { return _loader; }
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

    public Assembly[] FindRootAssemblies ()
    {
      s_log.Debug ("Finding root assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Debug, "Time spent for finding and loading root assemblies: {0}."))
      {
        var combinedFinder = CreateCombinedFinder ();
        return combinedFinder.FindRootAssemblies()
            .LogAndReturn (s_log, LogLevel.Debug, result => string.Format ("Found {0} root assemblies.", result.Length));
      }
    }

    public virtual CompositeRootAssemblyFinder CreateCombinedFinder ()
    {
      var finders = new List<IRootAssemblyFinder> { new DirectoryRootAssemblyFinder (_loader, _baseDirectory) };

      if (!string.IsNullOrEmpty (_relativeSearchPath))
      {
        foreach (string privateBinPath in _relativeSearchPath.Split (';'))
          finders.Add (new DirectoryRootAssemblyFinder (_loader, privateBinPath));
      }

      if (_considerDynamicDirectory && !string.IsNullOrEmpty (_dynamicDirectory))
      {
        finders.Add (new DirectoryRootAssemblyFinder (_loader, _dynamicDirectory));
      }

      return new CompositeRootAssemblyFinder (finders);
    }
  }
}