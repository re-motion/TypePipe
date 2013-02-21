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
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AbstractTypeTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void NoChanges_BecomesConcrete ()
    {
      var type = AssembleType<AbstractTypeWithoutMethods> (
          proxyType =>
          {
            Assertion.IsNotNull (proxyType.BaseType);
            Assert.That (proxyType.BaseType.IsAbstract, Is.True);
            Assert.That (proxyType.IsAbstract, Is.False);
          });

      Assert.That (type.IsAbstract, Is.False);
      // The generated default constructor of abstract class has family visibility (protected in C#).
      Assert.That (() => Activator.CreateInstance (type, nonPublic: true), Throws.Nothing);
    }

    [Test]
    public void ImplementPartially_RemainsAbstract ()
    {
      var type = AssembleType<AbstractTypeWithTwoMethods> (
          proxyType =>
          {
            var abstractBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithTwoMethods obj) => obj.Method1());
            proxyType.GetOrAddOverride (abstractBaseMethod).SetBody (ctx => Expression.Empty());

            Assert.That (proxyType.IsAbstract, Is.True);
          });

      Assert.That (type.IsAbstract, Is.True);
    }

    [Test]
    public void AddAbstractMethod_BecomesAbstract ()
    {
      var type = AssembleType<ConcreteType> (
          proxyType =>
          {
            Assert.That (proxyType.IsAbstract, Is.False);

            var method = proxyType.AddAbstractMethod ("Dummy", MethodAttributes.Public, typeof (int), ParameterDeclaration.None);
            Assert.That (method.IsAbstract, Is.True);

            Assert.That (proxyType.IsAbstract, Is.True);
          });

      Assert.That (type.IsAbstract, Is.True);
    }

    [Test]
    public void ImplementFully_BecomesConcrete ()
    {
      var type = AssembleType<AbstractTypeWithOneMethod> (
          proxyType =>
          {
            Assert.That (proxyType.IsAbstract, Is.True);

            var abstractBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithOneMethod obj) => obj.Method());
            var method = proxyType.GetOrAddOverride (abstractBaseMethod);

            Assert.That (method.IsAbstract, Is.True);
            method.SetBody (ctx => Expression.Empty());
            Assert.That (method.IsAbstract, Is.False);

            Assert.That (proxyType.IsAbstract, Is.False);
          });

      Assert.That (type.IsAbstract, Is.False);
    }

    [Test]
    public void Override_LeaveAbstract_ResultsInValidCode ()
    {
      var type = AssembleType<AbstractTypeWithOneMethod> (
          proxyType =>
          {
            var abstractBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithOneMethod obj) => obj.Method());
            var method = proxyType.GetOrAddOverride (abstractBaseMethod);
            Assert.That (method.IsAbstract, Is.True);
          });

      Assert.That (type.IsAbstract, Is.True);
    }

    [Test]
    public void AccessingBodyOrCallingAbstractMethod_Throws ()
    {
      var message = "An abstract method has no body.";
      AssembleType<AbstractTypeWithOneMethod> (
          proxyType =>
          {
            var abstractBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithOneMethod obj) => obj.Method());
            var method = proxyType.GetOrAddOverride (abstractBaseMethod);

            Assert.That (() => method.Body, Throws.InvalidOperationException.With.Message.EqualTo (message));
            method.SetBody (
                ctx =>
                {
                  Assert.That (() => ctx.PreviousBody, Throws.InvalidOperationException.With.Message.EqualTo (message));
                  Assert.That (() => ctx.InvokePreviousBodyWithArguments(), Throws.InvalidOperationException.With.Message.EqualTo (message));
                  Assert.That (
                      () => ctx.CallBase (abstractBaseMethod),
                      Throws.ArgumentException.With.Message.EqualTo ("Cannot perform base call on abstract method.\r\nParameter name: baseMethod"));

                  return Expression.Empty();
                });
          });
    }

    [UsedImplicitly] public class ConcreteType { }

    public abstract class AbstractTypeWithoutMethods { }

    public abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method ();
    }

    public abstract class AbstractTypeWithTwoMethods
    {
      public abstract void Method1 ();
      public abstract void Method2 ();
    }
  }
}