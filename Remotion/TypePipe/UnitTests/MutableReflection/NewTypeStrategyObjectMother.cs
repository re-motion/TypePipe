using System;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class NewTypeStrategyObjectMother
  {
    private class UnspecifiedType { }

    public static NewTypeStrategy Create (
        Type baseType = null,
        TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
        Type[] interfaces = null,
        FieldInfo[] fields = null,
        ConstructorInfo[] constructors = null)
    {
      return new NewTypeStrategy (
          baseType ?? typeof(UnspecifiedType),
          attributes,
          interfaces ?? Type.EmptyTypes,
          fields ?? new FieldInfo[0],
          constructors ?? new ConstructorInfo[0]);
    }
  }
}