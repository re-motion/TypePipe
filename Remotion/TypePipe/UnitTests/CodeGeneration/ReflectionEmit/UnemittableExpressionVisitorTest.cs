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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class UnemittableExpressionVisitorTest
  {
    private IEmittableOperandProvider _emittableOperandProviderMock;

    private UnemittableExpressionVisitor _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _visitorPartialMock = MockRepository.GeneratePartialMock<UnemittableExpressionVisitor>(_emittableOperandProviderMock);
    }

    [Test]
    public void VisitConstant_ReplacesValue ()
    {
      var expression = Expression.Constant ("operand", typeof(object));
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableOperand ("operand")).Return ("emittable");

      var result = ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);

      _emittableOperandProviderMock.VerifyAllExpectations();
      Assert.That (result, Is.AssignableTo<ConstantExpression>());
      var constantExpression = (ConstantExpression) result;
      Assert.That (constantExpression.Value, Is.EqualTo("emittable"));
      Assert.That (constantExpression.Type, Is.SameAs (typeof (object)));
    }

    [Test]
    public void VisitConstant_SameValue ()
    {
      var value = "emittable";
      var expression = Expression.Constant (value, typeof (object));
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableOperand (value)).Return (value);

      var result = ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);

      _emittableOperandProviderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), MatchType = MessageMatch.StartsWith, ExpectedMessage =
        "It is not supported to have a ConstantExpression of type 'String' because instances of 'String' exist only at " +
        "code generation time, not at runtime.")]
    public void VisitConstant_NotAssignableValue ()
    {
      var expresison = Expression.Constant ("operand");
      _emittableOperandProviderMock.Stub (stub => stub.GetEmittableOperand ("operand")).Return (7);

      ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expresison);
    }

    [Test]
    public void VisitConstant_NullValue ()
    {
      var expression = Expression.Constant (null);

      var result = ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);

      _emittableOperandProviderMock.AssertWasNotCalled (mock => mock.GetEmittableOperand (Arg<object>.Is.Anything));
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_StaticClosure ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression();
      var parameter = Expression.Parameter (typeof (int));      
      var expression = Expression.Lambda<Action<int>> (body, parameter);
      var fakeStaticClosure = ExpressionTreeObjectMother.GetSomeExpression();
      _visitorPartialMock.Expect (mock => mock.Visit (body)).Return (fakeStaticClosure);

      var result = ExpressionVisitorTestHelper.CallVisitLambda (_visitorPartialMock, expression);

      Assert.That (result, Is.InstanceOf<LambdaExpression>());
      var lambdaExpression = ((LambdaExpression) result);
      Assert.That (lambdaExpression.Parameters, Is.EqualTo (new[] { parameter }));
      Assert.That (lambdaExpression.Body, Is.SameAs (fakeStaticClosure));
    }

    [Test]
    public void VisitLambda_InstanceClosure ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var parameter = Expression.Parameter (typeof (int));
      var expression = Expression.Lambda<Action<int>> (body, parameter);
      var type = ReflectionObjectMother.GetSomeType();
      var fakeInstanceClosure = new ThisExpression (type);
      _visitorPartialMock.Expect (mock => mock.Visit (body)).Return (fakeInstanceClosure);

      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();
      var thisClosure = Expression.Parameter (type, "thisClosure");
      var expectedTree =
          Expression.Block (
              new[] { thisClosure },
              Expression.Assign (thisClosure, new ThisExpression (type)),
              Expression.Lambda<Action<int>> (thisClosure, parameter));
      _visitorPartialMock
          .Expect (mock => mock.Visit (Arg<Expression>.Is.Anything))
          .WhenCalled (mi => ExpressionTreeComparer.CheckAreEqualTrees (expectedTree, (Expression) mi.Arguments[0]))
          .Return (fakeExpression);

      var result = ExpressionVisitorTestHelper.CallVisitLambda (_visitorPartialMock, expression);

      Assert.That (result, Is.SameAs (fakeExpression));
    }

    [Test]
    public void VisitOriginalBody_Method ()
    {
      var methodBase = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainClass obj) => obj.Method (7, "string"));
      var arguments = new ArgumentTestHelper (7, "string").Expressions;
      var expression = new OriginalBodyExpression (methodBase, typeof (double), arguments);
      Action<MethodInfo> checkMethodInCallExpressionAction =
          methodInfo => Assert.That (methodInfo, Is.TypeOf<NonVirtualCallMethodInfoAdapter>().And.Property ("AdaptedMethodInfo").SameAs (methodBase));

      CheckVisitOriginalBodyForInstanceMethod (expression, arguments, checkMethodInCallExpressionAction);
    }

    [Test]
    public void VisitOriginalBody_StaticMethod ()
    {
      var methodBase = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainClass.StaticMethod (7, "string"));
      var arguments = new ArgumentTestHelper (7, "string").Expressions;
      var expression = new OriginalBodyExpression (methodBase, typeof (double), arguments);
      Action<MethodCallExpression> checkMethodCallExpressionAction = methodCallExpression =>
      {
        Assert.That (methodCallExpression.Object, Is.Null);
        Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>().And.Property ("AdaptedMethodInfo").SameAs (methodBase));
      };

      CheckVisitOriginalBody (expression, arguments, checkMethodCallExpressionAction);
    }

    [Test]
    public void VisitOriginalBody_Constructor ()
    {
      var methodBase = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainClass());
      var arguments = new Expression[0];
      var expression = new OriginalBodyExpression (methodBase, typeof (void), arguments);
      Action<MethodInfo> checkMethodInCallExpressionAction = methodInfo =>
      {
        Assert.That (methodInfo, Is.TypeOf<NonVirtualCallMethodInfoAdapter> ());
        var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) methodInfo;
        Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethodInfo, Is.TypeOf<ConstructorAsMethodInfoAdapter>());
        var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) nonVirtualCallMethodInfoAdapter.AdaptedMethodInfo;
        Assert.That (constructorAsMethodInfoAdapter.ConstructorInfo, Is.SameAs (methodBase));
      };

      CheckVisitOriginalBodyForInstanceMethod (expression,arguments, checkMethodInCallExpressionAction);
    }

    [Test]
    public void VisitOriginalBody_StaticConstructor ()
    {
      var methodBase = typeof (DomainClass).GetConstructors (BindingFlags.NonPublic | BindingFlags.Static).Single();
      var arguments = new Expression[0];
      var expression = new OriginalBodyExpression (methodBase, typeof (void), arguments);
      Action<MethodCallExpression> checkMethodInCallExpressionAction = methodCallExpression =>
      {
        Assert.That (methodCallExpression.Object, Is.Null);

        Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter> ());
        var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) methodCallExpression.Method;
        Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethodInfo, Is.TypeOf<ConstructorAsMethodInfoAdapter> ());
        var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) nonVirtualCallMethodInfoAdapter.AdaptedMethodInfo;
        Assert.That (constructorAsMethodInfoAdapter.ConstructorInfo, Is.SameAs (methodBase));
      };

      CheckVisitOriginalBody (expression, arguments, checkMethodInCallExpressionAction);
    }

    private void CheckVisitOriginalBodyForInstanceMethod (
        OriginalBodyExpression expression, Expression[] expectedMethodCallArguments, Action<MethodInfo> checkMethodInCallExpressionAction)
    {
      CheckVisitOriginalBody (expression, expectedMethodCallArguments, methodCallExpression =>
      {
        Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression>().With.Property ("Type").SameAs (typeof (DomainClass)));

        checkMethodInCallExpressionAction (methodCallExpression.Method);
      });
    }

    private void CheckVisitOriginalBody (
        OriginalBodyExpression expression, Expression[] expectedMethodCallArguments, Action<MethodCallExpression> checkMethodCallExpressionAction)
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

                checkMethodCallExpressionAction (methodCallExpression);
                Assert.That (methodCallExpression.Arguments, Is.EqualTo (expectedMethodCallArguments));
              });

      var result = TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (_visitorPartialMock, expression);

      _visitorPartialMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    public class DomainClass
    {
// ReSharper disable EmptyConstructor
      static DomainClass () { }
// ReSharper restore EmptyConstructor

      public static double StaticMethod (int p1, string p2)
      {
        Dev.Null = p1;
        Dev.Null = p2;
        return 7.7;
      }

      public double Method (int p1, string p2)
      {
        Dev.Null = p1;
        Dev.Null = p2;
        return 7.7;
      }
    }
  }
}