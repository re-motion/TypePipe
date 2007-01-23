namespace Rubicon
{
  public delegate void Proc();
  public delegate void Proc<A1> (A1 a1);
  public delegate void Proc<A1, A2> (A1 a1, A2 a2);
  public delegate void Proc<A1, A2, A3> (A1 a1, A2 a2, A3 a3);
  public delegate void Proc<A1, A2, A3, A4> (A1 a1, A2 a2, A3 a3, A4 a4);
  public delegate void Proc<A1, A2, A3, A4, A5> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
  public delegate void Proc<A1, A2, A3, A4, A5, A6> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  public delegate void Proc<A1, A2, A3, A4, A5, A6, A7> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);
  public delegate void Proc<A1, A2, A3, A4, A5, A6, A7, A8> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
  public delegate void Proc<A1, A2, A3, A4, A5, A6, A7, A8, A9> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9);
  public delegate void Proc<A1, A2, A3, A4, A5, A6, A7, A8, A9, A10> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10);

  public delegate R Func<R>();
  public delegate R Func<A1, R> (A1 a1);
  public delegate R Func<A1, A2, R> (A1 a1, A2 a2);
  public delegate R Func<A1, A2, A3, R> (A1 a1, A2 a2, A3 a3);
  public delegate R Func<A1, A2, A3, A4, R> (A1 a1, A2 a2, A3 a3, A4 a4);
  public delegate R Func<A1, A2, A3, A4, A5, R> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
  public delegate R Func<A1, A2, A3, A4, A5, A6, R> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);
  public delegate R Func<A1, A2, A3, A4, A5, A6, A7, R> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);
  public delegate R Func<A1, A2, A3, A4, A5, A6, A7, A8, R> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
  public delegate R Func<A1, A2, A3, A4, A5, A6, A7, A8, A9, R> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9);
  public delegate R Func<A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, R> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8, A9 a9, A10 a10);
}