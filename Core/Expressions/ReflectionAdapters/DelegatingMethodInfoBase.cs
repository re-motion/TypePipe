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
using System.Globalization;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions.ReflectionAdapters
{
  /// <summary>
  /// Represents a <see cref="MethodBase"/> object as a <see cref="MethodInfo"/>, delegating all calls to the inner <see cref="MethodBase"/>.
  /// </summary>
  /// <typeparam name="TMethodBase">The type of the <see cref="MethodBase"/> to wrap.</typeparam>
  public abstract class DelegatingMethodInfoBase<TMethodBase> : MethodInfo
      where TMethodBase : MethodBase
  {
    private readonly TMethodBase _innerMethod;

    protected DelegatingMethodInfoBase (TMethodBase innerMethod)
    {
      ArgumentUtility.CheckNotNull ("innerMethod", innerMethod);
      _innerMethod = innerMethod;
    }

    public abstract override Type ReturnType { get; }
    
    protected TMethodBase InnerMethod
    {
      get { return _innerMethod; }
    }

    public override Type DeclaringType
    {
      get { return _innerMethod.DeclaringType; }
    }

    public override MethodAttributes Attributes
    {
      get { return _innerMethod.Attributes; }
    }

    public override CallingConventions CallingConvention
    {
      get { return _innerMethod.CallingConvention; }
    }

    public override string Name
    {
      get { return _innerMethod.Name; }
    }

    public override Type ReflectedType
    {
      get { return _innerMethod.ReflectedType; }
    }

    public override int MetadataToken
    {
      get { return _innerMethod.MetadataToken; }
    }

    public override Module Module
    {
      get { return _innerMethod.Module; }
    }

    public override RuntimeMethodHandle MethodHandle
    {
      get { return _innerMethod.MethodHandle; }
    }

    public override MemberTypes MemberType
    {
      get { return _innerMethod.MemberType; }
    }

    public override bool ContainsGenericParameters
    {
      get { return _innerMethod.ContainsGenericParameters; }
    }

    public override bool IsGenericMethod
    {
      get { return _innerMethod.IsGenericMethod; }
    }

    public override bool IsGenericMethodDefinition
    {
      get { return _innerMethod.IsGenericMethodDefinition; }
    }

    public override Type[] GetGenericArguments ()
    {
      return _innerMethod.GetGenericArguments ();
    }

    public override MethodBody GetMethodBody ()
    {
      return _innerMethod.GetMethodBody ();
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _innerMethod.GetParameters();
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return _innerMethod.GetCustomAttributes (inherit);
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      return _innerMethod.IsDefined (attributeType, inherit);
    }

    public override MethodImplAttributes GetMethodImplementationFlags ()
    {
      return _innerMethod.GetMethodImplementationFlags();
    }

    public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      return _innerMethod.Invoke (obj, invokeAttr, binder, parameters, culture);
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      return _innerMethod.GetCustomAttributes (attributeType, inherit);
    }

    public override string ToString ()
    {
      return _innerMethod.ToString();
    }
  }
}