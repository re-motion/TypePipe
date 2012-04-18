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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ModifyGenericTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ClosedGenericType_AddMethod ()
    {
      var type = AssembleType<DomainType<string>> (
          mutableType => mutableType.AddMethod (
              "AnotherMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              typeof (int),
              ParameterDeclaration.EmptyParameters,
              ctx => Expression.Constant (7)));

      var result = type.GetMethod ("AnotherMethod").Invoke (null, null);

      Assert.That (result, Is.EqualTo (7));
    }

    // TODO 4744: Implement MutableType.GetGenericArguments, GetGenericTypeDefinition, IsGenericType, IsGenericTypeDefinition, etc.; use them in an integration test.

    [Test]
    public void ClosedGenericType_ReplaceBodyOfMethodWithGenericParameter ()
    {
      var type = AssembleType<DomainType<string>> (
          mutableType =>
          {
            var mutableMethod = mutableType.ExistingMethods.Single (m => m.Name == "Method");
            mutableMethod.SetBody (ctx => ctx.Parameters[0]);
          });

      var instance = Activator.CreateInstance (type);
      var method = type.GetMethod ("Method");
      var result = method.Invoke (instance, new object[] { "hello" });

      Assert.That (result, Is.EqualTo ("hello"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Original type must not be sealed, an interface, a value type, an enum, a delegate, an array, a byref type, a pointer, a generic parameter, "
        + "contain generic parameters and must have an accessible constructor.\r\nParameter name: originalType")]
    public void OpenGenericType_Throws ()
    {
      AssembleType (typeof (DomainType<>));
    }

    public class DomainType<T>
    {
      public virtual T Method (T t)
      {
        Dev.Null = t;
        throw new NotImplementedException();
      }
    }
  }
}