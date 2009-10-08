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
using Remotion.Logging;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Reflection
{
  /// <summary>
  /// Use the <see cref="AssemblyFinder"/> class to find all (referenced) assemblies identified by a marker <see cref="Attribute"/>.
  /// </summary>
  public class AssemblyFinder : IAssemblyFinder
  {
    private readonly static ILog s_log = LogManager.GetLogger (typeof (AssemblyFinder));

    private readonly IRootAssemblyFinder _rootAssemblyFinder;
    private readonly IAssemblyLoader _referencedAssemblyLoader;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyFinder"/> class.
    /// </summary>
    /// <param name="rootAssemblyFinder">The <see cref="IRootAssemblyFinder"/> to use for finding the root assemblies.</param>
    /// <param name="referencedAssemblyLoader">The <see cref="IAssemblyLoader"/> to use for loading referenced assemblies.</param>
    public AssemblyFinder (IRootAssemblyFinder rootAssemblyFinder, IAssemblyLoader referencedAssemblyLoader)
    {
      ArgumentUtility.CheckNotNull ("rootAssemblyFinder", rootAssemblyFinder);
      ArgumentUtility.CheckNotNull ("referencedAssemblyLoader", referencedAssemblyLoader);

      _rootAssemblyFinder = rootAssemblyFinder;
      _referencedAssemblyLoader = referencedAssemblyLoader;
    }

    public IRootAssemblyFinder RootAssemblyFinder
    {
      get { return _rootAssemblyFinder; }
    }

    public IAssemblyLoader ReferencedAssemblyLoader
    {
      get { return _referencedAssemblyLoader; }
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
    /// Returns the root assemblies as well as all directly or indirectly referenced assemblies matching the filter specified
    /// at construction time.
    /// </summary>
    /// <returns>An array of assemblies matching the <see cref="IAssemblyFinderFilter"/> specified at construction time.</returns>
    public virtual Assembly[] FindAssemblies ()
    {
      s_log.Debug ("Finding assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Info, "Time spent for finding and loading assemblies: {0}."))
      {
        var rootAssemblies = _rootAssemblyFinder.FindRootAssemblies();
        var resultSet = new HashSet<Assembly> (rootAssemblies);

        resultSet.UnionWith (FindReferencedAssemblies (rootAssemblies));
        return resultSet.ToArray ().LogAndReturn (s_log, LogLevel.Info, result => string.Format ("Found {0} assemblies.", result.Length));
      }
    }

    private IEnumerable<Assembly> FindReferencedAssemblies (Assembly[] rootAssemblies)
    {
      s_log.Debug ("Finding referenced assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Debug, "Time spent for finding and loading referenced assemblies: {0}."))
      {
        var processedAssemblyNames = new HashSet<string>();
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

              Assembly referencedAssembly = _referencedAssemblyLoader.TryLoadAssembly (referencedAssemblyName, currentRoot.FullName);
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
