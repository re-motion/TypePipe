using System;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class FutureFieldInfoObjectMother
  {
    private class UnspecifiedType { }

    public static FutureFieldInfo Create (
        Type declaringType = null,
        string name = "_newField",
        Type fieldType = null,
        FieldAttributes attributes = FieldAttributes.Private)
    {
      return new FutureFieldInfo (
          declaringType ?? typeof (UnspecifiedType),
          name,
          fieldType ?? typeof (UnspecifiedType),
          attributes);
    }
  }
}