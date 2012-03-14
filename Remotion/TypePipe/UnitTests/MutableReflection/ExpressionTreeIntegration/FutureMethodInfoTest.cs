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

namespace Remotion.TypePipe.UnitTests.MutableReflection.ExpressionTreeIntegration
{
  [TestFixture]
  public class FutureMethodInfoTest
  {
    [Test]
    public void Call_Static_NoArguments ()
    {
      var method = FutureMethodInfoObjectMother.Create (methodAttributes: MethodAttributes.Static);

      var expression = Expression.Call (method);

      Assert.That (expression.Method, Is.SameAs (method));
    }

    [Test]
    public void Call_Static_WithArguments ()
    {
      var arguments = new ArgumentTestHelper ("string", 7, new object());
      var method = FutureMethodInfoObjectMother.Create (methodAttributes: MethodAttributes.Static, parameterDeclarations: arguments.ParameterDeclarations);

      var expression = Expression.Call (method, arguments.Expressions);

      Assert.That (expression.Method, Is.SameAs (method));
    }

    [Test]
    public void Call_Instance_NoArguments ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var instance = Expression.Variable (declaringType);
      var method = FutureMethodInfoObjectMother.Create (declaringType: declaringType);

      var expression = Expression.Call (instance, method);

      Assert.That (expression.Method, Is.SameAs (method));
    }

    [Test]
    public void Call_Instance_WithArguments ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var instance = Expression.Variable (declaringType);
      var arguments = new ArgumentTestHelper ("string", 7, new object());
      var method = FutureMethodInfoObjectMother.Create (declaringType: declaringType, parameterDeclarations: arguments.ParameterDeclarations);

      var expression = Expression.Call (instance, method, arguments.Expressions);

      Assert.That (expression.Method, Is.SameAs (method));
    }
  }
}