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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class OriginalBodyReplacingExpressionVisitorTest
  {
    private MutableType _declaringType;
    private IMutableMethodBase _mutableMethodStub;
    private MethodInfo _methodRepresentingOriginalBody;

    private OriginalBodyReplacingExpressionVisitor _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainClass));
      _mutableMethodStub = MockRepository.GenerateStub<IMutableMethodBase>();
      _mutableMethodStub.Stub (stub => stub.DeclaringType).Return (_declaringType);
      _methodRepresentingOriginalBody = MemberInfoFromExpressionUtility.GetMethod ((DomainClass obj) => obj.Method (7, "string"));

      _visitorPartialMock = MockRepository.GeneratePartialMock<OriginalBodyReplacingExpressionVisitor> (
          _mutableMethodStub, 
          _methodRepresentingOriginalBody);
    }

    [Test]
    public void VisitOriginalBody ()
    {
      var arguments = new ArgumentTestHelper (7, "string").Expressions;
      var expression = new OriginalBodyExpression (typeof (void), arguments);
      var fakeResult = ExpressionTreeObjectMother.GetSomeExpression();

      _mutableMethodStub.Stub (stub => stub.IsNew).Return (false);
      _mutableMethodStub.Stub (stub => stub.IsStatic).Return (false);

      _visitorPartialMock
          .Expect (mock => mock.Visit (Arg<Expression>.Matches (e => e is MethodCallExpression)))
          .Return (fakeResult)
          .WhenCalled (mi =>
          {
            var methodCallExpression = (MethodCallExpression) mi.Arguments[0];
            Assert.That (methodCallExpression.Object, Is.TypeOf<TypeAsUnderlyingSystemTypeExpression> ());
            var typeAsUnderlyingSystemTypeExpression = ((TypeAsUnderlyingSystemTypeExpression) methodCallExpression.Object);
            Assert.That (
                typeAsUnderlyingSystemTypeExpression.InnerExpression,
                Is.TypeOf<ThisExpression>().With.Property ("Type").SameAs (_declaringType));
            Assert.That (
                methodCallExpression.Method, 
                Is.SameAs (_methodRepresentingOriginalBody));
            Assert.That (methodCallExpression.Arguments, Is.EqualTo (arguments));
          });

      var result = TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (_visitorPartialMock, expression);

      _visitorPartialMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitOriginalBody_WithNewMember ()
    {
      _mutableMethodStub.Stub (stub => stub.IsNew).Return (true);
      _mutableMethodStub.Stub (stub => stub.IsStatic).Return (false);

      var visitorPartialMock = new OriginalBodyReplacingExpressionVisitor (_mutableMethodStub, _methodRepresentingOriginalBody);

      var expression = new OriginalBodyExpression (typeof (void), Enumerable.Empty<Expression>());

      var expectedMessage = string.Format (
          "The body of an added or static member ('{0}', declared for mutable type 'DomainClass') must not contain an OriginalBodyExpression.", 
          _mutableMethodStub);
      Assert.That (
          () => TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (visitorPartialMock, expression),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (expectedMessage));
    }

    [Test]
    public void VisitOriginalBody_WithStaticMember ()
    {
      _mutableMethodStub.Stub (stub => stub.IsNew).Return (false);
      _mutableMethodStub.Stub (stub => stub.IsStatic).Return (true);

      var visitorPartialMock = new OriginalBodyReplacingExpressionVisitor (_mutableMethodStub, _methodRepresentingOriginalBody);

      var expression = new OriginalBodyExpression (typeof (void), Enumerable.Empty<Expression> ());

      var expectedMessage = string.Format (
          "The body of an added or static member ('{0}', declared for mutable type 'DomainClass') must not contain an OriginalBodyExpression.",
          _mutableMethodStub);
      Assert.That (
          () => TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (visitorPartialMock, expression),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (expectedMessage));
    }

    public class DomainClass
    {
// ReSharper disable UnusedParameter.Local
      public void Method (int p1, string p2)
// ReSharper restore UnusedParameter.Local
      {
        throw new NotImplementedException();
      }
    }
  }
}