using System;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ExistingTypeInfoObjectMother
  {
    private class UnspecifiedType { }

    public static ExistingTypeInfo Create (Type originalType = null)
    {
      return new ExistingTypeInfo (originalType ?? typeof (UnspecifiedType));
    }
  }
}