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

  public static class ActionDelegates
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

      if (argumentTypes.Length == 0)
        return typeof (Action);

      return GetOpenType (argumentTypes.Length).MakeGenericType (argumentTypes);
    }
  }
}
