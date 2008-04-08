using System;

namespace Remotion.Core.UnitTests.Reflection
{
  [AttributeUsage (AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
  public class MarkerAttribute : Attribute
  {
    public MarkerAttribute ()
    {
    }
  }
}