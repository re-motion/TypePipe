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
  [TestFixture]
  public class TypePipeExpressionBaseTest
  {
    private Type _type;
    private TypePipeExpressionBase _typePipeExpressionBaseMock;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType ();
      _typePipeExpressionBaseMock = MockRepository.GeneratePartialMock<TypePipeExpressionBase> (_type);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_typePipeExpressionBaseMock.Type, Is.SameAs (_type));
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (_typePipeExpressionBaseMock.NodeType, Is.EqualTo ((ExpressionType) 1337));
    }

    [Test]
    public void VisitChildren_StandardExpressionVisitor ()
    {
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();
      var expectedResult = ExpressionTreeObjectMother.GetSomeExpression();
      expressionVisitorMock
        .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitExtension", _typePipeExpressionBaseMock))
        .Return (expectedResult);

      var result = ExpressionTestHelper.CallAccept (_typePipeExpressionBaseMock, expressionVisitorMock);

      expressionVisitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void VisitChildren_TypePipeExpressionVisitor ()
    {
      // Stubs cannot implement multiple interfaces
      var expressionVisitorMock = MockRepository.GenerateMock<ExpressionVisitor, ITypePipeExpressionVisitor> ();
      var expectedResult = ExpressionTreeObjectMother.GetSomeExpression ();
      _typePipeExpressionBaseMock.Expect (mock => mock.Accept ((ITypePipeExpressionVisitor) expressionVisitorMock)).Return (expectedResult);

      var result = ExpressionTestHelper.CallAccept (_typePipeExpressionBaseMock, expressionVisitorMock);

      _typePipeExpressionBaseMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }
  }
}