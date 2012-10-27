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

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class CollectingExpressionVisitorTest
  {
    [Test]
    public void Visit_ContainsMatchingNode ()
    {
      var expr1 = Expression.Constant ("a");
      var expr2 = Expression.Constant ("b");
      var expr3 = Expression.Constant ("ab");
      var tree = Expression.Block (expr1, expr2, expr3);
      var visitor = new CollectingExpressionVisitor (exp => exp is ConstantExpression && ((ConstantExpression) exp).Value.ToString().Contains ("a"));

      visitor.Visit (tree);

      Assert.That (visitor.MatchingNodes, Is.EqualTo (new[] { expr1, expr3 }));
    }
  }
}