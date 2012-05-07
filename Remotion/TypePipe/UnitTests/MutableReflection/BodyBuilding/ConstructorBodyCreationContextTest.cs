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
using System.Linq;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.BodyBuilding
{
  [TestFixture]
  public class ConstructorBodyCreationContextTest
  {
    private IMemberSelector _memberSelectorMock;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
    }

    [Test]
    public void Initialization ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var context = new ConstructorBodyCreationContext (mutableType, Enumerable.Empty<ParameterExpression> (), _memberSelectorMock);

      Assert.That (context.IsStatic, Is.False);
    }

    [Test]
    public void GetConstructorCall ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (ClassWithConstructor));
      var context = new ConstructorBodyCreationContext (mutableType, Enumerable.Empty<ParameterExpression> (), _memberSelectorMock);

      var argumentExpressions = new ArgumentTestHelper ("string").Expressions;
      var result = context.GetConstructorCall (argumentExpressions);

      Assert.That (result, Is.AssignableTo<MethodCallExpression> ());
      var methodCallExpression = (MethodCallExpression) result;

      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression> ());
      Assert.That (methodCallExpression.Object.Type, Is.SameAs (mutableType));

      Assert.That (methodCallExpression.Arguments, Is.EqualTo (argumentExpressions));
    }

    private class ClassWithConstructor
    {
      public ClassWithConstructor (object o)
      {
        Dev.Null = o;
      }
    }
  }
}