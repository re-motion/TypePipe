using System;
using Rubicon.Utilities;
using System.Reflection;

namespace Rubicon.Reflection
{
  /// <summary>
  /// This interface allows invokers with fixed arguments to be returned without references to their generic argument types. 
  /// </summary>
  /// <remarks>
  /// <p>Note that casting a struct like <see cref="FuncInvoker{TResult}"/> to an interface is a boxing operation, thus creating an object on the
  /// heap and garbage collecting it later. For very performance-critical scenarios, it be better to avoid this and accept the references to 
  /// the invoker's generic argument types.</p>
  /// <p>It is recommended to wrap this interface within a <see cref="FuncInvokerWrapper{TResult}"/>, because returning an interface could lead to 
  /// ambigous castings if the final call to <see cref="With()"/> is missing, while using structs will usually lead to a compile-time error as 
  /// expected.</p>
  /// </remarks>
  /// <typeparam name="TResult"> Return type of the method that will be invoked. </typeparam>
  public partial interface IFuncInvoker<TResult>
  {
    // @begin-skip
    TResult Invoke (Type[] valueTypes, object[] values);
    TResult Invoke (object[] values);
    // @end-skip

    // @begin-template first=1 generate=0..7 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    TResult With<A1> (A1 a1);
    // @end-template
  }

  /// <summary>
  /// Used to wrap an <see cref="IFuncInvoker{TResult}"/> object rather than returning it directly.
  /// </summary>
  /// <typeparam name="TResult"> Return type of the method that will be invoked. </typeparam>
  public partial struct FuncInvokerWrapper<TResult> : IFuncInvoker<TResult>
  {
    // @begin-skip
    private IFuncInvoker<TResult> _invoker;
    private Func<TResult, TResult> _afterAction;

    public FuncInvokerWrapper (IFuncInvoker<TResult> invoker)
        : this (invoker, null)
    {
    }

    public FuncInvokerWrapper (IFuncInvoker<TResult> invoker, Func<TResult, TResult> afterAction)
    {
      _invoker = invoker;
      _afterAction = afterAction;
    }

    public TResult Invoke (Type[] valueTypes, object[] values)
    {
      return PerformAfterAction (_invoker.Invoke (valueTypes, values));
      
    }
    public TResult Invoke (object[] values)
    {
      return PerformAfterAction (_invoker.Invoke (values));
    }

    private TResult PerformAfterAction (TResult result)
    {
      if (_afterAction != null)
        result = _afterAction (result);
      return result;
    }

    // @end-skip

    // @begin-template first=1 generate=0..7 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    public TResult With<A1> (A1 a1)
    {
      // @replace "a<n>" ", "
      return PerformAfterAction (_invoker.With (a1));
    }

    // @end-template
  }

  // @begin-template first=1 template=1 generate=0..3 suppressTemplate=true

  // @replace "TFixedArg<n>, "
  public partial struct FuncInvoker<TFixedArg1, TResult> : IFuncInvoker<TResult>
  {
    private DelegateSelector _delegateSelector;

    // @begin-repeat
    // @replace-one "<n>"
    private TFixedArg1 _fixedArg1;
    // @end-repeat

    // @replace-one "c_argCount = <n>"
    private const int c_argCount = 1;

    // @replace ", TFixedArg<n> fixedArg<n>"
    public FuncInvoker (DelegateSelector delegateSelector, TFixedArg1 fixedArg1)
    {
      _delegateSelector = delegateSelector;
      // @begin-repeat
      // @replace-one "fixedArg<n>"
      _fixedArg1 = fixedArg1;
      // @end-repeat
    }

#pragma warning disable 162 // disable unreachable code warning. 
    private Type[] GetValueTypes (Type[] valueTypes)
    {
      if (c_argCount == 0)
        return valueTypes;
      // @replace "typeof (TFixedArg<n>)" ", "
      Type[] fixedArgTypes = new Type[] { typeof (TFixedArg1) };
      return ArrayUtility.Combine (fixedArgTypes, valueTypes);
    }

    private object[] GetValues (object[] values)
    {
      if (c_argCount == 0)
        return values;
      // @replace "_fixedArg<n>" ", "
      object[] fixedArgs = new object[] { _fixedArg1 };
      return ArrayUtility.Combine (fixedArgs, values);
    }
#pragma warning restore 162

    public TResult Invoke (Type[] valueTypes, object[] values)
    {
      InvokerUtility.CheckInvokeArguments (valueTypes, values);
      return (TResult) GetDelegate (GetValueTypes (valueTypes)).DynamicInvoke (GetValues (values));
    }

    public TResult Invoke (object[] values)
    {
      Type[] valueTypes = InvokerUtility.GetValueTypes (values);
      return (TResult) GetDelegate (GetValueTypes (valueTypes)).DynamicInvoke (GetValues (values));
    }

    public Delegate GetDelegate (params Type[] parameterTypes)
    {
      return GetDelegate (FuncDelegates.MakeClosedType (typeof (TResult), parameterTypes));
    }

    public TDelegate GetDelegate<TDelegate> ()
    {
      return (TDelegate) (object) GetDelegate (typeof (TDelegate));
    }

    public Delegate GetDelegate (Type delegateType)
    {
      return _delegateSelector (delegateType);
    }

    // @rem the With() and GetDelegate() methods with no type parameters are specified explicitly because the template generator cannot handle
    // @rem the combination of zero or more fixed arguments AND zero or more open arguments.
    public TResult With ()
    {
      // @replace "TFixedArg<n>" ", " "<" ">"
      // @replace "_fixedArg<n>" ", " 
      return GetDelegateWith () (_fixedArg1);
    }

    // @replace "TFixedArg<n>, "
    public Func<TFixedArg1, TResult> GetDelegateWith ()
    {
      // @replace "TFixedArg<n>, "
      return GetDelegate<Func<TFixedArg1, TResult>> ();
    }
  }
  // @end-template


  // @rem the template is split so that the two parts can have different suppressTemplate settings
  // @begin-template first=1 template=1 generate=0..3 suppressTemplate=false

    // @replace "TFixedArg<n>, "
    public partial struct FuncInvoker<TFixedArg1, TResult>
    {
      // @rem the following 2 replace-statements are part of the outer template's scope (because that's where they are declared) but only apply to the
      // @rem inner template (because they are preceeding it immediately). However, within the inner template, they apply to every single line.

      // @replace "TFixedArg<n>, "
      // @replace "_fixedArg<n>, "
      // @replace "typeof (TFixedArg<n>), "
      // @begin-template first=1 generate=1..7 suppressTemplate=parent

        // @replace "A<n>" ", " "<" ">"
        // @replace "A<n> a<n>" ", "
        public TResult With<A1> (A1 a1)
        {
          // @replace "A<n>" ", "
          // @replace "a<n>" ", "
          return GetDelegateWith<A1> () (_fixedArg1, a1);
        }

        // @replace "A<n>, "
        // @replace "A<n>" ", "
        public Func<TFixedArg1, A1, TResult> GetDelegateWith<A1> ()
        {
          // @replace "A<n>, "
          return GetDelegate<Func<TFixedArg1, A1, TResult>> ();
        }
      // @end-template
    }
  // @end-template
}