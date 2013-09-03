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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class BodyContextUtilityTest
  {
    private ParameterExpression[] _parameters;
    private Expression _previousBody;

    [SetUp]
    public void SetUp ()
    {
      _parameters = new[] { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (object)) };
      _previousBody = Expression.Block (_parameters[0], _parameters[1]);
    }

    [Test]
    public void ReplaceParameters ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      var invokedBody = CallReplaceParameters (arg1, arg2);

      var expectedBody = Expression.Block (arg1, arg2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The argument count (0) does not match the parameter count (2).\r\nParameter name: arguments")]
    public void ReplaceParameters_WrongNumberOfArguments ()
    {
      CallReplaceParameters();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The argument at index 0 has an invalid type: Type 'System.String' cannot be implicitly converted to type 'System.Int32'. "
        + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.\r\n"
        + "Parameter name: arguments")]
    public void ReplaceParameters_WrongArgumentType ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (typeof (string));
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      CallReplaceParameters (arg1, arg2);
    }

    [Test]
    public void ReplaceParameters_ConvertibleArgumentType ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (typeof (int)); // convert from int to object

      var invokedBody = CallReplaceParameters (arg1, arg2);

      var expectedBody = Expression.Block (arg1, Expression.Convert (arg2, typeof (object)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    public void GetArgumentTypes ()
    {
      var arguments =
          new Expression[] { Expression.Constant (""), Expression.Parameter (typeof (double)), Expression.Parameter (typeof (int).MakeByRefType()) };

      var result = BodyContextUtility.GetArgumentTypes (arguments.AsOneTime());

      Assert.That (result, Is.EqualTo (new[] { typeof (string), typeof (double), typeof (int).MakeByRefType() }));
    }

    private Expression CallReplaceParameters (params Expression[] arguments)
    {
      return BodyContextUtility.ReplaceParameters (_parameters.AsOneTime(), _previousBody, arguments.AsOneTime());
    }
  }
}