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
using Remotion.BridgeInterfaces;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.BridgeImplementations
{
  public partial class ParamListCreateImplementation
    // @begin-skip
      : IParamListCreateImplementation
    // @end-skip
  {
    // @begin-skip
    private readonly ParamList _empty = new ParamListImplementation ();
    
    public ParamList GetEmpty ()
    {
      return _empty;
    }
    // @end-skip

    // @begin-template first=1 generate=1..20 suppressTemplate=true
    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    // @replace "a<n>" ", " " " " "
    public ParamList Create<A1> (A1 a1) { return new ParamListImplementation<A1> ( a1 ); }
    // @end-template

    // @begin-skip
    /// <summary>
    /// Creates a strongly typed list of parameters to be passed to a function or action.
    /// </summary>
    /// <returns>A <see cref="ParamList"/> encapsulating the passed parameters.</returns>
    public ParamList CreateDynamic (Type[] parameterTypes, object[] parameterValues)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("parameterValues", parameterValues);

      return new DynamicParamList (parameterTypes, parameterValues);
    }

    //@end-skip
  }
}