using System;

namespace Remotion.UnitTests.Reflection
{
  [AttributeUsage (AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
  public class DerivedMarkerAttribute : MarkerAttribute
  {
    public DerivedMarkerAttribute ()
    {
    }
  }
}