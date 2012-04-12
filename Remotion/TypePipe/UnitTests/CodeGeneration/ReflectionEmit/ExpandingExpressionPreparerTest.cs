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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ExpandingExpressionPreparerTest
  {
    private ExpandingExpressionPreparer _preparer;

    [SetUp]
    public void SetUp ()
    {
      _preparer = new ExpandingExpressionPreparer();
    }

    [Test]
    public void PrepareConstructorBody_ExpandsOriginalBodyExpressions ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (object));
      var ctor = mutableType.ExistingConstructors.Single();
      Assert.That (ctor.Body, Is.TypeOf<OriginalBodyExpression>());

      var result = _preparer.PrepareConstructorBody (ctor);

      Assert.That (result, Is.AssignableTo<MethodCallExpression> ());
    }
    
    [Test]
    public void PrepareMethodBody_DoesNothingAndJustReturnsMethodBody ()
    {
      // TODO 4753
      // TODO 4772
      var body1 = Expression.Empty();
      var body2 = new OriginalBodyExpression (ReflectionObjectMother.GetSomeType(), Enumerable.Empty<Expression>());

      var method1 = MutableMethodInfoObjectMother.Create (body: body1);
      var method2 = MutableMethodInfoObjectMother.Create (body: body2);

      var result1 = _preparer.PrepareMethodBody (method1);
      var result2 = _preparer.PrepareMethodBody (method2);

      Assert.That (result1, Is.SameAs (body1));
      Assert.That (result2, Is.SameAs (body2));
    }
  }
}