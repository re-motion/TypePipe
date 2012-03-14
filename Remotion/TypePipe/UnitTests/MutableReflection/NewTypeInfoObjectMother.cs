using System;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class NewTypeInfoObjectMother
  {
    private class UnspecifiedType { }

    public static NewTypeInfo Create (
        Type baseType = null,
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
        Type[] interfaces = null,
        FieldInfo[] fields = null,
        ConstructorInfo[] constructors = null)
    {
      return new NewTypeInfo (
          baseType ?? typeof(UnspecifiedType),
          attributes,
          interfaces ?? Type.EmptyTypes,
          fields ?? new FieldInfo[0],
          constructors ?? new ConstructorInfo[0]);
    }
  }
}