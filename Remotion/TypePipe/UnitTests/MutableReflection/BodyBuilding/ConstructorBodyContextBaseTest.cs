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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class ConstructorBodyContextBaseTest
  {
    private MutableType _declaringType;
    private ParameterExpression[] _parameters;
    private IMemberSelector _memberSelectorMock;

    private ConstructorBodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.CreateForExisting (typeof (ClassWithConstructor));
      _parameters = new[] { Expression.Parameter (typeof (string)) };
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _context = new TestableConstructorBodyContextBase (_declaringType, _parameters.AsOneTime(), _memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.IsStatic, Is.False);
    }

    [Test]
    public void GetConstructorCall ()
    {
      var argumentExpressions = new ArgumentTestHelper ("string").Expressions;

      var result = _context.GetConstructorCall (argumentExpressions);

      Assert.That (result, Is.AssignableTo<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) result;

      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression>());
      var thisExpression = (ThisExpression) methodCallExpression.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_declaringType));

      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>());
      var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) methodCallExpression.Method;
      Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethod, Is.TypeOf<ConstructorAsMethodInfoAdapter>());
      var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) nonVirtualCallMethodInfoAdapter.AdaptedMethod;

      Assert.That (constructorAsMethodInfoAdapter.AdaptedConstructor, Is.TypeOf<MutableConstructorInfo>());
      //var mutableCtor = (MutableConstructorInfo) constructorAsMethodInfoAdapter.AdaptedConstructor;
      //var expectedUnderlyingCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new ClassWithConstructor (null));
      //Assert.That (mutableCtor.UnderlyingSystemConstructorInfo, Is.EqualTo (expectedUnderlyingCtor));
      // TODO

      Assert.That (methodCallExpression.Arguments, Is.EqualTo (argumentExpressions));
    }

    [Test]
    [ExpectedException (typeof (MissingMemberException), ExpectedMessage =
        "Could not find a public instance constructor with signature (System.Int32, System.Int32) on type 'ClassWithConstructor'.")]
    public void GetConstructorCall_NoMatchingConstructor ()
    {
      var argumentExpressions = new ArgumentTestHelper (7, 8).Expressions;
      _context.GetConstructorCall (argumentExpressions);
    }

    private class ClassWithConstructor
    {
      public ClassWithConstructor (string s)
      {
        Dev.Null = s;
      }
    }
  }
}