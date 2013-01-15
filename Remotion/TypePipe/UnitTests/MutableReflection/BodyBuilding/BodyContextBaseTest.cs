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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class BodyContextBaseTest
  {
    private ProxyType _declaringType;
    private IMemberSelector _memberSelectorMock;

    private BodyContextBase _staticContext;
    private BodyContextBase _instanceContext;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ProxyTypeObjectMother.Create (typeof (DomainType));
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _staticContext = new TestableBodyContextBase (_declaringType, true, _memberSelectorMock);
      _instanceContext = new TestableBodyContextBase (_declaringType, false, _memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      var isStatic = BooleanObjectMother.GetRandomBoolean();
      var context = new TestableBodyContextBase (_declaringType, isStatic, _memberSelectorMock);

      Assert.That (context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (context.IsStatic, Is.EqualTo (isStatic));
    }

    [Test]
    public void This ()
    {
      Assert.That (_instanceContext.This, Is.TypeOf<ThisExpression>());
      Assert.That (_instanceContext.This.Type, Is.SameAs (_declaringType));
    }

    [Test]
    public void This_StaticContext ()
    {
      Assert.That (() => _staticContext.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
    }

    [Test]
    public void GetBaseCall_Name_Params ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var baseMethods = typeof (DomainTypeBase).GetMethods (bindingFlags);
      var arguments = new ArgumentTestHelper (7);
      var fakeBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.FakeBaseMethod (1));
      _memberSelectorMock
          .Expect (mock => mock.SelectSingleMethod (baseMethods, Type.DefaultBinder, bindingFlags, "Method", _declaringType, arguments.Types, null))
          .Return (fakeBaseMethod);

      var result = _instanceContext.GetBaseCall ("Method", arguments.Expressions.AsOneTime());

      Assert.That (result.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = (ThisExpression) result.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_declaringType));

      Assert.That (result.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter> ());
      var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) result.Method;
      Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethod, Is.SameAs (fakeBaseMethod));

      Assert.That (result.Arguments, Is.EqualTo (arguments.Expressions));
    }

    [Test]
    [ExpectedException (typeof(InvalidOperationException), ExpectedMessage = "Type 'Object' has no base type.")]
    public void GetBaseCall_Name_Params_NoBaseType ()
    {
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (object),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);
      var context = new TestableBodyContextBase (proxyType, false, _memberSelectorMock);

      context.GetBaseCall ("DoesNotExist");
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Instance method 'Foo' could not be found on base type "
        + "'Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding.BodyContextBaseTest+DomainTypeBase'.\r\nParameter name: baseMethod")]    
    public void GetBaseCall_Name_Params_NoMatchingMethod ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var baseMethods = typeof (DomainTypeBase).GetMethods (bindingFlags);
      var arguments = new ArgumentTestHelper (7);
      _memberSelectorMock
          .Expect (mock => mock.SelectSingleMethod (baseMethods, Type.DefaultBinder, bindingFlags, "Foo", _declaringType, arguments.Types, null))
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
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "Can only call public, protected, or protected internal methods.\r\nParameter name: baseMethod")]
    public void GetBaseCall_Name_Params_DisallowedVisibility ()
    {
      var internalMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.InternalMethod());
      _memberSelectorMock
          .Expect (mock => mock.SelectSingleMethod<MethodInfo> (null, null, 0, null, null, null, null)).IgnoreArguments()
          .Return (internalMethod);

      _instanceContext.GetBaseCall ("InternalMethod");
    }

    [Test]
    public void GetBaseCall_MethodInfo_Params ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (1));
      var arguments = new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof (int)) };

      var result = _instanceContext.GetBaseCall (method, arguments.AsOneTime());

      Assert.That (result.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = (ThisExpression) result.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_declaringType));

      CheckBaseCallMethodInfo (method, result);

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
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot perform base call for static method.\r\nParameter name: baseMethod")]
    public void GetBaseCall_MethodInfo_Params_StaticMethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeStaticMethod();
      _instanceContext.GetBaseCall (method);
    }

    [Test]
    public void GetBaseCall_MethodInfo_Params_AllowedVisibility ()
    {
      var protectedMethod = typeof (DomainType).GetMethod ("ProtectedMethod", BindingFlags.NonPublic | BindingFlags.Instance);
      var protectedInternalMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ProtectedInternalMethod ());

      var result1 = _instanceContext.GetBaseCall (protectedMethod);
      var result2 = _instanceContext.GetBaseCall (protectedInternalMethod);

      CheckBaseCallMethodInfo(protectedMethod, result1);
      CheckBaseCallMethodInfo(protectedInternalMethod, result2);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "Can only call public, protected, or protected internal methods.\r\nParameter name: baseMethod")]
    public void GetBaseCall_MethodInfo_Params_DisallowedVisibility ()
    {
      var internalMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.InternalMethod());
      _instanceContext.GetBaseCall (internalMethod);
    }
    
    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot perform base call on abstract method.\r\nParameter name: baseMethod")]
    public void GetBaseCall_MethodInfo_Abstract_Throws ()
    {
      var abstractMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractType obj) => obj.Method());
      _instanceContext.GetBaseCall (abstractMethod);
    }

    [Test]
    public void GetCopiedMethodBody_Params ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));
      GetCopiedMethodBodyAndCheckOriginalBodyExpression (_instanceContext, method);
    }

    [Test]
    public void GetCopiedMethodBody_FromStaticToInstance ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.StaticMetod (7));
      GetCopiedMethodBodyAndCheckOriginalBodyExpression (_instanceContext, method);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException),
        ExpectedMessage = "The body of an instance method cannot be copied into a static method.\r\nParameter name: otherMethod")]
    public void GetCopiedMethodBody_FromInstanceToStatic ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));
      GetCopiedMethodBodyAndCheckOriginalBodyExpression (_staticContext, method);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "The specified method is declared by a different type 'UnrelatedType'.\r\nParameter name: otherMethod")]
    public void GetCopiedMethodBody_Params_DeclaredByUnrelatedType ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnrelatedType obj) => obj.UnrelatedMethod (7));
      var methodToCopy = MutableMethodInfoObjectMother.CreateForExisting (underlyingMethod: method);

      _instanceContext.GetCopiedMethodBody (methodToCopy);
    }

    [Test]
    public void GetCopiedMethodBody_Enumerable ()
    {
      var methodToCopy = MutableMethodInfoObjectMother.Create (
          declaringType: _declaringType, returnType: typeof (int), parameters: new[] { new ParameterDeclaration (typeof (int), "i") });
      methodToCopy.SetBody (ctx => ctx.Parameters[0]);
      var argument = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));

      var result = _instanceContext.GetCopiedMethodBody (methodToCopy, new[] { argument }.AsOneTime());

      Assert.That (result, Is.SameAs (argument));
    }

    private void CheckBaseCallMethodInfo (MethodInfo method, MethodCallExpression baseCallExpression)
    {
      Assert.That (baseCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter> ());
      var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) baseCallExpression.Method;
      Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethod, Is.SameAs (method));
    }

    private void GetCopiedMethodBodyAndCheckOriginalBodyExpression (BodyContextBase context, MethodInfo method)
    {
      var methodToCopy = MutableMethodInfoObjectMother.CreateForExisting (underlyingMethod: method);
      var argument = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));

      var result = context.GetCopiedMethodBody (methodToCopy, argument);

      Assert.That (result, Is.TypeOf<OriginalBodyExpression> ());
      var originalBodyExpression = (OriginalBodyExpression) result;
      Assert.That (originalBodyExpression.MethodBase, Is.SameAs (method));
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (new[] { argument }));
    }

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
    private class DomainTypeBase { }

    private class DomainType : DomainTypeBase
    {
      public void Method (int i) { }
      public void StaticMetod (int i) { }

      public void FakeBaseMethod (int i) { }

      protected void ProtectedMethod () { }
      protected internal void ProtectedInternalMethod () { }
      internal void InternalMethod () { }
    }

    private abstract class AbstractType
    {
      public abstract void Method ();
    }

    private class UnrelatedType
    {
      public void UnrelatedMethod (int i) { }
    }
// ReSharper restore UnusedParameter.Local
// ReSharper restore UnusedMember.Local
  }
}