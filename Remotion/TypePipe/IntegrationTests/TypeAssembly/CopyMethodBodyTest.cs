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
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class CopyMethodBodyTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void FromInstanceMethod_WithOriginalBodyExpression ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var methodToCopy = mutableType.ExistingMutableMethods.Single (m => m.Name == "Add");
            Assert.That (methodToCopy.Body, Is.TypeOf<OriginalBodyExpression>());

            var method = mutableType.ExistingMutableMethods.Single (m => m.Name == "Method");
            method.SetBody (ctx => ctx.GetCopiedMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.Method (7), Is.EqualTo (14));
      Assert.That (instance.AddWasCalled, Is.True);
    }

    [Test]
    public void FromInstanceMetod_WithoutOriginalBodyExpression ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var methodToCopy = mutableType.ExistingMutableMethods.Single (m => m.Name == "Add");
            methodToCopy.SetBody (ctx => Expression.Add (ctx.Parameters[0], ctx.Parameters[1]));
            // TODO: better assert -> make sure that OriginalBodyExpression is not contained in, instead of simple typeof check
            Assert.That (methodToCopy.Body, Is.Not.TypeOf<OriginalBodyExpression>());

            var method = mutableType.ExistingMutableMethods.Single (m => m.Name == "Method");
            method.SetBody (ctx => ctx.GetCopiedMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.Method (7), Is.EqualTo (14));
      Assert.That (instance.AddWasCalled, Is.False);
    }

    [Test]
    public void FromInstanceMetod_ModifyingOriginalBodyDoesNotAffectCopiedBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var methodToCopy = mutableType.ExistingMutableMethods.Single (m => m.Name == "Add");
            var method = mutableType.ExistingMutableMethods.Single (m => m.Name == "Method");
            method.SetBody (ctx => ctx.GetCopiedMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0]));
            methodToCopy.SetBody (ctx => Expression.Add (ctx.Parameters[0], ctx.Parameters[1]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);

      instance.Add (1, 2);
      Assert.That (instance.AddWasCalled, Is.False);
      Assert.That (instance.Method (7), Is.EqualTo (14));
      Assert.That (instance.AddWasCalled, Is.True);
    }

    [Test]
    public void FromStaticMetod_ToNewStaticMethod ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var methodToCopy = mutableType.ExistingMutableMethods.Single (m => m.Name == "Multiply");
            Assert.That (methodToCopy.IsStatic, Is.True);

            mutableType.AddMethod (
                "StaticMethod",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (int),
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => ctx.GetCopiedMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0]));
          });

      var method = type.GetMethod ("StaticMethod");

      var result = method.Invoke (null, new object[] { 7 });
      Assert.That (result, Is.EqualTo (49));
    }

    [Test]
    public void FromInstanceMethod_ToConstructor ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var methodToCopy = mutableType.ExistingMutableMethods.Single (m => m.Name == "Add");
            mutableType.AddConstructor (
                MethodAttributes.Public,
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Block (
                    ctx.GetConstructorCall(),
                    ctx.GetCopiedMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type, new object[] { 7 });

      Assert.That (instance.AddWasCalled, Is.True);
    }

    public class DomainType
    {
      public bool AddWasCalled { get; private set; }

      public virtual int Add (int a, int b)
      {
        AddWasCalled = true;
        return a + b;
      }

      public virtual int Method (int i)
      {
        Dev.Null = i;
        return -1;
      }

      public static int Multiply (int a, int b)
      {
        return a * b;
      }
    }
  }
}