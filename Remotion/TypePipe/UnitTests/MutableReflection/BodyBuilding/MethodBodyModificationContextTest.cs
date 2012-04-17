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
using System.Collections.Generic;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyModificationContextTest
  {
    private MutableType _declaringType;
    private List<ParameterExpression> _parameters;
    private Expression _previousBody;
    private bool _isStatic;

    private MethodBodyModificationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create ();
      _parameters = new List<ParameterExpression> { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (object)) };
      _previousBody = Expression.Block (_parameters[0], _parameters[1]);
      _isStatic = BooleanObjectMother.GetRandomBoolean();

      _context = new MethodBodyModificationContext (_declaringType, _parameters, _previousBody, _isStatic);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_context.Parameters, Is.EqualTo (_parameters));
      Assert.That (_context.GetPreviousBody (), Is.SameAs (_previousBody));
      Assert.That (_context.IsStatic, Is.EqualTo (_isStatic));
    }

    [Test]
    public void GetPreviousBody_NoParameter ()
    {
      var invokedBody = _context.GetPreviousBody ();

      Assert.That (invokedBody, Is.SameAs (_previousBody));
    }

    [Test]
    public void GetPreviousBody_Params ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      var invokedBody = _context.GetPreviousBody (arg1, arg2);

      var expectedBody = Expression.Block (arg1, arg2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The argument count (1) does not match the parameter count (2).\r\nParameter name: arguments")]
    public void GetPreviousBody_Params_WrongNumberOfArguments ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);

      _context.GetPreviousBody (arg1);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The argument at index 0 has an invalid type: Type 'System.String' cannot be implicitly converted to type 'System.Int32'. "
        + "Use Expression.Convert or Expression.ConvertChecked to make the conversion explicit.\r\n"
        + "Parameter name: arguments")]
    public void GetPreviousBody_Params_WrongArgumentType ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (typeof (string));
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      _context.GetPreviousBody (arg1, arg2);
    }

    [Test]
    public void GetPreviousBody_Params_ConvertibleArgumentType ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (typeof (int)); // convert from int to object

      var invokedBody = _context.GetPreviousBody (arg1, arg2);

      var expectedBody = Expression.Block (arg1, Expression.Convert (arg2, typeof (object)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }
  }
}