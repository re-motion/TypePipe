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
using System.Collections.Generic;
using System.Text;
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
