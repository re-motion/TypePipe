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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class ExpressionTest
  {
    [Test]
    public void NewDelegate ()
    {
      var delegateType = typeof (Action);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method());
      var target = ExpressionTreeObjectMother.GetSomeExpression (method.DeclaringType);

      var result = Expression.NewDelegate (delegateType, target, method);

      Assert.That (result.Type, Is.SameAs (delegateType));
      Assert.That (result.Target, Is.SameAs (target));
      Assert.That (result.Method, Is.SameAs (method));
    }

    [Test]
    public void NewDelegate_StaticMethod ()
    {
      var delegateType = typeof (Action);
      var staticMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => StaticMethod());

      var result = Expression.NewDelegate (delegateType, null, staticMethod);

      Assert.That (result.Target, Is.Null);
    }

    [Test]
    public void ArrayConstant ()
    {
      var constantValues = new IComparable[] { "7" };

      var result = Expression.ArrayConstant (constantValues.AsOneTime());

      Assert.That (result.Type, Is.SameAs (typeof (IComparable[])));
      var constantValue = (ConstantExpression) result.Expressions.Single();
      Assert.That (constantValue.Type, Is.SameAs (typeof (IComparable)));
      Assert.That (constantValue.Value, Is.EqualTo ("7"));
    }

    [Test]
    public void BlockOrEmpty_NonEmpty ()
    {
      var expressions = new[] { ExpressionTreeObjectMother.GetSomeExpression() };

      var result = Expression.BlockOrEmpty (expressions.AsOneTime());

      var block = (BlockExpression) result;
      Assert.That (block.Expressions, Is.EqualTo (expressions));
    }

    [Test]
    public void BlockOrEmpty_Empty ()
    {
      var result = Expression.BlockOrEmpty (new Expression[0]);

      Assert.That (result, Is.TypeOf<DefaultExpression>().And.Property ("Type").SameAs (typeof (void)));
    }

    void Method () { }
    static void StaticMethod () { }
  }
}