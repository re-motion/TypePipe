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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyContextBaseTest
  {
    private MutableType _declaringType;
    private bool _isStatic;
    private ParameterExpression[] _parameters;
    private Type[] _genericParameters;
    private Type _returnType;
    private MethodInfo _baseMethod;

    private MethodBodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();
      _isStatic = BooleanObjectMother.GetRandomBoolean();
      _parameters = new[] { Expression.Parameter (typeof (string)) };
      _genericParameters = new[] { ReflectionObjectMother.GetSomeGenericParameter() };
      _returnType = ReflectionObjectMother.GetSomeType();
      _baseMethod = ReflectionObjectMother.GetSomeMethod();

      _context = new TestableMethodBodyContextBase (
          _declaringType, _isStatic, _parameters.AsOneTime(), _genericParameters.AsOneTime(), _returnType, _baseMethod);
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
    }

    [Test]
    public void HasBaseMethod ()
    {
      Assert.That (_context.HasBaseMethod, Is.True);

      var context = MethodBodyContextBaseObjectMother.Create (baseMethod: null);
      Assert.That (context.HasBaseMethod, Is.False);
    }

    [Test]
    public void BaseMethod ()
    {
      Assert.That (_context.BaseMethod, Is.SameAs (_baseMethod));

      var context = MethodBodyContextBaseObjectMother.Create (baseMethod: null);
      Assert.That (
          () => context.BaseMethod, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("This method does not override another method."));
    }

    [Test]
    public void DelegateTo_Instance_WithParameters ()
    {
      var context = MethodBodyContextBaseObjectMother.Create (parameterExpressions: _parameters);
      var instance = Expression.Default (typeof (object));
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object o) => o.Equals (null));

      var result = context.DelegateTo (instance, method);

      Assert.That (result.Object, Is.SameAs (instance));
      Assert.That (result.Method, Is.SameAs (method));
      Assert.That (result.Arguments, Is.EqualTo (_parameters));
    }

    [Test]
    public void DelegateTo_Static_WithGenericParameters ()
    {
      var context = MethodBodyContextBaseObjectMother.Create (genericParameters: _genericParameters);
      var method = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Enumerable.Empty<Dev.T>());

      var result = context.DelegateTo (null, method);

      Assert.That (result.Object, Is.Null);
      Assert.That (result.Method, Is.SameAs (method.MakeTypePipeGenericMethod (_genericParameters)));
    }
  }
}