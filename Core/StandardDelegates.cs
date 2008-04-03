using System;
using Remotion.Utilities;
using System.ComponentModel;

namespace Remotion
{
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate void Proc ();
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate void Proc<A0> (A0 a0);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate void Proc<A0, A1> (A0 a0, A1 a1);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate void Proc<A0, A1, A2> (A0 a0, A1 a1, A2 a2);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate void Proc<A0, A1, A2, A3> (A0 a0, A1 a1, A2 a2, A3 a3);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16, A17 a17);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16, A17 a17, A18 a18);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16, A17 a17, A18 a18, A19 a19);

  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate R Func<R> ();
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate R Func<A0, R> (A0 a0);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate R Func<A0, A1, R> (A0 a0, A1 a1);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate R Func<A0, A1, A2, R> (A0 a0, A1 a1, A2 a2);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate R Func<A0, A1, A2, A3, R> (A0 a0, A1 a1, A2 a2, A3 a3);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16, A17 a17);
  [EditorBrowsable (EditorBrowsableState.Never)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16, A17 a17, A18 a18);
  [EditorBrowsable (EditorBrowsableState.Always)]
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10, A11 a11, A12 a12, A13 a13, A14 a14, A15 a15, A16 a16, A17 a17, A18 a18, A19 a19);

  public static class FuncDelegates
  {
    private static Type[] s_types = new Type[]
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
    private static Type[] s_types = new Type[]
        {
            typeof (Proc),
            typeof (Proc<>),
            typeof (Proc<,>),
            typeof (Proc<,,>),
            typeof (Proc<,,,>),
            typeof (Proc<,,,,>),
            typeof (Proc<,,,,,>),
            typeof (Proc<,,,,,,>),
            typeof (Proc<,,,,,,,>),
            typeof (Proc<,,,,,,,,>),
            typeof (Proc<,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,,,,>)
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