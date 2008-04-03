using System;
using Remotion.Utilities;
using System.Reflection;

namespace Remotion.Reflection
{
  /// <summary>
  /// This interface allows invokers with fixed arguments to be returned without references to their generic argument types. 
  /// </summary>
  /// <remarks>
  /// <p>Note that casting a struct like <see cref="ProcInvoker"/> to an interface is a boxing operation, thus creating an object on the
  /// heap and garbage collecting it later. For very performance-critical scenarios, it be better to avoid this and accept the references to 
  /// the invoker's generic argument types.</p>
  /// <p>It is recommended to wrap this interface within a <see cref="ProcInvokerWrapper"/>, because returning an interface could lead to 
  /// ambigous castings if the final call to <see cref="With()"/> is missing, while using structs will usually lead to a compile-time error as 
  /// expected.</p>
  /// </remarks>
  public partial interface IProcInvoker
  {
    // @begin-skip
    void Invoke (Type[] valueTypes, object[] values);
    void Invoke (object[] values);
    // @end-skip

    // @begin-template first=1 generate=0..7 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    void With<A1> (A1 a1);
    // @end-template
  }

  /// <summary>
  /// Used to wrap an <see cref="IProcInvoker"/> object rather than returning it directly.
  /// </summary>
  public partial struct ProcInvokerWrapper: IProcInvoker
  {
    // @begin-skip
    private IProcInvoker _invoker;

    public ProcInvokerWrapper (IProcInvoker invoker)
    {
      _invoker = invoker;
    }

    public void Invoke (Type[] valueTypes, object[] values)
    {
      _invoker.Invoke (valueTypes, values);
    }
    public void Invoke (object[] values)
    {
      _invoker.Invoke (values);
    }
    // @end-skip

    // @begin-template first=1 generate=0..7 suppressTemplate=true

    // @replace "A<n>" ", " "<" ">"
    // @replace "A<n> a<n>" ", "
    public void With<A1> (A1 a1)
    {
      // @replace "a<n>" ", "
      _invoker.With (a1);
    }
    // @end-template
  }

  // @begin-template first=1 template=1 generate=0..3 suppressTemplate=true

  // @replace "TFixedArg<n>" ", " "<" ">"
  public partial struct ProcInvoker<TFixedArg1> : IProcInvoker
  {
    private DelegateSelector _delegateSelector;

    // @begin-repeat
    // @replace-one "<n>"
    private TFixedArg1 _fixedArg1;
    // @end-repeat

    // @replace-one "c_argCount = <n>"
    private const int c_argCount = 1;

    // @replace ", TFixedArg<n> fixedArg<n>"
    public ProcInvoker (DelegateSelector delegateSelector, TFixedArg1 fixedArg1)
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

    public void Invoke (Type[] valueTypes, object[] values)
    {
      InvokerUtility.CheckInvokeArguments (valueTypes, values);
      GetDelegate (GetValueTypes (valueTypes)).DynamicInvoke (GetValues (values));
    }

    public void Invoke (object[] values)
    {
      Type[] valueTypes = InvokerUtility.GetValueTypes (values);
      GetDelegate (GetValueTypes (valueTypes)).DynamicInvoke (GetValues (values));
    }

    public Delegate GetDelegate (params Type[] parameterTypes)
    {
      return GetDelegate (ProcDelegates.MakeClosedType (parameterTypes));
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
    public void With ()
    {
      // @replace "TFixedArg<n>" ", " "<" ">"
      // @replace "_fixedArg<n>" ", " 
      GetDelegateWith () (_fixedArg1);
    }

    // @replace "TFixedArg<n>" ", " "<" ">"
    public Proc<TFixedArg1> GetDelegateWith ()
    {
      // @replace "TFixedArg<n>" ", " "<" ">"
      return GetDelegate<Proc<TFixedArg1>> ();
    }
  }
  // @end-template


  // @rem the template is split so that the two parts can have different suppressTemplate settings
  // @begin-template first=1 template=1 generate=0..3 suppressTemplate=false

    // @replace "TFixedArg<n>" ", " "<" ">"
    public partial struct ProcInvoker<TFixedArg1>
    {
      // @rem the following 2 replace-statements are part of the outer template's scope (because that's where they are declared) but only apply to the
      // @rem inner template (because they are preceeding it immediately). However, within the inner template, they apply to every single line.

      // @replace "TFixedArg<n>, "
      // @replace "_fixedArg<n>, "
      // @replace "typeof (TFixedArg<n>), "
      // @begin-template first=1 generate=1..7 suppressTemplate=parent

        // @replace "A<n>" ", " "<" ">"
        // @replace "A<n> a<n>" ", "
        public void With<A1> (A1 a1)
        {
          // @replace "A<n>" ", "
          // @replace "a<n>" ", "
          GetDelegateWith<A1> () (_fixedArg1, a1);
        }

        // @replace "A<n>" ", "
        public Proc<TFixedArg1, A1> GetDelegateWith<A1> ()
        {
          // @replace "A<n>" ", "
          return GetDelegate<Proc<TFixedArg1, A1>> ();
        }
      // @end-template
    }
  // @end-template
}