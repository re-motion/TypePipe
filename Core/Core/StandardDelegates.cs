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
using Remotion.Utilities;

namespace Remotion
{
  public static class FuncDelegates
  {
    private static readonly Type[] s_types = new Type[]
        {
            typeof (Func<>),
            typeof (Func<,>),
            typeof (Func<,,>),
            typeof (Func<,,,>),
            typeof (Func<,,,,>),
            typeof (Func<,,,,,>),
            typeof (Func<,,,,,,>),
            typeof (Func<,,,,,,,>),
            typeof (Func<,,,,,,,,>),
            typeof (Func<,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,,,,>)
        };

    public static int MaxArguments 
    {
      get { return s_types.Length - 1; }
    }

    public static Type GetOpenType (int arguments)
    {
      if (arguments > MaxArguments)
        throw new ArgumentOutOfRangeException ("arguments");

      return s_types[arguments];
    }

    public static Type MakeClosedType (Type returnType, params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNullOrItemsNull ("argumentTypes", argumentTypes);
      if (argumentTypes.Length > MaxArguments)
        throw new ArgumentOutOfRangeException ("argumentTypes");

      Type[] typeArguments = ArrayUtility.Combine (argumentTypes, returnType);
      return GetOpenType (argumentTypes.Length).MakeGenericType (typeArguments);
    }
  }

  public static class ProcDelegates
  {
    private static readonly Type[] s_types = new Type[]
        {
            typeof (Action),
            typeof (Action<>),
            typeof (Action<,>),
            typeof (Action<,,>),
            typeof (Action<,,,>),
            typeof (Action<,,,,>),
            typeof (Action<,,,,,>),
            typeof (Action<,,,,,,>),
            typeof (Action<,,,,,,,>),
            typeof (Action<,,,,,,,,>),
            typeof (Action<,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,,,,,,>)
        };

    public static int MaxArguments
    {
      get { return s_types.Length - 1; }
    }

    public static Type GetOpenType (int arguments)
    {
      if (arguments > MaxArguments)
        throw new ArgumentOutOfRangeException ("arguments");

      return s_types[arguments];
    }

    public static Type MakeClosedType (params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNullOrItemsNull ("argumentTypes", argumentTypes);
      if (argumentTypes.Length > MaxArguments)
        throw new ArgumentOutOfRangeException ("argumentTypes");

      return GetOpenType (argumentTypes.Length).MakeGenericType (argumentTypes);
    }
  }
}
