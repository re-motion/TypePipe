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
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// An abstract base class for <see cref="ExpressionVisitor"/> implementations that also need to handle <see cref="ITypePipeExpression"/> nodes.
  /// </summary>
  public abstract class TypePipeExpressionVisitorBase : ExpressionVisitor, ITypePipeExpressionVisitor
  {
    Expression ITypePipeExpressionVisitor.VisitThis (ThisExpression expression)
    {
      return VisitThis (expression);
    }

    protected virtual Expression VisitThis (ThisExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return VisitExtension (expression);
    }

    Expression ITypePipeExpressionVisitor.VisitOriginalBody (OriginalBodyExpression expression)
    {
      return VisitOriginalBody (expression);
    }

    protected virtual Expression VisitOriginalBody (OriginalBodyExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return VisitExtension (expression);
    }

    Expression ITypePipeExpressionVisitor.VisitMethodAddress (MethodAddressExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return VisitMethodAddress (expression);
    }

    protected virtual Expression VisitMethodAddress (MethodAddressExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return VisitExtension (expression);
    }
  }
}