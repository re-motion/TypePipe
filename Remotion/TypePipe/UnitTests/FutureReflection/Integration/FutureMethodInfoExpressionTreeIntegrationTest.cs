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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;

namespace Remotion.TypePipe.UnitTests.FutureReflection.Integration
{
  [TestFixture]
  public class FutureMethodInfoExpressionTreeIntegrationTest
  {
    [Test]
    public void Call_Static_NoParameters ()
    {
      var method = New.FutureMethodInfo (methodAttributes: MethodAttributes.Static);

      var expression = Expression.Call (method);

      Assert.That (expression.Method, Is.SameAs (method));
    }

    [Test]
    public void Call_Static_WithParameters ()
    {
      var arguments = new Arguments ("string", 7, new object());
      var method = New.FutureMethodInfo (methodAttributes: MethodAttributes.Static, parameters: arguments.Parameters);

      var expression = Expression.Call (method, arguments.Expressions);

      Assert.That (expression.Method, Is.SameAs (method));
    }

    [Test]
    public void Call_Instance_NoParameters ()
    {
      var type = New.FutureType();
      var instance = Expression.Parameter (type);
      var method = New.FutureMethodInfo (declaringType: type);

      var expression = Expression.Call (instance, method);

      Assert.That (expression.Method, Is.SameAs (method));
    }

    [Test]
    public void Call_Instance_WithParameters ()
    {
      var type = New.FutureType();
      var instance = Expression.Parameter (type);
      var arguments = new Arguments ("string", 7, new object());
      var method = New.FutureMethodInfo (declaringType: type, parameters: arguments.Parameters);

      var expression = Expression.Call (instance, method, arguments.Expressions);

      Assert.That (expression.Method, Is.SameAs (method));
    }
  }
}