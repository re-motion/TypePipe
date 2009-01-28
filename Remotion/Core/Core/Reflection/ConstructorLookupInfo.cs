// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
    private static readonly ICache<object, Delegate> s_delegateCache = new InterlockedCache<object, Delegate> ();
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

      object key = GetCacheKey(delegateType);
      Delegate result;
      if (! s_delegateCache.TryGetValue (key, out result))
        result = s_delegateCache.GetOrCreateValue (key, arg => CreateDelegate (delegateType));
      return result;
    }

    protected virtual object GetCacheKey (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      return new Tuple<Type, Type> (_definingType, delegateType);
    }

    protected virtual Delegate CreateDelegate (Type delegateType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));

      Type[] parameterTypes = GetParameterTypes (delegateType);
      if (_definingType.IsValueType && parameterTypes.Length == 0)
        return CreateValueTypeDefaultDelegate (_definingType, delegateType);

      ConstructorInfo ctor = GetConstructor(parameterTypes);
      return CreateDelegate (ctor, delegateType);
    }

    protected Delegate CreateDelegate (ConstructorInfo ctor, Type delegateType)
    {
      ArgumentUtility.CheckNotNull ("ctor", ctor);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("delegateType", delegateType, typeof (Delegate));


      ParameterInfo[] parameters = ctor.GetParameters ();
      Type type = ctor.DeclaringType;
      DynamicMethod method = new DynamicMethod ("ConstructorWrapper", type, EmitUtility.GetParameterTypes (parameters), type);
      ILGenerator ilgen = method.GetILGenerator ();

      EmitUtility.PushParameters (ilgen, parameters.Length);
      ilgen.Emit (OpCodes.Newobj, ctor);
      ilgen.Emit (OpCodes.Ret);

      try
      {
        return method.CreateDelegate (delegateType);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException ("Parameters of constructor and delegate type do not match.", ex);
      }
    }

    /// <summary>
    /// Since value types do not have default constructors, an activation with zero parameters must create the object with the initobj IL opcode.
    /// </summary>
    private Delegate CreateValueTypeDefaultDelegate (Type type, Type delegateType)
    {
      DynamicMethod method = new DynamicMethod ("ConstructorWrapper", type, Type.EmptyTypes, type);
      ILGenerator ilgen = method.GetILGenerator ();

      ilgen.DeclareLocal (type);
      ilgen.Emit (OpCodes.Ldloca_S, 0);     // load address of local variable
      ilgen.Emit (OpCodes.Initobj, type);   // initialize that object with default value
      ilgen.Emit (OpCodes.Ldloc_0);         // load local variable value
      ilgen.Emit (OpCodes.Ret);             // and return it

      return method.CreateDelegate (delegateType);
    }

    public object DynamicInvoke (Type[] parameterTypes, object[] parameterValues)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("parameterValues", parameterValues);

      var ctor = GetConstructor (parameterTypes);
      return ctor.Invoke (parameterValues);
    }

    private ConstructorInfo GetConstructor (Type[] parameterTypes)
    {
      ConstructorInfo ctor = _definingType.GetConstructor (BindingFlags, Binder, CallingConvention, parameterTypes, ParameterModifiers);
      if (ctor == null)
      {
        string message = string.Format ("Type {0} does not contain a constructor with the following arguments types: {1}.",
                                        _definingType.FullName, SeparatedStringBuilder.Build (", ", parameterTypes, t => t.FullName));
        throw new MissingMethodException (message);
      }
      return ctor;
    }
  }
}
