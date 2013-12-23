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
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Create an instance of a known type using fixed parameter types for the constructor.
  /// </summary>
  /// <remarks>
  /// While <see cref="Activator.CreateInstance(Type,object[])"/> uses the types of passed arguments to select the correct constructor, this class
  /// uses the types of the expressions you use at compile time. Use the following code to create an instance of a class called MyClass using a 
  /// constructor that has an argument of type string:
  /// <code>
  ///   ParameterType p;
  ///   YourClass obj = TypesafeActivator.CreateInstance&lt;YourClass&gt;().With (p);
  /// </code>
  /// This code always selects the constructor that accepts an argument of type ParameterType, even if the value passed is null or an instance
  /// of a subclass of ParameterType.
  /// </remarks>
  [Obsolete ("Use System.Activator instead. (Version 1.15.7.0)")]
  public static class TypesafeActivator
  {
    [Obsolete ("Use System.Activator.CreateInstance<T>() instead. (Version 1.15.7.0)")]
    public static FuncInvoker<T> CreateInstance<T> ()
    {
      return new FuncInvoker<T> (new ConstructorLookupInfo (typeof (T)).GetDelegate);
    }

    [Obsolete ("Use (T) System.Activator.CreateInstance (typeof (T), bindingFlags, null, new object[0], null) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<T> CreateInstance<T> (BindingFlags bindingFlags)
    {
      return new FuncInvoker<T> (new ConstructorLookupInfo (typeof (T), bindingFlags).GetDelegate);
    }

    [Obsolete ("Use (T) System.Activator.CreateInstance (typeof (T), bindingFlags, binder, new object[0], null) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<T> CreateInstance<T> (
        BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new FuncInvoker<T> (new ConstructorLookupInfo (typeof (T), bindingFlags, binder, callingConvention, parameterModifiers).GetDelegate);
    }

    [Obsolete ("Use System.Activator.CreateInstance (type) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<object> CreateInstance (Type type)
    {
      return CreateInstance<object> (type);
    }

    [Obsolete ("Use (TMinimal) System.Activator.CreateInstance (type) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<TMinimal> CreateInstance<TMinimal> (Type type)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("type", type, typeof (TMinimal));
      return new FuncInvoker<TMinimal> (new ConstructorLookupInfo (type).GetDelegate);
    }

    [Obsolete ("Use (T) System.Activator.CreateInstance (typeof (T), bindingFlags, null, new object[0], null) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<object> CreateInstance (Type type, BindingFlags bindingFlags)
    {
      return CreateInstance<object> (type, bindingFlags);
    }

    [Obsolete ("Use (TMinimal) System.Activator.CreateInstance (type, bindingFlags, null, new object[0], null) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<TMinimal> CreateInstance<TMinimal> (Type type, BindingFlags bindingFlags)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("type", type, typeof (TMinimal));
      return new FuncInvoker<TMinimal> (new ConstructorLookupInfo (type, bindingFlags).GetDelegate);
    }

    [Obsolete ("Use (T) System.Activator.CreateInstance (typeof (T), bindingFlags, binder, new object[0], null) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<object> CreateInstance (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return CreateInstance<object> (type, bindingFlags, binder, callingConvention, parameterModifiers);
    }

    [Obsolete ("Use (TMinimal) System.Activator.CreateInstance (type, bindingFlags, binder, new object[0], null) instead. (Version 1.15.7.0)")]
    public static FuncInvoker<TMinimal> CreateInstance<TMinimal> (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("type", type, typeof (TMinimal));
      return new FuncInvoker<TMinimal> (new ConstructorLookupInfo (type, bindingFlags, binder, callingConvention, parameterModifiers).GetDelegate);
    }
  }
}
