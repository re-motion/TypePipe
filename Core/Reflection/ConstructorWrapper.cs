using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Rubicon.Text;

namespace Rubicon.Reflection
{
  /// <summary>
  /// Creates wrapper methods for constructors so they can be called via delegates instead of reflection.
  /// </summary>
  public class ConstructorWrapper
  {
    public static GetDelegateWith<T> GetConstructor<T> ()
    {
      return GetConstructor<T> (BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<T> GetConstructor<T> (BindingFlags bindingFlags)
    {
      return GetConstructor<T> (bindingFlags, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<T> GetConstructor<T> (
        BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new CachedGetDelegateWith<T> (
          delegate (Type[] types, Type delegateType)
          {
            return CreateConstructorDelegate (typeof (T), bindingFlags, binder, callingConvention, types, parameterModifiers, delegateType);
          });
    }

    public static GetDelegateWith<object> GetConstructor (Type type)
    {
      return GetConstructor (type, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<object> GetConstructor (Type type, BindingFlags bindingFlags)
    {
      return GetConstructor (type, bindingFlags, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<object> GetConstructor (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new CachedGetDelegateWith<object,Type> (
          type,
          delegate (Type[] types, Type delegateType)
          {
            return CreateConstructorDelegate (type, bindingFlags, binder, callingConvention, types, parameterModifiers, delegateType);
          });
    }

    public static Delegate CreateConstructorDelegate (
        Type type,
        BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, Type[] parameterTypes, ParameterModifier[] parameterModifiers,
        Type delegateType)
    {
      ConstructorInfo ctor = type.GetConstructor (bindingFlags, binder, callingConvention, parameterTypes, parameterModifiers);
      if (ctor == null)
      {
        throw new MissingMethodException (
          "Type " + type.FullName + " does not contain a constructor with the following arguments types: "
          + SeparatedStringBuilder.Build (", ", parameterTypes, delegate (Type t) { return t.FullName; }));
      }
      return CreateDelegate (ctor, delegateType);
    }

    public static Delegate CreateDelegate (ConstructorInfo ctor, Type delegateType)
    {
      ParameterInfo[] parameters = ctor.GetParameters ();
      Type type = ctor.DeclaringType;
      DynamicMethod method = new DynamicMethod ("ConstructorWrapper", type, EmitUtility.GetParameterTypes (parameters), type);
      ILGenerator ilgen = method.GetILGenerator ();
      EmitUtility.PushParameters (ilgen, parameters.Length);
      ilgen.Emit (OpCodes.Newobj, ctor);
      ilgen.Emit (OpCodes.Ret);

      return method.CreateDelegate (delegateType);
    }
  }
}
