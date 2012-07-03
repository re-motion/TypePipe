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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyModificationContextTest
  {
    private MutableType _declaringType;
    private ParameterExpression[] _parameters;
    private Expression _previousBody;
    private MethodInfo _baseMethod;
    private bool _isStatic;

    private MethodBodyModificationContext _context;
    private IMemberSelector _memberSelector;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create ();
      _parameters = new[] { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (object)) };
      _previousBody = Expression.Block (_parameters[0], _parameters[1]);
      _isStatic = BooleanObjectMother.GetRandomBoolean();
      _baseMethod = ReflectionObjectMother.GetSomeMethod();
      _memberSelector = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _context = new MethodBodyModificationContext (_declaringType, _parameters.AsOneTime(), _previousBody, _isStatic, _baseMethod, _memberSelector);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_context.Parameters, Is.EqualTo (_parameters));
      Assert.That (_context.PreviousBody, Is.SameAs (_previousBody));
      Assert.That (_context.IsStatic, Is.EqualTo (_isStatic));
      Assert.That (_context.BaseMethod, Is.SameAs(_baseMethod));
    }

    [Test]
    public void PreviousBody ()
    {
      Assert.That (_context.PreviousBody, Is.SameAs (_previousBody));
    }

    [Test]
    public void GetPreviousBodyWithArguments_Params ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      var invokedBody = _context.GetPreviousBodyWithArguments (arg1, arg2);

      var expectedBody = Expression.Block (arg1, arg2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    public void GetPreviousBodyWithArguments_Enumerable ()
    {
      var invokedBody = _context.GetPreviousBodyWithArguments (_parameters.Cast<Expression> ().AsOneTime ());

      var expectedBody = Expression.Block (_parameters[0], _parameters[1]);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }
  }
}