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
using System.Runtime.InteropServices;
using Remotion.Collections;
using Remotion.Logging;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Reflection
{
  /// <summary>
  /// Use the <see cref="AssemblyFinder"/> class to find all (referenced) assemblies identified by a marker <see cref="Attribute"/>.
  /// </summary>
  public class AssemblyFinder
  {
    private readonly static ILog s_log = LogManager.GetLogger (typeof (AssemblyFinder));
    
    private readonly bool _considerDynamicDirectory;
    
    private AssemblyLoader _loader;
    private Assembly[] _rootAssemblies;

    private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private readonly string _relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
    private readonly string _dynamicDirectory = AppDomain.CurrentDomain.DynamicDirectory;
    private readonly IRootAssemblyFinder _rootAssemblyFinder;

    /// <summary>
    /// Initializes a new instance of the  <see cref="AssemblyFinder"/> type with a predetermined set of <paramref name="rootAssemblies"/>.
    /// These assemblies are then used as startng points for looking up any referenced assembly matching the given <paramref name="filter"/>
    /// applied.
    /// </summary>
    /// <param name="filter">The <see cref="IAssemblyFinderFilter"/> used to filter the referenced assemblies.</param>
    /// <param name="rootAssemblies">The <see cref="Assembly"/> array used as starting point for finding the referenced assemblies. All of these
    /// assemblies will be included in the result list, no matter whether they match the filter or not.</param>
    public AssemblyFinder (IAssemblyFinderFilter filter, params Assembly[] rootAssemblies)
        : this (filter, false, AppDomain.CurrentDomain.BaseDirectory, null, null)
    {
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("rootAssemblies", rootAssemblies);
      _rootAssemblies = rootAssemblies;
    }

    /// <summary>
    /// Initializes a new instance of the  <see cref="AssemblyFinder"/> type to look for assemblies within the current
    /// <see cref="AppDomain"/>'s <see cref="AppDomain.BaseDirectory"/> matching the <paramref name="filter"/>. These assemblies are then used as 
    /// startng points for looking up any referenced assembly also matching the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">The <see cref="IAssemblyFinderFilter"/> used to filter the referenced assemblies.</param>
    /// <param name="considerDynamicDirectory">Specifies whether to search the <see cref="AppDomain.DynamicDirectory"/> as well as the base
    /// directory.</param>
    public AssemblyFinder (IAssemblyFinderFilter filter, bool considerDynamicDirectory)
        : this (filter, considerDynamicDirectory, AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath, AppDomain.CurrentDomain.DynamicDirectory)
    {
    }

    protected AssemblyFinder (IAssemblyFinderFilter filter, bool considerDynamicDirectory, string baseDirectory, string relativeSearchPath, string dynamicDirectory)
    {
      ArgumentUtility.CheckNotNull ("filter", filter);
      ArgumentUtility.CheckNotNull ("baseDirectory", baseDirectory);

      _loader = new AssemblyLoader (filter);
      _considerDynamicDirectory = considerDynamicDirectory;
      _rootAssemblies = null; // will be retrieved in GetRootAssemblies

      _baseDirectory = baseDirectory;
      _relativeSearchPath = relativeSearchPath;
      _dynamicDirectory = dynamicDirectory;

      _rootAssemblyFinder = new SearchPathRootAssemblyFinder (
          _loader, 
          _baseDirectory, 
          _relativeSearchPath, 
          _considerDynamicDirectory, 
          _dynamicDirectory);
    }

    /// <summary>
    /// Gets the base directory used for loading root assemblies.
    /// </summary>
    /// <value>The base directory.</value>
    public string BaseDirectory
    {
      get { return _baseDirectory; }
    }

    /// <summary>
    /// Gets the semicolon-separated relative search path used for loading root assemblies.
    /// </summary>
    /// <value>The relative search path.</value>
    public string RelativeSearchPath
    {
      get { return _relativeSearchPath; }
    }

    /// <summary>
    /// Gets the dynamic directory used for loading root assemblies.
    /// </summary>
    /// <value>The dynamic directory.</value>
    public string DynamicDirectory
    {
      get { return _dynamicDirectory; }
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="DynamicDirectory"/> is used for loading root assemblies.
    /// </summary>
    /// <value>true if the dynamic directory is used; otherwise, false.</value>
    public bool ConsiderDynamicDirectory
    {
      get { return _considerDynamicDirectory; }
    }

    /// <summary>
    /// Gets the <see cref="IAssemblyFinderFilter"/> passed during initialization.
    /// </summary>
    public IAssemblyFinderFilter Filter
    {
      get { return Loader.Filter; }
    }

    /// <summary>
    /// Gets or sets the <see cref="AssemblyLoader"/> used to load assemblies.
    /// </summary>
    /// <value>The loader used to load assemblies.</value>
    public AssemblyLoader Loader
    {
      get { return _loader; }
      set { _loader = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Returns the root assemblies as well as all directly or indirectly referenced assemblies matching the filter specified
    /// at construction time.
    /// </summary>
    /// <returns>An array of assemblies matching the <see cref="IAssemblyFinderFilter"/> specified at construction time.</returns>
    public virtual Assembly[] FindAssemblies ()
    {
      s_log.Debug ("Finding assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Info, "Time spent for finding and loading assemblies: {0}."))
      {
        Assembly[] rootAssemblies = GetRootAssemblies();
        var resultSet = new HashSet<Assembly> (rootAssemblies);

        resultSet.UnionWith (FindReferencedAssemblies (rootAssemblies));
        return resultSet.ToArray ().LogAndReturn (s_log, LogLevel.Info, result => string.Format ("Found {0} assemblies.", result.Length));
      }
    }

    /// <summary>
    /// Returns the root assemblies as well as all directly or indirectly referenced assemblies matching the filter specified
    /// at construction time.
    /// </summary>
    /// <returns>An array of assemblies matching the <see cref="IAssemblyFinderFilter"/> specified at construction time.</returns>
    /// <remarks>This method exists primarily for testing purposes.</remarks>
    [CLSCompliant (false)]
    public virtual _Assembly[] FindMockableAssemblies ()
    {
      return FindAssemblies ();
    }

    /// <summary>
    /// Gets the array of root assembies identified by the constructor arguments or retrieved from the AppDomain's directory.
    /// </summary>
    public Assembly[] GetRootAssemblies ()
    {
      if (_rootAssemblies == null)
        _rootAssemblies = _rootAssemblyFinder.FindRootAssemblies();
      return _rootAssemblies;
    }

    private IEnumerable<Assembly> FindReferencedAssemblies (Assembly[] rootAssemblies)
    {
      s_log.Debug ("Finding referenced assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Debug, "Time spent for finding and loading referenced assemblies: {0}."))
      {
        var processedAssemblyNames = new Set<string>();
        var referenceRoots = new HashSet<Assembly> (rootAssemblies);

        while (referenceRoots.Count > 0)
        {
          Assembly currentRoot = referenceRoots.First();
          referenceRoots.Remove (currentRoot);

          foreach (AssemblyName referencedAssemblyName in currentRoot.GetReferencedAssemblies())
          {
            if (!processedAssemblyNames.Contains (referencedAssemblyName.FullName))
            {
              processedAssemblyNames.Add (referencedAssemblyName.FullName);

              Assembly referencedAssembly = Loader.TryLoadAssembly (referencedAssemblyName, currentRoot.FullName);
              if (referencedAssembly != null)
              {
                referenceRoots.Add (referencedAssembly);
                yield return referencedAssembly;
              }
            }
          }
        }
      }
    }
  }
}
