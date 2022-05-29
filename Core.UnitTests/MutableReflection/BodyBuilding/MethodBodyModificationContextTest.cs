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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyModificationContextTest
  {
    private MutableType _declaringType;
    private bool _isStatic;
    private ParameterExpression[] _parameters;
    private Type[] _genericParameters;
    private Type _returnType;
    private MethodInfo _baseMethod;
    private Expression _previousBody;

    private MethodBodyModificationContext _context;
    private MethodBodyModificationContext _contextWithoutPreviousBody;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create ();
      _isStatic = BooleanObjectMother.GetRandomBoolean();
      _parameters = new[] { Expression.Parameter (typeof (int)), Expression.Parameter (typeof (object)) };
      _baseMethod = ReflectionObjectMother.GetSomeMethod();
      _genericParameters = new[] { ReflectionObjectMother.GetSomeGenericParameter() };
      _returnType = ReflectionObjectMother.GetSomeType();
      _previousBody = Expression.Block (_parameters[0], _parameters[1]);

      _context = new MethodBodyModificationContext (
          _declaringType, _isStatic, _parameters.AsOneTime(), _genericParameters.AsOneTime(), _returnType, _baseMethod, _previousBody);
      _contextWithoutPreviousBody = new MethodBodyModificationContext (
          _declaringType, _isStatic, _parameters, _genericParameters, _returnType, _baseMethod, null);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_context.IsStatic, Is.EqualTo (_isStatic));
      Assert.That (_context.Parameters, Is.EqualTo (_parameters));
      Assert.That (_context.GenericParameters, Is.EqualTo (_genericParameters));
      Assert.That (_context.ReturnType, Is.SameAs (_returnType));
      Assert.That (_context.BaseMethod, Is.SameAs (_baseMethod));
      Assert.That (_context.PreviousBody, Is.SameAs (_previousBody));
    }

    [Test]
    public void HasPreviousBody_False ()
    {
      Assert.That (_contextWithoutPreviousBody.HasPreviousBody, Is.False);
    }
      
    [Test]
    public void PreviousBody_ThrowsForNoPreviousBody ()
    {
      Assert.That (
          () => Dev.Null = _contextWithoutPreviousBody.PreviousBody,
          Throws.InvalidOperationException
              .With.Message.EqualTo ("An abstract method has no body."));
    }

    [Test]
    public void InvokePreviousBodyWithArguments_Params ()
    {
      var arg1 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[0].Type);
      var arg2 = ExpressionTreeObjectMother.GetSomeExpression (_parameters[1].Type);

      var invokedBody = _context.InvokePreviousBodyWithArguments (arg1, arg2);

      var expectedBody = Expression.Block (arg1, arg2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    public void InvokePreviousBodyWithArguments_Enumerable ()
    {
      var invokedBody = _context.InvokePreviousBodyWithArguments (_parameters.Cast<Expression> ().AsOneTime ());

      var expectedBody = Expression.Block (_parameters[0], _parameters[1]);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, invokedBody);
    }

    [Test]
    public void InvokePreviousBodyWithArguments_Params_ThrowsForNoPreviousBody ()
    {
      Assert.That (
          () => _contextWithoutPreviousBody.InvokePreviousBodyWithArguments(),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("An abstract method has no body."));
    }
  }
}