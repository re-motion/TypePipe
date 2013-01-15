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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyModificationContextTest
  {
    private ProxyType _declaringType;
    private ParameterExpression[] _parameters;
    private Expression _previousBody;
    private MethodInfo _baseMethod;
    private bool _isStatic;
    private IMemberSelector _memberSelector;

    private MethodBodyModificationContext _context;
    private MethodBodyModificationContext _contextWithoutPreviousBody;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ProxyTypeObjectMother.Create ();
      _parameters = new[] { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (object)) };
      _previousBody = Expression.Block (_parameters[0], _parameters[1]);
      _isStatic = BooleanObjectMother.GetRandomBoolean();
      _baseMethod = ReflectionObjectMother.GetSomeMethod();
      _memberSelector = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _context = new MethodBodyModificationContext (_declaringType, _parameters.AsOneTime(), _previousBody, _isStatic, _baseMethod, _memberSelector);
      _contextWithoutPreviousBody = new MethodBodyModificationContext (_declaringType, _parameters, null, _isStatic, _baseMethod, _memberSelector);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_context.Parameters, Is.EqualTo (_parameters));
      Assert.That (_context.HasPreviousBody, Is.True);
      Assert.That (_context.PreviousBody, Is.SameAs (_previousBody));
      Assert.That (_context.IsStatic, Is.EqualTo (_isStatic));
      Assert.That (_context.BaseMethod, Is.SameAs(_baseMethod));
    }

    [Test]
    public void HasPreviousBody_False ()
    {
      Assert.That (_contextWithoutPreviousBody.HasPreviousBody, Is.False);
    }
      
    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "An abstract method has no body.")]
    public void PreviousBody_ThrowsForNoPreviousBody ()
    {
      Dev.Null = _contextWithoutPreviousBody.PreviousBody;
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

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "An abstract method has no body.")]
    public void GetPreviousBodyWithArguments_Params_ThrowsForNoPreviousBody ()
    {
      _contextWithoutPreviousBody.GetPreviousBodyWithArguments();
    }
  }
}