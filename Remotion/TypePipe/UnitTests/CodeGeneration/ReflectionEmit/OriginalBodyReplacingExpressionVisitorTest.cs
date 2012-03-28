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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class OriginalBodyReplacingExpressionVisitorTest
  {
    private MethodInfo _baseMethod;
    private OriginalBodyReplacingExpressionVisitor _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _baseMethod = ReflectionObjectMother.GetMethod ((DomainClass dc) => dc.M (7, "string"));
      _visitorPartialMock = MockRepository.GeneratePartialMock<OriginalBodyReplacingExpressionVisitor>(_baseMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Method must not be static.\r\nParameter name: baseMethod")]
    public void Initialization_StaticMethod ()
    {
      var staticMethod = typeof (int).GetMethod ("Parse", new[] { typeof (string) });
      Assert.That (staticMethod.IsStatic, Is.True);

      new OriginalBodyReplacingExpressionVisitor (staticMethod);
    }

    [Test]
    public void VisitOriginalBody ()
    {
      var arguments = new ArgumentTestHelper (7, "string").Expressions;
      var expression = new OriginalBodyExpression (_baseMethod.ReturnType, arguments);
      var fakeResult = ExpressionTreeObjectMother.GetSomeExpression();

      _visitorPartialMock
          .Expect (mock => mock.Visit (Arg<Expression>.Matches (e => e is MethodCallExpression)))
          .Return (fakeResult)
          .WhenCalled (mi =>
          {
            var methodCallExpression = (MethodCallExpression) mi.Arguments[0];
            Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression> ().With.Property ("Type").SameAs (_baseMethod.DeclaringType));
            Assert.That (methodCallExpression.Method, Is.SameAs (_baseMethod));
            Assert.That (methodCallExpression.Arguments, Is.EqualTo (arguments));
          });

      var result = TypePipeExpressionVisitorTestHelper.CallVisitOriginalBody (_visitorPartialMock, expression);

      _visitorPartialMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    public class DomainClass
    {
// ReSharper disable UnusedParameter.Global
      public int M (int p1, string p2)
// ReSharper restore UnusedParameter.Global
      {
        throw new NotImplementedException();
      }
    }
  }
}