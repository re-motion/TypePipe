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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class MethodBaseBodyContextBaseTest
  {
    private MutableType _declaringType;
    private bool _isStatic;

    private MethodBaseBodyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();
      _isStatic = BooleanObjectMother.GetRandomBoolean();

      _context = new TestableMethodBaseBodyContextBase (_declaringType, new ParameterExpression[0], _isStatic);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_context.IsStatic, Is.EqualTo (_isStatic));
      Assert.That (_context.Parameters, Is.Empty);
    }

    [Test]
    public void Initialization_Parameters ()
    {
      var parameter1 = Expression.Parameter (typeof (int), "i");
      var parameter2 = Expression.Parameter (typeof (string), "s");

      var context = new TestableMethodBaseBodyContextBase (_declaringType, new[] { parameter1, parameter2 }, _isStatic);

      Assert.That (context.Parameters, Is.EqualTo (new[] { parameter1, parameter2 }));
    }
  }
}