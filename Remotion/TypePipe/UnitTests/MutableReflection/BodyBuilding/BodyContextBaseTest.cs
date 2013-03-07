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
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
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
    private ProxyType _proxyType;
    private IMemberSelector _memberSelectorMock;

    private BodyContextBase _staticContext;
    private BodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _proxyType = ProxyTypeObjectMother.Create (baseType: typeof (DomainType));
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();

      _staticContext = new TestableBodyContextBase (_proxyType, true, _memberSelectorMock);
      _context = new TestableBodyContextBase (_proxyType, false, _memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      var isStatic = BooleanObjectMother.GetRandomBoolean();
      var context = new TestableBodyContextBase (_proxyType, isStatic, _memberSelectorMock);

      Assert.That (context.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (context.IsStatic, Is.EqualTo (isStatic));
    }

    [Test]
    public void This ()
    {
      Assert.That (_context.This.Type, Is.SameAs (_proxyType));
    }

    [Test]
    public void This_StaticContext ()
    {
      Assert.That (() => _staticContext.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
    }

    [Test]
    public void CallBase ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (1));
      var arguments = new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof (int)) };

      var result = _context.CallBase (method, arguments.AsOneTime());

      Assert.That (result.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = (ThisExpression) result.Object;
      Assert.That (thisExpression.Type, Is.SameAs (_proxyType));

      CheckBaseCallMethodInfo (method, result);

      Assert.That (result.Arguments, Is.EqualTo (arguments));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot perform base call from static method.")]
    public void CallBase_StaticContext ()
    {
      var method = ReflectionObjectMother.GetSomeInstanceMethod();
      _staticContext.CallBase (method);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot perform base call for static method.\r\nParameter name: baseMethod")]
    public void CallBase_StaticMethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeStaticMethod();
      _context.CallBase (method);
    }

    [Test]
    public void CallBase_AllowedVisibility ()
    {
      var protectedMethod = typeof (DomainType).GetMethod ("ProtectedMethod", BindingFlags.NonPublic | BindingFlags.Instance);
      var protectedInternalMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ProtectedInternalMethod ());

      var result1 = _context.CallBase (protectedMethod);
      var result2 = _context.CallBase (protectedInternalMethod);

      CheckBaseCallMethodInfo(protectedMethod, result1);
      CheckBaseCallMethodInfo(protectedInternalMethod, result2);
    }

    [Test]
    [ExpectedException (typeof (MemberAccessException), ExpectedMessage =
        "Matching base method 'DomainType.InternalMethod' is not accessible from proxy type.")]
    public void CallBase_DisallowedVisibility ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.InternalMethod());
      _context.CallBase (method);
    }
    
    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot perform base call on abstract method.\r\nParameter name: baseMethod")]
    public void CallBase_Abstract ()
    {
      var abstractMethod = ReflectionObjectMother.GetSomeAbstractMethod();
      _context.CallBase (abstractMethod);
    }

    [Test]
    public void CopyMethodBody ()
    {
      CopyMethodBodyAndCheckResult (_context, 0 /* instance */);
      CopyMethodBodyAndCheckResult (_context, MethodAttributes.Static);
      CopyMethodBodyAndCheckResult (_staticContext, MethodAttributes.Static);

      Assert.That (
          () => _staticContext.CopyMethodBody (MutableMethodInfoObjectMother.Create (_proxyType)),
          Throws.ArgumentException.With.Message.EqualTo (
              "The body of an instance method cannot be copied into a static method.\r\nParameter name: otherMethod"));
      Assert.That (
          () => _context.CopyMethodBody (MutableMethodInfoObjectMother.Create (ProxyTypeObjectMother.Create (name: "Abc"))),
          Throws.ArgumentException.With.Message.EqualTo (
              "The specified method is declared by a different type 'Abc'.\r\nParameter name: otherMethod"));
    }

    [Test]
    public void CopyMethodBody_Enumerable ()
    {
      var methodToCopy = MutableMethodInfoObjectMother.Create (
          declaringType: _proxyType, returnType: typeof (int), parameters: new[] { new ParameterDeclaration (typeof (int), "i") });
      methodToCopy.SetBody (ctx => ctx.Parameters[0]);
      var argument = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));

      var result = _context.CopyMethodBody (methodToCopy, new[] { argument }.AsOneTime());

      Assert.That (result, Is.SameAs (argument));
    }

    private void CheckBaseCallMethodInfo (MethodInfo method, MethodCallExpression baseCallExpression)
    {
      Assert.That (baseCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter> ());
      var nonVirtualCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) baseCallExpression.Method;
      Assert.That (nonVirtualCallMethodInfoAdapter.AdaptedMethod, Is.SameAs (method));
    }

    private void CopyMethodBodyAndCheckResult (BodyContextBase context, MethodAttributes methodAttributes)
    {
      var parameter = ParameterDeclarationObjectMother.CreateMultiple (2);
      var constantBodyPart = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var body = Expression.Block (parameter[0].Expression, parameter[1].Expression, constantBodyPart);
      var methodToCopy = MutableMethodInfoObjectMother.Create (
          declaringType: _proxyType, attributes: methodAttributes, returnType: typeof (int), parameters: parameter, body: body);
      var arguments = parameter.Select (p => ExpressionTreeObjectMother.GetSomeExpression (p.Type)).ToArray();

      var result = context.CopyMethodBody (methodToCopy, arguments.AsOneTime());

      var expectedBody = Expression.Block (arguments[0], arguments[1], constantBodyPart);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, result);
    }

    public class DomainType
    {
      public void Method (int i) { Dev.Null = i; }
      public void MethodWithByRefParam (ref int i) { i++; }

      [UsedImplicitly] protected void ProtectedMethod () { }
      protected internal void ProtectedInternalMethod () { }
      internal void InternalMethod () { }
    }
  }
}