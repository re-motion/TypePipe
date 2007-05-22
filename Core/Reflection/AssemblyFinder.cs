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

      List<Assembly> assemblies = new List<Assembly> (AppDomain.CurrentDomain.GetAssemblies());
      LoadAssemblies (assemblies, Directory.GetFiles (AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly));
      LoadAssemblies (assemblies, Directory.GetFiles (AppDomain.CurrentDomain.BaseDirectory, "*.exe", SearchOption.TopDirectoryOnly));

      _rootAssemblies = assemblies.FindAll (delegate (Assembly assembly) { return assembly.IsDefined (assemblyMarkerAttribute, false); }).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the  <see cref="AssemblyFinder"/> type to look for assemblies within the specified 
    /// <paramref name="baseDirectory"/> having the <paramref name="assemblyMarkerAttribute"/> applied. These assemblies are then used as 
    /// startng points for looking up any referenced assembly having the <paramref name="assemblyMarkerAttribute"/> applied as well.
    /// </summary>
    /// <param name="assemblyMarkerAttribute">The <see cref="Attribute"/> to used to filter the assemblies.</param>
    /// <param name="baseDirectory">The base directory used to look-up the assemblies.</param>
    /// <remarks>
    /// The constructor with load the assemblies in a reflection only state. It is only intended for use in tools that work with the assemblies'
    /// reflection metadata.
    /// </remarks>
    public AssemblyFinder (Type assemblyMarkerAttribute, string baseDirectory)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("assemblyMarkerAttribute", assemblyMarkerAttribute, typeof (Attribute));
      ArgumentUtility.CheckNotNullOrEmpty ("baseDirectory", baseDirectory);

      List<Assembly> assemblies = new List<Assembly>();
      ReflectionOnlyLoadAssemblies (assemblies, Directory.GetFiles (baseDirectory, "*.dll", SearchOption.TopDirectoryOnly));
      ReflectionOnlyLoadAssemblies (assemblies, Directory.GetFiles (baseDirectory, "*.exe", SearchOption.TopDirectoryOnly));

      _rootAssemblies = assemblies.FindAll (delegate (Assembly assembly) { return IsAttributeDefined (assembly, assemblyMarkerAttribute); }).ToArray();
    }

    /// <summary>
    /// Gets the array of assembies identified by the constructor arguments.
    /// </summary>
    public Assembly[] RootAssemblies
    {
      get { return _rootAssemblies; }
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

    private void ReflectionOnlyLoadAssemblies (List<Assembly> assemblies, string[] paths)
    {
      foreach (string path in paths)
      {
        Assembly assembly = ReflectionOnlyTryLoadAssembly (path);
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

    private Assembly ReflectionOnlyTryLoadAssembly (string path)
    {
      try
      {
        return Assembly.ReflectionOnlyLoadFrom (path);
      }
      catch (BadImageFormatException)
      {
        return null;
      }
    }

    private bool IsAttributeDefined (Assembly assembly, Type assemblyMarkerAttribute)
    {
      foreach (CustomAttributeData customAttributeData in CustomAttributeData.GetCustomAttributes (assembly))
      {
        for (Type customAttributeType = customAttributeData.Constructor.DeclaringType;
             customAttributeType != typeof (Attribute);
             customAttributeType = customAttributeType.BaseType)
        {
          if (customAttributeType.AssemblyQualifiedName.Equals (assemblyMarkerAttribute.AssemblyQualifiedName, StringComparison.Ordinal))
            return true;
        }
      }

      return false;
    }
  }
}