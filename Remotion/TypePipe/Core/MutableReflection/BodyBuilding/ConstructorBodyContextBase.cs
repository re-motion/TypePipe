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
using Remotion.FunctionalProgramming;
using Remotion.Text;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Base class for constructor body context classes.
  /// </summary>
  public abstract class ConstructorBodyContextBase : MethodBaseBodyContextBase
  {
    private const BindingFlags c_allInstanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    protected ConstructorBodyContextBase (MutableType declaringType, bool isStatic, IEnumerable<ParameterExpression> parameterExpressions)
        : base (declaringType, isStatic, parameterExpressions)
    {
    }

    public MethodCallExpression CallBaseConstructor (params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return CallBaseConstructor ((IEnumerable<Expression>) arguments);
    }

    public MethodCallExpression CallBaseConstructor (IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);
      EnsureNotStatic();

      var args = arguments.ConvertToCollection();
      var constructor = GetConstructor (DeclaringType.BaseType, args);
      if (!SubclassFilterUtility.IsVisibleFromSubclass (constructor))
        throw new MemberAccessException ("The matching constructor is not visible from the proxy type.");

      return CallConstructor (constructor, args);
    }

    public MethodCallExpression CallThisConstructor (params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      return CallThisConstructor (((IEnumerable<Expression>) arguments));
    }

    public MethodCallExpression CallThisConstructor (IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);
      EnsureNotStatic();

      var args = arguments.ConvertToCollection();
      return CallConstructor (GetConstructor (DeclaringType, args), args);
    }

    private static ConstructorInfo GetConstructor (Type type, ICollection<Expression> arguments)
    {
      var argumentTypes = BodyContextUtility.GetArgumentTypes (arguments);
      var constructor = type.GetConstructor (c_allInstanceMembers, null, argumentTypes, null);
      if (constructor == null)
      {
        var message = String.Format (
            "Could not find an instance constructor with signature ({0}) on type '{1}'.", string.Join (", ", (IEnumerable<Type>) argumentTypes), type.Name);
        throw new MissingMemberException (message);
      }

      return constructor;
    }

    private MethodCallExpression CallConstructor (ConstructorInfo constructor, ICollection<Expression> arguments)
    {
      return Expression.Call (This, NonVirtualCallMethodInfoAdapter.Adapt (constructor), arguments);
    }

    private void EnsureNotStatic ()
    {
      if (IsStatic)
        throw new InvalidOperationException ("Cannot call other constructor from type initializer.");
    }
  }
}