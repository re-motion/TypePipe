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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBodyContextBaseTest
  {
    private ReadOnlyCollection<ParameterExpression> _emptyParameters;
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _emptyParameters = new List<ParameterExpression> ().AsReadOnly ();
      _mutableType = MutableTypeObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      var parameter1 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameter2 = Expression.Parameter (ReflectionObjectMother.GetSomeType ());
      var parameters = new List<ParameterExpression> { parameter1, parameter2 }.AsReadOnly ();

      var isStatic = BooleanObjectMother.GetRandomBoolean();
      var context = new TestableMethodBodyContextBase (_mutableType, parameters.AsOneTime(), isStatic);

      Assert.That (context.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (context.Parameters, Is.EqualTo (new[] { parameter1, parameter2 }));
      Assert.That (context.IsStatic, Is.EqualTo(isStatic));
    }

    [Test]
    public void This ()
    {
      var context = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, false);

      Assert.That (context.This, Is.TypeOf<ThisExpression>());
      Assert.That (context.This.Type, Is.SameAs (_mutableType));
    }

    [Test]
    public void This_ThrowsForStaticMethods ()
    {
      var context = new TestableMethodBodyContextBase (_mutableType, _emptyParameters, true);

      Assert.That (() => context.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
    }
  }
}