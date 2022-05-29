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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.UnitTests.Expressions;
using Moq;
using Moq.Protected;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Expressions
{
  [TestFixture]
  public class UnaryExpressionBaseTest
  {
    private Expression _operand;
    private Type _type;

    private Mock<UnaryExpressionBase> _expressionPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _operand = ExpressionTreeObjectMother.GetSomeExpression();
      _type = ReflectionObjectMother.GetSomeType();

      _expressionPartialMock = new Mock<UnaryExpressionBase> (_operand, _type) { CallBase = true };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expressionPartialMock.Object.Operand, Is.SameAs (_operand));
      Assert.That (_expressionPartialMock.Object.Type, Is.SameAs (_type));
    }

    [Test]
    public void Update_NoChanges ()
    {
      var result = _expressionPartialMock.Object.Update (_operand);

      Assert.That (result, Is.SameAs (_expressionPartialMock.Object));
    }

    [Test]
    public void Update_WithChanges ()
    {
      var newOperand = ExpressionTreeObjectMother.GetSomeExpression();
      var fakeResult = new Mock<UnaryExpressionBase> (_operand, _type).Object;
      _expressionPartialMock
          .Protected()
          .Setup<UnaryExpressionBase> ("CreateSimiliar", newOperand)
          .Returns (fakeResult)
          .Verifiable();

      var result = _expressionPartialMock.Object.Update (newOperand);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitChildren_NoChanges ()
    {
      var expression = new BoxAndCastExpression (_operand, _type);
      ExpressionTestHelper.CheckVisitChildren_NoChanges (expression, expression.Operand);
    }

    [Test]
    public void VisitChildren_WithChanges ()
    {
      var expressionVisitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var fakeOperand = ExpressionTreeObjectMother.GetSomeExpression();
      var fakeResult = new Mock<UnaryExpressionBase> (_operand, _type).Object;
      expressionVisitorMock.Setup (mock => mock.Visit (_operand)).Returns (fakeOperand).Verifiable();
      _expressionPartialMock.Protected().Setup<UnaryExpressionBase> ("CreateSimiliar", fakeOperand).Returns (fakeResult).Verifiable();

      var result = _expressionPartialMock.Object.Invoke ("VisitChildren", expressionVisitorMock.Object);

      expressionVisitorMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}