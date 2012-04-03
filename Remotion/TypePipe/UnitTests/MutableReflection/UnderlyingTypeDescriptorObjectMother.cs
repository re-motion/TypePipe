using System;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.ReflectionEmit;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class UnderlyingTypeDescriptorObjectMother
  {
    private class UnspecifiedType { }

    public static UnderlyingTypeDescriptor Create (Type originalType = null, IMemberFilter memberFilter = null)
    {
      return UnderlyingTypeDescriptor.Create (originalType ?? typeof (UnspecifiedType), memberFilter ?? new MemberFilter());
    }
  }
}