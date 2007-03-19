using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Rubicon.Utilities;

namespace Rubicon.Reflection
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
