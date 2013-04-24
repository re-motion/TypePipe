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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Expressions
{
  [TestFixture]
  public class UnboxExpressionTest
  {
    private Expression _operand;
    private Type _type;

    private UnboxExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _operand = ExpressionTreeObjectMother.GetSomeExpression();
      _type = ReflectionObjectMother.GetSomeType();

      _expression = new UnboxExpression (_operand, _type);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.Operand, Is.SameAs (_operand));
      Assert.That (_expression.Type, Is.SameAs (_type));
    }

    [Test]
    public void Accept ()
    {
      ExpressionTestHelper.CheckAccept (_expression, mock => mock.VisitUnbox (_expression));
    }

    [Test]
    public void CreateSimiliar ()
    {
      var newOperand = ExpressionTreeObjectMother.GetSomeExpression();

      var result = _expression.Invoke<UnaryExpressionBase> ("CreateSimiliar", newOperand);

      Assert.That (result, Is.TypeOf<UnboxExpression>());
      Assert.That (result.Type, Is.SameAs (_expression.Type));
      Assert.That (result.Operand, Is.SameAs ((newOperand)));
    }
  }
}