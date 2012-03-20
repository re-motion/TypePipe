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
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class TypeAsUnderlyingSystemTypeExpressionTest
  {
    private MutableType _typeWithUnderlyingSystemType;
    private Expression _innerExpression;

    private TypeAsUnderlyingSystemTypeExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _typeWithUnderlyingSystemType = MutableTypeObjectMother.CreateForExistingType();
      Assert.That (_typeWithUnderlyingSystemType.UnderlyingSystemType, Is.Not.Null.And.Not.SameAs (_typeWithUnderlyingSystemType));
      _innerExpression = Expression.Constant (null, _typeWithUnderlyingSystemType);

      _expression = new TypeAsUnderlyingSystemTypeExpression (_innerExpression);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.InnerExpression, Is.SameAs (_innerExpression));
    }

    [Test]
    public void Type ()
    {
      Assert.That (_expression.Type, Is.SameAs (_typeWithUnderlyingSystemType.UnderlyingSystemType));
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (_expression.NodeType, Is.EqualTo (ExpressionType.Extension));
    }

    [Test]
    public void Accept ()
    {
      var visitorMock = MockRepository.GenerateMock<ITypePipeExpressionVisitor> ();

      _expression.Accept (visitorMock);

      visitorMock.AssertWasCalled (mock => mock.VisitTypeAsUnderlyingSystemTypeExpression (_expression));
    }

    [Test]
    public void VisitChildren ()
    {
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      // Expectation: No calls to expressionVisitorMock.
      var result = ExpressionTestHelper.CallVisitChildren (_expression, expressionVisitorMock);

      Assert.That (result, Is.SameAs (_expression));
    }
  }
}