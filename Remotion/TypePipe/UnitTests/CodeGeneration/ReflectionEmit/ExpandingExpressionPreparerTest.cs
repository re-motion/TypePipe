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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
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
      var ctor = mutableType.ExistingMutableConstructors.Single();
      Assert.That (ctor.Body, Is.TypeOf<OriginalBodyExpression>());

      var result = _preparer.PrepareConstructorBody (ctor);

      Assert.That (result, Is.AssignableTo<MethodCallExpression> ());
      var methodCallExpression = ((MethodCallExpression) result);
      Assert.That (methodCallExpression.Method, Is.TypeOf<ConstructorAsMethodInfoAdapter>());
      Assert.That (((ConstructorAsMethodInfoAdapter) methodCallExpression.Method).ConstructorInfo, Is.SameAs (ctor.UnderlyingSystemConstructorInfo));
    }

    [Test]
    public void PrepareMethodBody_ExpandsOriginalBodyExpressions ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (object));
      var method = mutableType.ExistingMutableMethods.First ();
      Assert.That (method.Body, Is.TypeOf<OriginalBodyExpression> ());

      var result = _preparer.PrepareMethodBody (method);

      Assert.That (result, Is.AssignableTo<MethodCallExpression> ());
      var methodCallExpression = ((MethodCallExpression) result);
      Assert.That (methodCallExpression.Method, Is.TypeOf<BaseCallMethodInfoAdapter> ());
      Assert.That (((BaseCallMethodInfoAdapter) methodCallExpression.Method).AdaptedMethodInfo, Is.SameAs (method.UnderlyingSystemMethodInfo));
    }
  }
}