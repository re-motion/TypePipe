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

namespace Remotion.Reflection
{
  /// <summary>
  /// A function that will create a delegate to call from <see cref="FuncInvoker{TResult}"/>.
  /// </summary>
  /// <param name="delegateType"> Type of the delegate that will be created. </param>
  /// <returns> The delegate used to call the wrapped method. </returns>
  public delegate Delegate DelegateSelector (Type delegateType);

  public class MemberLookupInfo
  {
    private string _memberName;
    private BindingFlags _bindingFlags;
    private Binder _binder;
    private CallingConventions _callingConvention;
    private ParameterModifier[] _parameterModifiers;

    public MemberLookupInfo (
        string memberName, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      _memberName = memberName;
      _bindingFlags = bindingFlags;
      _binder = binder;
      _callingConvention = callingConvention;
      _parameterModifiers = parameterModifiers;
    }

    public MemberLookupInfo (string memberName, BindingFlags bindingFlags)
      : this (memberName, bindingFlags, null, CallingConventions.Any, null)
    {
    }

    public MemberLookupInfo (string memberName)
      : this (memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
    {
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
  }
}