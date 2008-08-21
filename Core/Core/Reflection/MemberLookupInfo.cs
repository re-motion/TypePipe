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
using Remotion.Utilities;

namespace Remotion.Reflection
{
  public class MemberLookupInfo
  {
    private readonly string _memberName;
    private readonly BindingFlags _bindingFlags;
    private readonly Binder _binder;
    private readonly CallingConventions _callingConvention;
    private readonly ParameterModifier[] _parameterModifiers;

    public MemberLookupInfo (string memberName, BindingFlags bindingFlags)
        : this (memberName, bindingFlags, null, CallingConventions.Any, null)
    {
    }

    public MemberLookupInfo (string memberName)
        : this (memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
    {
    }

    public MemberLookupInfo (
        string memberName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("memberName", memberName);

      _memberName = memberName;
      _bindingFlags = bindingFlags;
      _binder = binder;
      _callingConvention = callingConvention;
      _parameterModifiers = parameterModifiers;
    }

    public string MemberName
    {
      get { return _memberName; }
    }

    public BindingFlags BindingFlags
    {
      get { return _bindingFlags; }
    }

    public Binder Binder
    {
      get { return _binder; }
    }

    public CallingConventions CallingConvention
    {
      get { return _callingConvention; }
    }

    public ParameterModifier[] ParameterModifiers
    {
      get { return _parameterModifiers; }
    }

    public Type[] GetParameterTypes (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom("delegateType", delegateType, typeof (Delegate));

      MethodInfo invokeMethod = delegateType.GetMethod ("Invoke");
      Assertion.IsNotNull (invokeMethod, "Delegate has no Invoke() method."); // according to the CLI specs, each delegate type must define this method

      return EnumerableUtility.SelectToArray<ParameterInfo, Type> (
          invokeMethod.GetParameters (),
          delegate (ParameterInfo par) { return par.ParameterType; });
    }
  }
}