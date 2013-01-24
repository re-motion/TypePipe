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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class UnemittableExpressionVisitorTest
  {
    private ProxyType _proxyType;
    private IEmittableOperandProvider _emittableOperandProviderMock;
    private IMethodTrampolineProvider _methodTrampolineProviderMock;
    private CodeGenerationContext _context;

    private UnemittableExpressionVisitor _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _proxyType = ProxyTypeObjectMother.Create (baseType: typeof (DomainType), underlyingTypeFactory: new UnderlyingTypeFactory());
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
      _context = CodeGenerationContextObjectMother.GetSomeContext (_proxyType, emittableOperandProvider: _emittableOperandProviderMock);
      _methodTrampolineProviderMock = MockRepository.GenerateStrictMock<IMethodTrampolineProvider>();

      _visitorPartialMock = MockRepository.GeneratePartialMock<UnemittableExpressionVisitor> (_context, _methodTrampolineProviderMock);
    }

    [Test]
    public void VisitConstant_SimpleValue ()
    {
      var expression = Expression.Constant ("emittable");

      var result = ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitConstant_EmittableOperand ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var emittableType = ReflectionObjectMother.GetSomeType();
      var field = ReflectionObjectMother.GetSomeField();
      var emittableField = ReflectionObjectMother.GetSomeField();
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableConstructor = ReflectionObjectMother.GetSomeConstructor();
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionObjectMother.GetSomeMethod();

      CheckVisitConstant (type, emittableType, (p, t) => p.GetEmittableType (t));
      CheckVisitConstant (field, emittableField, (p, f) => p.GetEmittableField (f));
      CheckVisitConstant (constructor, emittableConstructor, (p, c) => p.GetEmittableConstructor (c));
      CheckVisitConstant (method, emittableMethod, (p, c) => p.GetEmittableMethod (c));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), MatchType = MessageMatch.StartsWith, ExpectedMessage =
        "It is not supported to have a ConstantExpression of type 'ProxyType' because instances of 'ProxyType' exist only at " +
        "code generation time, not at runtime.")]
    public void VisitConstant_NotAssignableValue ()
    {
      var proxyType = ProxyTypeObjectMother.Create();
      var expression = Expression.Constant (proxyType);
      _emittableOperandProviderMock.Stub (stub => stub.GetEmittableType (proxyType)).Return (typeof (int));

      ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);
    }

    [Test]
    public void VisitConstant_NullValue ()
    {
      var expression = Expression.Constant (null);

      var result = ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_StaticClosure ()
    {
      var body = Expression.Constant ("original body");
      var parameter = Expression.Parameter (typeof (int));
      var expression = Expression.Lambda<Action<int>> (body, parameter);
      var fakeStaticClosure = Expression.Constant ("fake body");
      _visitorPartialMock.Expect (mock => mock.Visit (body)).Return (fakeStaticClosure);

      var result = ExpressionVisitorTestHelper.CallVisitLambda (_visitorPartialMock, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_InstanceClosureWithBaseCall ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7, ""));
      var parameters = new[] { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (string)) };
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var expression = Expression.Lambda<Func<int, string, double>> (body, parameters);

      var fakeBody = Expression.Call (
          ExpressionTreeObjectMother.GetSomeThisExpression (_proxyType), new NonVirtualCallMethodInfoAdapter (method), parameters);
      _visitorPartialMock.Expect (mock => mock.Visit (body)).Return (fakeBody);

      var fakeTrampolineMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.TrampolineMethod (7, ""));
      _methodTrampolineProviderMock.Expect (mock => mock.GetNonVirtualCallTrampoline (_context, method)).Return (fakeTrampolineMethod);

      var thisClosure = Expression.Parameter (_proxyType, "thisClosure");
      var expectedTree =
          Expression.Block (
              new[] { thisClosure },
              Expression.Assign (thisClosure, new ThisExpression (_proxyType)),
              Expression.Lambda<Func<int, string, double>> (Expression.Call (thisClosure, fakeTrampolineMethod, parameters), parameters));
      var fakeResultExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _visitorPartialMock
          .Expect (mock => mock.Visit (Arg<Expression>.Is.Anything))
          .WhenCalled (mi => ExpressionTreeComparer.CheckAreEqualTrees (expectedTree, (Expression) mi.Arguments[0]))
          .Return (fakeResultExpression);

      var result = ExpressionVisitorTestHelper.CallVisitLambda (_visitorPartialMock, expression);

      Assert.That (result, Is.SameAs (fakeResultExpression));
    }

    private void CheckVisitConstant<T> (T value, T emittableValue, Func<IEmittableOperandProvider, T, T> getEmittableOperandFunc)
    {
      var expression = Expression.Constant (value, typeof (object));
      _emittableOperandProviderMock.BackToRecord();
      _emittableOperandProviderMock.Expect (mock => getEmittableOperandFunc (mock, value)).Return (emittableValue);
      _emittableOperandProviderMock.Replay();

      var result = ExpressionVisitorTestHelper.CallVisitConstant (_visitorPartialMock, expression);

      _emittableOperandProviderMock.VerifyAllExpectations();
      Assert.That (result, Is.AssignableTo<ConstantExpression>());
      var constantExpression = (ConstantExpression) result;
      Assert.That (constantExpression.Value, Is.SameAs (emittableValue));
      Assert.That (constantExpression.Type, Is.SameAs (typeof (object)));
    }

    public class DomainType
    {
      public double Method (int p1, string p2) { Dev.Null = p1; Dev.Null = p2; return 7.7; }
      public double TrampolineMethod (int p1, string p2) { Dev.Null = p1; Dev.Null = p2; return 0; }
    }
  }
}