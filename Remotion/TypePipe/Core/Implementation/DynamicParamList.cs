// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Implements the <see cref="ParamList"/> class for scenarios where the number or types of parameters are chosen at runtime.
  /// </summary>
  public class DynamicParamList : ParamList
  {
    private static readonly Type[] s_funcTypes =
    {
        typeof (Func<>),
        typeof (Func<,>),
        typeof (Func<,,>),
        typeof (Func<,,,>),
        typeof (Func<,,,,>),
        typeof (Func<,,,,,>),
        typeof (Func<,,,,,,>),
        typeof (Func<,,,,,,,>),
        typeof (Func<,,,,,,,,>),
        typeof (Func<,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,,,,,,>),
        typeof (Func<,,,,,,,,,,,,,,,,,,,,>)
    };

    private static readonly Type[] s_actionTypes =
    {
        typeof (Action),
        typeof (Action<>),
        typeof (Action<,>),
        typeof (Action<,,>),
        typeof (Action<,,,>),
        typeof (Action<,,,,>),
        typeof (Action<,,,,,>),
        typeof (Action<,,,,,,>),
        typeof (Action<,,,,,,,>),
        typeof (Action<,,,,,,,,>),
        typeof (Action<,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,,,,,,>),
        typeof (Action<,,,,,,,,,,,,,,,,,,,>)
    };

    static DynamicParamList ()
    {
      Assertion.IsTrue (s_funcTypes.Length == s_actionTypes.Length);
    }

    private readonly Type[] _parameterTypes;
    private readonly object[] _parameterValues;

    public DynamicParamList (Type[] parameterTypes, object[] parameterValues)
    {
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("parameterValues", parameterValues);

      if (parameterValues.Length != parameterTypes.Length)
        throw new ArgumentException ("The number of parameter values must match the number of parameter types.", "parameterValues");

      if (parameterTypes.Length >= s_funcTypes.Length)
      {
        throw new ArgumentException (
            string.Format (
                "The number of parameters ({0}) must not exceed the maximum supported number of parameters ({1}).",
                parameterTypes.Length,
                s_funcTypes.Length - 1),
            "parameterTypes");
      }

      _parameterTypes = parameterTypes;
      _parameterValues = parameterValues;
    }

    public override Type FuncType
    {
      get
      {
        var typeArguments = new Type[_parameterTypes.Length + 1];
        _parameterTypes.CopyTo (typeArguments, 0);
        typeArguments[_parameterTypes.Length] = typeof (object);

        var openType = s_funcTypes[_parameterTypes.Length];

        return openType.MakeGenericType (typeArguments);
      }
    }

    public override Type ActionType
    {
      get
      {
        if (_parameterTypes.Length == 0)
          return typeof (Action);

        var openType = s_actionTypes[_parameterTypes.Length];
        return openType.MakeGenericType (_parameterTypes);
      }
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

    public override Type[] GetParameterTypes ()
    {
      return (Type[]) _parameterTypes.Clone();
    }

    public override object[] GetParameterValues ()
    {
      return (object[]) _parameterValues.Clone();
    }

    private ArgumentException CreateActionTypeException (Delegate action)
    {
      var message = string.Format (
          "Parameter 'action' has type '{0}' when a delegate with the following parameter signature was expected: ({1}).",
          action.GetType(),
          string.Join (", ", _parameterTypes.Select (t => t.FullName)));
      return new ArgumentException (message, "action");
    }

    private ArgumentException CreateFuncTypeException (Delegate func)
    {
      var message = string.Format (
          "Parameter 'func' has type '{0}' when a delegate returning System.Object with the following parameter signature was expected: ({1}).",
          func.GetType(),
          string.Join (", ", _parameterTypes.Select (t => t.FullName)));
      return new ArgumentException (message, "func");
    }
  }
}
