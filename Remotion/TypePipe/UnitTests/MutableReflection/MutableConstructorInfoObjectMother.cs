using System;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableConstructorInfoObjectMother
  {
    private class UnspecifiedType { }

    public static MutableConstructorInfo Create (
        Type declaringType = null,
        MethodAttributes attributes = MethodAttributes.Public,
        ParameterDeclaration[] parameterDeclarations = null)
    {
      return new MutableConstructorInfo (
          declaringType ?? typeof (UnspecifiedType),
          attributes,
          parameterDeclarations ?? new ParameterDeclaration[0]);
    }
  }
}