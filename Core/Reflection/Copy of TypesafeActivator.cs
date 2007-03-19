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
  ///   string s;
  ///   YourClass obj = TypesafeActivator.CreateInstance&ltYourClass&gt;().With (s);
  /// </code>
  /// This code always selects the constructor that accepts a string argument, even if the string is null and other constructors that could accept
  /// a null reference are available.
  /// </remarks>
  public static class TypesafeActivator
  {
    public class TypesafeActivatorInvokeWith<T> : InvokeWith<T>
    {
      private BindingFlags _bindingFlags;
      private Binder _binder;
      private CallingConventions _callingConvention;
      private ParameterModifier[] _parameterModifiers;

      public TypesafeActivatorInvokeWith (
          BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifier)
      {
        _bindingFlags = bindingFlags;
        _binder = binder;
        _callingConvention = callingConvention;
        _parameterModifiers = parameterModifier;
      }

      protected virtual Type GetClassType ()
      {
        return typeof (T);
      }

      protected override T Invoke (Type[] valueTypes, object[] values)
      {
        ConstructorInfo ctor = GetClassType().GetConstructor (_bindingFlags, _binder, _callingConvention, valueTypes, _parameterModifiers);
        if (ctor == null)
        {
          throw new MissingMethodException (
            "Type " + GetClassType().FullName + " does not contain a constructor with the following arguments types: "
            + SeparatedStringBuilder.Build (", ", valueTypes, delegate (Type t) { return t.FullName; }));
        }
        return (T) ctor.Invoke (values);
      }
    }

    public class TypesafeActivatorInvokeWith : TypesafeActivatorInvokeWith<object>
    {
      private Type _type;

      public TypesafeActivatorInvokeWith (
          Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifier)
        : base (bindingFlags, binder, callingConvention, parameterModifier)
      {
        _type = type;
      }

      protected override Type GetClassType ()
      {
        return _type;
      }
    }

    public static TypesafeActivatorInvokeWith<T> CreateInstance<T> ()
    {
      return CreateInstance<T> (BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static TypesafeActivatorInvokeWith<T> CreateInstance<T> (BindingFlags bindingFlags)
    {
      return CreateInstance<T> (bindingFlags, null, CallingConventions.Any, null);
    }

    public static TypesafeActivatorInvokeWith<T> CreateInstance<T> (
        BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new TypesafeActivatorInvokeWith<T> (bindingFlags, binder, callingConvention, parameterModifiers);
    }

    public static TypesafeActivatorInvokeWith CreateInstance (Type type)
    {
      return CreateInstance (type, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static TypesafeActivatorInvokeWith CreateInstance (Type type, BindingFlags bindingFlags)
    {
      return CreateInstance (type, bindingFlags, null, CallingConventions.Any, null);
    }

    public static TypesafeActivatorInvokeWith CreateInstance (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new TypesafeActivatorInvokeWith (type, bindingFlags, binder, callingConvention, parameterModifiers);
    }
  }
}
