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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class ConstructorBodyContextBaseTest
  {
    private MutableType _declaringType;
    private ParameterExpression[] _parameters;

    private ConstructorBodyContextBase _context;
    private ConstructorBodyContextBase _staticContext;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create (baseType: typeof (DomainType), name: "Domain_Proxy", copyCtorsFromBase: true);
      _parameters = new[] { Expression.Parameter (typeof (string)) };

      _context = new TestableConstructorBodyContextBase (_declaringType, false, _parameters.AsOneTime());
      _staticContext = new TestableConstructorBodyContextBase (_declaringType, true, _parameters.AsOneTime());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.IsStatic, Is.False);
      Assert.That (_staticContext.IsStatic, Is.True);
    }

    [Test]
    public void CallBaseConstructor ()
    {
      var arguments = new ArgumentTestHelper ("string").Expressions;

      var result = _context.CallBaseConstructor (arguments.AsOneTime());

      var expectedCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (""));
      CheckConstructorCall (expectedCtor, arguments, result);
    }

    [Test]
    [ExpectedException (typeof (MemberAccessException), ExpectedMessage = "The matching constructor is not visible from the proxy type.")]
    public void CallBaseConstructor_NotVisibleFromProxy ()
    {
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      Assert.That (ctor, Is.Not.Null);

      _context.CallBaseConstructor();
    }

    [Test]
    public void CallThisConstructor ()
    {
      var arguments = new ArgumentTestHelper ("string").Expressions;

      var result = _context.CallThisConstructor (arguments.AsOneTime());

      var expectedCtor = _declaringType.AddedConstructors.Single (c => c.GetParameters().Single().ParameterType == typeof (string));
      CheckConstructorCall (expectedCtor, arguments, result);
    }

    [Test]
    public void CallXXXConstructor_ByRefParams ()
    {
      var argument = new Expression[] { Expression.Parameter (typeof (int).MakeByRefType()) };

      var result1 = _context.CallBaseConstructor (argument);
      var result2 = _context.CallThisConstructor (argument);

      var baseCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (out Dev<int>.Dummy));
      var thisCtor = _declaringType.AddedConstructors.Single (c => c.GetParameters().Single().ParameterType == typeof (int).MakeByRefType());
      CheckConstructorCall (baseCtor, argument, result1);
      CheckConstructorCall (thisCtor, argument, result2);
    }

    [Test]
    public void CallXXXConstructor_Exceptions ()
    {
      var arguments = new ArgumentTestHelper (7, "8").Expressions;

      Assert.That (
          () => _context.CallBaseConstructor (arguments),
          Throws.TypeOf<MissingMemberException>()
                .With.Message.EqualTo ("Could not find an instance constructor with signature (System.Int32, System.String) on type 'DomainType'."));
      Assert.That (
          () => _context.CallThisConstructor (arguments),
          Throws.TypeOf<MissingMemberException> ()
                .With.Message.EqualTo ("Could not find an instance constructor with signature (System.Int32, System.String) on type 'Domain_Proxy'."));

      Assert.That (
          () => _staticContext.CallBaseConstructor (arguments),
          Throws.InvalidOperationException.With.Message.EqualTo ("Cannot call other constructor from type initializer."));
      Assert.That (
          () => _staticContext.CallThisConstructor (arguments),
          Throws.InvalidOperationException.With.Message.EqualTo ("Cannot call other constructor from type initializer."));
    }

    private void CheckConstructorCall (ConstructorInfo expectedConstructor, Expression[] arguments, MethodCallExpression actualCall)
    {
      var expected = Expression.Call (new ThisExpression (_declaringType), NonVirtualCallMethodInfoAdapter.Adapt (expectedConstructor), arguments);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, actualCall);
    }

    public class DomainType
    {
      public DomainType (string s) { Dev.Null = s; }
      internal DomainType () { }
      public DomainType (out int i) { i = 7; }
    }
  }
}