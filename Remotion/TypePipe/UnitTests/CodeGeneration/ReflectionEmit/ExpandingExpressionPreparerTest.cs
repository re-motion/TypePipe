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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ExpandingExpressionPreparerTest
  {
    private IEmittableOperandProvider _emittableOperandProviderMock;

    private ExpandingExpressionPreparer _preparer;

    [SetUp]
    public void SetUp ()
    {
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _preparer = new ExpandingExpressionPreparer();
    }

    [Test]
    public void PrepareBody_ExpandsOriginalBodyExpressionsForMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var body = Expression.Block (new OriginalBodyExpression (method, typeof (void), new Expression[0]));

      var result = _preparer.PrepareBody (body, _emittableOperandProviderMock);

      Assert.That (result, Is.AssignableTo<BlockExpression>());
      var blockExpression = (BlockExpression) result;
      Assert.That (blockExpression.Result, Is.AssignableTo<MethodCallExpression>());
      var methodCallExpression = ((MethodCallExpression) blockExpression.Result);
      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>());
      Assert.That (((NonVirtualCallMethodInfoAdapter) methodCallExpression.Method).AdaptedMethodInfo, Is.SameAs (method));
    }

    [Test]
    public void PrepareBody_ExpandsOriginalBodyExpressionsForConstructor ()
    {
      var ctor = MemberInfoFromExpressionUtility.GetConstructor (() => new object ());
      var body = new OriginalBodyExpression (ctor, typeof (void), new Expression[0]);

      var result = _preparer.PrepareBody (body, _emittableOperandProviderMock);

      Assert.That (result, Is.AssignableTo<MethodCallExpression>());
      var methodCallExpression = ((MethodCallExpression) result);
      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>());
      var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) methodCallExpression.Method;
      Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethodInfo, Is.TypeOf<ConstructorAsMethodInfoAdapter>());
      var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) nonVirtualCallMethodInfoAdapter.AdaptedMethodInfo;
      Assert.That (constructorAsMethodInfoAdapter.ConstructorInfo, Is.SameAs (ctor));
    }

    [Test]
    public void PrepareBody_ReplacesMutableMembersInConstantExpressions ()
    {
      var mutableMember = MutableFieldInfoObjectMother.Create();
      var body = Expression.Block (Expression.Constant (mutableMember));

      var fakeMember = ReflectionObjectMother.GetSomeField();
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableOperand (mutableMember)).Return (fakeMember);

      var result = _preparer.PrepareBody (body, _emittableOperandProviderMock);

      _emittableOperandProviderMock.VerifyAllExpectations();
      Assert.That (result, Is.AssignableTo<BlockExpression> ());
      var blockExpression = (BlockExpression) result;
      Assert.That (blockExpression.Result, Is.AssignableTo<ConstantExpression>());
      var constantExpression = ((ConstantExpression) blockExpression.Result);
      Assert.That (constantExpression.Value, Is.SameAs (fakeMember));
    }
  }
}