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

namespace Remotion.Reflection
{
  // @begin-skip
  /// <summary>
  /// This interface allows invokers with fixed arguments to be returned without references to their generic argument types. 
  /// </summary>
  /// <remarks>
  /// <p>Note that casting a <see cref="T:Remotion.Reflection.FuncInvoker"/> struct to an interface is a boxing operation, thus creating an object on the
  /// heap and garbage collecting it later. For very performance-critical scenarios, it be better to avoid this and accept the references to 
  /// the invoker's generic argument types.</p>
  /// <p>It is recommended to wrap this interface within a <see cref="FuncInvokerWrapper{TResult}"/>, because returning an interface could lead to 
  /// ambigous castings if the final call to <see cref="With{A1}"/> is missing, while using structs will usually lead to a compile-time error as 
  /// expected.</p>
  /// </remarks>
  /// <typeparam name="TResult"> Return type of the method that will be invoked. </typeparam>
  // @end-skip
  public partial interface IFuncInvoker<TResult>
  {
    // @begin-skip
    TResult Invoke (Type[] valueTypes, object[] values);
    TResult Invoke (object[] values);
    // @end-skip

    // @begin-template first=1 generate=0..17 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    TResult With<A1> (A1 a1);
    // @end-template
  }

  // @begin-skip
  /// <summary>
  /// Used to wrap an <see cref="IFuncInvoker{TResult}"/> object rather than returning it directly.
  /// </summary>
  /// <typeparam name="TResult"> Return type of the method that will be invoked. </typeparam>
  // @end-skip
  public partial struct FuncInvokerWrapper<TResult> : IFuncInvoker<TResult>
  {
    // @begin-skip
    private readonly IFuncInvoker<TResult> _invoker;
    private readonly Func<TResult, TResult> _afterAction;

    public FuncInvokerWrapper (IFuncInvoker<TResult> invoker)
        : this (invoker, null)
    {
    }

    public FuncInvokerWrapper (IFuncInvoker<TResult> invoker, Func<TResult, TResult> afterAction)
    {
      _invoker = invoker;
      _afterAction = afterAction;
    }

    public TResult Invoke (Type[] valueTypes, object[] values)
    {
      return PerformAfterAction (_invoker.Invoke (valueTypes, values));      
    }

    public TResult Invoke (object[] values)
    {
      return PerformAfterAction (_invoker.Invoke (values));
    }

    private TResult PerformAfterAction (TResult result)
    {
      if (_afterAction != null)
        result = _afterAction (result);
      return result;
    }

    // @end-skip

    // @begin-template first=1 generate=0..17 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    public TResult With<A1> (A1 a1)
    {
      // @replace "a<n>" ", "
      return PerformAfterAction (_invoker.With (a1));
    }

    // @end-template
  }
}
