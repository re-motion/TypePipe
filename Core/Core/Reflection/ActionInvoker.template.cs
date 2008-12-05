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
using Remotion.Utilities;

namespace Remotion.Reflection
{
  // @begin-template first=1 template=1 generate=0..3 suppressTemplate=true

  // @replace "TFixedArg<n>" ", " "<" ">"
  public partial struct ActionInvoker<TFixedArg1> : IActionInvoker
  {
    private DelegateSelector _delegateSelector;

    // @begin-repeat
    // @replace-one "<n>"
    private TFixedArg1 _fixedArg1;
    // @end-repeat

    // @replace-one "c_argCount = <n>"
    private const int c_argCount = 1;

    // @replace ", TFixedArg<n> fixedArg<n>"
    public ActionInvoker (DelegateSelector delegateSelector, TFixedArg1 fixedArg1)
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
      return GetDelegate (ActionDelegates.MakeClosedType (parameterTypes));
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
    public Action<TFixedArg1> GetDelegateWith ()
    {
      // @replace "TFixedArg<n>" ", " "<" ">"
      return GetDelegate<Action<TFixedArg1>> ();
    }
  }
  // @end-template


  // @rem the template is split so that the two parts can have different suppressTemplate settings
  // @begin-template first=1 template=1 generate=0..3 suppressTemplate=false

    // @replace "TFixedArg<n>" ", " "<" ">"
    public partial struct ActionInvoker<TFixedArg1>
    {
      // @rem the following 2 replace-statements are part of the outer template's scope (because that's where they are declared) but only apply to the
      // @rem inner template (because they are preceeding it immediately). However, within the inner template, they apply to every single line.

      // @replace "TFixedArg<n>, "
      // @replace "_fixedArg<n>, "
      // @replace "typeof (TFixedArg<n>), "
      // @begin-template first=1 generate=1..17 suppressTemplate=parent

        // @replace "A<n>" ", " "<" ">"
        // @replace "A<n> a<n>" ", "
        public void With<A1> (A1 a1)
        {
          // @replace "A<n>" ", "
          // @replace "a<n>" ", "
          GetDelegateWith<A1> () (_fixedArg1, a1);
        }

        // @replace "A<n>" ", "
        public Action<TFixedArg1, A1> GetDelegateWith<A1> ()
        {
          // @replace "A<n>" ", "
          return GetDelegate<Action<TFixedArg1, A1>> ();
        }
      // @end-template
    }
  // @end-template
}
