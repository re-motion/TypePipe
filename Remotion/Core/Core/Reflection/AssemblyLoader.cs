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
using System.IO;
using System.Reflection;
using Remotion.Logging;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Loads assemblies from a file path by first getting their corresponding <see cref="AssemblyName"/> and then loading the assembly with that name.
  /// This means that only assemblies from the assembly search path (application directory, dynamic directory, GAC) can be loaded, and that GAC 
  /// assemblies are preferred. The advantage of this load mode is that assemblies are loaded exactly the same way as if loaded directly by .NET:
  /// they are always loaded into the correct context and references are resolved correctly.
  /// </summary>
  public class AssemblyLoader : IAssemblyLoader
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

      s_log.InfoFormat ("Attempting to get assembly name for path '{0}'.", filePath);
      AssemblyName assemblyName = PerformGuardedLoadOperation (filePath, null, () => AssemblyName.GetAssemblyName (filePath));
      if (assemblyName == null)
        return null;

      s_log.InfoFormat ("Assembly name for path '{0}' is '{1}'.", filePath, assemblyName.FullName);

      return TryLoadAssembly(assemblyName, filePath);
    }

    public virtual Assembly TryLoadAssembly (AssemblyName assemblyName, string context)
    {
      ArgumentUtility.CheckNotNull ("assemblyName", assemblyName);
      ArgumentUtility.CheckNotNull ("context", context);

      if (PerformGuardedLoadOperation (assemblyName.FullName, context, () => _filter.ShouldConsiderAssembly (assemblyName)))
      {
        s_log.InfoFormat ("Attempting to load assembly with name '{0}' in context '{1}'.", assemblyName, context);
        Assembly loadedAssembly = PerformGuardedLoadOperation (assemblyName.FullName, context, () => Assembly.Load (assemblyName));
        s_log.InfoFormat ("Success: {0}", loadedAssembly != null);

        if (loadedAssembly == null)
          return null;
        else if (PerformGuardedLoadOperation (assemblyName.FullName, context, () => _filter.ShouldIncludeAssembly (loadedAssembly)))
          return loadedAssembly;
        else
          return null;
      }
      else
        return null;
    }

    public T PerformGuardedLoadOperation<T> (string assemblyDescription, string loadContext, Func<T> loadOperation)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyDescription", assemblyDescription);
      ArgumentUtility.CheckNotNull ("loadOperation", loadOperation);

      var assemblyDescriptionText = "'" + assemblyDescription + "'";
      if (loadContext != null)
        assemblyDescriptionText += " (loaded in the context of '" + loadContext + "')";

      try
      {
        return loadOperation ();
      }
      catch (BadImageFormatException ex)
      {
        s_log.InfoFormat (ex, "Assembly {0} triggered BadImageFormatException - it is probably no .NET assembly.", assemblyDescriptionText);
        return default (T);
      }
      catch (FileLoadException ex)
      {
        s_log.WarnFormat (
            ex,
            "Assembly {0} triggered FileLoadException - maybe the assembly is DelaySigned, but signing has not been completed?",
            assemblyDescriptionText);
        return default (T);
      }
      catch (FileNotFoundException ex)
      {
        string message = string.Format ("Assembly {0} triggered a FileNotFoundException - maybe the assembly does not exist or a referenced assembly "
            + "is missing?\r\nFileNotFoundException message: {1}", assemblyDescriptionText, ex.Message);
        throw new AssemblyLoaderException (message, ex);
      }
      catch (Exception ex)
      {
        string message = string.Format ("Assembly {0} triggered an unexpected exception of type {1}.\r\nUnexpected exception message: {2}", 
            assemblyDescriptionText, ex.GetType().FullName, ex.Message);
        throw new AssemblyLoaderException (message, ex);
      }
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
