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
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  public class ApplicationAssemblyLoaderFilter : IAssemblyLoaderFilter
  {
    public static readonly ApplicationAssemblyLoaderFilter Instance = new ApplicationAssemblyLoaderFilter();

    private static string MakeMatchExpression (IEnumerable<string> assemblyMatchStrings)
    {
      ArgumentUtility.CheckNotNull ("assemblyMatchStrings", assemblyMatchStrings);

      return "^((" + SeparatedStringBuilder.Build (")|(", assemblyMatchStrings) + "))$";
    }

    private List<string> _nonApplicationAssemblyNames;

    private RegexAssemblyLoaderFilter _assemblyNameFilter;
    private readonly object _assemblyNameFilterLock = new object();

    private ApplicationAssemblyLoaderFilter ()
    {
      Reset();
    }

    public void Reset ()
    {
      lock (_assemblyNameFilterLock)
      {
        _nonApplicationAssemblyNames = new List<string> (
            new string[]
                {
                    @"mscorlib",
                    @"System",
                    @"System\..*",
                    @"Microsoft\..*",
                    @"Remotion\..*\.Generated\..*",
                });
        _assemblyNameFilter = null;
      }
    }

    public string SystemAssemblyMatchExpression
    {
      get { return AssemblyNameFilter.MatchExpressionString; }
    }

    private RegexAssemblyLoaderFilter AssemblyNameFilter
    {
      get
      {
        lock (_assemblyNameFilterLock)
        {
          if (_assemblyNameFilter == null)
          {
            string matchExpression = MakeMatchExpression (_nonApplicationAssemblyNames);
            _assemblyNameFilter = new RegexAssemblyLoaderFilter (matchExpression, RegexAssemblyLoaderFilter.MatchTargetKind.SimpleName);
          }
          return _assemblyNameFilter;
        }
      }
    }

    public void AddIgnoredAssembly (string simpleNameRegularExpression)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("simpleNameRegularExpression", simpleNameRegularExpression);
      lock (_assemblyNameFilterLock)
      {
        _nonApplicationAssemblyNames.Add (simpleNameRegularExpression);
        _assemblyNameFilter = null;
      }
    }

    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      ArgumentUtility.CheckNotNull ("assemblyName", assemblyName);
      return !AssemblyNameFilter.ShouldConsiderAssembly (assemblyName);
    }

    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);
      return !assembly.IsDefined (typeof (NonApplicationAssemblyAttribute), false);
    }
  }
}
