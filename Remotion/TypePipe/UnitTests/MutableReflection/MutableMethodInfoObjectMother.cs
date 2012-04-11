using System;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableMethodInfoObjectMother
  {
    private class UnspecifiedType { }

    public static MutableMethodInfo Create (
        Type declaringType = null,
        string name = "UnspecifiedMethod",
        MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        ParameterDeclaration[] parameterDeclarations = null,
        Expression body = null)
    {
      return new MutableMethodInfo (
          declaringType ?? typeof (UnspecifiedType), 
          name,
          methodAttributes,
          returnType ?? typeof(UnspecifiedType),
          parameterDeclarations ?? ParameterDeclaration.EmptyParameters,
          body ?? ExpressionTreeObjectMother.GetSomeExpression(returnType));
    }
  }
}