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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Expressions;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  public static class TypePipeExpressionVisitorTestHelper
  {
    public static Expression CallVisitThis (TypePipeExpressionVisitorBase expressionVisitor, ThisExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitThis", expression);
    }

    public static Expression CallVisitOriginalBody (TypePipeExpressionVisitorBase expressionVisitor, OriginalBodyExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitOriginalBody", expression);
    }

    public static Expression CallVisitMethodAddress (TypePipeExpressionVisitorBase expressionVisitor, MethodAddressExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitMethodAddress", expression);
    }

    public static Expression CallVisitVirtualMethodAddress (TypePipeExpressionVisitorBase expressionVisitor, VirtualMethodAddressExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitVirtualMethodAddress", expression);
    }

    public static Expression CallVisitNewDelegate (TypePipeExpressionVisitorBase expressionVisitor, NewDelegateExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitNewDelegate", expression);
    }
  }
}