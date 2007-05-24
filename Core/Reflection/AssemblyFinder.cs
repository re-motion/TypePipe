using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  /// <summary>
  /// Use the <see cref="AssemblyFinder"/> class to find all (referenced) assemblies identified by a marker <see cref="Attribute"/>.
  /// </summary>
  public class AssemblyFinder
  {
    private readonly Assembly[] _rootAssemblies;
    private Type _assemblyMarkerAttribute;

    /// <summary>
    /// Initializes a new instance of the  <see cref="AssemblyFinder"/> type with a predetermined set of <paramref name="rootAssemblies"/>.
    /// These assemblies are then used as startng points for looking up any referenced assembly having the <paramref name="assemblyMarkerAttribute"/>
    /// applied.
    /// </summary>
    /// <param name="assemblyMarkerAttribute">The <see cref="Attribute"/> to used to filter the referenced assemblies.</param>
    /// <param name="rootAssemblies">The <see cref="Assembly"/> array used as starting point for finding the referenced assemblies.</param>
    public AssemblyFinder (Type assemblyMarkerAttribute, params Assembly[] rootAssemblies)
    {
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("rootAssemblies", rootAssemblies);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("assemblyMarkerAttribute", assemblyMarkerAttribute, typeof (Attribute));

      _assemblyMarkerAttribute = assemblyMarkerAttribute;
      _rootAssemblies = rootAssemblies;
    }

    /// <summary>
    /// Initializes a new instance of the  <see cref="AssemblyFinder"/> type to look for assemblies within the current <see cref="AppDomain"/>'s 
    /// <see cref="AppDomain.BaseDirectory"/> having the <paramref name="assemblyMarkerAttribute"/> applied. These assemblies are then used as 
    /// startng points for looking up any referenced assembly having the <paramref name="assemblyMarkerAttribute"/> applied as well.
    /// </summary>
    /// <param name="assemblyMarkerAttribute">The <see cref="Attribute"/> to used to filter the assemblies.</param>
    public AssemblyFinder (Type assemblyMarkerAttribute)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("assemblyMarkerAttribute", assemblyMarkerAttribute, typeof (Attribute));

      _assemblyMarkerAttribute = assemblyMarkerAttribute;

      List<Assembly> assemblies = new List<Assembly> (AppDomain.CurrentDomain.GetAssemblies ());

      LoadAssemblies(assemblies, AppDomain.CurrentDomain.BaseDirectory);

      if (!string.IsNullOrEmpty (AppDomain.CurrentDomain.RelativeSearchPath))
      {
        foreach (string privateBinPath in AppDomain.CurrentDomain.RelativeSearchPath.Split (';'))
          LoadAssemblies(assemblies, privateBinPath);
      }
      
      if (!string.IsNullOrEmpty (AppDomain.CurrentDomain.DynamicDirectory))
        LoadAssemblies (assemblies, AppDomain.CurrentDomain.DynamicDirectory);

      _rootAssemblies = assemblies.FindAll (HasAssemblyMarkerAttributeDefined).ToArray();
    }

    /// <summary>
    /// Gets the attribute <see cref="Type"/> passed during initialization.
    /// </summary>
    public Type AssemblyMarkerAttribute
    {
      get { return _assemblyMarkerAttribute; }
    }

    /// <summary>
    /// Gets the array of assembies identified by the constructor arguments.
    /// </summary>
    public Assembly[] RootAssemblies
    {
      get { return _rootAssemblies; }
    }
    
    /// <summary>
    /// Returns the <see cref="RootAssemblies"/> as well as all referenced assemblies having the <see cref="AssemblyMarkerAttribute"/> defined.
    /// </summary>
    /// <returns>An array of assemblies.</returns>
    public Assembly[] FindAssemblies ()
    {
      List<Assembly> assemblies = new List<Assembly> (_rootAssemblies);
      for (int i = 0; i < assemblies.Count; i++)
      {
        foreach (AssemblyName referencedAssemblyName in assemblies[i].GetReferencedAssemblies())
        {
          Assembly referencedAssembly = Assembly.Load (referencedAssemblyName);
          if (!assemblies.Contains (referencedAssembly) && HasAssemblyMarkerAttributeDefined (referencedAssembly))
            assemblies.Add (referencedAssembly);
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
        Assembly assembly = TryLoadAssembly (filePath);
        if (assembly != null && !assemblies.Contains (assembly))
          assemblies.Add (assembly);
      }
    }

    private Assembly TryLoadAssembly (string filePath)
    {
      try
      {
        return Assembly.Load (Path.GetFileNameWithoutExtension (filePath));
      }
      catch (BadImageFormatException)
      {
        return null;
      }
    }

    private bool HasAssemblyMarkerAttributeDefined (Assembly assembly)
    {
      return assembly.IsDefined (_assemblyMarkerAttribute, false);
    }
  }
}