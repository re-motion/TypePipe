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
    /// Tries to load an assembly from the given <paramref name="filePath"/>, returning <see langword="null"/> if there is any problem loading
    /// the assembly.
    /// </summary>
    /// <param name="filePath">The file path to load the assembly from.</param>
    /// <returns>The loaded assembly, or <see langword="null"/> if the assembly can't be loaded.</returns>
    Assembly TryLoadAssembly (string filePath);

    /// <summary>
    /// Tries to load all assemblies from the given <paramref name="filePaths"/> as if <see cref="TryLoadAssembly"/> was called for each of them,
    /// returning only those assemblies that were successfully loaded.
    /// </summary>
    /// <param name="filePaths">The file paths to load assemblies from.</param>
    /// <returns>An enumeration of the assemblies successfully loaded from the <paramref name="filePaths"/>. The enumeration might be lazy.</returns>
    IEnumerable<Assembly> LoadAssemblies (params string[] filePaths);
  }
}