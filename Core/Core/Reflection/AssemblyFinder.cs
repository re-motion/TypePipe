// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Runtime.InteropServices;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Use the <see cref="AssemblyFinder"/> class to find all (referenced) assemblies identified by a marker <see cref="Attribute"/>.
  /// </summary>
  public class AssemblyFinder
  {
    private readonly bool _considerDynamicDirectory;
    
    private AssemblyLoader _loader;
    private Assembly[] _rootAssemblies;

    private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private readonly string _relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
    private readonly string _dynamicDirectory = AppDomain.CurrentDomain.DynamicDirectory;

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
      set { _loader = value; }
    }

    /// <summary>
    /// Returns the root assemblies as well as all directly or indirectly referenced assemblies matching the filter specified
    /// at construction time.
    /// </summary>
    /// <returns>An array of assemblies matching the <see cref="IAssemblyFinderFilter"/> specified at construction time.</returns>
    public virtual Assembly[] FindAssemblies ()
    {
      Assembly[] rootAssemblies = GetRootAssemblies ();
      Set<Assembly> resultSet = new Set<Assembly> (rootAssemblies);

      resultSet.AddRange (FindReferencedAssemblies (rootAssemblies));
      return resultSet.ToArray ();
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
        _rootAssemblies = FindRootAssemblies();
      return _rootAssemblies;
    }

    private Assembly[] FindRootAssemblies ()
    {
      Set<Assembly> rootAssemblies = new Set<Assembly> ();
      rootAssemblies.AddRange (FindAssembliesInPath (_baseDirectory));

      if (!string.IsNullOrEmpty (_relativeSearchPath))
      {
        foreach (string privateBinPath in _relativeSearchPath.Split (';'))
          rootAssemblies.AddRange (FindAssembliesInPath (privateBinPath));
      }

      if (_considerDynamicDirectory && !string.IsNullOrEmpty (_dynamicDirectory))
        rootAssemblies.AddRange (FindAssembliesInPath (_dynamicDirectory));

      return rootAssemblies.ToArray ();
    }

    private IEnumerable<Assembly> FindReferencedAssemblies (Assembly[] rootAssemblies)
    {
      Set<string> processedAssemblyNames = new Set<string> ();
      Set<Assembly> referenceRoots = new Set<Assembly> (rootAssemblies);

      while (referenceRoots.Count > 0)
      {
        Assembly currentRoot = referenceRoots.GetAny ();
        referenceRoots.Remove (currentRoot);

        foreach (AssemblyName referencedAssemblyName in currentRoot.GetReferencedAssemblies ())
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

    protected virtual IEnumerable<Assembly> FindAssembliesInPath (string searchPath)
    {
      return EnumerableUtility.Combine (
        Loader.LoadAssemblies (Directory.GetFiles (searchPath, "*.dll", SearchOption.TopDirectoryOnly)),
        Loader.LoadAssemblies (Directory.GetFiles (searchPath, "*.exe", SearchOption.TopDirectoryOnly)));
    }    
  }
}
