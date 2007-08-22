using System;
using System.Reflection;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  [Serializable]
  public class AttributeAssemblyFinderFilter : IAssemblyFinderFilter
  {
    private readonly Type _attributeType;

    public AttributeAssemblyFinderFilter (Type attributeType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("attributeType", attributeType, typeof (Attribute));
      _attributeType = attributeType;
    }

    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      ArgumentUtility.CheckNotNull ("assemblyName", assemblyName);
      return true;
    }

    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);
      return assembly.IsDefined (_attributeType, false);
    }
  }
}