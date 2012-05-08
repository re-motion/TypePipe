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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class BodyContextBaseTest
  {
    private ReadOnlyCollection<ParameterExpression> _emptyParameters;
    private MutableType _mutableType;
    private IMemberSelector _memberSelector;
    private BodyContextBase _staticContext;
    private BodyContextBase _instanceContext;

    [SetUp]
    public void SetUp ()
    {
      _emptyParameters = new List<ParameterExpression> ().AsReadOnly ();
      _mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainType));
      _memberSelector = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _staticContext = new TestableBodyContextBase (_mutableType, _emptyParameters, true, _memberSelector);
      _instanceContext = new TestableBodyContextBase (_mutableType, _emptyParameters, false, _memberSelector);
    }

    [Test]
    public void Initialization ()
    {
      var parameter1 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameter2 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameters = new List<ParameterExpression> { parameter1, parameter2 }.AsReadOnly ();

      var isStatic = BooleanObjectMother.GetRandomBoolean();
      var context = new TestableBodyContextBase (_mutableType, parameters.AsOneTime(), isStatic, _memberSelector);

      Assert.That (context.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (context.Parameters, Is.EqualTo (new[] { parameter1, parameter2 }));
      Assert.That (context.IsStatic, Is.EqualTo (isStatic));
    }

    [Test]
    public void This ()
    {
      Assert.That (_instanceContext.This, Is.TypeOf<ThisExpression>());
      Assert.That (_instanceContext.This.Type, Is.SameAs (_mutableType));
    }

    [Test]
    public void This_StaticContext ()
    {
      Assert.That (() => _staticContext.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
    }

    [Test]
    public void GetBaseCall_Name_Params ()
    {
      var baseMethods = typeof (object).GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
      var arguments = new ArgumentTestHelper (7);
      var fakeBaseMethod = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.FakeBaseMethod (1));
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      _memberSelector
          .Expect (mock => mock.SelectSingleMethod (baseMethods, Type.DefaultBinder, bindingFlags, "Method", _mutableType, arguments.Types, null))
          .Return (fakeBaseMethod);

      var result = _instanceContext.GetBaseCall ("Method", arguments.Expressions);

      Assert.That (result.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = (ThisExpression) result.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_mutableType));

      Assert.That (result.Method, Is.TypeOf<BaseCallMethodInfoAdapter> ());
      var baseCallMethodInfoAdapter = (BaseCallMethodInfoAdapter) result.Method;
      Assert.That (baseCallMethodInfoAdapter.AdaptedMethodInfo, Is.SameAs (fakeBaseMethod));

      Assert.That (result.Arguments, Is.EqualTo (arguments.Expressions));
    }

    [Test]
    [ExpectedException (typeof(InvalidOperationException), ExpectedMessage = "Type 'System.Object' has no base type.")]
    public void GetBaseCall_Name_Params_NoBaseType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (object));
      var context = new TestableBodyContextBase (mutableType, _emptyParameters, false, _memberSelector);

      context.GetBaseCall ("DoesNotExist");
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Instance method 'Foo' could not be found on base type 'System.Object'.\r\nParameter name: methodName")]    
    public void GetBaseCall_Name_Params_NoMatchingMethod ()
    {
      var baseMethods = typeof (object).GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
      var arguments = new ArgumentTestHelper (7);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      _memberSelector
          .Expect (mock => mock.SelectSingleMethod (baseMethods, Type.DefaultBinder, bindingFlags, "Foo", _mutableType, arguments.Types, null))
          .Return (null);

      _instanceContext.GetBaseCall ("Foo", arguments.Expressions);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot perform base call from static method.")]
    public void GetBaseCall_Name_Params_StaticContext ()
    {
      _staticContext.GetBaseCall ("NotImportant");
    }

    [Test]
    public void GetBaseCall_MethodInfo_Params ()
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
    public void GetBaseCall_MethodInfo_Params_StaticContext ()
    {
      var method = ReflectionObjectMother.GetSomeInstanceMethod();
      _staticContext.GetBaseCall (method);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot perform base call for static method.")]
    public void GetBaseCall_MethodInfo_Params_StaticMethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeStaticMethod();
      _instanceContext.GetBaseCall (method);
    }

    private class DomainType
    {
      public void Method (int i) { }
      public void FakeBaseMethod (int i) { }
    }
  }
}