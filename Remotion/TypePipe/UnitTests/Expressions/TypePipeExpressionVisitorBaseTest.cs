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
    public void Visit_XXX ()
    {
      var thisExpression = ExpressionTreeObjectMother.GetSomeThisExpression();
      CheckDefaultVisitImplementation (
          thisExpression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitThis (mock, thisExpression),
          visitor => visitor.VisitThis (thisExpression));

      var originalBodyExpression = ExpressionTreeObjectMother.GetSomeOriginalBodyExpression();
      CheckDefaultVisitImplementation (
          originalBodyExpression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (mock, originalBodyExpression),
          visitor => visitor.VisitOriginalBody (originalBodyExpression));

      var methodAddressExpression = ExpressionTreeObjectMother.GetSomeMethodAddressExpression();
      CheckDefaultVisitImplementation (
          methodAddressExpression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitMethodAddress (mock, methodAddressExpression),
          visitor => visitor.VisitMethodAddress (methodAddressExpression));

      var virtualMethodAddressExpression = ExpressionTreeObjectMother.GetSomeVirtualMethodAddressExpression();
      CheckDefaultVisitImplementation (
          virtualMethodAddressExpression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitVirtualMethodAddress (mock, virtualMethodAddressExpression),
          visitor => visitor.VisitVirtualMethodAddress (virtualMethodAddressExpression));

      var newDelegateExpression = ExpressionTreeObjectMother.GetSomeNewDelegateExpression ();
      CheckDefaultVisitImplementation (
          newDelegateExpression,
          mock => TypePipeExpressionVisitorTestHelper.CallVisitNewDelegate (mock, newDelegateExpression),
          visitor => visitor.VisitNewDelegate (newDelegateExpression));
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