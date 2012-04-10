using System;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutablePropertyInfoObjectMother
  {
    private class UnspecifiedType { }

    public static MutablePropertyInfo Create (
        Type declaringType = null,
        Type propertyType = null,
        MethodInfo getMethod = null,
        MethodInfo setMethod = null)
    {
      if (getMethod == null && setMethod == null)
        getMethod = MutableMethodInfoObjectMother.Create();

      return new MutablePropertyInfo (
          declaringType ?? typeof (UnspecifiedType),
          propertyType ?? typeof (UnspecifiedType),
          getMethod,
          setMethod);
    }
  }
}