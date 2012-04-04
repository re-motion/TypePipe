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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ConstructorBodyModificationContextTest
  {
    private List<ParameterExpression> _parameters;
    private ConstructorBodyModificationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _parameters = new List<ParameterExpression>
                    { Expression.Parameter (ReflectionObjectMother.GetSomeType()), Expression.Parameter (ReflectionObjectMother.GetSomeType()) };
      var previousBody = Expression.Block (_parameters[0], _parameters[1]);
      _context = new ConstructorBodyModificationContext (MutableTypeObjectMother.Create(), _parameters, previousBody);
    }

    [Test]
    public void GetPreviousBody ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      var invokedBody = _context.GetPreviousBody (arg1, arg2);

      var expectedBody = Expression.Block (arg1, arg2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The argument count (0) does not match the parameter count (2).\r\nParameter name: arguments")]
    public void GetPreviousBody_WrongNumberOfArguments ()
    {
      _context.GetPreviousBody();
    }
  }
}