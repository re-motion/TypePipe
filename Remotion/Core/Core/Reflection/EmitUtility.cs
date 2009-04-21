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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Provides helper methods for Reflection.Emit.
  /// </summary>
  public static class EmitUtility
  {
    public static Type[] GetParameterTypes (ParameterInfo[] parameters)
    {
      // return SelectToArray<ParameterInfo,Type> (parameters, delegate (ParameterInfo p) { return p.ParameterType; });

      IEnumerable<Type> res = EnumerableUtility.Select<ParameterInfo,Type> (parameters, delegate (ParameterInfo p) { return p.ParameterType; });
      List<Type> l = new List<Type> (res);
      return l.ToArray ();
    }

    public static void PushParameters (ILGenerator ilgen, int numParams)
    {
      for (int i = 0; i < numParams; ++i)
        ilgen.Emit (OpCodes.Ldarg, i);
    }
  }
}
