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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.TypePipe.Expressions;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class DelegateBasedExpressionVisitorTest
  {
    [Test]
    public void Visit_NoChanges ()
    {
      var visitedExpressions = new List<Expression>();
      Func<Expression, Expression> delegate_ = exp =>
      {
        visitedExpressions.Add (exp);
        return exp;
      };
      var visitor = new DelegateBasedExpressionVisitor (delegate_);

      var expression1 = ExpressionTreeObjectMother.GetSomeExpression();
      var expression2 = ExpressionTreeObjectMother.GetSomeExpression();
      var blockExpression = Expression.Block (expression1, expression2);

      var result = visitor.Visit (blockExpression);

      Assert.That (result, Is.SameAs (blockExpression));
      Assert.That (visitedExpressions, Is.EqualTo (new[] { expression1, expression2, blockExpression }));
    }

    [Test]
    public void Visit_Changes ()
    {
      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      Func<Expression, Expression> delegate_ = exp => fakeExpression;
      var visitor = new DelegateBasedExpressionVisitor (delegate_);

      var expression = ExpressionTreeObjectMother.GetSomeExpression();

      var result = visitor.Visit (expression);

      Assert.That (result, Is.SameAs (fakeExpression));
    }

    [Test]
    public void Visit_Null ()
    {
      var visitor = new DelegateBasedExpressionVisitor (exp => { Assert.Fail ("Should not be called."); return null; });

      var result = visitor.Visit (node: null);

      Assert.That (result, Is.Null);
    }
  }
}