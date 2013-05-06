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
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.Implementation.ReflectionEmit.Expressions
{
  [TestFixture]
  public class UnaryExpressionBaseTest
  {
    private Expression _operand;
    private Type _type;

    private UnaryExpressionBase _expressionPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _operand = ExpressionTreeObjectMother.GetSomeExpression();
      _type = ReflectionObjectMother.GetSomeType();

      _expressionPartialMock = MockRepository.GeneratePartialMock<UnaryExpressionBase> (_operand, _type);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expressionPartialMock.Operand, Is.SameAs (_operand));
      Assert.That (_expressionPartialMock.Type, Is.SameAs (_type));
    }

    [Test]
    public void Update_NoChanges ()
    {
      var result = _expressionPartialMock.Update (_operand);

      Assert.That (result, Is.SameAs (_expressionPartialMock));
    }

    [Test]
    public void Update_WithChanges ()
    {
      var newOperand = ExpressionTreeObjectMother.GetSomeExpression();
      var fakeResult = MockRepository.GenerateStrictMock<UnaryExpressionBase>(_operand, _type);
      _expressionPartialMock.Expect (mock => mock.Invoke ("CreateSimiliar", newOperand)).Return (fakeResult);

      var result = _expressionPartialMock.Update (newOperand);

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
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor>();
      var fakeOperand = ExpressionTreeObjectMother.GetSomeExpression();
      var fakeResult = MockRepository.GenerateStub<UnaryExpressionBase> (_operand, _type);
      expressionVisitorMock.Expect (mock => mock.Visit (_operand)).Return (fakeOperand);
      _expressionPartialMock.Expect (mock => mock.Update (fakeOperand)).Return (fakeResult);

      var result = _expressionPartialMock.Invoke ("VisitChildren", expressionVisitorMock);

      expressionVisitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}