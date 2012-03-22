using System;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.ReflectionEmit;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ExistingTypeStrategyObjectMother
  {
    private class UnspecifiedType { }

    public static ExistingTypeStrategy Create (Type originalType = null, IMemberFilter memberFilter = null)
    {
      return new ExistingTypeStrategy (
        originalType ?? typeof (UnspecifiedType),
        memberFilter ?? new MemberFilter());
    }
  }
}