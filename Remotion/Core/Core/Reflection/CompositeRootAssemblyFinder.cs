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
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Reflection
{
  /// <summary>
  /// Composes several <see cref="IRootAssemblyFinder"/> instances into one, combining all results into one, eliminating duplicates in the process.
  /// </summary>
  public class CompositeRootAssemblyFinder : IRootAssemblyFinder
  {
    private readonly IRootAssemblyFinder[] _innerFinders;

    public CompositeRootAssemblyFinder (IEnumerable<IRootAssemblyFinder> finders)
    {
      ArgumentUtility.CheckNotNull ("finders", finders);
      _innerFinders = finders.ToArray();
    }

    public IRootAssemblyFinder[] InnerFinders
    {
      get { return _innerFinders; }
    }

    public Assembly[] FindRootAssemblies (IAssemblyLoader loader)
    {
      var combinedAssemblies = new HashSet<Assembly>();
      foreach (var finder in _innerFinders)
      {
        combinedAssemblies.UnionWith (finder.FindRootAssemblies (loader));
      }
      return combinedAssemblies.ToArray ();
    }
  }
}