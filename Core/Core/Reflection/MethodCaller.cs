/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Reflection;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  using CacheKey = Collections.Tuple<Type, string>; // delegate type, member name (reflected type is the delegate's first parameter type)

  public static class MethodCaller
  {
    public class MethodLookupInfo : MemberLookupInfo
    {
      public MethodLookupInfo (string name, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
          : base (name, bindingFlags, binder, callingConvention, parameterModifiers)
      {
      }

      public MethodLookupInfo (string name, BindingFlags bindingFlags)
          : base (name, bindingFlags)
      {
      }

      public MethodLookupInfo (string name)
          : base (name)
      {
      }

      public Delegate GetInstanceMethodDelegate (Type delegateType)
      {
        CacheKey key = new CacheKey (delegateType, MemberName);
        Delegate result;
        if (! s_instanceMethodCache.TryGetValue (key, out result))
        {
          result = s_instanceMethodCache.GetOrCreateValue (
              key,
              delegate
              {
                Type[] parameterTypes = ConstructorWrapper.GetParameterTypes (delegateType);
                if (parameterTypes.Length == 0)
                  throw new InvalidOperationException ("Method call delegate must have at least one argument for the current instance ('this' in C# or 'Me' in Visual Basic).");
                Type definingType = parameterTypes[0];
                parameterTypes = ArrayUtility.Skip (parameterTypes, 1);
                MethodInfo method = definingType.GetMethod (MemberName, BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
                // TODO: verify return type
                return Delegate.CreateDelegate (delegateType, method);
              });
        }
        return result;
      }

      public Delegate GetInstancePropertyGetMethodDelegate (Type delegateType)
      {
        CacheKey key = new CacheKey (delegateType, MemberName);
        Delegate result;
        if (!s_instancePropertyGetMethodCache.TryGetValue (key, out result))
        {
          result = s_instancePropertyGetMethodCache.GetOrCreateValue (
              key,
              delegate
              {
                Type[] parameterTypes = ConstructorWrapper.GetParameterTypes (delegateType);
                if (parameterTypes.Length == 0)
                  throw new InvalidOperationException ("Method call delegate must have at least one argument for the current instance ('this' in C# or 'Me' in Visual Basic).");
                Type definingType = parameterTypes[0];
                parameterTypes = ArrayUtility.Skip (parameterTypes, 1);
                MethodInfo method = definingType.GetMethod (MemberName, BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
                // TODO: verify return type
                return Delegate.CreateDelegate (delegateType, method);
              });
        }
        return result;
      }
    }

    public class PropertyLookupInfo : MemberLookupInfo
    {
      public PropertyLookupInfo (string name, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
        : base (name, bindingFlags, binder, callingConvention, parameterModifiers)
      {
      }

      public PropertyLookupInfo (string name, BindingFlags bindingFlags)
        : base (name, bindingFlags)
      {
      }

      public PropertyLookupInfo (string name)
        : base (name)
      {
      }

      public Delegate GetInstanceMethodDelegate (Type delegateType)
      {
        CacheKey key = new CacheKey (delegateType, MemberName);
        Delegate result;
        if (!s_instanceMethodCache.TryGetValue (key, out result))
        {
          result = s_instanceMethodCache.GetOrCreateValue (
              key,
              delegate
              {
                Type[] parameterTypes = ConstructorWrapper.GetParameterTypes (delegateType);
                if (parameterTypes.Length == 0)
                  throw new InvalidOperationException ("Method call delegate must have at least one argument for the current instance ('this' in C# or 'Me' in Visual Basic).");
                Type definingType = parameterTypes[0];
                parameterTypes = ArrayUtility.Skip (parameterTypes, 1);
                MethodInfo method = definingType.GetMethod (MemberName, BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
                // TODO: verify return type
                return Delegate.CreateDelegate (delegateType, method);
              });
        }
        return result;
      }

      public Delegate GetInstancePropertyGetMethodDelegate (Type delegateType)
      {
        CacheKey key = new CacheKey (delegateType, MemberName);
        Delegate result;
        if (!s_instancePropertyGetMethodCache.TryGetValue (key, out result))
        {
          result = s_instancePropertyGetMethodCache.GetOrCreateValue (
              key,
              delegate
              {
                Type[] parameterTypes = ConstructorWrapper.GetParameterTypes (delegateType);
                if (parameterTypes.Length == 0)
                  throw new InvalidOperationException ("Method call delegate must have at least one argument for the current instance ('this' in C# or 'Me' in Visual Basic).");
                Type definingType = parameterTypes[0];
                parameterTypes = ArrayUtility.Skip (parameterTypes, 1);
                MethodInfo method = definingType.GetMethod (MemberName, BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
                // TODO: verify return type
                return Delegate.CreateDelegate (delegateType, method);
              });
        }
        return result;
      }
    }

    private static ICache<CacheKey, Delegate> s_instanceMethodCache = new InterlockedCache<CacheKey, Delegate> ();
    private static ICache<CacheKey, Delegate> s_instancePropertyGetMethodCache = new InterlockedCache<CacheKey, Delegate> ();

    public static FuncInvoker<TReturn> CallFunc<TReturn> (string methodName)
    {
      return new FuncInvoker<TReturn> (new MethodLookupInfo (methodName).GetInstanceMethodDelegate);
    }

    public static FuncInvoker<TReturn> CallFunc<TReturn> (
        string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new FuncInvoker<TReturn> (new MethodLookupInfo (methodName, bindingFlags, binder, callingConvention, parameterModifiers).GetInstanceMethodDelegate);
    }

    public static FuncInvoker<TReturn> CallFunc<TReturn> (
        string methodName, BindingFlags bindingFlags)
    {
      return new FuncInvoker<TReturn> (new MethodLookupInfo (methodName, bindingFlags).GetInstanceMethodDelegate);
    }

    public static ProcInvoker CallProc (string methodName)
    {
      return new ProcInvoker (new MethodLookupInfo (methodName).GetInstanceMethodDelegate);
    }

    public static ProcInvoker CallProc (
        string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new ProcInvoker (new MethodLookupInfo (methodName, bindingFlags, binder, callingConvention, parameterModifiers).GetInstanceMethodDelegate);
    }

    public static ProcInvoker CallProc (
        string methodName, BindingFlags bindingFlags)
    {
      return new ProcInvoker (new MethodLookupInfo (methodName, bindingFlags).GetInstanceMethodDelegate);
    }
  }

  //public static class PropertyAccessor
  //{
  //  private static ICache<CacheKey, Delegate> s_getterCache = new InterlockedCache<CacheKey, Delegate> ();

  //  public static Func<TClass, TValue> PropertyGetter<TClass, TValue> (string propertyName)
  //  {
  //    Delegate result;
  //    CacheKey key = new CacheKey (typeof (TClass), propertyName);
  //    if (!s_getterCache.TryGetValue (key, out result))
  //    {
  //      result = s_getterCache.GetOrCreateValue (
  //          key,
  //          delegate
  //          {
  //            PropertyInfo property = typeof (TClass).GetProperty (propertyName);
  //            MethodInfo method = property.GetGetMethod ();
  //            return Delegate.CreateDelegate (typeof (Func<TClass, TValue>), method);
  //          });
  //    }
  //    return (Func<TClass, TValue>) result;
  //  }
  //}
}
