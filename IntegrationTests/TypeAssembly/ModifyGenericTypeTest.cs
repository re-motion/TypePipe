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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ModifyGenericTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ClosedGenericType_AddMethod ()
    {
      var type = AssembleType<DomainType<string>> (
          proxyType => proxyType.AddMethod (
              "AnotherMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              typeof (int),
              ParameterDeclaration.None,
              ctx => Expression.Constant (7)));

      var result = type.GetMethod ("AnotherMethod").Invoke (null, null);

      Assert.That (result, Is.EqualTo (7));
    }

    // TODO 4744: Implement MutableType.GetGenericArguments, GetGenericTypeDefinition, IsGenericType, IsGenericTypeDefinition, etc.; use them in an integration test.

    [Test]
    public void ClosedGenericType_OverrideMethodWithGenericParameter ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType<string> obj) => obj.Method (""));
      var type = AssembleType<DomainType<string>> (
          p => p.GetOrAddOverride (baseMethod).SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" test"))));

      var instance = Activator.CreateInstance (type);
      var method = type.GetMethod ("Method");
      var result = method.Invoke (instance, new object[] { "hello" });

      Assert.That (result, Is.EqualTo ("hello test"));
    }

    [Test]
    public void OpenGenericType_Throws ()
    {
      Assert.That (
          () => AssembleType (typeof (DomainType<>)),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Base type must not be sealed, an interface, an array, a byref type, a pointer, a generic parameter, "
                  + "contain generic parameters and must have an accessible constructor. Type: 'Remotion.TypePipe.IntegrationTests.TypeAssembly.ModifyGenericTypeTest+DomainType`1'"
                  + "\r\nParameter name: baseType"));
    }

    public class DomainType<T>
    {
      public virtual T Method (T t) { return t; }
    }
  }
}