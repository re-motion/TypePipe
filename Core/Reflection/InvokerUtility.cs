using System;
using Rubicon.Utilities;

namespace Rubicon.Reflection
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