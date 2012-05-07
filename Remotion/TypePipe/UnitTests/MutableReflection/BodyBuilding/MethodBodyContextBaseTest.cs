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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyContextBaseTest
  {
    private ReadOnlyCollection<ParameterExpression> _emptyParameters;
    private MutableType _mutableType;
    private IMemberSelector _memberSelector;
    private TestableMethodBodyContextBase _staticContext;
    private TestableMethodBodyContextBase _instanceContext;

    [SetUp]
    public void SetUp ()
    {
      _emptyParameters = new List<ParameterExpression> ().AsReadOnly ();
      _mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainType));
      _memberSelector = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _staticContext = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, true, _memberSelector);
      _instanceContext = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, false, _memberSelector);
    }

    [Test]
    public void Initialization ()
    {
      var parameter1 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameter2 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameters = new List<ParameterExpression> { parameter1, parameter2 }.AsReadOnly ();

      var isStatic = BooleanObjectMother.GetRandomBoolean();
      var context = new TestableMethodBodyContextBase (_mutableType, parameters.AsOneTime(), isStatic, _memberSelector);

      Assert.That (context.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (context.Parameters, Is.EqualTo (new[] { parameter1, parameter2 }));
      Assert.That (context.IsStatic, Is.EqualTo(isStatic));
    }

    [Test]
    public void This ()
    {
      Assert.That (_instanceContext.This, Is.TypeOf<ThisExpression>());
      Assert.That (_instanceContext.This.Type, Is.SameAs (_mutableType));
    }

    [Test]
    public void This_ThrowsForStaticMethods ()
    {
      Assert.That (() => _staticContext.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
    }

    [Test]
    [Ignore("TODO 4818")]
    public void GetBaseCall_Name ()
    {
      var method = _mutableType.GetMethod ("Method");
      var arguments = new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof (int)) };

      var fakeBaseMethod = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.FakeBaseMethod (1));
      //_memberSelector.Expect (mock => mock.GetBaseMethod (method)).Return (fakeBaseMethod); // TODO 0

      var result = _instanceContext.GetBaseCall ("Method", arguments);

      Assert.That (result.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = (ThisExpression) result.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_mutableType));

      Assert.That (result.Method, Is.TypeOf<BaseCallMethodInfoAdapter> ());
      var baseCallMethodInfoAdapter = (BaseCallMethodInfoAdapter) result.Method;
      Assert.That (baseCallMethodInfoAdapter.AdaptedMethodInfo, Is.SameAs (method));

      Assert.That (result.Arguments, Is.EqualTo (arguments));
    }

    [Test]
    [Ignore ("TODO 4818")]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot perform base call from static method.")]
    public void GetBaseCall_Name_ForStaticMethods ()
    {
      _staticContext.GetBaseCall ("Method", new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof (int)) });
    }

    [Test]
    public void GetBaseCall_MethodInfo ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (1));
      var arguments = new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof (int)) };

      var result = _instanceContext.GetBaseCall (method, arguments);

      Assert.That (result.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = (ThisExpression) result.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_mutableType));

      Assert.That (result.Method, Is.TypeOf<BaseCallMethodInfoAdapter>());
      var baseCallMethodInfoAdapter = (BaseCallMethodInfoAdapter) result.Method;
      Assert.That (baseCallMethodInfoAdapter.AdaptedMethodInfo, Is.SameAs (method));

      Assert.That (result.Arguments, Is.EqualTo (arguments));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot perform base call from static method.")]
    public void GetBaseCall_MethodInfo_ForStaticMethods ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      _staticContext.GetBaseCall (method);
    }

    private class DomainType
    {
      public void Method (int i) { }

      public void FakeBaseMethod (int i) { }
    }

  }
}