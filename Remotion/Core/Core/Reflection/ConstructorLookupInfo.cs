// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Collections;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  public class ConstructorLookupInfo: MemberLookupInfo, IConstructorLookupInfo
  {
    private static readonly ICache<object, Delegate> s_delegateCache = CacheFactory.CreateWithLocking<object, Delegate>();

    private readonly IConstructorDelegateFactory _constructorDelegateFactory = new ConstructorDelegateFactory();
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
      var returnType = delegateSignature.Item2;

      if (_definingType.IsValueType && parameterTypes.Length == 0)
        return CreateValueTypeDefaultDelegate (_definingType, delegateType, returnType);

      ConstructorInfo ctor = GetConstructor(parameterTypes);
      return _constructorDelegateFactory.CreateConstructorCall (ctor, delegateType);
    }

    protected virtual ConstructorInfo GetConstructor (Type[] parameterTypes)
    {
      ConstructorInfo ctor = _definingType.GetConstructor (BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
      if (ctor == null)
      {
        string message = string.Format ("Type '{0}' does not contain a constructor with the following arguments types: {1}.",
                                        _definingType, SeparatedStringBuilder.Build (", ", parameterTypes));
        throw new MissingMethodException (message);
      }
      return ctor;
    }

    /// <summary>
    /// Since value types do not have default constructors, an activation with zero parameters must create the object with the initobj IL opcode.
    /// </summary>
    private Delegate CreateValueTypeDefaultDelegate (Type type, Type delegateType, Type returnType)
    {
      var method = new DynamicMethod ("ConstructorWrapper", returnType, Type.EmptyTypes, type);
      ILGenerator ilgen = method.GetILGenerator ();

      var localBuilder = ilgen.DeclareLocal (type);
      ilgen.Emit (OpCodes.Ldloca_S, localBuilder);     // load address of local variable
      ilgen.Emit (OpCodes.Initobj, type);   // initialize that object with default value
      ilgen.Emit (OpCodes.Ldloc_0);         // load local variable value
      if (!returnType.IsValueType)
        ilgen.Emit (OpCodes.Box, type);       // box it, if required
      ilgen.Emit (OpCodes.Ret);             // and return it

      return method.CreateDelegate (delegateType);
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
