/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Use the <see cref="AssemblyFinder"/> class to find all (referenced) assemblies identified by a marker <see cref="Attribute"/>.
  /// </summary>
  public class AssemblyFinder
  {
    private readonly Assembly[] _rootAssemblies;
    private readonly IAssemblyFinderFilter _filter;

    /// <summary>
    /// Initializes a new instance of the  <see cref="AssemblyFinder"/> type with a predetermined set of <paramref name="rootAssemblies"/>.
    /// These assemblies are then used as startng points for looking up any referenced assembly matching the given <paramref name="filter"/>
    /// applied.
    /// </summary>
    /// <param name="filter">The <see cref="IAssemblyFinderFilter"/> used to filter the referenced assemblies.</param>
    /// <param name="rootAssemblies">The <see cref="Assembly"/> array used as starting point for finding the referenced assemblies. All of these
    /// assemblies will be included in the result list, no matter whether they match the filter or not.</param>
    public AssemblyFinder (IAssemblyFinderFilter filter, params Assembly[] rootAssemblies)
    {
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("rootAssemblies", rootAssemblies);
      ArgumentUtility.CheckNotNull ("filter", filter);

      _rootAssemblies = rootAssemblies;
      _filter = filter;
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
    {
      ArgumentUtility.CheckNotNull ("filter", filter);

      _filter = filter;

      List<Assembly> assemblies = new List<Assembly> ();
      LoadAssemblies (assemblies, AppDomain.CurrentDomain.BaseDirectory);

      if (!string.IsNullOrEmpty (AppDomain.CurrentDomain.RelativeSearchPath))
      {
        foreach (string privateBinPath in AppDomain.CurrentDomain.RelativeSearchPath.Split (';'))
          LoadAssemblies(assemblies, privateBinPath);
      }

      if (considerDynamicDirectory && !string.IsNullOrEmpty (AppDomain.CurrentDomain.DynamicDirectory))
        LoadAssemblies (assemblies, AppDomain.CurrentDomain.DynamicDirectory);

      _rootAssemblies = assemblies.FindAll (_filter.ShouldIncludeAssembly).ToArray();
    }

    /// <summary>
    /// Gets the <see cref="IAssemblyFinderFilter"/> passed during initialization.
    /// </summary>
    public IAssemblyFinderFilter Filter
    {
      get { return _filter; }
    }

    /// <summary>
    /// Gets the array of assembies identified by the constructor arguments.
    /// </summary>
    public Assembly[] RootAssemblies
    {
      get { return _rootAssemblies; }
    }
    
    /// <summary>
    /// Returns the <see cref="RootAssemblies"/> as well as all directly or indirectly referenced assemblies matching the filter specified
    /// at construction time.
    /// </summary>
    /// <returns>An array of assemblies matching the <see cref="IAssemblyFinderFilter"/> specified at construction time.</returns>
    public virtual Assembly[] FindAssemblies ()
    {
      List<Assembly> assemblies = new List<Assembly> (_rootAssemblies);
      for (int i = 0; i < assemblies.Count; i++)
      {
        foreach (AssemblyName referencedAssemblyName in assemblies[i].GetReferencedAssemblies())
        {
          if (_filter.ShouldConsiderAssembly (referencedAssemblyName))
          {
            try
            {
              Assembly referencedAssembly = Assembly.Load (referencedAssemblyName);
              if (!assemblies.Contains (referencedAssembly) && _filter.ShouldIncludeAssembly (referencedAssembly))
                assemblies.Add (referencedAssembly);
            }
            catch (FileLoadException ex)
            {
              string message = string.Format ("There was a problem when loading referenced assemblies of assembly '{0}': {1}", assemblies[i].FullName,
                  ex.Message);
              throw new FileLoadException (message, ex);
            }
          }
        }
      }

      return assemblies.ToArray();
    }

    private void LoadAssemblies (List<Assembly> assemblies, string searchPath)
    {
      LoadAssemblies (assemblies, Directory.GetFiles (searchPath, "*.dll", SearchOption.TopDirectoryOnly));
      LoadAssemblies (assemblies, Directory.GetFiles (searchPath, "*.exe", SearchOption.TopDirectoryOnly));
    }

    private void LoadAssemblies (List<Assembly> assemblies, string[] filePaths)
    {
      foreach (string filePath in filePaths)
      {
        try
        {
          Assembly assembly = TryLoadAssembly (filePath);
          if (assembly != null && !assemblies.Contains (assembly))
            assemblies.Add (assembly);
        }
        catch (FileNotFoundException ex)
        {
          string message = string.Format ("{0}: {1}", filePath, ex.Message);
          throw new FileLoadException (message, ex);
        }
      }
    }

    private Assembly TryLoadAssembly (string filePath)
    {
      AssemblyName assemblyName;
      try
      {
        assemblyName = AssemblyName.GetAssemblyName (filePath);
      }
      catch (BadImageFormatException)
      {
        return null;
      }

      if (_filter.ShouldConsiderAssembly (assemblyName))
        return Assembly.Load (assemblyName);
      else
        return null;
    }
  }
}
