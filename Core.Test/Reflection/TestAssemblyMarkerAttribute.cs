using System;

namespace Rubicon.Core.UnitTests.Reflection
{
  [AttributeUsage (AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
  public class TestAssemblyMarkerAttribute : Attribute
  {
    public TestAssemblyMarkerAttribute ()
    {
    }
  }
}