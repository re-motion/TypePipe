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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class ConstructorBodyContextBaseTest
  {
    private ProxyType _declaringType;
    private ParameterExpression[] _parameters;

    private ConstructorBodyContextBase _context;
    private ConstructorBodyContextBase _staticContext;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ProxyTypeObjectMother.Create (typeof (DomainType), name: "Domain_Proxy");
      _parameters = new[] { Expression.Parameter (typeof (string)) };
      var memberSelectorStub = MockRepository.GenerateStub<IMemberSelector>();

      _context = new TestableConstructorBodyContextBase (_declaringType, false, _parameters.AsOneTime(), memberSelectorStub);
      _staticContext = new TestableConstructorBodyContextBase (_declaringType, true, _parameters.AsOneTime(), memberSelectorStub);
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
      var expected = Expression.Call (new ThisExpression (_declaringType), NonVirtualCallMethodInfoAdapter.Adapt (expectedCtor), arguments);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    [ExpectedException (typeof (MemberAccessException), ExpectedMessage = "The matching constructor is not visible from the proxy type.")]
    public void CallBaseConstructor_NotVisibleFromProxy ()
    {
      _context.CallBaseConstructor();
    }

    [Test]
    public void CallThisConstructor ()
    {
      var arguments = new ArgumentTestHelper ("string").Expressions;

      var result = _context.CallThisConstructor (arguments.AsOneTime());

      var expectedCtor = _declaringType.AddedConstructors.Single();
      var expected = Expression.Call (new ThisExpression (_declaringType), NonVirtualCallMethodInfoAdapter.Adapt (expectedCtor), arguments);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
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

    class DomainType
    {
      public DomainType (string s) { Dev.Null = s; }
      internal DomainType () { }
    }
  }
}