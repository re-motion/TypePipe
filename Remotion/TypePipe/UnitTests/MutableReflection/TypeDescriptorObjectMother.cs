using System;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class TypeDescriptorObjectMother
  {
    private class UnspecifiedType { }

    public static TypeDescriptor Create (Type underlyingType = null)
    {
      return TypeDescriptor.Create (underlyingType ?? typeof (UnspecifiedType));
    }
  }
}