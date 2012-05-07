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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.Expressions;
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
    private IRelatedMethodFinder _relatedMetodFinder;

    [SetUp]
    public void SetUp ()
    {
      _emptyParameters = new List<ParameterExpression> ().AsReadOnly ();
      _mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (DomainType));
      _relatedMetodFinder = MockRepository.GenerateStrictMock<IRelatedMethodFinder> ();
    }

    [Test]
    public void Initialization ()
    {
      var parameter1 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameter2 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameters = new List<ParameterExpression> { parameter1, parameter2 }.AsReadOnly ();

      var isStatic = BooleanObjectMother.GetRandomBoolean();
      var context = new TestableMethodBodyContextBase (_mutableType, parameters.AsOneTime(), isStatic, _relatedMetodFinder);

      Assert.That (context.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (context.Parameters, Is.EqualTo (new[] { parameter1, parameter2 }));
      Assert.That (context.IsStatic, Is.EqualTo(isStatic));
    }

    [Test]
    public void This ()
    {
      var context = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, false, _relatedMetodFinder);

      Assert.That (context.This, Is.TypeOf<ThisExpression>());
      Assert.That (context.This.Type, Is.SameAs (_mutableType));
    }

    [Test]
    public void This_ThrowsForStaticMethods ()
    {
      var context = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, true, _relatedMetodFinder);

      Assert.That (() => context.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
    }

    //[Test]
    //public void GetBaseCall ()
    //{
    //  var context = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, false, _relatedMetodFinder);

    //  var method = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((DomainTypeBase obj) => obj.ShadowedVirtualMethod(1));
    //  var arguments = new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof(int)) };
    //  var result = context.GetBaseCall (method, arguments);

    //  Assert.That (result.Method, Is.SameAs (method));
    //  Assert.That (result.Arguments, Is.EqualTo (arguments));
    //  Assert.That (result.Object, Is.TypeOf<ThisExpression>());
    //  var thisExpression = (ThisExpression) result.Object;
    //  Assert.That (thisExpression.Type, Is.SameAs (_mutableType));
    //}

    //[Test]
    //[ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot perform base call from static method.")]
    //public void GetBaseCall_ForStaticMethods ()
    //{
    //  var context = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, true, _relatedMetodFinder);

    //  var method = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((DomainTypeBase obj) => obj.ShadowedVirtualMethod (1));
    //  var arguments = new[] { ExpressionTreeObjectMother.GetSomeExpression (typeof (int)) };
    //  context.GetBaseCall (method, arguments);
    //}

    private class DomainTypeBase
    {
      public virtual void ShadowedVirtualMethod (int i) { }
    }

    private class DomainType : DomainTypeBase
    {
      public new virtual void ShadowedVirtualMethod (int i) { }
    }

  }
}