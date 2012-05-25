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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class OriginalBodyExpressionTest
  {
    private MethodBase _methodBase;
    private Type _returnType;
    private IEnumerable<Expression> _argumentExpressions;
    private OriginalBodyExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _methodBase = ReflectionObjectMother.GetSomeMethod();
      _returnType = ReflectionObjectMother.GetSomeType();
      _argumentExpressions = new ArgumentTestHelper (7, "string").Expressions;

      _expression = new OriginalBodyExpression (_methodBase, _returnType, _argumentExpressions.AsOneTime()); 
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.Type, Is.SameAs (_returnType));
      Assert.That (_expression.MethodBase, Is.SameAs (_methodBase));
      Assert.That (_expression.Arguments, Is.EqualTo (_argumentExpressions));
    }

    [Test]
    public void Accept ()
    {
      ExpressionTestHelper.CheckAccept (_expression, mock => mock.VisitOriginalBody (_expression));
    }

    [Test]
    public void VisitChildren_NoChanges ()
    {
      ExpressionTestHelper.CheckVisitChildren_NoChanges (_expression, _expression.Arguments);
    }

    [Test]
    public void VisitChildren_WithChanges ()
    {
      var newInnerExpression = ExpressionTreeObjectMother.GetSomeExpression ();

      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();
      expressionVisitorMock.Expect (mock => mock.Visit (_expression.Arguments[0])).Return (newInnerExpression);
      expressionVisitorMock.Expect (mock => mock.Visit (_expression.Arguments[1])).Return (_expression.Arguments[1]);

      var result = ExpressionTestHelper.CallVisitChildren (_expression, expressionVisitorMock);

      expressionVisitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (result.Type, Is.SameAs (_expression.Type));
      Assert.That (result, Is.TypeOf<OriginalBodyExpression> ());

      var originalBodyExpression = (OriginalBodyExpression) result;
      Assert.That (originalBodyExpression.MethodBase, Is.SameAs (_methodBase));
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (new[] { newInnerExpression, _expression.Arguments[1] }));
    }
  }
}