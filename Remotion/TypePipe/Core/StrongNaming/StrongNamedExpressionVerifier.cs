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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  /// <summary>
  /// Determines wheter an <see cref="Expression"/> tree only contains access to strong-named types.
  /// </summary>
  public interface IStrongNamedExpressionVerifier
  {
    bool IsStrongNamed (Expression expression);
  }

  public class StrongNamedExpressionVerifier : PrimitiveTypePipeExpressionVisitorBase, IStrongNamedExpressionVerifier
  {
    private readonly IStrongNamedTypeVerifier _typeVerifier;

    private bool _isStrongNamed = true;

    public StrongNamedExpressionVerifier (IStrongNamedTypeVerifier typeVerifier)
    {
      ArgumentUtility.CheckNotNull ("typeVerifier", typeVerifier);

      _typeVerifier = typeVerifier;
    }

    public bool IsStrongNamed (Expression expression)
    {
      Visit (expression);


      return _isStrongNamed;
    }

    public override Expression Visit (Expression node)
    {
      if (node != null && !_typeVerifier.IsStrongNamed (node.Type))
      {
        _isStrongNamed = false;
        return node;
      }

      return base.Visit (node);
    }

    protected internal override Expression VisitMethodCall (MethodCallExpression node)
    {
      // TODO Review: Also check generic arguments of method.
      if (!_typeVerifier.IsStrongNamed (node.Method.DeclaringType))
      {
        _isStrongNamed = false;
        return node;
      }

      return base.VisitMethodCall (node);
    }


    protected internal override Expression VisitMember (MemberExpression node)
    {
      if (!_typeVerifier.IsStrongNamed (node.Member.DeclaringType))
      {
        _isStrongNamed = false;
        return node;
      }

      return base.VisitMember (node);
    }

    // TODO Review: VisitBinary, VisitUnary can also have methods
    // TODO Review: VisitDynamic => DelegateType
    // TODO Review: ElementInit => AddMethod
    // TODO Review: Check remaining expressions for potential strong-naming relevant members.

    protected override CatchBlock VisitCatchBlock (CatchBlock node)
    {
      if (!_typeVerifier.IsStrongNamed (node.Test))
      {
        _isStrongNamed = false;
        return node;
      }
      return base.VisitCatchBlock (node);
    }
  }
}