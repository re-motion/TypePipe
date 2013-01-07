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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  /// <summary>
  /// An <see cref="ExpressionVisitor"/> that can be used to check whether or not an <see cref="Expression"/> is strong-name compatible.
  /// </summary>
  public class ExpressionAnalyzingVisitor : PrimitiveTypePipeExpressionVisitorBase
  {
    private readonly ITypeAnalyzer _typeAnalyzer;

    private bool _isStrongNameCompatible = true;

    public ExpressionAnalyzingVisitor (ITypeAnalyzer typeAnalyzer)
    {
      ArgumentUtility.CheckNotNull ("typeAnalyzer", typeAnalyzer);

      _typeAnalyzer = typeAnalyzer;
    }

    public bool IsStrongNameCompatible
    {
      get { return _isStrongNameCompatible; }
    }

    public override Expression Visit (Expression node)
    {
      if (node == null)
        return null;

      return CheckType (node.Type) ? base.Visit (node) : node;
    }

    protected internal override Expression VisitBinary (BinaryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckMethod (node.Method) ? base.VisitBinary (node) : node;
    }

    protected override CatchBlock VisitCatchBlock (CatchBlock node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckType (node.Test) ? base.VisitCatchBlock (node) : node;
    }

    protected internal override Expression VisitMember (MemberExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckMember (node.Member) ? base.VisitMember (node) : node;
    }

    protected internal override Expression VisitMethodCall (MethodCallExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckMethod (node.Method) ? base.VisitMethodCall (node) : node;
    }

    protected internal override Expression VisitUnary (UnaryExpression node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      return CheckMethod (node.Method) ? base.VisitUnary (node) : node;
    }

    private bool CheckType (Type type)
    {
      var isCompatible = _typeAnalyzer.IsStrongNamed (type);
      if (!isCompatible)
        _isStrongNameCompatible = false;

      return isCompatible;
    }

    private bool CheckMember (MemberInfo member)
    {
      return CheckType (member.DeclaringType);
    }

    private bool CheckMethod (MethodInfo method)
    {
      return method == null || CheckMember (method) && method.GetGenericArguments().All (CheckType);
    }

    // TODO Review: VisitDynamic => DelegateType

    // TODO Review: ElementInit => AddMethod

    // TODO Review: Check remaining expressions for potential strong-naming relevant members.
  }
}