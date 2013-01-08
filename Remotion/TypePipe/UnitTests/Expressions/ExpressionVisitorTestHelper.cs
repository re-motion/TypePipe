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
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  public static class ExpressionVisitorTestHelper
  {
    public static Expression CallVisitBinary (ExpressionVisitor expressionVisitor, BinaryExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitBinary", expression);
    }

    public static CatchBlock CallVisitCatchBlock (ExpressionVisitor expressionVisitor, CatchBlock expression)
    {
      return (CatchBlock) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitCatchBlock", expression);
    }

    public static Expression CallVisitConstant (ExpressionVisitor expressionVisitor, ConstantExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitConstant", expression);
    }

    public static Expression CallVisitDynamic (ExpressionVisitor expressionVisitor, DynamicExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitDynamic", expression);
    }

    public static ElementInit CallVisitElementInit (ExpressionVisitor expressionVisitor, ElementInit expression)
    {
      return (ElementInit) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitElementInit", expression);
    }

    public static Expression CallVisitExtension (ExpressionVisitor expressionVisitor, Expression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitExtension", expression);
    }

    public static Expression CallVisitIndex (ExpressionVisitor expressionVisitor, IndexExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitIndex", expression);
    }

    public static Expression CallVisitLambda<T> (ExpressionVisitor expressionVisitor, Expression<T> expression)
    {
      var method = typeof (ExpressionVisitor).GetMethod ("VisitLambda", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod (typeof (T));
      return (Expression) method.Invoke (expressionVisitor, new object[] { expression });
    }

    public static Expression CallVisitMember (ExpressionVisitor expressionVisitor, MemberExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitMember", expression);
    }

    public static Expression CallVisitSwitch (ExpressionVisitor expressionVisitor, SwitchExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitSwitch", expression);
    }

    public static Expression CallVisitMethodCall (ExpressionVisitor expressionVisitor, MethodCallExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitMethodCall", expression);
    }

    public static Expression CallVisitTypeBinary (ExpressionVisitor expressionVisitor, TypeBinaryExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitTypeBinary", expression);
    }

    public static Expression CallVisitUnary (ExpressionVisitor expressionVisitor, UnaryExpression expression)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expressionVisitor, "VisitUnary", expression);
    }
  }
}