using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  public class AssemblyFinder
  {
    private readonly Assembly[] _rootAssemblies;

    public AssemblyFinder (params Assembly[] rootAssemblies)
    {
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("rootAssemblies", rootAssemblies);
      _rootAssemblies = rootAssemblies;
    }


    public AssemblyFinder (Type assemblyMarkerAttribute)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("assemblyMarkerAttribute", assemblyMarkerAttribute, typeof (Attribute));

      List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies ());
      LoadAssemblies (assemblies, Directory.GetFiles (AppDomain.CurrentDomain.BaseDirectory, "*.dll"));
      LoadAssemblies (assemblies, Directory.GetFiles (AppDomain.CurrentDomain.BaseDirectory, "*.exe"));

      _rootAssemblies = assemblies.FindAll (delegate (Assembly assembly) { return assembly.IsDefined (assemblyMarkerAttribute, false); }).ToArray ();
    }

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

    private Assembly TryLoadAssembly (string path)
    {
      try
      {
        return Assembly.LoadFile (path);
      }
      catch (BadImageFormatException)
      {
        return null;
      }
    }
  }
}