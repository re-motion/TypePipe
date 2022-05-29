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
using NUnit.Framework;
using Remotion.TypePipe.Development.UnitTesting.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class BodyProviderUtilityTest
  {
    private BodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new TestableBodyContextBase (MutableTypeObjectMother.Create(), false);
    }

    [Test]
    public void GetTypedBody_VoidConversion ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      var bodyProvider = CreateBodyProvider(body);

      var result = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, _context);

      var expectedBody = Expression.Block (typeof (void), body);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, result);
    }

    [Test]
    public void GetTypedBody_BoxingConversion ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      var bodyProvider = CreateBodyProvider (body);

      var result = BodyProviderUtility.GetTypedBody (typeof (object), bodyProvider, _context);

      var expectedBody = Expression.Convert (body, typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, result);
    }

    [Test]
    public void GetTypedBody_ThrowsForNullBody ()
    {
      var bodyProvider = CreateBodyProvider (returnedBody: null);
      Assert.That (
          () => BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, _context),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Provider must not return null.", "bodyProvider"));
    }

    private Func<BodyContextBase, Expression> CreateBodyProvider (Expression returnedBody)
    {
      return ctx =>
      {
        Assert.That (ctx, Is.EqualTo (_context));
        return returnedBody;
      };
    }
  }
}