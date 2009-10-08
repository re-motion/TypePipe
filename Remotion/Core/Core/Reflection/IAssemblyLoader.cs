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

namespace Remotion.Reflection
{
  /// <summary>
  /// Defines an interface for classes that can load assemblies from a file path.
  /// </summary>
  public interface IAssemblyLoader
  {
    /// <summary>
    /// Tries to load an assembly from the given <paramref name="filePath"/>, returning <see langword="null"/> if the file exists but is no assembly.
    /// </summary>
    /// <param name="filePath">The file path to load the assembly from.</param>
    /// <returns>The loaded assembly, or <see langword="null"/> if the assembly can't be loaded.</returns>
    /// <exception cref="AssemblyLoaderException">Thrown when the file cannot be found or an unexpected exception occurs while loading it.</exception>
    Assembly TryLoadAssembly (string filePath);

    /// <summary>
    /// Tries the load an assembly from the given <paramref name="assemblyName"/>, returning <see langword="null"/> if the file exists but is no 
    /// assembly.
    /// </summary>
    /// <param name="assemblyName">The assembly name to load the assembly from.</param>
    /// <param name="context">Context information to be included with the exception message when the assembly cannot be found or an unexpected 
    /// exception occurs while loading it.</param>
    /// <returns>The loaded assembly, or <see langword="null"/> if the assembly can't be loaded.</returns>
    /// <exception cref="AssemblyLoaderException">Thrown when the assembly cannot be found or an unexpected exception occurs while loading it.</exception>
    Assembly TryLoadAssembly (AssemblyName assemblyName, string context);

    /// <summary>
    /// Tries to load all assemblies from the given <paramref name="filePaths"/> as if <see cref="TryLoadAssembly(string)"/> was called for each of them,
    /// returning only those assemblies that were successfully loaded.
    /// </summary>
    /// <param name="filePaths">The file paths to load assemblies from.</param>
    /// <returns>An enumeration of the assemblies successfully loaded from the <paramref name="filePaths"/>. The enumeration might be lazy.</returns>
    IEnumerable<Assembly> LoadAssemblies (params string[] filePaths);
  }
}