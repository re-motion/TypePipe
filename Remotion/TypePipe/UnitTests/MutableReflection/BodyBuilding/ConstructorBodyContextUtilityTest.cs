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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class ConstructorBodyContextUtilityTest
  {
    [Test]
    public void GetConstructorCall ()
    {
      var thisExpression = ExpressionTreeObjectMother.GetSomeExpression(typeof(ClassWithConstructors));
      var argumentExpressions = new ArgumentTestHelper ("string").Expressions;
      var result = ConstructorBodyContextUtility.GetConstructorCallExpression (thisExpression, argumentExpressions.AsOneTime());

      Assert.That (result, Is.AssignableTo<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) result;

      Assert.That (methodCallExpression.Object, Is.SameAs(thisExpression));
      Assert.That (methodCallExpression.Method, Is.TypeOf<ConstructorAsMethodInfoAdapter> ());
      var constructorAsMethodInfoAdapter = (ConstructorAsMethodInfoAdapter) methodCallExpression.Method;

      var expectedCtor = MemberInfoFromExpressionUtility.GetConstructor(() => new ClassWithConstructors(null));
      Assert.That (constructorAsMethodInfoAdapter.ConstructorInfo, Is.EqualTo (expectedCtor));

      Assert.That (methodCallExpression.Arguments, Is.EqualTo (argumentExpressions));
    }

    [Test]
    [ExpectedException (typeof (MemberNotFoundException), ExpectedMessage =
        "Could not find a constructor with signature (System.Int32, System.Int32) on type " +
        "'Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding.ConstructorBodyContextUtilityTest+ClassWithConstructors'.")]
    public void GetConstructorCall_NoMatchingConstructor ()
    {
      var thisExpression = ExpressionTreeObjectMother.GetSomeExpression (typeof (ClassWithConstructors));
      var argumentExpressions = new ArgumentTestHelper (7, 8).Expressions;
      ConstructorBodyContextUtility.GetConstructorCallExpression (thisExpression, argumentExpressions);
    }

    private class ClassWithConstructors
    {
      public ClassWithConstructors (object o)
      {
        Dev.Null = o;
      }
    }
  }
}