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

namespace Remotion.Reflection
{
  internal static class InvokerUtility
  {
    public static void CheckInvokeArguments (Type[] valueTypes, object[] values)
    {
      ArgumentUtility.CheckNotNull ("valueTypes", valueTypes);
      ArgumentUtility.CheckNotNull ("values", values);
      if (valueTypes.Length != values.Length)
        throw new InvalidOperationException ("Arguments must be of same size.");

#if DEBUG
      for (int i = 0; i < values.Length; ++i)
      {
        Assertion.IsTrue(
            values[i] == null || valueTypes[i].IsAssignableFrom (values[i].GetType()), "Incompatible types at array index " + i + ".");
      }
#endif
    }

    public static Type[] GetValueTypes (object[] values)
    {
      ArgumentUtility.CheckNotNull ("values", values);

      Type[] valueTypes = new Type[values.Length];
      for (int i = 0; i < values.Length; ++i)
      {
        object value = values[i];
        valueTypes[i] = (value != null) ? value.GetType() : typeof (object);
      }

      return valueTypes;
    }
  }
}
