using System;
using System.Collections.Generic;
using System.Reflection;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  [Serializable]
  public class ApplicationAssemblyFinderFilter : IAssemblyFinderFilter
  {
    public static readonly ApplicationAssemblyFinderFilter Instance = new ApplicationAssemblyFinderFilter();

    private static string MakeMatchExpression (IEnumerable<string> assemblyMatchStrings)
    {
      ArgumentUtility.CheckNotNull ("assemblyMatchStrings", assemblyMatchStrings);

      return "^((" + SeparatedStringBuilder.Build (")|(", assemblyMatchStrings) + "))$";
    }

    private List<string> _nonApplicationAssemblyNames;

    private RegexAssemblyFinderFilter _assemblyNameFilter;
    private readonly object _assemblyNameFilterLock = new object();

    private ApplicationAssemblyFinderFilter ()
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
                    @"Microsoft\..*"
                });
        _assemblyNameFilter = null;
      }
    }

    public string SystemAssemblyMatchExpression
    {
      get { return AssemblyNameFilter.MatchExpressionString; }
    }

    private RegexAssemblyFinderFilter AssemblyNameFilter
    {
      get
      {
        lock (_assemblyNameFilterLock)
        {
          if (_assemblyNameFilter == null)
          {
            string matchExpression = MakeMatchExpression (_nonApplicationAssemblyNames);
            _assemblyNameFilter = new RegexAssemblyFinderFilter (matchExpression, RegexAssemblyFinderFilter.MatchTargetKind.SimpleName);
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