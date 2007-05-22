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

      List<Assembly> assemblies = new List<Assembly> (AppDomain.CurrentDomain.GetAssemblies());
      LoadAssemblies (assemblies, Directory.GetFiles (AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly));
      LoadAssemblies (assemblies, Directory.GetFiles (AppDomain.CurrentDomain.BaseDirectory, "*.exe", SearchOption.TopDirectoryOnly));

      _rootAssemblies = assemblies.FindAll (HasAssemblyMarkerAttributeDefined).ToArray();
    }

    /// <summary>
    /// Gets the array of assembies identified by the constructor arguments.
    /// </summary>
    public Assembly[] RootAssemblies
    {
      get { return _rootAssemblies; }
    }

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

    private void LoadAssemblies (List<Assembly> assemblies, string[] paths)
    {
      foreach (string path in paths)
      {
        Assembly assembly = TryLoadAssembly (path);
        if (assembly != null && !assemblies.Contains (assembly))
          assemblies.Add (assembly);
      }
    }

    private Assembly TryLoadAssembly (string path)
    {
      try
      {
        return Assembly.Load (Path.GetFileNameWithoutExtension (path));
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