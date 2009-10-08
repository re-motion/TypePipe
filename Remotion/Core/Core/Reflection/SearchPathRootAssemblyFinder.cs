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