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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class BodyProviderUtilityTest
  {
    [Test]
    public void GetVoidBody ()
    {
      var context = new TestableConstructorBodyContextBase (MutableTypeObjectMother.Create(), new ParameterExpression[0]);

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      Func<TestableConstructorBodyContextBase, Expression> bodyProvider = c =>
      {
        Assert.That (c, Is.EqualTo (context));
        return fakeBody;
      };

      var result = BodyProviderUtility.GetVoidBody (bodyProvider, context);
      
      Assert.That (result, Is.SameAs (fakeBody));
    }

    [Test]
    public void GetVoidBody_WrapsNonVoidBody ()
    {

      var context = new TestableConstructorBodyContextBase (MutableTypeObjectMother.Create (), new ParameterExpression[0]);

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<TestableConstructorBodyContextBase, Expression> bodyProvider = c =>
      {
        Assert.That (c, Is.EqualTo (context));
        return fakeBody;
      };

      var result = BodyProviderUtility.GetVoidBody (bodyProvider, context);

      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, result);
    }
  }
}