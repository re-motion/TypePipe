using System;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.ReflectionEmit;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ExistingTypeInfoObjectMother
  {
    private class UnspecifiedType { }

    public static ExistingTypeInfo Create (Type originalType = null, IMemberFilter memberFilter = null)
    {
      return new ExistingTypeInfo (
        originalType ?? typeof (UnspecifiedType),
        memberFilter ?? new MemberFilter());
    }
  }
}