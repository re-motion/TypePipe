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
using System.Reflection;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  public class MethodLookupInfo : MemberLookupInfo
  {
    private static readonly ICache<Tuple<Type, string>, Delegate> s_instanceMethodCache = new InterlockedCache<Tuple<Type, string>, Delegate> ();

    public MethodLookupInfo (string name, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
        : base (name, bindingFlags, binder, callingConvention, parameterModifiers)
    {
    }

    public MethodLookupInfo (string name, BindingFlags bindingFlags)
        : base (name, bindingFlags)
    {
    }

    public MethodLookupInfo (string name)
        : base (name)
    {
    }

    public Delegate GetInstanceMethodDelegate (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      Tuple<Type, string> key = new Tuple<Type, string> (delegateType, MemberName);
      Delegate result;
      if (! s_instanceMethodCache.TryGetValue (key, out result))
      {
        result = s_instanceMethodCache.GetOrCreateValue (
            key,
            delegate
            {
              Type[] parameterTypes = GetParameterTypes (delegateType);
              if (parameterTypes.Length == 0)
                throw new InvalidOperationException ("Method call delegate must have at least one argument for the current instance ('this' in C# or 'Me' in Visual Basic).");
              Type definingType = parameterTypes[0];
              parameterTypes = ArrayUtility.Skip (parameterTypes, 1);
              MethodInfo method = definingType.GetMethod (MemberName, BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
              // TODO: verify return type
              return Delegate.CreateDelegate (delegateType, method);
            });
      }
      return result;
    }
  }
}