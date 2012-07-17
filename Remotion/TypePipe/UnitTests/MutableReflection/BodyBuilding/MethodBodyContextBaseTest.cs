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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyContextBaseTest
  {
    private MutableType _declaringType;
    private ParameterExpression[] _parameters;
    private bool _isStatic;
    private MethodInfo _baseMethod;
    private IMemberSelector _memberSelectorMock;

    private MethodBodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();
      _parameters = new[] { Expression.Parameter (typeof (string)) };
      _isStatic = BooleanObjectMother.GetRandomBoolean();
      _baseMethod = ReflectionObjectMother.GetSomeMethod();
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector> ();

      _context = new TestableMethodBodyContextBase (_declaringType, _parameters.AsOneTime (), _isStatic, _baseMethod, _memberSelectorMock);
    }

    [Test]
    public void HasBaseMethod ()
    {
      Assert.That (_context.HasBaseMethod, Is.True);

      var context = new TestableMethodBodyContextBase (_declaringType, _parameters.AsOneTime (), _isStatic, null, _memberSelectorMock);
      Assert.That (context.HasBaseMethod, Is.False);
    }

    [Test]
    public void BaseMethod ()
    {
      Assert.That (_context.BaseMethod, Is.SameAs(_baseMethod));

      var context = new TestableMethodBodyContextBase (_declaringType, _parameters.AsOneTime (), _isStatic, null, _memberSelectorMock);
      Assert.That (
          () => context.BaseMethod, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("This method does not override another method."));
    }
  }
}