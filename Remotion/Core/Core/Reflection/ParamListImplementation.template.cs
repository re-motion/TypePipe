// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// This framework is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this framework; if not, see http://www.gnu.org/licenses.
// 
using System;
using Remotion.Implementation;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  // @begin-template first=1 generate=0..20 suppressTemplate=true
  // @replace "A<n>" ", " "<" ">"
  // @replace "private readonly A<n> _a<n>;" " "
  // @replace "A<n> a<n>" ", " " " " "
  // @replace "_a<n> = a<n>;" " "
  // @replace "A<n>" ", " " " ", "
  // @replace "_a<n>" ", "
  // @replace "typeof (A<n>)" ", "
  /// <summary>
  /// Implements <see cref="ParamList"/> for a specific number of arguments. Use one of the <see cref="ParamList.Create"/> overloads to create
  /// instances of the <see cref="ParamList"/> implementation classes.
  /// </summary>
  public class ParamListImplementation<A1> : ParamList
  {
    private readonly A1 _a1;

    public ParamListImplementation ( A1 a1 )
    {
      _a1 = a1;
    }

    public override Type FuncType
    {
      get { return typeof (Func< A1, object>); }
    }

    public override Type ActionType
    {
      get { return typeof (Action<A1>); }
    }

    public override void InvokeAction (Delegate action)
    {
      ArgumentUtility.CheckNotNull ("action", action);

      Action<A1> castAction;
      try
      {
        castAction = (Action<A1>) action;
      }
      catch (InvalidCastException)
      {
        throw new ArgumentTypeException ("action", ActionType, action.GetType ());
      }

      castAction (_a1);
    }

    public override object InvokeFunc (Delegate func)
    {
      ArgumentUtility.CheckNotNull ("func", func);

      Func< A1, object> castFunc;
      try
      {
      castFunc = (Func< A1, object>) func;
      }
      catch (InvalidCastException)
      {
        throw new ArgumentTypeException ("func", FuncType, func.GetType ());
      }

      return castFunc (_a1);
    }

    public override object InvokeConstructor (IConstructorLookupInfo constructorLookupInfo)
    {
      ArgumentUtility.CheckNotNull ("constructorLookupInfo", constructorLookupInfo);
      var funcDelegate = constructorLookupInfo.GetDelegate (FuncType);
      return InvokeFunc (funcDelegate);
    }

    public override Type[] GetParameterTypes ()
    {
      return new Type[] { typeof (A1) };
    }

    public override object[] GetParameterValues ()
    {
      return new object[] { _a1 };
    }
  }
  // @end-template
}