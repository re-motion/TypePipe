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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class OriginalBodyReplacingExpressionVisitorTest
  {
    private MutableType _declaringType;
    private IMutableMethodBase _mutableMethodStub;

    private OriginalBodyReplacingExpressionVisitor _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainClass));
      _mutableMethodStub = MockRepository.GenerateStub<IMutableMethodBase>();
      _mutableMethodStub.Stub (stub => stub.DeclaringType).Return (_declaringType);

      _visitorPartialMock = MockRepository.GeneratePartialMock<OriginalBodyReplacingExpressionVisitor> (_mutableMethodStub);
    }

    [Test]
    public void VisitOriginalBody_Method ()
    {
      var methodBase = MemberInfoFromExpressionUtility.GetMethod ((DomainClass obj) => obj.Method (7, "string"));
      var arguments = new ArgumentTestHelper (7, "string").Expressions;
      var expression = new OriginalBodyExpression (methodBase, typeof (double), arguments);
      Action<MethodInfo> checkMethodInCallExpressionAction =
          methodInfo => Assert.That (methodInfo, Is.TypeOf<BaseCallMethodInfoAdapter>().And.Property ("AdaptedMethodInfo").SameAs (methodBase));

      CheckVisitOriginalBody (expression, _declaringType, arguments, checkMethodInCallExpressionAction);
    }

    [Test]
    public void VisitOriginalBody_Constructor ()
    {
      var methodBase = MemberInfoFromExpressionUtility.GetConstructor (() => new DomainClass());
      var arguments = new Expression[0];
      var expression = new OriginalBodyExpression (methodBase, typeof (void), arguments);
      Action<MethodInfo> checkMethodInCallExpressionAction = methodInfo =>
      {
        Assert.That (methodInfo, Is.TypeOf<BaseCallMethodInfoAdapter> ());
        var baseCallMethodInfoAdapter = (BaseCallMethodInfoAdapter) methodInfo;
        Assert.That (baseCallMethodInfoAdapter.AdaptedMethodInfo, Is.TypeOf<ConstructorAsMethodInfoAdapter>());
        var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) baseCallMethodInfoAdapter.AdaptedMethodInfo;
        Assert.That (constructorAsMethodInfoAdapter.ConstructorInfo, Is.SameAs (methodBase));
      };

      CheckVisitOriginalBody (expression,_declaringType,arguments, checkMethodInCallExpressionAction);
    }

    private void CheckVisitOriginalBody (
        OriginalBodyExpression expression,
        MutableType expectedDeclaringType,
        Expression[] expectedMethodCallArguments,
        Action<MethodInfo> checkMethodInCallExpressionAction)
    {
      var fakeResult = ExpressionTreeObjectMother.GetSomeExpression();
      _visitorPartialMock
          .Expect (mock => mock.Visit (Arg<Expression>.Is.Anything))
          .Return (fakeResult)
          .WhenCalled (
              mi =>
              {
                Assert.That (mi.Arguments[0], Is.InstanceOf<MethodCallExpression>());
                var methodCallExpression = (MethodCallExpression) mi.Arguments[0];
                Assert.That (methodCallExpression.Object, Is.TypeOf<TypeAsUnderlyingSystemTypeExpression>());

                var typeAsUnderlyingSystemTypeExpression = ((TypeAsUnderlyingSystemTypeExpression) methodCallExpression.Object);
                Assert.That (
                    typeAsUnderlyingSystemTypeExpression.InnerExpression,
                    Is.TypeOf<ThisExpression>().With.Property ("Type").SameAs (expectedDeclaringType));

                checkMethodInCallExpressionAction (methodCallExpression.Method);

                Assert.That (methodCallExpression.Arguments, Is.EqualTo (expectedMethodCallArguments));
              });

      var result = TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (_visitorPartialMock, expression);

      _visitorPartialMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    private void CheckThrow (IMutableMethodBase mutableMethodStub)
    {
      var visitor = new OriginalBodyReplacingExpressionVisitor (mutableMethodStub);

      var expression = new OriginalBodyExpression (MemberInfoFromExpressionUtility.GetMethod ((DomainClass obj) => obj.Method (7, "string")), typeof (void), Enumerable.Empty<Expression>());

      var expectedMessage = string.Format (
          "The body of an added or static member ('{0}', declared for mutable type '{1}') must not contain an OriginalBodyExpression.",
          mutableMethodStub,
          mutableMethodStub.DeclaringType.Name);
      Assert.That (
          () => TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (visitor, expression),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (expectedMessage));
    }

    public class DomainClass
    {
      public double Method (int p1, string p2)
      {
        Dev.Null = p1;
        Dev.Null = p2;
        return 7.7;
      }
    }
  }
}