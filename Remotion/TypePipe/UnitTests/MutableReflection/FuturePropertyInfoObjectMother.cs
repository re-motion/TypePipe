using System;
using System.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class FuturePropertyInfoObjectMother
  {
    private class UnspecifiedType { }

    public static FuturePropertyInfo Create (
        Type declaringType = null,
        Type propertyType = null,
        MethodInfo getMethod = null,
        MethodInfo setMethod = null)
    {
      if (getMethod == null && setMethod == null)
        getMethod = FutureMethodInfoObjectMother.Create();

      return new FuturePropertyInfo (
          declaringType ?? typeof (UnspecifiedType),
          propertyType ?? typeof (UnspecifiedType),
          Maybe.ForValue (getMethod),
          Maybe.ForValue (setMethod));
    }
  }
}