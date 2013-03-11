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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  public static class ExpressionTestHelper
  {
    public static Expression CallVisitChildren (Expression expression, ExpressionVisitor expressionVisitor)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expression, "VisitChildren", expressionVisitor);
    }

    public static Expression CallAccept (Expression expression, ExpressionVisitor expressionVisitor)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expression, "Accept", expressionVisitor);
    }

    public static void CheckAccept (IPrimitiveTypePipeExpression expression, Function<IPrimitiveTypePipeExpressionVisitor, Expression> expectation)
    {
      var visitorMock = MockRepository.GenerateMock<IPrimitiveTypePipeExpressionVisitor>();
      var visitorResult = ExpressionTreeObjectMother.GetSomeExpression();
      visitorMock
          .Expect (expectation)
          .Return (visitorResult);

      var result = expression.Accept (visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (visitorResult));
    }

    public static void CheckVisitChildren_NoChanges (Expression parentExpression, params Expression[] childExpressions)
    {
      var expressionVisitor = MockRepository.GenerateStrictMock<ExpressionVisitor>();
      foreach (var childExpression in childExpressions)
      {
        var childExpressionCopy = childExpression;
        expressionVisitor.Expect (mock => mock.Visit (childExpressionCopy)).Return (childExpressionCopy);
      }

      var result = CallVisitChildren (parentExpression, expressionVisitor);

      expressionVisitor.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (parentExpression));
    }
  }
}