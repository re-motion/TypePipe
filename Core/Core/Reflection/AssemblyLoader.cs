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
using Remotion.Logging;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  public class AssemblyLoader
  {
    private static readonly ILog s_log = LogManager.GetLogger (typeof (AssemblyFinder));
    private readonly IAssemblyFinderFilter _filter;

    public AssemblyLoader (IAssemblyFinderFilter filter)
    {
      ArgumentUtility.CheckNotNull ("filter", filter);
      _filter = filter;
    }

    public IAssemblyFinderFilter Filter
    {
      get { return _filter; }
    }

    public virtual Assembly TryLoadAssembly (string filePath)
    {
      ArgumentUtility.CheckNotNull ("filePath", filePath);

      AssemblyName assemblyName;
      try
      {
        s_log.InfoFormat ("Attempting to get assembly name for path {0}.", filePath);
        assemblyName = AssemblyName.GetAssemblyName (filePath);
      }
      catch (BadImageFormatException ex)
      {
        s_log.InfoFormat (ex, "Path {0} triggered BadImageFormatException - is probably no .NET assembly.", filePath);
        return null;
      }
      catch (FileLoadException ex)
      {
        s_log.WarnFormat (
            ex,
            "Assembly {0} triggered FileLoadException - maybe the assembly is DelaySigned, but signing has not been completed?",
            filePath);
        return null;
      }

      return TryLoadAssembly(assemblyName, filePath);
    }

    public virtual Assembly TryLoadAssembly (AssemblyName assemblyName, string context)
    {
      ArgumentUtility.CheckNotNull ("assemblyName", assemblyName);
      ArgumentUtility.CheckNotNull ("context", context);

      if (_filter.ShouldConsiderAssembly (assemblyName))
      {
        try
        {
          Assembly loadedAssembly = Assembly.Load (assemblyName);
          return _filter.ShouldIncludeAssembly (loadedAssembly) ? loadedAssembly : null;
        }
        catch (FileLoadException ex)
        {
          s_log.WarnFormat (
              ex,
              "Assembly {0} triggered FileLoadException - maybe a referenced assembly is missing? Or the assembly could be DelaySigned, but signing "
              + "has not been completed. The assembly was loaded in the following context: {1}.", assemblyName, context);
          return null;
        }
      }
      else
        return null;
    }

    public virtual IEnumerable<Assembly> LoadAssemblies (params string[] filePaths)
    {
      ArgumentUtility.CheckNotNull ("filePaths", filePaths);
      foreach (string filePath in filePaths)
      {
        Assembly assembly = TryLoadAssembly (filePath);
        if (assembly != null)
          yield return assembly;
      }
    }

  }
}