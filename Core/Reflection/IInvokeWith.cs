using System;

namespace Rubicon.Reflection
{
  public interface IInvokeWith<T>
  {
    T With (Type[] valueTypes, object[] values);

    T With (object[] values);

    T With();

    T With<A1> (A1 a1);

    T With<A1, A2> (A1 a1, A2 a2);

    T With<A1, A2, A3> (A1 a1, A2 a2, A3 a3);

    T With<A1, A2, A3, A4> (A1 a1, A2 a2, A3 a3, A4 a4);

    T With<A1, A2, A3, A4, A5> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);

    T With<A1, A2, A3, A4, A5, A6> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  }
}