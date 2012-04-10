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
    private ConstructorBodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new TestableConstructorBodyContextBase (MutableTypeObjectMother.Create (), new ParameterExpression[0]);
    }

    [Test]
    public void GetVoidBody ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      var bodyProvider = CreateBodyProvider(body);

      var result = BodyProviderUtility.GetVoidBody (bodyProvider, _context);
      
      Assert.That (result, Is.SameAs (body));
    }

    [Test]
    public void GetVoidBody_WrapsNonVoidBody ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      var bodyProvider = CreateBodyProvider(body);

      var result = BodyProviderUtility.GetVoidBody (bodyProvider, _context);

      var expectedBody = Expression.Block (typeof (void), body);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, result);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Body provider must return non-null body.")]
    public void GetVoidBody_ThrowsForNullBody ()
    {
      Func<ConstructorBodyContextBase, Expression> bodyProvider = c => null;

      BodyProviderUtility.GetVoidBody (bodyProvider, _context);
    }

    private Func<ConstructorBodyContextBase, Expression> CreateBodyProvider (Expression body)
    {
      return c =>
      {
        Assert.That (c, Is.EqualTo (_context));
        return body;
      };
    }
  }
}