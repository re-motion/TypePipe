using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Rubicon.Text;

namespace Rubicon.Reflection
{
  /// <summary>
  /// Create an instance of a known type using fixed parameter types for the constructor.
  /// </summary>
  /// <remarks>
  /// While <see cref="System.Activator.CreateInstance"/> uses the types of passed arguments to select the correct constructor, this class
  /// uses the types of the expressions you use at compile time. Use the following code to create an instance of a class called MyClass using a 
  /// constructor that has an argument of type string:
  /// <code>
  ///   ParameterType p;
  ///   YourClass obj = TypesafeActivator.CreateInstance&lt;YourClass&gt;().With (p);
  /// </code>
  /// This code always selects the constructor that accepts an argument of type ParameterType, even if the value passed is null or an instance
  /// of a subclass of ParameterType.
  /// </remarks>
  public static class TypesafeActivator
  {
    public static InvokeWith<T> CreateInstance<T> ()
    {
      return CreateInstance<T> (BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static InvokeWith<T> CreateInstance<T> (BindingFlags bindingFlags)
    {
      return CreateInstance<T> (bindingFlags, null, CallingConventions.Any, null);
    }

    public static InvokeWith<T> CreateInstance<T> (
        BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new InvokeWith<T> (ConstructorWrapper.GetConstructor<T> (bindingFlags, binder, callingConvention, parameterModifiers));
    }

    public static InvokeWith<object> CreateInstance (Type type)
    {
      return CreateInstance (type, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static InvokeWith<object> CreateInstance (Type type, BindingFlags bindingFlags)
    {
      return CreateInstance (type, bindingFlags, null, CallingConventions.Any, null);
    }

    public static InvokeWith<object> CreateInstance (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new InvokeWith<object> (ConstructorWrapper.GetConstructor (type, bindingFlags, binder, callingConvention, parameterModifiers));
    }
  }
}
