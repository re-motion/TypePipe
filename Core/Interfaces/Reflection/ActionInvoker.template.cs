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

namespace Remotion.Reflection
{
  // @begin-skip
  /// <summary>
  /// This interface allows invokers with fixed arguments to be returned without references to their generic argument types. 
  /// </summary>
  /// <remarks>
  /// <p>Note that casting a <see cref="T:Remotion.Reflection.ActionInvoker"/> to an interface is a boxing operation, thus creating an object on the
  /// heap and garbage collecting it later. For very performance-critical scenarios, it be better to avoid this and accept the references to 
  /// the invoker's generic argument types.</p>
  /// <p>It is recommended to wrap this interface within a <see cref="ActionInvokerWrapper"/>, because returning an interface could lead to 
  /// ambigous castings if the final call to <see cref="With{A1}"/> is missing, while using structs will usually lead to a compile-time error as 
  /// expected.</p>
  /// </remarks>
  // @end-skip
  public partial interface IActionInvoker
  {
    // @begin-skip
    void Invoke (Type[] valueTypes, object[] values);
    void Invoke (object[] values);
    // @end-skip

    // @begin-template first=1 generate=0..17 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    void With<A1> (A1 a1);
    // @end-template
  }

  // @begin-skip
  /// <summary>
  /// Used to wrap an <see cref="IActionInvoker"/> object rather than returning it directly.
  /// </summary>
  // @end-skip
  public partial struct ActionInvokerWrapper : IActionInvoker
  {
    // @begin-skip
    private readonly IActionInvoker _invoker;

    public ActionInvokerWrapper (IActionInvoker invoker)
    {
      _invoker = invoker;
    }

    public void Invoke (Type[] valueTypes, object[] values)
    {
      _invoker.Invoke (valueTypes, values);
    }

    public void Invoke (object[] values)
    {
      _invoker.Invoke (values);
    }
    // @end-skip

    // @begin-template first=1 generate=0..17 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    public void With<A1> (A1 a1)
    {
      // @replace "a<n>" ", "
      _invoker.With (a1);
    }
    // @end-template
  }
}
