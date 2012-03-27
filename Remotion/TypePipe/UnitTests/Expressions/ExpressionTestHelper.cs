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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  public static class ExpressionTestHelper
  {
    public static Expression CallVisitChildren (Expression expression, ExpressionVisitor expressionVisitorMock)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expression, "VisitChildren", expressionVisitorMock);
    }

    public static Expression CallAccept (Expression expression, ExpressionVisitor expressionVisitorMock)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expression, "Accept", expressionVisitorMock);
    }

    public static void CheckAccept (ITypePipeExpression expression, Function<ITypePipeExpressionVisitor, Expression> expectation)
    {
      var visitorMock = MockRepository.GenerateMock<ITypePipeExpressionVisitor>();
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
      CheckVisitChildren_NoChanges (parentExpression, (IEnumerable<Expression>) childExpressions);
    }

    public static void CheckVisitChildren_NoChanges (Expression parentExpression, IEnumerable<Expression> childExpressions)
    {
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor>();
      foreach (var childExpression in childExpressions)
      {
        var childExpressionCopy = childExpression;
        expressionVisitorMock.Expect (mock => mock.Visit (childExpressionCopy)).Return (childExpressionCopy);
      }

      var result = CallVisitChildren (parentExpression, expressionVisitorMock);

      expressionVisitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (parentExpression));
    }
  }
}