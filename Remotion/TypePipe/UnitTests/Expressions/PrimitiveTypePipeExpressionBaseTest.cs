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
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class PrimitiveTypePipeExpressionBaseTest
  {
    private Type _type;

    private PrimitiveTypePipeExpressionBase _expressionPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType ();

      _expressionPartialMock = MockRepository.GeneratePartialMock<PrimitiveTypePipeExpressionBase> (_type);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expressionPartialMock.Type, Is.SameAs (_type));
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (_expressionPartialMock.NodeType, Is.EqualTo ((ExpressionType) 1337));
    }

    [Test]
    public void VisitAccept_StandardExpressionVisitor ()
    {
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor>();
      var expectedResult = ExpressionTreeObjectMother.GetSomeExpression();
      expressionVisitorMock.Expect (mock => mock.Invoke ("VisitExtension", _expressionPartialMock)).Return (expectedResult);

      var result = _expressionPartialMock.Invoke ("Accept", expressionVisitorMock);

      expressionVisitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void VisitAccept_PrimitiveTypePipeExpressionVisitor ()
    {
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor, IPrimitiveTypePipeExpressionVisitor>();
      var expectedResult = ExpressionTreeObjectMother.GetSomeExpression();
      _expressionPartialMock.Expect (mock => mock.Accept ((IPrimitiveTypePipeExpressionVisitor) expressionVisitorMock)).Return (expectedResult);

      var result = _expressionPartialMock.Invoke ("Accept", expressionVisitorMock);

      _expressionPartialMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }
  }
}