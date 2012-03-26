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
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ConstructorAdditionContextTest
  {
    private ReadOnlyCollection<ParameterExpression> _emptyParameters;

    [SetUp]
    public void SetUp ()
    {
      _emptyParameters = new List<ParameterExpression> ().AsReadOnly ();
    }

    [Test]
    public void Initialization ()
    {
      var mutableType = MutableTypeObjectMother.Create ();
      var parameter1 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameter2 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameters = new List<ParameterExpression> { parameter1, parameter2 }.AsReadOnly ();

      var context = new ConstructorAdditionContext (mutableType, parameters);

      Assert.That (context.ParameterExpressions, Is.EqualTo (new[] { parameter1, parameter2 }));
    }

    [Test]
    public void ThisExpression ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var context = new ConstructorAdditionContext (mutableType, _emptyParameters);

      Assert.That (context.ThisExpression, Is.TypeOf<ThisExpression>());
      Assert.That (context.ThisExpression.Type, Is.SameAs (mutableType));
    }

    [Test]
    public void GetConstructorCallExpression ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (ClassWithConstructors));
      var context = new ConstructorAdditionContext (mutableType, _emptyParameters);

      var argumentExpressions = new ArgumentTestHelper ("string").Expressions;
      var result = context.GetConstructorCallExpression (argumentExpressions);

      Assert.That (result, Is.AssignableTo<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) result;

      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression> ());
      Assert.That (methodCallExpression.Object.Type, Is.SameAs (mutableType));

      Assert.That(methodCallExpression.Method, Is.TypeOf<ConstructorAsMethodInfoAdapter>());
      var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) methodCallExpression.Method;
      var expectedCtor = mutableType.GetConstructor (new[] { typeof (object) });
      Assert.That (constructorAsMethodInfoAdapter.ConstructorInfo, Is.SameAs (expectedCtor));

      Assert.That (methodCallExpression.Arguments, Is.EqualTo(argumentExpressions));
    }

    [Ignore("TODO 4686")]
    [Test]
    [ExpectedException(typeof(MemberNotFoundException), ExpectedMessage =
      "Could not find a constructor with signature (Int32, Int32) on type 'ClassWithConstructors'.")]
    public void GetConstructorCallExpression_NoMatchingConstructor ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (ClassWithConstructors));
      var context = new ConstructorAdditionContext (mutableType, _emptyParameters);

      var argumentExpressions = new ArgumentTestHelper (7, 8).Expressions;
      context.GetConstructorCallExpression (argumentExpressions);
    }

    private class ClassWithConstructors
    {
      public ClassWithConstructors (object o)
      {
      }
    }
  }
}