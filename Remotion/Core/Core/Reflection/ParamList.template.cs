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
    public static ParamList Create<A1> (A1 a1) { return new ParamListImplementation<A1> ( a1 ); }
    // @end-template
    // @begin-skip
    public abstract Type FuncType { get; }
    public abstract Type ActionType { get; }

    public abstract void ExecuteAction (Delegate action);
    public abstract object ExecuteFunc (Delegate action);

    public abstract Type[] GetParameterTypes ();
    public abstract object[] GetParameterValues ();
    // @end-skip
  }
}