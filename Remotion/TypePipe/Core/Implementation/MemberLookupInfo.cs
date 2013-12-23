// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Reflection;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  public class MemberLookupInfo
  {
    protected readonly DelegateFactory DelegateFactory = new DelegateFactory();

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
      return GetSignature (delegateType).Item1;
    }

    public Tuple<Type[], Type> GetSignature (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom("delegateType", delegateType, typeof (Delegate));

      return DelegateFactory.GetSignature (delegateType);
    }
  }
}
