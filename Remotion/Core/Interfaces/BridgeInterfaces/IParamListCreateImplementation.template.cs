// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Implementation;
using Remotion.Reflection;

namespace Remotion.BridgeInterfaces
{
  // @begin-skip
  [ConcreteImplementation (
      "Remotion.BridgeImplementations.ParamListCreateImplementation, Remotion, Version=<version>, Culture=neutral, PublicKeyToken=<publicKeyToken>")]
  // @end-skip
  public partial interface IParamListCreateImplementation
  {
    // @begin-skip
    ParamList GetEmpty ();
    // @end-skip

    // @begin-template first=1 generate=1..20 suppressTemplate=true
    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    ParamList Create<A1> (A1 a1);
    // @end-template

    // @begin-skip
    ParamList CreateDynamic (Type[] parameterTypes, object[] parameterValues);
    // @end-skip
  }
}