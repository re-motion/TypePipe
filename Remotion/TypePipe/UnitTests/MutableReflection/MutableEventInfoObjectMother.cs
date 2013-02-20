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
    public static MutableEventInfo CreateWithAccessors (
        ProxyType declaringType = null,
        string name = "UnspecifiedEvent",
        EventAttributes attributes = EventAttributes.None,
        MutableMethodInfo addMethod = null,
        MutableMethodInfo removeMethod = null,
        MutableMethodInfo raiseMethod = null)
    {
      Assertion.IsTrue (addMethod != null && removeMethod != null);
      declaringType = declaringType ?? ProxyTypeObjectMother.Create();

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }

    public static MutableEventInfo Create (
      ProxyType declaringType = null,
      string name = "UnspecifiedEvent",
        EventAttributes attributes = EventAttributes.None,
      Type handlerType = null)
    {
      declaringType = declaringType ?? ProxyTypeObjectMother.Create();
      handlerType = handlerType ?? typeof (Func<,>).MakeGenericType (ReflectionObjectMother.GetSomeType(), ReflectionObjectMother.GetSomeOtherType());
      Assertion.IsTrue (handlerType.IsSubclassOf (typeof (Delegate)));

      var invokeMethod = handlerType.GetMethod ("Invoke");
      var argumentTypes = invokeMethod.GetParameters().Select (p => p.ParameterType);
      var returnType = invokeMethod.ReturnType;

      var addRemoveParameters = new[] { new ParameterDeclaration (handlerType, "handler") };
      var addMethod = MutableMethodInfoObjectMother.Create (declaringType, returnType: typeof (void), parameters: addRemoveParameters);
      var removeMethod = MutableMethodInfoObjectMother.Create (declaringType, returnType: typeof (void), parameters: addRemoveParameters);
      var raiseParameters = argumentTypes.Select ((t, i) => new ParameterDeclaration (t, i.ToString()));
      var raiseMethod = MutableMethodInfoObjectMother.Create (declaringType, returnType: returnType, parameters: raiseParameters);

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }
  }
}