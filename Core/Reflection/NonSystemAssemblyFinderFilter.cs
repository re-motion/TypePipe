using System;
using System.Reflection;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  [Serializable]
  public class NonSystemAssemblyFinderFilter : IAssemblyFinderFilter
  {
    private static readonly string[] s_systemAssemblyNames = new string[]
        {
          @"mscorlib",
          @"System",
          @"System\..*",
          @"Microsoft\..*",
        };

    private static readonly RegexAssemblyFinderFilter s_systemAssemblyFilter;
    private static readonly string s_matchExpression;

    static NonSystemAssemblyFinderFilter ()
    {
      s_matchExpression = MakeMatchExpression (s_systemAssemblyNames);
      s_systemAssemblyFilter = new RegexAssemblyFinderFilter (s_matchExpression, RegexAssemblyFinderFilter.MatchTargetKind.SimpleName);
    }

    private static string MakeMatchExpression (string[] assemblyMatchStrings)
    {
      ArgumentUtility.CheckNotNull ("assemblyMatchStrings", assemblyMatchStrings);

      return "^((" + SeparatedStringBuilder.Build (")|(", assemblyMatchStrings) + "))$";
    }

    public string SystemAssemblyMatchExpression
    {
      get { return s_matchExpression; }
    }

    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      return !s_systemAssemblyFilter.ShouldConsiderAssembly (assemblyName);
    }

    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      return true;
    }
  }
}