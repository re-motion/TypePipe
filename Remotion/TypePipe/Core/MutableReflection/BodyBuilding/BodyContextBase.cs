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
using System.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Common base class for body context classes.
  /// </summary>
  public abstract class BodyContextBase
  {
    private readonly MutableType _declaringType;
    private readonly bool _isStatic;

    protected BodyContextBase (MutableType declaringType, bool isStatic)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      _declaringType = declaringType;
      _isStatic = isStatic;
    }

    public MutableType DeclaringType
    {
      get { return _declaringType; }
    }

    public ThisExpression This
    {
      get
      {
        if (IsStatic)
          throw new InvalidOperationException ("Static methods cannot use 'This'.");

        return new ThisExpression (_declaringType);
      }
    }

    public bool IsStatic
    {
      get { return _isStatic; }
    }

    public MethodCallExpression CallBase (MethodInfo baseMethod, params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("baseMethod", baseMethod);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return CallBase (baseMethod, (IEnumerable<Expression>) arguments);
    }

    public MethodCallExpression CallBase (MethodInfo baseMethod, IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("baseMethod", baseMethod);
      ArgumentUtility.CheckNotNull ("arguments", arguments);
      EnsureNotStatic();
      CheckNotStatic (baseMethod);
      CheckNotAbstract (baseMethod);
      CheckNoGenericMethodDefinition (baseMethod);
      CheckVisibility (baseMethod);
      // TODO: Check if really base call!

      return Expression.Call (This, NonVirtualCallMethodInfoAdapter.Adapt (baseMethod), arguments);
    }

    public Expression CopyMethodBody (MutableMethodInfo otherMethod, params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("otherMethod", otherMethod);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return CopyMethodBody (otherMethod, (IEnumerable<Expression>) arguments);
    }

    public Expression CopyMethodBody (MutableMethodInfo otherMethod, IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("otherMethod", otherMethod);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      // ReSharper disable PossibleUnintendedReferenceComparison
      if (otherMethod.DeclaringType != _declaringType)
      // ReSharper restore PossibleUnintendedReferenceComparison
      {
        var message = string.Format ("The specified method is declared by a different type '{0}'.", otherMethod.DeclaringType);
        throw new ArgumentException (message, "otherMethod");
      }

      if (IsStatic && !otherMethod.IsStatic)
        throw new ArgumentException ("The body of an instance method cannot be copied into a static method.", "otherMethod");

      return BodyContextUtility.ReplaceParameters (otherMethod.ParameterExpressions, otherMethod.Body, arguments);
    }

    private void EnsureNotStatic ()
    {
      if (IsStatic)
        throw new InvalidOperationException ("Cannot perform base call from static method.");
    }

    private void CheckNotStatic (MethodInfo baseMethod)
    {
      if (baseMethod.IsStatic)
        throw new ArgumentException ("Cannot perform base call for static method.", "baseMethod");
    }

    private void CheckNotAbstract (MethodInfo baseMethod)
    {
      if (baseMethod.IsAbstract)
        throw new ArgumentException ("Cannot perform base call on abstract method.", "baseMethod");
    }

    private void CheckNoGenericMethodDefinition (MethodInfo baseMethod)
    {
      if (baseMethod.IsGenericMethodDefinition)
      {
        var message = string.Format (
            "Cannot perform base call on generic method definition. Construct a method instantiation with MethodInfoExtensions.MakeTypePipeGenericMethod.");
        throw new ArgumentException (message, "baseMethod");
      }
    }

    private void CheckVisibility (MethodInfo baseMethod)
    {
      if (!SubclassFilterUtility.IsVisibleFromSubclass (baseMethod))
      {
        Assertion.IsNotNull (baseMethod.DeclaringType);
        var message = string.Format ("Base method '{0}.{1}' is not accessible from proxy type.", baseMethod.DeclaringType.Name, baseMethod.Name);
        throw new MemberAccessException (message);
      }
    }
  }
}