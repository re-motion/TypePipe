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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  public class ConstructorLookupInfo: MemberLookupInfo, IConstructorLookupInfo
  {
    private static readonly ICache<object, Delegate> s_delegateCache = CacheFactory.CreateWithLocking<object, Delegate>();

    private readonly Type _definingType;

    public ConstructorLookupInfo (Type definingType, BindingFlags bindingFlags)
        : this (definingType, bindingFlags, null, CallingConventions.Any, null)
    {
    }

    public ConstructorLookupInfo (Type definingType)
        : this (definingType, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, null)
    {
    }

    public ConstructorLookupInfo (
        Type definingType, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
        : base (".ctor", bindingFlags, binder, callingConvention, parameterModifiers)
    {
      ArgumentUtility.CheckNotNull ("definingType", definingType);

      _definingType = definingType;
    }

    public Type DefiningType
    {
      get { return _definingType; }
    }

    public virtual Delegate GetDelegate (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom("delegateType", delegateType, typeof (Delegate));

      CheckNotAbstract();

      object key = GetCacheKey(delegateType);
      Delegate result;
      if (! s_delegateCache.TryGetValue (key, out result))
        result = s_delegateCache.GetOrCreateValue (key, arg => CreateDelegate (delegateType));
      return result;
    }

    public object DynamicInvoke (Type[] parameterTypes, object[] parameterValues)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("parameterValues", parameterValues);

      CheckNotAbstract();

      // For value types' default ctors, there is no ConstructorInfo, so just use Activator instead.
      if (_definingType.IsValueType && parameterTypes.Length == 0)
        return Activator.CreateInstance (_definingType);

      // For other cases, don't use Activator, since we want to specify the parameter types.
      var ctor = GetConstructor (parameterTypes);
      return ctor.Invoke (parameterValues);
    }

    protected virtual object GetCacheKey (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      return new Tuple<Type, Type> (_definingType, delegateType);
    }

    protected virtual Delegate CreateDelegate (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      var delegateSignature = GetSignature (delegateType);
      var parameterTypes = delegateSignature.Item1;

      // Value types do not have default constructors.
      if (_definingType.IsValueType && parameterTypes.Length == 0)
        return DelegateFactory.CreateDefaultConstructorCall (_definingType, delegateType);

      ConstructorInfo ctor = GetConstructor (parameterTypes);
      return DelegateFactory.CreateConstructorCall (ctor, delegateType);
    }

    protected virtual ConstructorInfo GetConstructor (Type[] parameterTypes)
    {
      ConstructorInfo ctor = _definingType.GetConstructor (BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
      if (ctor == null)
      {
        string message = string.Format ("Type '{0}' does not contain a constructor with the following arguments types: {1}.",
                                        _definingType, string.Join (", ", (IEnumerable<Type>) parameterTypes));
        throw new MissingMethodException (message);
      }
      return ctor;
    }

    private void CheckNotAbstract ()
    {
      if (_definingType.IsAbstract)
      {
        var message = string.Format ("Cannot create an instance of '{0}' because it is an abstract type.", _definingType);
        throw new InvalidOperationException (message);
      }
    }
  }
}
