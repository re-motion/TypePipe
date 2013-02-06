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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class ThisExpressionTest
  {
    private Type _type;
    private ThisExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType ();
      _expression = new ThisExpression (_type);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.Type, Is.SameAs (_type));
    }

    [Test]
    public void Accept ()
    {
      ExpressionTestHelper.CheckAccept (_expression, mock => mock.VisitThis (_expression));
    }

    [Test]
    public void VisitChildren ()
    {
      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor>();

      // Expectation: No calls to expressionVisitorMock.
      var result = ExpressionTestHelper.CallVisitChildren (_expression, expressionVisitorMock);

      Assert.That (result, Is.SameAs (_expression));
    }
  }
}