using System;
using System.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class FutureMethodInfoObjectMother
  {
    private class UnspecifiedType { }

    public static FutureMethodInfo Create (
        Type declaringType = null,
        MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        ParameterDeclaration[] parameterDeclarations = null)
    {
      return new FutureMethodInfo (
          declaringType ?? typeof (UnspecifiedType), 
          methodAttributes,
          returnType ?? typeof(UnspecifiedType),
          parameterDeclarations ?? new ParameterDeclaration[0]);
    }
  }
}