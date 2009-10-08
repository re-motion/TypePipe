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
using System.Linq;

namespace Remotion.Reflection
{
  /// <summary>
  /// Finds assemblies using an <see cref="IRootAssemblyFinder"/> and an <see cref="IAssemblyLoader"/>. The <see cref="IRootAssemblyFinder"/> is
  /// used to find a set of root assemblies, the <see cref="AssemblyFinder"/> automatically traverses the assembly references to (transitively)
  /// find all referenced assemblies as well. The root assemblies and referenced assemblies are loaded with the <see cref="IAssemblyLoader"/>.
  /// </summary>
  public class AssemblyFinder : IAssemblyFinder
  {
    private readonly static ILog s_log = LogManager.GetLogger (typeof (AssemblyFinder));

    private readonly IRootAssemblyFinder _rootAssemblyFinder;
    private readonly IAssemblyLoader _assemblyLoader;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyFinder"/> class.
    /// </summary>
    /// <param name="rootAssemblyFinder">The <see cref="IRootAssemblyFinder"/> to use for finding the root assemblies.</param>
    /// <param name="assemblyLoader">The <see cref="IAssemblyLoader"/> to use for loading the assemblies found.</param>
    public AssemblyFinder (IRootAssemblyFinder rootAssemblyFinder, IAssemblyLoader assemblyLoader)
    {
      ArgumentUtility.CheckNotNull ("rootAssemblyFinder", rootAssemblyFinder);
      ArgumentUtility.CheckNotNull ("assemblyLoader", assemblyLoader);

      _rootAssemblyFinder = rootAssemblyFinder;
      _assemblyLoader = assemblyLoader;
    }

    public IRootAssemblyFinder RootAssemblyFinder
    {
      get { return _rootAssemblyFinder; }
    }

    public IAssemblyLoader AssemblyLoader
    {
      get { return _assemblyLoader; }
    }

    /// <summary>
    /// Uses the <see cref="RootAssemblyFinder"/> to find root assemblies and returns them together with all directly or indirectly referenced 
    /// assemblies. The assemblies are loaded via the <see cref="AssemblyLoader"/>.
    /// </summary>
    /// <returns>The root assemblies and their referenced assemblies.</returns>
    public virtual Assembly[] FindAssemblies ()
    {
      s_log.Debug ("Finding assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Info, "Time spent for finding and loading assemblies: {0}."))
      {
        Assembly[] rootAssemblies = FindRootAssemblies();
        var resultSet = new HashSet<Assembly> (rootAssemblies);

        resultSet.UnionWith (FindReferencedAssemblies (rootAssemblies));
        return resultSet.ToArray ().LogAndReturn (s_log, LogLevel.Info, result => string.Format ("Found {0} assemblies.", result.Length));
      }
    }

    private Assembly[] FindRootAssemblies ()
    {
      s_log.Debug ("Finding root assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Debug, "Time spent for finding and loading root assemblies: {0}."))
      {
        return _rootAssemblyFinder.FindRootAssemblies (_assemblyLoader)
            .LogAndReturn (s_log, LogLevel.Debug, result => string.Format ("Found {0} root assemblies.", result.Length));
      }
    }

    private IEnumerable<Assembly> FindReferencedAssemblies (Assembly[] rootAssemblies)
    {
      s_log.Debug ("Finding referenced assemblies...");
      using (StopwatchScope.CreateScope (s_log, LogLevel.Debug, "Time spent for finding and loading referenced assemblies: {0}."))
      {
        var processedAssemblyNames = new HashSet<string>(); // used to avoid loading assemblies twice
        var referenceRoots = new HashSet<Assembly> (rootAssemblies); // referenced assemblies added later in order to get their references as well

        while (referenceRoots.Count > 0)
        {
          Assembly currentRoot = referenceRoots.First(); // take any reference
          referenceRoots.Remove (currentRoot); // don't handle again

          foreach (AssemblyName referencedAssemblyName in currentRoot.GetReferencedAssemblies())
          {
            if (!processedAssemblyNames.Contains (referencedAssemblyName.FullName)) // don't process an assembly name twice
            {
              processedAssemblyNames.Add (referencedAssemblyName.FullName);

              Assembly referencedAssembly = _assemblyLoader.TryLoadAssembly (referencedAssemblyName, currentRoot.FullName);
              if (referencedAssembly != null) // might return null if filtered by the loader
              {
                referenceRoots.Add (referencedAssembly); // store as a root in order to process references transitively
                yield return referencedAssembly;
              }
            }
          }
        }
      }
    }
  }
}
