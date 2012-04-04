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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.Expressions;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class ReplacingExpressionVisitorTest
  {
    [Test]
    public void Visit_NoModifications ()
    {
      var replacedExpr = Expression.Constant ("I am not present in the original tree.");
      var replacingExpr = Expression.Constant ("I are baboon!");
      var originalTree = Expression.Block (Expression.Constant("test"), Expression.Constant (7));
      var expectedTree = originalTree;
      var visitor = CreateVisitor (replacedExpr, replacingExpr);

      var actualTree = visitor.Visit (originalTree);

      Assert.That (actualTree.DebugView, Is.EqualTo (expectedTree.DebugView));
    }

    [Test]
    public void Visit_Modifications ()
    {
      var replacedExpr = Expression.Constant ("Replace me!");
      var replacingExpr = Expression.Constant ("I are baboon!");
      var originalTree = Expression.Block (replacedExpr, Expression.Block (replacedExpr), Expression.Constant (7));
      var expectedTree = Expression.Block (replacingExpr, Expression.Block (replacingExpr), Expression.Constant (7));
      Assert.That (originalTree.DebugView, Is.Not.EqualTo (expectedTree.DebugView));
      var visitor = CreateVisitor (replacedExpr, replacingExpr);

      var actualTree = visitor.Visit (originalTree);

      Assert.That (actualTree.DebugView, Is.EqualTo (expectedTree.DebugView));
    }

    [Test]
    public void Visit_NonRecursively ()
    {
      var replacedExpr = Expression.Constant ("Only replace me in original!");
      var replacingExpr = Expression.Block (replacedExpr); // Replacing expression contains replaced expression
      var originalTree = replacedExpr;
      var expectedTree = replacingExpr;
      Assert.That (originalTree.DebugView, Is.Not.EqualTo (expectedTree.DebugView));
      var visitor = CreateVisitor (replacedExpr, replacingExpr);

      var actualTree = visitor.Visit (originalTree);

      Assert.That (actualTree.DebugView, Is.EqualTo (expectedTree.DebugView));
    }

    private ReplacingExpressionVisitor CreateVisitor (Expression replacedExpr, Expression replacingExpr)
    {
      var replacements = new Dictionary<Expression, Expression> { { replacedExpr, replacingExpr } };
      return new ReplacingExpressionVisitor (replacements);
    }
  }
}