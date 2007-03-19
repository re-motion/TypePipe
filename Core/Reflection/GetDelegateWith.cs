using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Collections;
using Rubicon.Utilities;

namespace Rubicon.Reflection
{
  using FunctionCache = InterlockedCache<Type[], Delegate>;
  using IFunctionCache = ICache<Type[], Delegate>;

  public class GetDelegateWith<T>
  {
    private Func<Type[], Type, Delegate> _getDelegateFunction;

    public GetDelegateWith (Func<Type[], Type, Delegate> getDelegateFunction)
    {
      _getDelegateFunction = getDelegateFunction;
    }

    protected virtual TDelegate GetDelegate<TDelegate> (params Type[] types)
    {
      return (TDelegate) (object) _getDelegateFunction (types, typeof (TDelegate));
    }

    public Delegate With (params Type[] types)
    {
      ArgumentUtility.CheckNotNull ("types", types);
      return GetDelegate<Func<T>> (types);
    }

    public Func<T> With ()
    {
      return GetDelegate<Func<T>> (Type.EmptyTypes);
    }
    public Func<A1, T> With<A1> ()
    {
      return GetDelegate<Func<A1, T>> (typeof (A1));
    }
    public Func<A1, A2, T> With<A1, A2> ()
    {
      return GetDelegate<Func<A1, A2, T>> (typeof (A1), typeof (A2));
    }
    public Func<A1, A2, A3, T> With<A1, A2, A3> ()
    {
      return GetDelegate<Func<A1, A2, A3, T>> (typeof (A1), typeof (A2), typeof (A3));
    }
    public Func<A1, A2, A3, A4, T> With<A1, A2, A3, A4> ()
    {
      return GetDelegate<Func<A1, A2, A3, A4, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4));
    }
    public Func<A1, A2, A3, A4, A5, T> With<A1, A2, A3, A4, A5> ()
    {
      return GetDelegate<Func<A1, A2, A3, A4, A5, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5));
    }
    public Func<A1, A2, A3, A4, A5, A6, T> With<A1, A2, A3, A4, A5, A6> ()
    {
      return GetDelegate<Func<A1, A2, A3, A4, A5, A6, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6));
    }
    public Func<A1, A2, A3, A4, A5, A6, A7, T> With<A1, A2, A3, A4, A5, A6, A7> ()
    {
      return GetDelegate<Func<A1, A2, A3, A4, A5, A6, A7, T>> (typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6), typeof (A7));
    }
  }

  public abstract class BaseCachedGetDelegateWith<T> : GetDelegateWith<T>
  {
    public BaseCachedGetDelegateWith (Func<Type[], Type, Delegate> getDelegateFunction)
      : base (getDelegateFunction)
    {
    }

    protected abstract IFunctionCache Cache { get; }

    protected override TDelegate GetDelegate<TDelegate> (params Type[] types)
    {
      Delegate del;
      if (Cache.TryGetValue (types, out del))
        return (TDelegate) (object) del;

      return (TDelegate) (object) Cache.GetOrCreateValue (types, BaseGetDelegate<TDelegate>);
    }

    private Delegate BaseGetDelegate<TDelegate> (Type[] parameterTypes)
    {
      return (Delegate) (object) base.GetDelegate<TDelegate> (parameterTypes);
    }
  }

  /// <summary>
  /// A version of GetDelegateWith that has a single global cache for each parameter type list.
  /// </summary>
  public class CachedGetDelegateWith<T> : BaseCachedGetDelegateWith<T>
  {
    private static IFunctionCache s_functionCache = new FunctionCache (new ArrayComparer<Type> ());

    public CachedGetDelegateWith (Func<Type[], Type, Delegate> getDelegateFunction)
      : base (getDelegateFunction)
    {
    }

    protected override IFunctionCache Cache
    {
      get { return s_functionCache; }
    }
  }

  /// <summary>
  /// A version of GetDelegateWith that has multiple caches for parameter type lists.
  /// </summary>
  /// <remarks>
  /// This class creates one global cache for each instance of TKey.
  /// </remarks>
  /// <typeparam name="T"> The delegate's return type. </typeparam>
  /// <typeparam name="TKey"> The type of the cache key. </typeparam>
  public class CachedGetDelegateWith<T,TKey> : BaseCachedGetDelegateWith<T>
  {
    private static ICache<TKey,IFunctionCache> s_caches = new InterlockedCache<TKey,IFunctionCache> ();

    private IFunctionCache _cache;

    public CachedGetDelegateWith (TKey cacheKey, Func<Type[], Type, Delegate> getDelegateFunction)
      : base (getDelegateFunction)
    {
      if (! s_caches.TryGetValue (cacheKey, out _cache))
      {
        _cache = s_caches.GetOrCreateValue (cacheKey, 
            delegate (TKey key) 
            {
              return new FunctionCache (new ArrayComparer<Type> ());
            });
      }
    }

    protected override IFunctionCache Cache
    {
      get { return _cache; }
    }
  }
}
