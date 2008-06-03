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
using System.Reflection;
using System.Text.RegularExpressions;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  [Serializable]
  public class RegexAssemblyFinderFilter : IAssemblyFinderFilter
  {
    public enum MatchTargetKind { FullName, SimpleName };

    private readonly Regex _matchExpression;
    private readonly string _matchExpressionString;
    private readonly MatchTargetKind _matchTarget;

    public RegexAssemblyFinderFilter (Regex matchExpression, MatchTargetKind matchTarget)
    {
      ArgumentUtility.CheckNotNull ("matchExpression", matchExpression);
      ArgumentUtility.CheckValidEnumValue ("matchTarget", matchTarget);
      _matchExpression = matchExpression;
      _matchExpressionString = matchExpression.ToString ();
      _matchTarget = matchTarget;
    }

    public RegexAssemblyFinderFilter (string matchExpression, MatchTargetKind matchTarget)
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
