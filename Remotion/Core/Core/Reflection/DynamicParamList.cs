// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.ObjectModel;
using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Implements the <see cref="ParamList"/> class for scenarios where the number or types of parameters are chosen at runtime.
  /// </summary>
  public class DynamicParamList : ParamList
  {
    private readonly Type[] _parameterTypes;
    private readonly object[] _parameterValues;

    public DynamicParamList (Type[] parameterTypes, object[] parameterValues)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("parameterValues", parameterValues);

      if (parameterValues.Length != parameterTypes.Length)
        throw new ArgumentException ("The number of parameter values must match the number of parameter types.", "parameterValues");

      _parameterTypes = parameterTypes;
      _parameterValues = parameterValues;
    }

    public override Type FuncType
    {
      get { return FuncUtility.MakeClosedType (typeof (object), _parameterTypes); }
    }

    public override Type ActionType
    {
      get { return ActionUtility.MakeClosedType (_parameterTypes); }
    }

    public override void InvokeAction (Delegate action)
    {
      ArgumentUtility.CheckNotNull ("action", action);

      try
      {
        action.DynamicInvoke (_parameterValues);
      }
      catch (TargetParameterCountException)
      {
        throw CreateActionTypeException (action);
      }
      catch (ArgumentException)
      {
        throw CreateActionTypeException (action);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException.PreserveStackTrace();
      }
    }

    public override object InvokeFunc (Delegate func)
    {
      ArgumentUtility.CheckNotNull ("func", func);

      try
      {
        return func.DynamicInvoke (_parameterValues);
      }
      catch (TargetParameterCountException)
      {
        throw CreateFuncTypeException (func);
      }
      catch (ArgumentException)
      {
        throw CreateFuncTypeException (func);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException.PreserveStackTrace();
      }
    }

    public override object InvokeConstructor (IConstructorLookupInfo constructorLookupInfo)
    {
      ArgumentUtility.CheckNotNull ("constructorLookupInfo", constructorLookupInfo);

      try
      {
        return constructorLookupInfo.DynamicInvoke (_parameterTypes, _parameterValues);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException.PreserveStackTrace();
      }
    }

    public override Type[] GetParameterTypes ()
    {
      return (Type[]) _parameterTypes.Clone();
    }

    public override object[] GetParameterValues ()
    {
      return (object[]) _parameterValues.Clone();
    }

    private ArgumentTypeException CreateActionTypeException (Delegate action)
    {
      var message = string.Format (
          "Argument action has type {0} when a delegate with the following parameter signature was expected: ({1}).",
          action.GetType(),
          SeparatedStringBuilder.Build (", ", _parameterTypes, t => t.FullName));
      return new ArgumentTypeException (message, "action", null, action.GetType());
    }

    private ArgumentTypeException CreateFuncTypeException (Delegate func)
    {
      var message = string.Format (
          "Argument action has type {0} when a delegate returning System.Object with the following parameter signature "
          + "was expected: ({1}).",
          func.GetType(),
          SeparatedStringBuilder.Build (", ", _parameterTypes, t => t.FullName));
      return new ArgumentTypeException (message, "func", null, func.GetType());
    }
  }
}