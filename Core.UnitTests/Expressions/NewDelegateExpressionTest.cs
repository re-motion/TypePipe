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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Moq;

namespace Remotion.TypePipe.UnitTests.Expressions
{
  [TestFixture]
  public class NewDelegateExpressionTest
  {
    private Type _delegateType;
    private Expression _target;
    private MethodInfo _nonVirtualInstanceMethod;

    private NewDelegateExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _nonVirtualInstanceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (7, null));
      _delegateType = typeof (Func<int, object, string>);
      _target = ExpressionTreeObjectMother.GetSomeExpression (_nonVirtualInstanceMethod.DeclaringType);

      _expression = new NewDelegateExpression (_delegateType, _target, _nonVirtualInstanceMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.Type, Is.SameAs (_delegateType));
      Assert.That (_expression.Target, Is.SameAs (_target));
      Assert.That (_expression.Method, Is.SameAs (_nonVirtualInstanceMethod));
    }

    [Test]
    public void Initialization_StaticMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => StaticMethod());

      var expression = new NewDelegateExpression (typeof (Action), null, method);

      Assert.That (expression.Target, Is.Null);
    }

    [Test]
    public void Initialization_DelegateTypeIsSubclassOfMulticastDelegate ()
    {
      Assert.That (
          () => new NewDelegateExpression (typeof (string), _target, _nonVirtualInstanceMethod),
          Throws.ArgumentException.With.Message.EqualTo (
              "Delegate type must be subclass of 'System.MulticastDelegate'.\r\nParameter name: delegateType"));
      Assert.That (
          () => new NewDelegateExpression (typeof (Delegate), _target, _nonVirtualInstanceMethod),
          Throws.ArgumentException.With.Message.EqualTo (
              "Delegate type must be subclass of 'System.MulticastDelegate'.\r\nParameter name: delegateType"));
      Assert.That (
          () => new NewDelegateExpression (typeof (MulticastDelegate), _target, _nonVirtualInstanceMethod),
          Throws.ArgumentException.With.Message.EqualTo (
              "Delegate type must be subclass of 'System.MulticastDelegate'.\r\nParameter name: delegateType"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Instance method requires target.\r\nParameter name: target")]
    public void Initialization_InstanceMethodRequiresTarget ()
    {
      var method = ReflectionObjectMother.GetSomeInstanceMethod();

      new NewDelegateExpression (typeof (Action), null, method);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Static method must not have target.\r\nParameter name: target")]
    public void Initialization_StaticMethodRequiresNullTarget ()
    {
      var method = ReflectionObjectMother.GetSomeStaticMethod();
      var target = ExpressionTreeObjectMother.GetSomeExpression (method.DeclaringType);

      new NewDelegateExpression (typeof (Action), target, method);
    }

    [Test]
    public void Initialization_MethodDeclaringTypeIsAssignableFromTargetType ()
    {
      var target = ExpressionTreeObjectMother.GetSomeExpression (typeof (DomainType));

      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method ());
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((BaseType obj) => obj.Method ());
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.Method ());
      var unrelatedMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnrelatedType obj) => obj.Method ());

      Assert.That (() => new NewDelegateExpression (typeof (Action), target, method), Throws.Nothing);
      Assert.That (() => new NewDelegateExpression (typeof (Action), target, baseMethod), Throws.Nothing);
      Assert.That (() => new NewDelegateExpression (typeof (Action), target, interfaceMethod), Throws.Nothing);

      Assert.That (
          () => new NewDelegateExpression (typeof (Action), target, unrelatedMethod),
          Throws.ArgumentException.With.Message.EqualTo ("Method is not declared on type hierarchy of target.\r\nParameter name: method"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Method signature must match delegate type.\r\nParameter name: method")]
    public void Initialization_MethodSignatureMustMatchDelegateType ()
    {
      var delegateType = typeof (Action<string>);
      var target = ExpressionTreeObjectMother.GetSomeExpression (typeof (DomainType));
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method ());

      new NewDelegateExpression (delegateType, target, method);
    }

    [Test]
    public void Accept ()
    {
      ExpressionTestHelper.CheckAccept (_expression, mock => mock.VisitNewDelegate (_expression));
    }

    [Test]
    public void VisitChildren_NoChanges ()
    {
      ExpressionTestHelper.CheckVisitChildren_NoChanges (_expression, _expression.Target);
    }

    [Test]
    public void VisitChildren_WithChanges ()
    {
      var newTargetExpression = ExpressionTreeObjectMother.GetSomeExpression (_expression.Target.Type);

      var expressionVisitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      expressionVisitorMock.Setup (mock => mock.Visit (_expression.Target)).Returns (newTargetExpression).Verifiable();

      var result = _expression.Invoke<Expression> ("VisitChildren", expressionVisitorMock.Object);

      expressionVisitorMock.Verify();

      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (result.Type, Is.SameAs (_expression.Type));
      Assert.That (result, Is.TypeOf<NewDelegateExpression>());

      var virtualMethodAddressExpression = (NewDelegateExpression) result;
      Assert.That (virtualMethodAddressExpression.Target, Is.SameAs (newTargetExpression));
      Assert.That (virtualMethodAddressExpression.Method, Is.SameAs (_expression.Method));
    }

    string Method (int i, object o)
    {
      Dev.Null = i;
      Dev.Null = o;
      return "";
    }
    static void StaticMethod () { }

    interface IDomainInterface { void Method (); }
    class BaseType { public virtual void Method () { } }
    class DomainType : BaseType, IDomainInterface { public override void Method () { } }
    class UnrelatedType { public virtual void Method () { } }
  }
}