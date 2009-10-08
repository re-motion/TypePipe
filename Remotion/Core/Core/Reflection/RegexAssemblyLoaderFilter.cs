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
using System.Reflection;
using System.Text.RegularExpressions;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  [Serializable]
  public class RegexAssemblyLoaderFilter : IAssemblyLoaderFilter
  {
    public enum MatchTargetKind { FullName, SimpleName };

    private readonly Regex _matchExpression;
    private readonly string _matchExpressionString;
    private readonly MatchTargetKind _matchTarget;

    public RegexAssemblyLoaderFilter (Regex matchExpression, MatchTargetKind matchTarget)
    {
      ArgumentUtility.CheckNotNull ("matchExpression", matchExpression);
      ArgumentUtility.CheckValidEnumValue ("matchTarget", matchTarget);
      _matchExpression = matchExpression;
      _matchExpressionString = matchExpression.ToString ();
      _matchTarget = matchTarget;
    }

    public RegexAssemblyLoaderFilter (string matchExpression, MatchTargetKind matchTarget)
        : this (new Regex (
            ArgumentUtility.CheckNotNull ("matchExpression", matchExpression),
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline), matchTarget)
    {
    }

    public string MatchExpressionString
    {
      get { return _matchExpressionString; }
    }

    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      switch (_matchTarget)
      {
        case MatchTargetKind.SimpleName:
          return _matchExpression.IsMatch (assemblyName.Name);
        default:
          return _matchExpression.IsMatch (assemblyName.FullName);
      }
    }

    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      return true;
    }
  }
}
