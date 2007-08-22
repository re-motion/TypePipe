using System;
using System.Reflection;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  [Serializable]
  public class NonSystemAssemblyFinderFilter : IAssemblyFinderFilter
  {
    private readonly string[] _systemAssemblyNames = new string[]
        {
          @"mscorlib",
          @"System",
          @"System\..*",
          @"Microsoft\..*"
        };

    private readonly RegexAssemblyFinderFilter _systemAssemblyFilter;
    private readonly string _matchExpression;

    public NonSystemAssemblyFinderFilter()
    {
      _matchExpression = MakeMatchExpression (_systemAssemblyNames);
      _systemAssemblyFilter = new RegexAssemblyFinderFilter (_matchExpression, RegexAssemblyFinderFilter.MatchTargetKind.SimpleName);
    }

    private static string MakeMatchExpression (string[] assemblyMatchStrings)
    {
      ArgumentUtility.CheckNotNull ("assemblyMatchStrings", assemblyMatchStrings);

      return "^(" + SeparatedStringBuilder.Build (")|(", assemblyMatchStrings) + ")$";
    }

    public string SystemAssemblyMatchExpression
    {
      get { return _matchExpression; }
    }

    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      return !_systemAssemblyFilter.ShouldConsiderAssembly (assemblyName);
    }

    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      return true;
    }
  }
}