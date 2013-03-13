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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Expressions;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Generics;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Expressions
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
      _proxyType = ProxyTypeObjectMother.Create (baseType: typeof (DomainType));
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
      _context = CodeGenerationContextObjectMother.GetSomeContext (_proxyType, emittableOperandProvider: _emittableOperandProviderMock);
      _methodTrampolineProviderMock = MockRepository.GenerateStrictMock<IMethodTrampolineProvider>();

      _visitorPartialMock = MockRepository.GeneratePartialMock<UnemittableExpressionVisitor> (_context, _methodTrampolineProviderMock);
    }

    [Test]
    public void VisitConstant_SimpleValue ()
    {
      var expression = Expression.Constant ("emittable");

      var result = _visitorPartialMock.Invoke ("VisitConstant", expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitConstant_EmittableOperand ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      var field = ReflectionObjectMother.GetSomeField();
      var emittableField = ReflectionObjectMother.GetSomeOtherField();
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableConstructor = ReflectionObjectMother.GetSomeOtherConstructor();
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionObjectMother.GetSomeOtherMethod();

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

      _visitorPartialMock.Invoke ("VisitConstant", expression);
    }

    [Test]
    public void VisitConstant_NullValue ()
    {
      var expression = Expression.Constant (null);

      var result = _visitorPartialMock.Invoke ("VisitConstant", expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNew_GenericParameterDefaultConstructor ()
    {
      var genericParameter = MutableGenericParameterObjectMother.Create();
      var defaultConstructor = new GenericParameterDefaultConstructor (genericParameter);
      var expression = Expression.New (defaultConstructor);

      var result = _visitorPartialMock.Invoke<Expression> ("VisitNew", expression);

      Assert.That (result.Type, Is.SameAs (genericParameter));
      Assert.That (result, Is.InstanceOf<MethodCallExpression>());

      var methodCallExpression = result.As<MethodCallExpression>();
      Assert.That (methodCallExpression.Object, Is.Null);
      Assert.That (methodCallExpression.Arguments, Is.Empty);

      var method = methodCallExpression.Method;
      var genericMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Activator.CreateInstance<Dev.T>());
      Assert.That (method.GetGenericMethodDefinition(), Is.EqualTo (genericMethodDefinition));
      Assert.That (method.GetGenericArguments(), Is.EqualTo (new[] { genericParameter }));
    }

    [Test]
    public void VisitNew_NoChange ()
    {
      var expression1 = Expression.New (typeof (object));
      var expression2 = Expression.New (typeof (int));

      var result1 = _visitorPartialMock.Invoke ("VisitNew", expression1);
      var result2 = _visitorPartialMock.Invoke ("VisitNew", expression2);

      Assert.That (result1, Is.SameAs (expression1));
      Assert.That (result2, Is.SameAs (expression2));
    }

    [Test]
    public void VisitUnary_Convert_ToGenericParameter_FromGenericParameter_BoxThenUnbox ()
    {
      var fromGenericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var toGenericParameter = MutableGenericParameterObjectMother.Create (constraints: new[] { fromGenericParameter });
      var expression = Expression.Convert (Expression.Default (fromGenericParameter), toGenericParameter);

      var result = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression);

      var expectedExpression = new UnboxExpression (new BoxExpression (expression.Operand, typeof (object)), toGenericParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnary_Convert_ToGenericParameter_FromReferenceType_UnboxAny ()
    {
      var fromReferenceType = ReflectionObjectMother.GetSomeClassType();
      var toGenericParameter = MutableGenericParameterObjectMother.Create (constraints: new[] { fromReferenceType });
      var expression = Expression.Convert (Expression.Default (fromReferenceType), toGenericParameter);

      var result = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression);

      var expectedExpression = new UnboxExpression (expression.Operand, toGenericParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnary_Convert_ToReferenceType_FromGenericParameter_Box ()
    {
      var toReferenceType = ReflectionObjectMother.GetSomeClassType();
      var fromGenericParameter = MutableGenericParameterObjectMother.Create (constraints: new[] { toReferenceType });
      var expression = Expression.Convert (Expression.Default (fromGenericParameter), toReferenceType);

      var result = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression);

      var expectedExpression = new BoxExpression (expression.Operand, toReferenceType);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnary_Convert_SameType_Unchanged ()
    {
      var fromGenericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var toGenericParameter = fromGenericParameter;
      var expression = Expression.Convert (Expression.Default (fromGenericParameter), toGenericParameter);

      var result = _visitorPartialMock.Invoke ("VisitUnary", expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitUnary_Convert_Unchanged ()
    {
      CheckVisitUnaryUnchanged (typeof (string), typeof (object));
      CheckVisitUnaryUnchanged (typeof (object), typeof (int));
      CheckVisitUnaryUnchanged (typeof (int), typeof (object));
      CheckVisitUnaryUnchanged (typeof (int), typeof (long));
    }

    [Test]
    public void VisitUnary_NonConvertExpression ()
    {
      var expression = Expression.Not (Expression.Constant (true));

      var result = _visitorPartialMock.Invoke ("VisitUnary", expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitUnary_ConvertChecked_AssertFactoryBehavior ()
    {
      var expression = Expression.ConvertChecked (Expression.Constant (new object()), typeof (string));
      Assert.That (expression.NodeType, Is.EqualTo (ExpressionType.Convert));
    }

    [Test]
    public void VisitLambda_InstanceClosureWithBaseCall ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7, ""));
      var delegateType = typeof (Func<int, string, double>);
      var parameters = new[] { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (string)) };
      var body = Expression.Call (
          ExpressionTreeObjectMother.GetSomeThisExpression (_proxyType), new NonVirtualCallMethodInfoAdapter (method), parameters.Cast<Expression>());
      var expression = Expression.Lambda (delegateType, body, parameters);

      var fakeTrampolineMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.TrampolineMethod (7, ""));
      _methodTrampolineProviderMock.Expect (mock => mock.GetNonVirtualCallTrampoline (_context, method)).Return (fakeTrampolineMethod);

      var thisClosure = Expression.Parameter (_proxyType, "thisClosure");
      var expectedTree =
          Expression.Block (
              new[] { thisClosure },
              Expression.Assign (thisClosure, new ThisExpression (_proxyType)),
              Expression.Lambda (delegateType, Expression.Call (thisClosure, fakeTrampolineMethod, parameters.Cast<Expression>()), parameters));
      var fakeResultExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _visitorPartialMock
          .Expect (mock => mock.Visit (Arg<Expression>.Is.Anything))
          .WhenCalled (mi => ExpressionTreeComparer.CheckAreEqualTrees (expectedTree, (Expression) mi.Arguments[0]))
          .Return (fakeResultExpression);

      var result = _visitorPartialMock.Invoke ("VisitLambda", expression);

      Assert.That (result, Is.SameAs (fakeResultExpression));
    }

    [Test]
    public void VisitLambda_StaticClosure ()
    {
      var body = Expression.Constant ("static body without ThisExpression");
      var parameter = Expression.Parameter (typeof (int));
      var expression = Expression.Lambda<Action<int>> (body, parameter);

      var result = _visitorPartialMock.Invoke ("VisitLambda", expression);

      Assert.That (result, Is.SameAs (expression));
    }

    private void CheckVisitConstant<T> (T value, T emittableValue, Func<IEmittableOperandProvider, T, T> getEmittableOperandFunc)
    {
      var expression = Expression.Constant (value, typeof (object));
      _emittableOperandProviderMock.BackToRecord();
      _emittableOperandProviderMock.Expect (mock => getEmittableOperandFunc (mock, value)).Return (emittableValue);
      _emittableOperandProviderMock.Replay();

      var result = _visitorPartialMock.Invoke ("VisitConstant", expression);

      _emittableOperandProviderMock.VerifyAllExpectations();
      Assert.That (result, Is.AssignableTo<ConstantExpression>());
      var constantExpression = (ConstantExpression) result;
      Assert.That (constantExpression.Value, Is.SameAs (emittableValue));
      Assert.That (constantExpression.Type, Is.SameAs (typeof (object)));
    }

    private void CheckVisitUnaryUnchanged (Type toType, Type fromType)
    {
      var expression = Expression.Convert (Expression.Default (fromType), toType);

      var result = _visitorPartialMock.Invoke ("VisitUnary", expression);

      Assert.That (result, Is.SameAs (expression));
    }

    public class DomainType
    {
      public void SimpleMethod () {}
      public double Method (int p1, string p2) { Dev.Null = p1; Dev.Null = p2; return 7.7; }
      public double TrampolineMethod (int p1, string p2) { Dev.Null = p1; Dev.Null = p2; return 0; }
    }

    public struct ValueTypeImplementingIDisposable : IDisposable
    {
      public void Dispose () {}
    }
  }
}