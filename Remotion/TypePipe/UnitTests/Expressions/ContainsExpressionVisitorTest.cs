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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class ContainsExpressionVisitorTest
  {
    [Test]
    public void Visit_DoesNotContainMatchingNode ()
    {
      var tree = Expression.Block (Expression.Constant (7), Expression.Constant (8));
      var visitor = new ContainsExpressionVisitor (exp => false);

      visitor.Visit (tree);

      Assert.That (visitor.Result, Is.False);
    }

    [Test]
    public void Visit_ContainsMatchingNode ()
    {
      var tree = Expression.Block (Expression.Constant (7), Expression.Constant (8));
      var visitor = new ContainsExpressionVisitor (exp => exp is ConstantExpression && ((ConstantExpression) exp).Value.Equals (8));

      visitor.Visit (tree);

      Assert.That (visitor.Result, Is.True);
    }

    [Test]
    public void Visit_ShortCurcuitEvaluation ()
    {
      var expr = Expression.Constant (8);
      var tree = Expression.Block (Expression.Constant (7), Expression.Block (expr));
      Predicate<Expression> predicate = exp => exp is ConstantExpression && ((ConstantExpression) exp).Value.Equals (7);
      var visitorPartialMock = MockRepository.GeneratePartialMock<ContainsExpressionVisitor> (predicate);

      visitorPartialMock.Visit (tree);

      visitorPartialMock.AssertWasNotCalled (mock => mock.Visit (expr));
    }
  }
}