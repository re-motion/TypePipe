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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Base class for context classes used to build new method bodies. (So-called body builders.)
  /// </summary>
  public abstract class BodyContextBase
  {
    private readonly MutableType _declaringType;
    private readonly ReadOnlyCollection<ParameterExpression> _parameters;
    private readonly bool _isStatic;
    private readonly IMemberSelector _memberSelector;

    protected BodyContextBase (
        MutableType declaringType, IEnumerable<ParameterExpression> parameterExpressions, bool isStatic, IMemberSelector memberSelector)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameterExpressions", parameterExpressions);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);

      _declaringType = declaringType;
      _parameters = parameterExpressions.ToList().AsReadOnly();
      _isStatic = isStatic;
      _memberSelector = memberSelector;
    }

    public MutableType DeclaringType
    {
      get { return _declaringType; }
    }

    public Expression This
    {
      get
      {
        if (IsStatic)
          throw new InvalidOperationException ("Static methods cannot use 'This'.");

        return new ThisExpression (_declaringType);
      }
    }

    public ReadOnlyCollection<ParameterExpression> Parameters
    {
      get { return _parameters; }
    }

    public bool IsStatic
    {
      get { return _isStatic; }
    }

    public MethodCallExpression GetBaseCall (string methodName, params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return GetBaseCall (methodName, (IEnumerable<Expression>) arguments);
    }

    public MethodCallExpression GetBaseCall (string methodName, IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("arguments", arguments);
      EnsureNotStatic ();

      // TODO visibility 4818

      var baseType = _declaringType.BaseType;
      if (baseType == null)
      {
        var message = string.Format ("Type '{0}' has no base type.", _declaringType);
        throw new InvalidOperationException (message);
      }

      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var baseTypeMethods = baseType.GetMethods (bindingFlags);
      var argumentTypes = arguments.Select (a => a.Type).ToArray ();
      var baseMethod = _memberSelector.SelectSingleMethod (
          baseTypeMethods, Type.DefaultBinder, bindingFlags, methodName, _declaringType, argumentTypes, null);

      if (baseMethod == null)
      {
        var message = string.Format ("Instance method '{0}' could not be found on base type '{1}'.", methodName, baseType);
        throw new ArgumentException (message, "methodName");
      }

      return GetBaseCall (baseMethod, arguments);
    }

    public MethodCallExpression GetBaseCall (MethodInfo method, params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return GetBaseCall (method, (IEnumerable<Expression>) arguments);
    }

    public MethodCallExpression GetBaseCall (MethodInfo method, IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("method", method);
      ArgumentUtility.CheckNotNull ("arguments", arguments);
      EnsureNotStatic ();
      CheckNotStatic (method);

      return Expression.Call (This, new BaseCallMethodInfoAdapter (method), arguments);
    }

    private void EnsureNotStatic ()
    {
      if (IsStatic)
        throw new InvalidOperationException ("Cannot perform base call from static method.");
    }

    private void CheckNotStatic (MethodInfo method)
    {
      if (method.IsStatic)
        throw new ArgumentException ("Cannot perform base call for static method.");
    }
  }
}