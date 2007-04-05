using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  using MethodDescriptor = Tuple<Type, string>;

  public static class MethodCaller
  {
    public static GetDelegateWith<TReturn> GetMethod<TType, TReturn> (string methodName)
    {
      return GetMethod<TType, TReturn> (methodName, BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<TReturn> GetMethod<TType, TReturn> (string methodName, BindingFlags bindingFlags)
    {
      return GetMethod<TType, TReturn> (methodName, bindingFlags, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<TReturn> GetMethod<TType, TReturn> (
        string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] modifiers)
    {
      return new CachedGetDelegateWith<TReturn, string> (
          methodName,
          delegate (Type[] argumentTypes, Type delegateType)
          {
            argumentTypes = ArrayUtility.Skip (argumentTypes, 1);
            MethodInfo method = typeof (TType).GetMethod (methodName, bindingFlags, binder, callingConvention, argumentTypes, modifiers);
            return Delegate.CreateDelegate (delegateType, method);
          });
    }

    public static GetDelegateWith<TReturn> GetMethod<TReturn> (Type type, string methodName)
    {
      return GetMethod<TReturn> (type, methodName, BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<TReturn> GetMethod<TReturn> (Type type, string methodName, BindingFlags bindingFlags)
    {
      return GetMethod<TReturn> (type, methodName, bindingFlags, null, CallingConventions.Any, null);
    }

    public static GetDelegateWith<TReturn> GetMethod<TReturn> (
        Type type, string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] modifiers)
    {
      return new CachedGetDelegateWith<TReturn, MethodDescriptor> (
          new MethodDescriptor (type, methodName),
          delegate (Type[] argumentTypes, Type delegateType)
          {
            MethodInfo method = type.GetMethod (methodName, bindingFlags, binder, callingConvention, argumentTypes, modifiers);
            return Delegate.CreateDelegate (delegateType, method);
          });
    }

    public static GetDelegateWith<TReturn> GetMethod<TReturn> (
        string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] modifiers)
    {
      return new CachedGetDelegateWith<TReturn> (
          delegate (Type[] argumentTypes, Type delegateType)
          {
            ArgumentUtility.CheckNotNullOrEmpty ("argumentTypes", argumentTypes);
            Type type = argumentTypes[0];
            argumentTypes = ArrayUtility.Skip (argumentTypes, 1);
            MethodInfo method = type.GetMethod (methodName, bindingFlags, binder, callingConvention, argumentTypes, modifiers);
            return Delegate.CreateDelegate (delegateType, method);
          });
    }

    public static InvokeWith<TReturn> Call<TReturn> (string methodName)
    {
      return Call<TReturn> (methodName, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null);
    }

    public static InvokeWith<TReturn> Call<TReturn> (
        string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] modifiers)
    {
      return new InvokeWith<TReturn> (
          GetMethod<TReturn> (methodName, bindingFlags, binder, callingConvention, modifiers));
    }


    public static InvokeWith<TReturn> Call<TType, TReturn> (TType obj, string methodName)
    {
      return Call<TType,TReturn> (obj, methodName, BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, null);
    }

    public static InvokeWith<TReturn> Call<TType, TReturn> (TType obj, string methodName, BindingFlags bindingFlags)
    {
      return Call<TType,TReturn> (obj, methodName, bindingFlags, null, CallingConventions.Any, null);
    }

    public static InvokeWith<TReturn> Call<TType, TReturn> (
        object obj, string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] modifiers)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);

      return new InvokeWith<TReturn> (GetMethod<TType, TReturn> (methodName, bindingFlags, binder, callingConvention, modifiers));
    }


    public static InvokeWith<TReturn> Call<TReturn> (object obj, string methodName)
    {
      return Call<TReturn> (obj, methodName, BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, null);
    }

    public static InvokeWith<TReturn> Call<TReturn> (object obj, string methodName, BindingFlags bindingFlags)
    {
      return Call<TReturn> (obj, methodName, bindingFlags, null, CallingConventions.Any, null);
    }

    public static InvokeWith<TReturn> Call<TReturn> (
        object obj, string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] modifiers)
    {
      ArgumentUtility.CheckNotNull ("obj", obj);

      return new InvokeWith<TReturn> (GetMethod<TReturn> (obj.GetType (), methodName, bindingFlags, binder, callingConvention, modifiers));
    }
  }
}
