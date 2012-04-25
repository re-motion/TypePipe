using System;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class UnderlyingTypeDescriptorObjectMother
  {
    private class UnspecifiedType { }

    public static UnderlyingTypeDescriptor Create (Type originalType = null)
    {
      return UnderlyingTypeDescriptor.Create (originalType ?? typeof (UnspecifiedType));
    }
  }
}