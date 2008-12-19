// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
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
