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
  /// <summary>
  /// A function that will create a delegate to call from <see cref="FuncInvoker{TResult}"/>.
  /// </summary>
  /// <param name="delegateType"> Type of the delegate that will be created. </param>
  /// <returns> The delegate used to call the wrapped method. </returns>
  public delegate Delegate DelegateSelector (Type delegateType);
}