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
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ModifyGenericTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ClosedGenericType_AddMethod ()
    {
      var type = AssembleType<GenericDomainType<string>> (
          mutableType => mutableType.AddMethod (
              "AnotherMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              typeof (int),
              ParameterDeclaration.EmptyParameters,
              ctx => Expression.Constant (7)));

      var result = type.GetMethod ("AnotherMethod").Invoke (null, null);

      Assert.That (result, Is.EqualTo (7));
    }

    // TODO 4775: Modify closed generic type: add method, replace body of a method using the generic parameter.
    // TODO Implement (override) MutableType.GetGenericArguments and MutableType.GetGenericTypeDefinition

    [Test]
    [Ignore ("TODO 4775")]
    public void ClosedGenericType_ReplaceMethodBodyUsingGenericParameter ()
    {
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Original type must not be sealed, an interface, a value type, an enum, a delegate, contain generic parameters and "
      + "must have an accessible constructor.\r\nParameter name: originalType")]
    public void OpenGenericType_Throws ()
    {
      AssembleType(typeof (GenericDomainType<>));
    }

    public class GenericDomainType<T>
    {
    }
  }
}