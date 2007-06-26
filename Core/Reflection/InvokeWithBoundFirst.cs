using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Utilities;
using Rubicon.Collections;

namespace Rubicon.Reflection
{
  public class InvokeWithBoundFirst<T, TFirstArgument>: IInvokeWith<T>
  {
    private GetDelegateWith<T> _getDelegateWith;
    private TFirstArgument _firstArgument;

    public InvokeWithBoundFirst (GetDelegateWith<T> getDelegateWith, TFirstArgument firstArgument)
    {
      _getDelegateWith = getDelegateWith;
      _firstArgument = firstArgument;
    }

    public T With (Type[] valueTypes, object[] values)
    {
      if (valueTypes.Length != values.Length)
        throw new InvalidOperationException ("Arguments must be of same size.");
      
      ArgumentUtility.CheckNotNull ("valueTypes", valueTypes);
      ArgumentUtility.CheckNotNull ("values", values);

      valueTypes = Prepend (typeof (TFirstArgument), valueTypes);
      values = Prepend<object> (_firstArgument, values);

      #if DEBUG
        for (int i = 0; i < values.Length; ++i)
          Assertion.Assert (values[i] == null || valueTypes[i].IsAssignableFrom (values[i].GetType ()), "Incompatible types in InvokeWith.With() at array index " + (i-1).ToString () + ".");
      #endif

      return (T) _getDelegateWith.With (valueTypes).DynamicInvoke (values);
    }

    public T With (object[] values)
    {
      ArgumentUtility.CheckNotNull ("values", values);

      values = Prepend<object> (_firstArgument, values);

      Type[] valueTypes = new Type[values.Length];
      for (int i = 0; i < values.Length; ++i)
      {
        object value = values[i];
        valueTypes[i] = (value != null) ? value.GetType () : typeof (object);
      }

      return (T) _getDelegateWith.With (valueTypes).DynamicInvoke (values);
    }

    private T1[] Prepend<T1> (T1 newValue, T1[] oldValues)
    {
      T1[] newValues = new T1[oldValues.Length + 1];
      newValues[0] = newValue;
      Array.Copy (oldValues, 0, newValues, 1, oldValues.Length);
      return newValues;
    }

    public T With ()
    {
      return _getDelegateWith.With<TFirstArgument> () (_firstArgument);
    }
    public T With<A1> (A1 a1)
    {
      return _getDelegateWith.With<TFirstArgument, A1> () (_firstArgument, a1);
    }
    public T With<A1, A2> (A1 a1, A2 a2)
    {
      return _getDelegateWith.With<TFirstArgument, A1, A2> () (_firstArgument, a1, a2);
    }
    public T With<A1, A2, A3> (A1 a1, A2 a2, A3 a3)
    {
      return _getDelegateWith.With<TFirstArgument, A1, A2, A3> () (_firstArgument, a1, a2, a3);
    }
    public T With<A1, A2, A3, A4> (A1 a1, A2 a2, A3 a3, A4 a4)
    {
      return _getDelegateWith.With<TFirstArgument, A1, A2, A3, A4> () (_firstArgument, a1, a2, a3, a4);
    }
    public T With<A1, A2, A3, A4, A5> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
    {
      return _getDelegateWith.With<TFirstArgument, A1, A2, A3, A4, A5> () (_firstArgument, a1, a2, a3, a4, a5);
    }
    public T With<A1, A2, A3, A4, A5, A6> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6)
    {
      return _getDelegateWith.With<TFirstArgument, A1, A2, A3, A4, A5, A6> () (_firstArgument, a1, a2, a3, a4, a5, a6);
    }
  }  
}
