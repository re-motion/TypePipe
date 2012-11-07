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
  public class ExpressionExtensionsTest
  {
    [Test]
    public void Collect ()
    {
      var expr1 = Expression.Constant ("a");
      var expr2 = Expression.Constant ("b");
      var expr3 = Expression.Constant ("ab");
      var nonDistinct = expr3;
      var tree = Expression.Block (expr1, expr2, expr3, nonDistinct);

      var result = tree.Collect (exp => exp is ConstantExpression && ((ConstantExpression) exp).Value.ToString ().Contains ("a"));

      Assert.That (result, Is.EquivalentTo (new[] { expr1, expr3 }));
    }

    [Test]
    public void Collect_Generic ()
    {
      var expr1 = Expression.Constant (null);
      var expr2 = Expression.Empty();
      var expr3 = Expression.Variable (typeof (int));
      var tree = Expression.Block (new Expression[] { expr1, expr2, expr3 });

      var result1 = tree.Collect<ConstantExpression>();
      var result2 = tree.Collect<ParameterExpression> (x => x.Type == typeof (int));

      Assert.That (result1, Is.EqualTo (new[] { expr1 }));
      Assert.That (result2, Is.EqualTo (new[] { expr3 }));
    }
  }
}