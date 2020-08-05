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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Moq;
using Moq.Protected;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class PrimitiveTypePipeExpressionBaseTest
  {
    private Type _type;

    private Mock<PrimitiveTypePipeExpressionBase> _expressionPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType ();

      _expressionPartialMock = new Mock<PrimitiveTypePipeExpressionBase> (_type) { CallBase = true };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expressionPartialMock.Object.Type, Is.SameAs (_type));
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (_expressionPartialMock.Object.NodeType, Is.EqualTo ((ExpressionType) 1337));
    }

    [Test]
    public void VisitAccept_StandardExpressionVisitor ()
    {
      var expressionVisitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var expectedResult = ExpressionTreeObjectMother.GetSomeExpression();
      expressionVisitorMock
          .Protected()
          .Setup<Expression> ("VisitExtension", _expressionPartialMock.Object)
          .Returns (expectedResult)
          .Verifiable();

      var result = _expressionPartialMock.Object.Invoke ("Accept", expressionVisitorMock.Object);

      expressionVisitorMock.Verify();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void VisitAccept_PrimitiveTypePipeExpressionVisitor ()
    {
      var expressionVisitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict).As<IPrimitiveTypePipeExpressionVisitor>();
      var expectedResult = ExpressionTreeObjectMother.GetSomeExpression();
      _expressionPartialMock.Setup (mock => mock.Accept (expressionVisitorMock.Object)).Returns (expectedResult).Verifiable();

      var result = _expressionPartialMock.Object.Invoke ("Accept", expressionVisitorMock.Object);

      _expressionPartialMock.Verify();
      Assert.That (result, Is.SameAs (expectedResult));
    }
  }
}