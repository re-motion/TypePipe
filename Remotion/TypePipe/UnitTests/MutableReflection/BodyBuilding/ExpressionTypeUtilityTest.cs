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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;
using System.Collections.Generic;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class ExpressionTypeUtilityTest
  {
    [Test]
    public void EnsureCorrectType_Exact ()
    {
      var expectedType = typeof (string);
      var expression = ExpressionTreeObjectMother.GetSomeExpression (expectedType);

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void EnsureCorrectType_Exact_Void ()
    {
      var expectedType = typeof (void);
      var expression = ExpressionTreeObjectMother.GetSomeExpression (expectedType);

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void EnsureCorrectType_Exact_ValueTypes ()
    {
      var expectedType = typeof (int);
      var expression = ExpressionTreeObjectMother.GetSomeExpression (expectedType);

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void EnsureCorrectType_ReferenceAssignable ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof(string));
      var expectedType = typeof (object);

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void EnsureCorrectType_BoxingConvertibleToBaseClass ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var expectedType = typeof (object);

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      CheckExpressionIsConverted (expression, expectedType, result);
    }

    [Test]
    public void EnsureCorrectType_BoxingConvertibleToInterface ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var expectedType = typeof (IComparable);

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      CheckExpressionIsConverted (expression, expectedType, result);
    }

    [Test]
    public void EnsureCorrectType_NonVoidExpression_WrappedAsVoid ()
    {
      var expectedType = typeof (void);
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (string));

      var result = ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);

      Assert.That (result, Is.AssignableTo<BlockExpression>());
      var blockExpression = (BlockExpression) result;

      Assert.That (blockExpression.Type, Is.SameAs (expectedType));
      Assert.That (blockExpression.Expressions, Is.EqualTo (new[] { expression }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "Type 'System.String' cannot be implicitly converted to type 'System.Collections.Generic.List`1[System.Int32]'. " 
        + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.")]
    public void EnsureCorrectType_CompletelyUnrelated ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (string));
      var expectedType = typeof (List<int>);

      ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "Type 'System.Object' cannot be implicitly converted to type 'System.String'. " 
        + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.")]
    public void EnsureCorrectType_UnsafeCastRequired ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      var expectedType = typeof (string);

      ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "Type 'System.Int64' cannot be implicitly converted to type 'System.Int32'. " 
        + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.")]
    public void EnsureCorrectType_UnsafeValueConversion ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (long));
      var expectedType = typeof (int);

      ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "Type 'System.Int32' cannot be implicitly converted to type 'System.Int64'. "
        + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.")]
    public void EnsureCorrectType_SafeValueConversion ()
    {
      var expression = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var expectedType = typeof (long);

      ExpressionTypeUtility.EnsureCorrectType (expression, expectedType);
    }

    private void CheckExpressionIsConverted (Expression expectedConvertedExpression, Type expectedType, Expression actualExpression)
    {
      Assert.That (actualExpression.NodeType, Is.EqualTo (ExpressionType.Convert));
      var unaryExpression = (UnaryExpression) actualExpression;
      Assert.That (unaryExpression.Operand, Is.SameAs (expectedConvertedExpression));
      Assert.That (unaryExpression.Type, Is.SameAs (expectedType));
    }
  }
}