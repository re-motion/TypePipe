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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Expressions;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Expressions
{
  [TestFixture]
  public class CodeGenerationExpressionBaseTest
  {
    private Type _type;

    private Mock<CodeGenerationExpressionBase> _expressionPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType();

      _expressionPartialMock = new Mock<CodeGenerationExpressionBase> (_type) { CallBase = true };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expressionPartialMock.Object.Type, Is.SameAs (_type));
    }

    [Test]
    public void Accept_PrimitiveTypePipeExpressionVisitor ()
    {
      var expressionVisitorMock = new Mock<IPrimitiveTypePipeExpressionVisitor> (MockBehavior.Strict);
      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      expressionVisitorMock.Setup (mock => mock.VisitExtension (_expressionPartialMock.Object)).Returns (fakeExpression).Verifiable();

      var result = _expressionPartialMock.Object.Accept (expressionVisitorMock.Object);

      _expressionPartialMock.Verify();
      Assert.That (result, Is.SameAs (fakeExpression));
    }

    [Test]
    public void Accept_CodeGenerationExpressionVisitor ()
    {
      var expressionVisitorMock = new Mock<ICodeGenerationExpressionVisitor> (MockBehavior.Strict);
      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _expressionPartialMock.Setup (mock => mock.Accept (expressionVisitorMock.Object)).Returns (fakeExpression).Verifiable();

      var result = _expressionPartialMock.Object.Accept (expressionVisitorMock.Object);

      _expressionPartialMock.Verify();
      Assert.That (result, Is.SameAs (fakeExpression));
    }
  }
}