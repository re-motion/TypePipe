using System;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  ///// <summary>
  ///// A function that will create a delegate to call from <see cref="FuncInvoker{TResult}"/>.
  ///// </summary>
  ///// <param name="parameterTypes"> Types of the parameters as used for the <see cref="FuncInvoker{TResult}.With" />(...) methods. </param> 
  ///// <param name="delegateType"> Type of the delegate that will be created. </param>
  ///// <returns> The delegate used to call the wrapped method. </returns>
  
  public delegate Delegate DelegateSelector (Type delegateType);

  //public interface DelegateSelector
  //{
  //  Delegate GetDelegate (Type delegateType);
  //}

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

  //using IFunctionCache = ICache<Type[], Delegate>;
  //using FunctionCache = InterlockedCache<Type[], Delegate>;

  //public class GetDelegateBase
  //{
  //  private DelegateSelector _delegateSelector;

  //  public GetDelegateBase (DelegateSelector delegateSelector)
  //  {
  //    _delegateSelector = delegateSelector;
  //  }

  //  public virtual TDelegate GetDelegate<TDelegate> (params Type[] types)
  //  {
  //    return (TDelegate) (object) _delegateSelector (types, typeof (TDelegate));
  //  }

  //  public Delegate GetDelegate (params Type[] types)
  //  {
  //    ArgumentUtility.CheckNotNull ("types", types);

  //    Type delegateType = FuncDelegates.MakeClosedType (typeof (T), types);
  //    return _delegateSelector (types, delegateType);
  //  }
  //}

  ///// <summary>
  ///// Use this base class to implement methods that use reflection-like argument type lists, like <see cref="Type.GetMethod (string, Type[])"/>.
  ///// </summary>
  ///// <include file='doc\include\Reflection\TypesafeInvoker.xml' path='TypesafeInvoker/Class/*' />
  //public class DelegateCreator: GetDelegateBase
  //{
  //  public Proc With ()
  //  {
  //    return GetDelegate<Proc> (Type.EmptyTypes);
  //  }
  //  public Proc<A1> With<A1> ()
  //  {
  //    return GetDelegate<Proc<A1>> (typeof (A1));
  //  }
  //  public Proc<A1, A2> With<A1, A2> ()
  //  {
  //    return GetDelegate<Proc<A1, A2>> (typeof (A1), typeof (A2));
  //  }
  //  public Proc<A1, A2, A3> With<A1, A2, A3> ()
  //  {
  //    return GetDelegate<Proc<A1, A2, A3>> (typeof (A1), typeof (A2), typeof (A3));
  //  }
  //  public Proc<A1, A2, A3, A4> With<A1, A2, A3, A4> ()
  //  {
  //    return GetDelegate<Proc<A1, A2, A3, A4>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4));
  //  }
  //  public Proc<A1, A2, A3, A4, A5> With<A1, A2, A3, A4, A5> ()
  //  {
  //    return GetDelegate<Proc<A1, A2, A3, A4, A5>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5));
  //  }
  //  public Proc<A1, A2, A3, A4, A5, A6> With<A1, A2, A3, A4, A5, A6> ()
  //  {
  //    return GetDelegate<Proc<A1, A2, A3, A4, A5, A6>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6));
  //  }
  //  public Proc<A1, A2, A3, A4, A5, A6, A7> With<A1, A2, A3, A4, A5, A6, A7> ()
  //  {
  //    return GetDelegate<Proc<A1, A2, A3, A4, A5, A6, A7>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6), typeof (A7));
  //  }
  //  public Proc<A1, A2, A3, A4, A5, A6, A7, A8> With<A1, A2, A3, A4, A5, A6, A7, A8> ()
  //  {
  //    return GetDelegate<Proc<A1, A2, A3, A4, A5, A6, A7, A8>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6), typeof (A7), typeof (A8));
  //  }
  //}

  ///// <summary>
  ///// Use this base class to implement methods that use reflection-like argument type lists, like <see cref="Type.GetMethod (string, Type[])"/>.
  ///// </summary>
  ///// <include file='doc\include\Reflection\TypesafeInvoker.xml' path='TypesafeInvoker/Class/*' />
  //public class GetDelegateWith<T>: GetDelegateBase
  //{
  //  public Func<T> With ()
  //  {
  //    return GetDelegate<Func<T>> (Type.EmptyTypes);
  //  }
  //  public Func<A1, T> With<A1> ()
  //  {
  //    return GetDelegate<Func<A1, T>> (typeof (A1));
  //  }
  //  public Func<A1, A2, T> With<A1, A2> ()
  //  {
  //    return GetDelegate<Func<A1, A2, T>> (typeof (A1), typeof (A2));
  //  }
  //  public Func<A1, A2, A3, T> With<A1, A2, A3> ()
  //  {
  //    return GetDelegate<Func<A1, A2, A3, T>> (typeof (A1), typeof (A2), typeof (A3));
  //  }
  //  public Func<A1, A2, A3, A4, T> With<A1, A2, A3, A4> ()
  //  {
  //    return GetDelegate<Func<A1, A2, A3, A4, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4));
  //  }
  //  public Func<A1, A2, A3, A4, A5, T> With<A1, A2, A3, A4, A5> ()
  //  {
  //    return GetDelegate<Func<A1, A2, A3, A4, A5, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5));
  //  }
  //  public Func<A1, A2, A3, A4, A5, A6, T> With<A1, A2, A3, A4, A5, A6> ()
  //  {
  //    return GetDelegate<Func<A1, A2, A3, A4, A5, A6, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6));
  //  }
  //  public Func<A1, A2, A3, A4, A5, A6, A7, T> With<A1, A2, A3, A4, A5, A6, A7> ()
  //  {
  //    return GetDelegate<Func<A1, A2, A3, A4, A5, A6, A7, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6), typeof (A7));
  //  }
  //  public Func<A1, A2, A3, A4, A5, A6, A7, A8, T> With<A1, A2, A3, A4, A5, A6, A7, A8> ()
  //  {
  //    return GetDelegate<Func<A1, A2, A3, A4, A5, A6, A7, A8, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6), typeof (A7), typeof (A8));
  //  }
  //}

  ////public abstract class BaseCachedGetDelegateWith<T> : GetDelegateWith<T>
  ////{
  ////  public BaseCachedGetDelegateWith (DelegateSelector delegateSelector)
  ////    : base (delegateSelector)
  ////  {
  ////  }

  ////  protected abstract IFunctionCache Cache { get; }

  ////  protected override TDelegate GetDelegate<TDelegate> (params Type[] types)
  ////  {
  ////    Delegate del;
  ////    if (Cache.TryGetValue (types, out del))
  ////      return (TDelegate) (object) del;

  ////    return (TDelegate) (object) Cache.GetOrCreateValue (types, BaseGetDelegate<TDelegate>);
  ////  }

  ////  private Delegate BaseGetDelegate<TDelegate> (Type[] parameterTypes)
  ////  {
  ////    return (Delegate) (object) base.GetDelegate<TDelegate> (parameterTypes);
  ////  }
  ////}

  /////// <summary>
  /////// A version of <see cref="GetDelegateWith{T}"/> that has a single global cache for each parameter type list.
  /////// </summary>
  ////public class CachedGetDelegateWith<T> : BaseCachedGetDelegateWith<T>
  ////{
  ////  private static IFunctionCache s_functionCache = new FunctionCache (new ArrayComparer<Type> ());

  ////  public CachedGetDelegateWith (DelegateSelector delegateSelector)
  ////    : base (delegateSelector)
  ////  {
  ////  }

  ////  protected override IFunctionCache Cache
  ////  {
  ////    get { return s_functionCache; }
  ////  }
  ////}

  /////// <summary>
  /////// A version of <see cref="GetDelegateWith{T}"/> that has multiple caches for parameter type lists.
  /////// </summary>
  /////// <remarks>
  /////// This class creates one global cache for each instance of TKey.
  /////// </remarks>
  /////// <typeparam name="T"> The delegate's return type. </typeparam>
  /////// <typeparam name="TKey"> The type of the cache key. </typeparam>
  ////public class CachedGetDelegateWith<T,TKey> : BaseCachedGetDelegateWith<T>
  ////{
  ////  private static ICache<TKey,IFunctionCache> s_caches = new InterlockedCache<TKey,IFunctionCache> ();

  ////  private IFunctionCache _cache;

  ////  public CachedGetDelegateWith (TKey cacheKey, DelegateSelector delegateSelector)
  ////    : base (delegateSelector)
  ////  {
  ////    if (! s_caches.TryGetValue (cacheKey, out _cache))
  ////    {
  ////      _cache = s_caches.GetOrCreateValue (cacheKey, 
  ////          delegate (TKey key) 
  ////          {
  ////            return new FunctionCache (new ArrayComparer<Type> ());
  ////          });
  ////    }
  ////  }

  ////  protected override IFunctionCache Cache
  ////  {
  ////    get { return _cache; }
  ////  }
  ////}
}
