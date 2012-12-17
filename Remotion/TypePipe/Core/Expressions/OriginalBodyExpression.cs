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
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Represents the original body of a modified method or constructor.
  /// </summary>
  public class OriginalBodyExpression : PrimitiveTypePipeExpressionBase
  {
    private readonly MethodBase _methodBase;
    private readonly ReadOnlyCollection<Expression> _arguments;

    public OriginalBodyExpression (MethodBase methodBase, Type returnType, IEnumerable<Expression> arguments)
        : base (ArgumentUtility.CheckNotNull ("returnType", returnType))
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);
      ArgumentUtility.CheckNotNull ("arguments", arguments);

      _methodBase = methodBase;
      _arguments = arguments.ToList ().AsReadOnly ();
    }

    public MethodBase MethodBase
    {
      get { return _methodBase; }
    }

    public ReadOnlyCollection<Expression> Arguments
    {
      get { return _arguments; }
    }

    public override Expression Accept (IPrimitiveTypePipeExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return visitor.VisitOriginalBody (this);
    }

    protected internal override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newArguments = visitor.Visit (_arguments);
      if (newArguments != _arguments)
        return new OriginalBodyExpression (_methodBase, Type, newArguments);
      else
        return this;
    }
  }
}