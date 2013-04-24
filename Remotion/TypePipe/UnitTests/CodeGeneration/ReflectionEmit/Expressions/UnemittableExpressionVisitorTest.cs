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
using Remotion.TypePipe.Dlr.Ast;
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
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Expressions
{
  [TestFixture]
  public class UnemittableExpressionVisitorTest
  {
    private MutableType _mutableType;
    private IEmittableOperandProvider _emittableOperandProviderMock;
    private IMethodTrampolineProvider _methodTrampolineProviderMock;
    private CodeGenerationContext _context;

    private UnemittableExpressionVisitor _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.Create (baseType: typeof (DomainType));
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
      _context = CodeGenerationContextObjectMother.GetSomeContext (_mutableType, emittableOperandProvider: _emittableOperandProviderMock);
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
        "It is not supported to have a ConstantExpression of type 'MutableType' because instances of 'MutableType' exist only at " +
        "code generation time, not at runtime.")]
    public void VisitConstant_NotAssignableValue ()
    {
      var proxyType = MutableTypeObjectMother.Create();
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
    public void VisitMethodCall ()
    {
      var arrayType = ArrayTypeBaseObjectMother.Create();
      var instance = ExpressionTreeObjectMother.GetSomeExpression (arrayType);
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((Array obj) => obj.GetValue (0));
      var method2 = arrayType.GetMethod ("Get");

      var expression1 = Expression.Call (instance, method1, Expression.Constant (7));
      var expression2 = Expression.Call (instance, method2, Expression.Constant (7));

      Assert.That (() => _visitorPartialMock.Invoke ("VisitMethodCall", expression1), Throws.Nothing);

      var message = "Methods on array types containing a custom element type cannot be used in expression trees. "
                    + "For one-dimensional arrays use the specialized expression factories ArrayAccess and ArrayLength."
                    + "For multi-dimensional arrays call Array.GetValue, Array.SetValue, Array.Length and related base members.";
      Assert.That (
          () => _visitorPartialMock.Invoke ("VisitMethodCall", expression2),
          Throws.Exception.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
    }

    [Test]
    public void VisitNewArray_MultiDimensionalArray ()
    {
      var customElementType = CustomTypeObjectMother.Create();
      var customElement = Expression.Constant (null, customElementType);

      var expression1 = Expression.NewArrayInit (customElementType, customElement, customElement);
      var expression2 = Expression.NewArrayBounds (customElementType, Expression.Constant (7));
      var expression3 = Expression.NewArrayBounds (typeof (int), Expression.Constant (7), Expression.Constant (7));
      var expression4 = Expression.NewArrayBounds (customElementType, Expression.Constant (7), Expression.Constant (7));

      Assert.That (() => _visitorPartialMock.Invoke ("VisitNewArray", expression1), Throws.Nothing);
      Assert.That (() => _visitorPartialMock.Invoke ("VisitNewArray", expression2), Throws.Nothing);
      Assert.That (() => _visitorPartialMock.Invoke ("VisitNewArray", expression3), Throws.Nothing);

      var message =
            "The expression factory NewArrayBounds is not supported for multi-dimensional arrays. "
            + "To create a multi-dimensional array call the static method Array.CreateInstance and cast the result to the specific array type.";
      Assert.That (
          () => _visitorPartialMock.Invoke ("VisitNewArray", expression4), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
    }

    [Test]
    public void VisitNew_ArrayConstructor ()
    {
      var vectorType = VectorTypeObjectMother.Create();
      var multiDimensionalArrayType = MultiDimensionalArrayTypeObjectMother.Create (rank: 1);

      var expression1 = Expression.New (vectorType.GetConstructor (new[] { typeof (int) }), Expression.Constant (7));
      var expression2 = Expression.New (multiDimensionalArrayType.GetConstructor (new[] { typeof (int) }), Expression.Constant (7));

      var message = "Array constructors of array types containing a custom element type cannot be used directly in expression trees. "
                    + "For one-dimensional arrays use the NewArrayBounds or NewArrayInit expression factories. "
                    + "For multi-dimensional arrays call the static method Array.CreateInstance and cast the result to the specific array type.";
      Assert.That (() => _visitorPartialMock.Invoke ("VisitNew", expression1), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
      Assert.That (() => _visitorPartialMock.Invoke ("VisitNew", expression2), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
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

      var expectedExpression = new UnboxExpression (new BoxAndCastExpression (expression.Operand, typeof (object)), toGenericParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnary_Convert_ToGenericParameter_FromReferenceType_UnboxAny ()
    {
      var fromReferenceType1 = ReflectionObjectMother.GetSomeClassType();
      var fromReferenceType2 = ReflectionObjectMother.GetSomeInterfaceType();
      var toGenericParameter1 = MutableGenericParameterObjectMother.Create (constraints: new[] { fromReferenceType1 });
      var toGenericParameter2 = MutableGenericParameterObjectMother.Create (constraints: new[] { fromReferenceType2 });
      var expression1 = Expression.Convert (Expression.Default (fromReferenceType1), toGenericParameter1);
      var expression2 = Expression.Convert (Expression.Default (fromReferenceType2), toGenericParameter2);

      var result1 = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression1);
      var result2 = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression2);

      var expectedExpression1 = new UnboxExpression (expression1.Operand, toGenericParameter1);
      var expectedExpression2 = new UnboxExpression (expression2.Operand, toGenericParameter2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression1, result1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression2, result2);
    }

    [Test]
    public void VisitUnary_Convert_ToReferenceType_FromGenericParameter_Box ()
    {
      var toReferenceType1 = ReflectionObjectMother.GetSomeClassType();
      var toReferenceType2 = ReflectionObjectMother.GetSomeInterfaceType();
      var fromGenericParameter1 = MutableGenericParameterObjectMother.Create (constraints: new[] { toReferenceType1 });
      var fromGenericParameter2 = MutableGenericParameterObjectMother.Create (constraints: new[] { toReferenceType2 });
      var expression1 = Expression.Convert (Expression.Default (fromGenericParameter1), toReferenceType1);
      var expression2 = Expression.Convert (Expression.Default (fromGenericParameter2), toReferenceType2);

      var result1 = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression1);
      var result2 = _visitorPartialMock.Invoke<Expression> ("VisitUnary", expression2);

      var expectedExpression1 = new BoxAndCastExpression (expression1.Operand, toReferenceType1);
      var expectedExpression2 = new BoxAndCastExpression (expression2.Operand, toReferenceType2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression1, result1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression2, result2);
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
          ExpressionTreeObjectMother.GetSomeThisExpression (_mutableType), new NonVirtualCallMethodInfoAdapter (method), parameters.Cast<Expression>());
      var expression = Expression.Lambda (delegateType, body, parameters);

      var fakeTrampolineMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.TrampolineMethod (7, ""));
      _methodTrampolineProviderMock.Expect (mock => mock.GetNonVirtualCallTrampoline (_context, method)).Return (fakeTrampolineMethod);

      var thisClosure = Expression.Parameter (_mutableType, "thisClosure");
      var expectedTree =
          Expression.Block (
              new[] { thisClosure },
              Expression.Assign (thisClosure, new ThisExpression (_mutableType)),
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