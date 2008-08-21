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

namespace Remotion.Reflection
{
  public static class MethodCaller
  {
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

    public static ActionInvoker CallAction (string methodName)
    {
      return new ActionInvoker (new MethodLookupInfo (methodName).GetInstanceMethodDelegate);
    }

    public static ActionInvoker CallAction (
        string methodName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new ActionInvoker (new MethodLookupInfo (methodName, bindingFlags, binder, callingConvention, parameterModifiers).GetInstanceMethodDelegate);
    }

    public static ActionInvoker CallAction (
        string methodName, BindingFlags bindingFlags)
    {
      return new ActionInvoker (new MethodLookupInfo (methodName, bindingFlags).GetInstanceMethodDelegate);
    }
  }
}
