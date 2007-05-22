using System;

namespace Rubicon.Core.UnitTests.Reflection.TestAssemblies.MarkerAttributeAssembly
{
  [AttributeUsage (AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
  public class MarkerAttribute : Attribute
  {
    public MarkerAttribute ()
    {
    }
  }
}