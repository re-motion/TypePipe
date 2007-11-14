using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Text;

namespace Rubicon.Reflection
{
  using CacheKey = Tuple<Type, Type>;
  using Rubicon.Utilities;

  /// <summary>
  /// Create an instance of a known type using fixed parameter types for the constructor.
  /// </summary>
  /// <remarks>
  /// While <see cref="System.Activator.CreateInstance"/> uses the types of passed arguments to select the correct constructor, this class
  /// uses the types of the expressions you use at compile time. Use the following code to create an instance of a class called MyClass using a 
  /// constructor that has an argument of type string:
  /// <code>
  ///   ParameterType p;
  ///   YourClass obj = TypesafeActivator.CreateInstance&lt;YourClass&gt;().With (p);
  /// </code>
  /// This code always selects the constructor that accepts an argument of type ParameterType, even if the value passed is null or an instance
  /// of a subclass of ParameterType.
  /// </remarks>
  public static class TypesafeActivator
  {
    public class ConstructorLookupInfo: MemberLookupInfo
    {
      private Type _definingType;

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
        : base (null, bindingFlags, binder, callingConvention, parameterModifiers)
      {
        _definingType = definingType;
      }

      public virtual Delegate GetDelegate (Type delegateType)
      {
        CacheKey key = new CacheKey (_definingType, delegateType);
        Delegate result;
        if (! s_delegateCache.TryGetValue (key, out result))
        {
          result = s_delegateCache.GetOrCreateValue (
              key,
              delegate
              {
                return CreateDelegate(delegateType);
              });
        }
        return result;
      }

      protected virtual Delegate CreateDelegate (Type delegateType)
      {
        return ConstructorWrapper.CreateDelegate (
            _definingType, delegateType, BindingFlags, Binder, CallingConvention, ParameterModifiers);
      }
    }

    private static readonly ICache<CacheKey, Delegate> s_delegateCache = new InterlockedCache<CacheKey, Delegate> ();

    public static FuncInvoker<T> CreateInstance<T> ()
    {
      return new FuncInvoker<T> (new ConstructorLookupInfo (typeof (T)).GetDelegate);
    }

    public static FuncInvoker<T> CreateInstance<T> (BindingFlags bindingFlags)
    {
      return new FuncInvoker<T> (new ConstructorLookupInfo (typeof (T), bindingFlags).GetDelegate);
    }

    public static FuncInvoker<T> CreateInstance<T> (
        BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return new FuncInvoker<T> (new ConstructorLookupInfo (typeof (T), bindingFlags, binder, callingConvention, parameterModifiers).GetDelegate);
    }

    public static FuncInvoker<object> CreateInstance (Type type)
    {
      return CreateInstance<object> (type);
    }

    public static FuncInvoker<TMinimal> CreateInstance<TMinimal> (Type type)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("type", type, typeof (TMinimal));
      return new FuncInvoker<TMinimal> (new ConstructorLookupInfo (type).GetDelegate);
    }

    public static FuncInvoker<object> CreateInstance (Type type, BindingFlags bindingFlags)
    {
      return CreateInstance<object> (type, bindingFlags);
    }

    public static FuncInvoker<TMinimal> CreateInstance<TMinimal> (Type type, BindingFlags bindingFlags)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("type", type, typeof (TMinimal));
      return new FuncInvoker<TMinimal> (new ConstructorLookupInfo (type, bindingFlags).GetDelegate);
    }

    public static FuncInvoker<object> CreateInstance (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      return CreateInstance<object> (type, bindingFlags, binder, callingConvention, parameterModifiers);
    }

    public static FuncInvoker<TMinimal> CreateInstance<TMinimal> (
        Type type, BindingFlags bindingFlags, Binder binder, CallingConventions callingConvention, ParameterModifier[] parameterModifiers)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("type", type, typeof (TMinimal));
      return new FuncInvoker<TMinimal> (new ConstructorLookupInfo (type, bindingFlags, binder, callingConvention, parameterModifiers).GetDelegate);
    }
  }
}
