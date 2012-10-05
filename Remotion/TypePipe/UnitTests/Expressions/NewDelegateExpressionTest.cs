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
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

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
      _nonVirtualInstanceMethod = ReflectionObjectMother.GetSomeNonVirtualInstanceMethod();
      _delegateType = typeof (Action);
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
      var method = ReflectionObjectMother.GetSomeStaticMethod();

      var expression = new NewDelegateExpression (typeof (Action), null, method);

      Assert.That (expression.Target, Is.Null);
    }

    [Test]
    public void Initialization_DelegateTypeIsSubclassOfSystemDelegate ()
    {
      Assert.That (
          () => new NewDelegateExpression (typeof (string), _target, _nonVirtualInstanceMethod),
          Throws.ArgumentException.With.Message.EqualTo ("Delegate type must be subclass of 'System.Delegate'.\r\nParameter name: delegateType"));
      Assert.That (
          () => new NewDelegateExpression (typeof (Delegate), _target, _nonVirtualInstanceMethod),
          Throws.ArgumentException.With.Message.EqualTo ("Delegate type must be subclass of 'System.Delegate'.\r\nParameter name: delegateType"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Instance method requires target.\r\nParameter name: method")]
    public void Initialization_InstanceMethodRequiresTarget ()
    {
      var method = ReflectionObjectMother.GetSomeInstanceMethod();

      new NewDelegateExpression (typeof (Action), null, method);
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

    // TODO 5080: check that delegatetype matches method

    [Test]
    public void CanReduce ()
    {
      Assert.That (_expression.CanReduce, Is.True);
    }

    [Test]
    public void Reduce ()
    {
      var result = _expression.Reduce();

      Assert.That (result, Is.TypeOf<NewExpression>());

      var newExpression = (NewExpression) result;
      Assert.That (newExpression.Constructor, Is.EqualTo (_delegateType.GetConstructor (new[] { typeof (object), typeof (IntPtr) })));
      Assert.That (newExpression.Arguments, Has.Count.EqualTo (2));
      Assert.That (newExpression.Arguments[0], Is.EqualTo (_target));
      Assert.That (newExpression.Arguments[1], Is.TypeOf<NonVirtualMethodAddressExpression>());

      var methodAddressExpression = (NonVirtualMethodAddressExpression) newExpression.Arguments[1];
      Assert.That (methodAddressExpression.Method, Is.SameAs (_nonVirtualInstanceMethod));
    }

    [Test]
    public void Reduce_StaticMethod ()
    {
      var delegateType = typeof (Action);
      var staticMethod = ReflectionObjectMother.GetSomeStaticMethod();
      var expression = new NewDelegateExpression (delegateType, null, staticMethod);

      var result = expression.Reduce();

      var newExpression = (NewExpression) result;
      Assert.That (newExpression.Arguments[0], Is.TypeOf<ConstantExpression>());

      var constantExpression = (ConstantExpression) newExpression.Arguments[0];
      Assert.That (constantExpression.Type, Is.SameAs (typeof (object)));
      Assert.That (constantExpression.Value, Is.Null);
    }

    [Test]
    public void Reduce_VirtualMethod ()
    {
      var delegateType = typeof (Action);
      var virtualMethod = ReflectionObjectMother.GetSomeVirtualMethod();
      var target = ExpressionTreeObjectMother.GetSomeExpression (virtualMethod.DeclaringType);
      var expression = new NewDelegateExpression (delegateType, target, virtualMethod);

      var result = expression.Reduce ();

      var newExpression = (NewExpression) result;
      Assert.That (newExpression.Arguments[1], Is.TypeOf<VirtualMethodAddressExpression>());

      var virtualMethodAddressExpression = (VirtualMethodAddressExpression) newExpression.Arguments[1];
      Assert.That (virtualMethodAddressExpression.Instance, Is.SameAs (target));
      Assert.That (virtualMethodAddressExpression.Method, Is.SameAs (virtualMethod));
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

      var expressionVisitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();
      expressionVisitorMock.Expect (mock => mock.Visit (_expression.Target)).Return (newTargetExpression);

      var result = ExpressionTestHelper.CallVisitChildren (_expression, expressionVisitorMock);

      expressionVisitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (result.Type, Is.SameAs (_expression.Type));
      Assert.That (result, Is.TypeOf<NewDelegateExpression>());

      var virtualMethodAddressExpression = (NewDelegateExpression) result;
      Assert.That (virtualMethodAddressExpression.Target, Is.SameAs (newTargetExpression));
      Assert.That (virtualMethodAddressExpression.Method, Is.SameAs (_expression.Method));
    }

    interface IDomainInterface { void Method (); }
    class BaseType { public virtual void Method () { } }
    class DomainType : BaseType, IDomainInterface { public override void Method () { } }
    class UnrelatedType { public virtual void Method () { } }
  }
}