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
  public class ExpressionAnalyzingVisitor : PrimitiveTypePipeExpressionVisitorBase
  {
    private readonly ITypeAnalyzer _typeAnalyzer;

    private bool _isStrongNameCompatible = true;

    public ExpressionAnalyzingVisitor (ITypeAnalyzer typeAnalyzer)
    {
      ArgumentUtility.CheckNotNull ("typeAnalyzer", typeAnalyzer);

      _typeAnalyzer = typeAnalyzer;
    }

    public bool IsStrongNameStrongNameCompatible
    {
      get { return _isStrongNameCompatible; }
    }

    public override Expression Visit (Expression node)
    {
      if (node == null)
        return null;

      return CheckCompatibility (node.Type) ? base.Visit (node) : node;
    }

    protected override CatchBlock VisitCatchBlock (CatchBlock node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckCompatibility (node.Test) ? base.VisitCatchBlock (node) : node;
    }

    protected internal override Expression VisitMember (MemberExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckCompatibility (node.Member.DeclaringType) ? base.VisitMember (node) : node;
    }

    protected internal override Expression VisitMethodCall (MethodCallExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckCompatibility (node.Method.DeclaringType) ? base.VisitMethodCall (node) : node;

      // TODO Review: Also check generic arguments of method.
    }

    private bool CheckCompatibility (Type type)
    {
      var isCompatible = _typeAnalyzer.IsStrongNamed (type);
      if (!isCompatible)
        _isStrongNameCompatible = false;

      return isCompatible;
    }


    // TODO Review: VisitBinary, VisitUnary can also have methods

    // TODO Review: VisitDynamic => DelegateType

    // TODO Review: ElementInit => AddMethod

    // TODO Review: Check remaining expressions for potential strong-naming relevant members.
  }
}