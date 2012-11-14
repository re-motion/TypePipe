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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class BodyProviderUtilityTest
  {
    private IMemberSelector _memberSelector;

    private BodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _memberSelector = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _context = new TestableBodyContextBase (MutableTypeObjectMother.Create(), false, _memberSelector);
    }

    [Test]
    public void GetNonNullBody ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression();
      var bodyProvider = CreateBodyProvider (body);

      var result = BodyProviderUtility.GetNonNullBody (bodyProvider, _context);

      Assert.That (result, Is.SameAs (body));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Body provider must return non-null body.")]
    public void GetNonNullBody_ThrowsForNull ()
    {
      var bodyProvider = CreateBodyProvider (returnedBody: null);

      BodyProviderUtility.GetNonNullBody (bodyProvider, _context);
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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Body provider must return non-null body.")]
    public void GetTypedBody_ThrowsForNullBody ()
    {
      var bodyProvider = CreateBodyProvider (returnedBody: null);

      BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, _context);
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