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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyContextBaseTest
  {
    private ProxyType _declaringType;
    private bool _isStatic;
    private ParameterExpression[] _parameters;
    private Type _returnType;
    private MethodInfo _baseMethod;
    private IMemberSelector _memberSelectorMock;

    private MethodBodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ProxyTypeObjectMother.Create();
      _isStatic = BooleanObjectMother.GetRandomBoolean();
      _parameters = new[] { Expression.Parameter (typeof (string)) };
      _returnType = ReflectionObjectMother.GetSomeType();
      _baseMethod = ReflectionObjectMother.GetSomeMethod();
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _context = new TestableMethodBodyContextBase (_declaringType, _isStatic, _parameters.AsOneTime(), _returnType, _baseMethod, _memberSelectorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_context.IsStatic, Is.EqualTo (_isStatic));
      Assert.That (_context.Parameters, Is.EqualTo (_parameters));
      Assert.That (_context.ReturnType, Is.SameAs (_returnType));
      Assert.That (_context.BaseMethod, Is.SameAs (_baseMethod));
    }

    [Test]
    public void HasBaseMethod ()
    {
      Assert.That (_context.HasBaseMethod, Is.True);

      var context = new TestableMethodBodyContextBase (_declaringType, _isStatic, _parameters.AsOneTime(), _returnType, null, _memberSelectorMock);
      Assert.That (context.HasBaseMethod, Is.False);
    }

    [Test]
    public void BaseMethod ()
    {
      Assert.That (_context.BaseMethod, Is.SameAs(_baseMethod));

      var context = new TestableMethodBodyContextBase (_declaringType, _isStatic, _parameters.AsOneTime(), _returnType, null, _memberSelectorMock);
      Assert.That (
          () => context.BaseMethod, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("This method does not override another method."));
    }
  }
}