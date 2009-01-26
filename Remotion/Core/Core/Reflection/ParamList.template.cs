// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// This framework is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this framework; if not, see http://www.gnu.org/licenses.
// 
using System;

namespace Remotion.Reflection
{
  // @begin-skip
  /// <summary>
  /// Represents a strongly typed list of parameters to be passed to a function or action delegate.
  /// </summary>
  // @end-skip
  public abstract partial class ParamList
  {
    // @begin-template first=1 generate=0..17 suppressTemplate=true
    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    // @replace "a<n>" ", " " " " "
    /// <summary>
    /// Creates a strongly typed list of parameters to be passed to a function or action.
    /// </summary>
    /// <returns>A <see cref="ParamList"/> encapsulating the passed parameters.</returns>
    public static ParamList Create<A1> (A1 a1) { return new ParamListImplementation<A1> ( a1 ); }
    // @end-template
    // @begin-skip
    /// <summary>
    /// Gets the type of <see cref="Func{TResult}"/> delegates supported by this <see cref="ParamList"/> instance.
    /// </summary>
    /// <value>The function delegate type supported by this <see cref="ParamList"/> instance, or <see langword="null"/> if the instance supports
    /// more than one delegate types.</value>
    public abstract Type FuncType { get; }
    /// <summary>
    /// Gets the type of <see cref="Action"/> delegates supported by this <see cref="ParamList"/> instance.
    /// </summary>
    /// <value>The action delegate type supported by this <see cref="ParamList"/> instance, or <see langword="null"/> if the instance supports
    /// more than one delegate types.</value>
    public abstract Type ActionType { get; }

    /// <summary>
    /// Executes the given action delegate, passing in the parameters encapsulated by this <see cref="ParamList"/>.
    /// </summary>
    /// <param name="action">The action to be executed. This delegate must match <see cref="ActionType"/>. If <see cref="ActionType"/> is null,
    /// it must match the types returned by <see cref="GetParameterTypes"/>.</param>
    public abstract void ExecuteAction (Delegate action);
    /// <summary>
    /// Executes the given function delegate, passing in the parameters encapsulated by this <see cref="ParamList"/>.
    /// </summary>
    /// <param name="action">The function to be executed. This delegate must match <see cref="FuncType"/>. If <see cref="FuncType"/> is null,
    /// it must match the types returned by <see cref="GetParameterTypes"/>, plus <see cref="System.Object"/> as the return type.</param>
    public abstract object ExecuteFunc (Delegate action);

    /// <summary>
    /// Gets the parameter types of the parameters encapsulated by this <see cref="ParamList"/>.
    /// </summary>
    /// <returns>The parameter types.</returns>
    public abstract Type[] GetParameterTypes ();
    /// <summary>
    /// Gets the parameter values of the parameters encapsulated by this <see cref="ParamList"/>.
    /// </summary>
    /// <returns>The parameter values.</returns>
    public abstract object[] GetParameterValues ();
    // @end-skip
  }
}