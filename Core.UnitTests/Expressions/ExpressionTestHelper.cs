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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Moq;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  public static class ExpressionTestHelper
  {
    public static void CheckAccept (
        IPrimitiveTypePipeExpression expression,
        System.Linq.Expressions.Expression<Func<IPrimitiveTypePipeExpressionVisitor, Expression>> expectation)
    {
      var visitorMock = new Mock<IPrimitiveTypePipeExpressionVisitor>();
      var visitorResult = ExpressionTreeObjectMother.GetSomeExpression();
      visitorMock.Setup (expectation).Returns (visitorResult).Verifiable();

      var result = expression.Accept (visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (visitorResult));
    }

    public static void CheckAccept (
        ICodeGenerationExpression expression,
        System.Linq.Expressions.Expression<Func<ICodeGenerationExpressionVisitor, Expression>> expectation)
    {
      var visitorMock = new Mock<ICodeGenerationExpressionVisitor>();
      var visitorResult = ExpressionTreeObjectMother.GetSomeExpression();
      visitorMock.Setup (expectation).Returns (visitorResult).Verifiable();

      var result = expression.Accept (visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (visitorResult));
    }

    public static void CheckVisitChildren_NoChanges (Expression parentExpression, params Expression[] childExpressions)
    {
      var expressionVisitor = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      foreach (var childExpression in childExpressions)
      {
        var childExpressionCopy = childExpression;
        expressionVisitor.Setup (mock => mock.Visit (childExpressionCopy)).Returns (childExpressionCopy).Verifiable();
      }

      var result = parentExpression.Invoke ("VisitChildren", expressionVisitor.Object);

      expressionVisitor.Verify();
      Assert.That (result, Is.SameAs (parentExpression));
    }
  }
}