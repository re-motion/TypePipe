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
using Rhino.Mocks.Interfaces;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class TypePipeExpressionVisitorBaseTest
  {
    [Test]
    public void VisitThis ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeThisExpression();

      CheckDefaultVisitImplementation (
          expression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitThis (mock, expression),
          visitor => visitor.VisitThis (expression));
    }

    [Test]
    public void VisitOriginalBody ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeOriginalBodyExpression ();

      CheckDefaultVisitImplementation (
          expression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (mock, expression),
          visitor => visitor.VisitOriginalBody (expression));
    }

    [Test]
    public void VisitMethodAddress ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeMethodAddressExpression ();

      CheckDefaultVisitImplementation (
          expression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitMethodAddress (mock, expression),
          visitor => visitor.VisitMethodAddress (expression));
    }

    [Test]
    public void VisitVirtualMethodAddress ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeVirtualMethodAddressExpression ();

      CheckDefaultVisitImplementation (
          expression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitVirtualMethodAddress (mock, expression),
          visitor => visitor.VisitVirtualMethodAddress (expression));
    }

    private void CheckDefaultVisitImplementation<T> (
      T expression,
      Function<TypePipeExpressionVisitorBase, Expression> expectedVisitMethod,
      Func<ITypePipeExpressionVisitor, Expression> invokedMethod)
        where T : Expression
    {
      var fakeResult = ExpressionTreeObjectMother.GetSomeExpression();

      var visitorBaseMock = MockRepository.GenerateStrictMock <TypePipeExpressionVisitorBase> ();
      visitorBaseMock
          .Expect (expectedVisitMethod)
          .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      visitorBaseMock
          .Expect (mock => ExpressionVisitorTestHelper.CallVisitExtension(mock, expression))
          .Return (fakeResult);

      var result = invokedMethod ((ITypePipeExpressionVisitor) visitorBaseMock);

      visitorBaseMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}