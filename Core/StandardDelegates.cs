using System;
using Remotion.Utilities;

namespace Remotion
{
  public static class FuncDelegates
  {
    private static readonly Type[] s_types = new Type[]
        {
            typeof (Func<>),
            typeof (Func<,>),
            typeof (Func<,,>),
            typeof (Func<,,,>),
            typeof (Func<,,,,>),
            typeof (Func<,,,,,>),
            typeof (Func<,,,,,,>),
            typeof (Func<,,,,,,,>),
            typeof (Func<,,,,,,,,>),
            typeof (Func<,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,,,,,>)
        };

    public static int MaxArguments 
    {
      get { return s_types.Length - 1; }
    }

    public static Type GetOpenType (int arguments)
    {
      if (arguments > MaxArguments)
        throw new ArgumentOutOfRangeException ("arguments");

      return s_types[arguments];
    }

    public static Type MakeClosedType (Type returnType, params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNullOrItemsNull ("argumentTypes", argumentTypes);
      if (argumentTypes.Length > MaxArguments)
        throw new ArgumentOutOfRangeException ("argumentTypes");

      Type[] typeArguments = ArrayUtility.Combine (argumentTypes, returnType);
      return GetOpenType (argumentTypes.Length).MakeGenericType (typeArguments);
    }
  }

  public static class ProcDelegates
  {
    private static readonly Type[] s_types = new Type[]
        {
            typeof (Proc),
            typeof (Proc<>),
            typeof (Proc<,>),
            typeof (Proc<,,>),
            typeof (Proc<,,,>),
            typeof (Proc<,,,,>),
            typeof (Proc<,,,,,>),
            typeof (Proc<,,,,,,>),
            typeof (Proc<,,,,,,,>),
            typeof (Proc<,,,,,,,,>),
            typeof (Proc<,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,,,>),
            typeof (Proc<,,,,,,,,,,,,,,,,,,,>)
        };

    public static int MaxArguments
    {
      get { return s_types.Length - 1; }
    }

    public static Type GetOpenType (int arguments)
    {
      if (arguments > MaxArguments)
        throw new ArgumentOutOfRangeException ("arguments");

      return s_types[arguments];
    }

    public static Type MakeClosedType (params Type[] argumentTypes)
    {
      ArgumentUtility.CheckNotNullOrItemsNull ("argumentTypes", argumentTypes);
      if (argumentTypes.Length > MaxArguments)
        throw new ArgumentOutOfRangeException ("argumentTypes");

      return GetOpenType (argumentTypes.Length).MakeGenericType (argumentTypes);
    }
  }
}