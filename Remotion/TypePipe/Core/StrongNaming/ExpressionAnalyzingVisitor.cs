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
  public class ExpressionAnalyzingVisitor : PrimitiveTypePipeExpressionVisitorBase, IExpressionAnalyzer
  {
    private readonly ITypeAnalyzer _typeAnalyzer;

    // TODO
    private bool _isCompatible = true;

    public ExpressionAnalyzingVisitor (ITypeAnalyzer typeAnalyzer)
    {
      ArgumentUtility.CheckNotNull ("typeAnalyzer", typeAnalyzer);

      _typeAnalyzer = typeAnalyzer;
    }

    public bool IsStrongNameCompatible (Expression expression)
    {
      Visit (expression);


      return _isCompatible;
    }

    public override Expression Visit (Expression node)
    {
      if (node != null && !_typeAnalyzer.IsStrongNamed (node.Type))
      {
        _isCompatible = false;
        return node;
      }

      return base.Visit (node);
    }

    protected internal override Expression VisitMethodCall (MethodCallExpression node)
    {
      // TODO Review: Also check generic arguments of method.
      if (!_typeAnalyzer.IsStrongNamed (node.Method.DeclaringType))
      {
        _isCompatible = false;
        return node;
      }

      return base.VisitMethodCall (node);
    }


    protected internal override Expression VisitMember (MemberExpression node)
    {
      if (!_typeAnalyzer.IsStrongNamed (node.Member.DeclaringType))
      {
        _isCompatible = false;
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
      if (!_typeAnalyzer.IsStrongNamed (node.Test))
      {
        _isCompatible = false;
        return node;
      }
      return base.VisitCatchBlock (node);
    }
  }
}