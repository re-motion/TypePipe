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
