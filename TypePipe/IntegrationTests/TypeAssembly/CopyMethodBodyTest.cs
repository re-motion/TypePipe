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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class CopyMethodBodyTest : TypeAssemblerIntegrationTestBase
  {
    private MethodInfo _add;
    private MethodInfo _method;
    private MethodInfo _multiply;

    public override void SetUp ()
    {
      base.SetUp ();

      _add = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Add (7, 8));
      _method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));
      _multiply = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.Multiply (7, 8));
    }

    [Test]
    public void FromInstanceMethod ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var methodToCopy = proxyType.GetOrAddOverride (_add);
            var method = proxyType.GetOrAddOverride (_method);
            method.SetBody (ctx => ctx.CopyMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.Method (7), Is.EqualTo (14));
      Assert.That (instance.AddWasCalled, Is.True);
    }

    [Test]
    public void FromInstanceMetod_ModifyingOriginalBodyDoesNotAffectCopiedBody ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var methodToCopy = proxyType.GetOrAddOverride (_add);
            var method = proxyType.GetOrAddOverride (_method);
            method.SetBody (ctx => ctx.CopyMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0]));
            methodToCopy.SetBody (ctx => Expression.Add (ctx.Parameters[0], ctx.Parameters[1]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);

      instance.Add (1, 2);
      Assert.That (instance.AddWasCalled, Is.False);
      Assert.That (instance.Method (7), Is.EqualTo (14));
      Assert.That (instance.AddWasCalled, Is.True);
    }

    [Test]
    public void FromStaticMetod_ToStaticMethod ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var methodToCopy = proxyType.AddMethod (
                "from",
                MethodAttributes.Static,
                typeof (int),
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Call (_multiply, ctx.Parameters[0], ctx.Parameters[0]));

            proxyType.AddMethod (
                "to",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (int),
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => ctx.CopyMethodBody (methodToCopy, ctx.Parameters[0]));
          });

      var method = type.GetMethod ("to");

      var result = method.Invoke (null, new object[] { 7 });
      Assert.That (result, Is.EqualTo (49));
    }

    [Test]
    public void FromInstanceMethod_ToConstructor ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var methodToCopy = proxyType.GetOrAddOverride (_add);
            proxyType.AddConstructor (
                MethodAttributes.Public,
                new[] { new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Block (
                    ctx.CallThisConstructor(),
                    ctx.CopyMethodBody (methodToCopy, ctx.Parameters[0], ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type, new object[] { 7 });

      Assert.That (instance.AddWasCalled, Is.True);
    }

    [UsedImplicitly]
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