namespace Rubicon
{
  public delegate void Proc();
  public delegate void Proc<A0> (A0 a0);
  public delegate void Proc<A0, A1> (A0 a0, A1 a1);
  public delegate void Proc<A0, A1, A2> (A0 a0, A1 a1, A2 a2);
  public delegate void Proc<A0, A1, A2, A3> (A0 a0, A1 a1, A2 a2, A3 a3);
  public delegate void Proc<A0, A1, A2, A3, A4> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
  public delegate void Proc<A0, A1, A2, A3, A4, A5> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
  public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9);

  public delegate R Func<R>();
  public delegate R Func<A0, R> (A0 a0);
  public delegate R Func<A0, A1, R> (A0 a0, A1 a1);
  public delegate R Func<A0, A1, A2, R> (A0 a0, A1 a1, A2 a2);
  public delegate R Func<A0, A1, A2, A3, R> (A0 a0, A1 a1, A2 a2, A3 a3);
  public delegate R Func<A0, A1, A2, A3, A4, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);
  public delegate R Func<A0, A1, A2, A3, A4, A5, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
  public delegate R Func<A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, R> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9);
}