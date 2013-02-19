using System;
using System.Linq;
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableEventInfoObjectMother
  {
    public static MutableEventInfo Create (
        ProxyType declaringType = null,
        string name = "UnspecifiedEvent",
        Type handlerType = null,
        EventAttributes attributes = EventAttributes.None,
        MutableMethodInfo addMethod = null,
        MutableMethodInfo removeMethod = null,
        MutableMethodInfo raiseMethod = null)
    {
      declaringType = declaringType ?? ProxyTypeObjectMother.Create();
      Assertion.IsTrue (
          handlerType == null || (addMethod == null && removeMethod == null && raiseMethod == null),
          "Can only declare handlerType XOR addMethod, removeMethod, raiseMethod.");
      Assertion.IsTrue (handlerType == null || handlerType.GetGenericArguments().Length == 2, "Can only handle generic types with two arguments.");

      var argumentType = handlerType != null ? handlerType.GetGenericArguments()[0] : ReflectionObjectMother.GetSomeType();
      var returnType = handlerType != null ? handlerType.GetGenericArguments()[1] : ReflectionObjectMother.GetSomeOtherType();
      handlerType = handlerType ?? typeof (Func<,>).MakeGenericType (argumentType, returnType);

      var addRemoveParameters = new[] { new ParameterDeclaration (handlerType, "handler") };
      addMethod = addMethod
                  ?? MutableMethodInfoObjectMother.Create (declaringType, "AddMethod", returnType: typeof (void), parameters: addRemoveParameters);
      removeMethod = removeMethod
                     ?? MutableMethodInfoObjectMother.Create (
                         declaringType, "RemoveMethod", returnType: typeof (void), parameters: addRemoveParameters);
      raiseMethod = raiseMethod
                    ?? MutableMethodInfoObjectMother.Create (
                        declaringType, "RaiseMethod", returnType: returnType, parameters: new[] { new ParameterDeclaration (argumentType, "arg") });
      Assertion.IsTrue (handlerType == addMethod.GetParameters().Single().ParameterType);
      Assertion.IsTrue (handlerType == removeMethod.GetParameters().Single().ParameterType);
      Assertion.IsTrue (argumentType == raiseMethod.GetParameters().Single().ParameterType && returnType == raiseMethod.ReturnType);

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }
  }
}