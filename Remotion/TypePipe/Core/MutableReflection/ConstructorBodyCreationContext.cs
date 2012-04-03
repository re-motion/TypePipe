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
using Microsoft.Scripting.Ast;
using Remotion.Text;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Provides access to parameters and custom expression for building constructor bodies.
  /// </summary>
  public class ConstructorBodyCreationContext
  {
    private readonly MutableType _declaringType;
    private readonly ReadOnlyCollection<ParameterExpression> _parameters;

    public ConstructorBodyCreationContext (MutableType declaringType, IEnumerable<ParameterExpression> parameterExpressions)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameterExpressions", parameterExpressions);

      _declaringType = declaringType;
      _parameters = parameterExpressions.ToList().AsReadOnly();
    }

    public ReadOnlyCollection<ParameterExpression> Parameters
    {
      get { return _parameters; }
    }

    public Expression This
    {
      get { return new ThisExpression(_declaringType); }
    }

    public Expression GetConstructorCall (params Expression[] arguments)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("arguments", arguments);

      return GetConstructorCall (((IEnumerable<Expression>) arguments));
    }

    public Expression GetConstructorCall (IEnumerable<Expression> arguments)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      var argumentTypes = arguments.Select (e => e.Type).ToArray();
      var constructor = _declaringType.GetConstructor (argumentTypes);
      if (constructor == null)
      {
        var message = string.Format ("Could not find a constructor with signature ({0}) on type '{1}'.",
            SeparatedStringBuilder.Build(", ", argumentTypes), _declaringType);
        throw new MemberNotFoundException (message);
      }

      var adapter = new ConstructorAsMethodInfoAdapter (constructor);

      return Expression.Call (This, adapter, arguments);
    }
  }
}