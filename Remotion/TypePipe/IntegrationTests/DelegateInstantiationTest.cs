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
using Remotion.Development.UnitTesting.Reflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class DelegateInstantiationTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void CreateFunc_FromStaticMethod ()
    {
      var targetMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DerivedType.StaticMethod());

      CheckStaticDelegateInstantiation (typeof (Func<string>), targetMethod, "static method");
    }

    [Test]
    public void CreateFunc_FromInstanceMethod ()
    {
      var targetMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedType obj) => obj.Method());

      CheckDelegateInstantiation (typeof (Func<string>), targetMethod, "method");
    }

    [Test]
    public void CreateAction ()
    {
      var targetMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedType obj) => obj.VoidMethod());

      CheckDelegateInstantiation (typeof (Action), targetMethod, expectedFieldValue: "void method");
    }

    [Ignore ("TODO 5080")]
    [Test]
    public void CreateVirtualFunc ()
    {
      var targetMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedType obj) => obj.VirtualMethod());

      CheckDelegateInstantiation (typeof (Func<string>), targetMethod, "derived");
    }

    private void CheckDelegateInstantiation (Type delegateType, MethodInfo targetMethod, string expectedReturnValue = null, string expectedFieldValue = null)
    {
      var type = AssembleType<DerivedType> (
          mutableType =>
          {
            var createDelegateMethod = mutableType.AllMutableMethods.Single (m => m.Name == "CreateDelegate");
            createDelegateMethod.SetBody (ctx => Expression.NewDelegate (delegateType, ctx.This, targetMethod));
          });

      var instance = (DerivedType) Activator.CreateInstance (type);
      var delegate_ = instance.CreateDelegate();

      Assert.That (delegate_.Method, Is.EqualTo (targetMethod));
      Assert.That (delegate_.Target, Is.EqualTo (instance));
      Assert.That (delegate_.DynamicInvoke(), Is.EqualTo (expectedReturnValue));
      Assert.That (instance.Field, Is.EqualTo (expectedFieldValue));
    }

    private void CheckStaticDelegateInstantiation (Type delegateType, MethodInfo targetMethod, string expectedReturnValue)
    {
      var type = AssembleType<DerivedType> (
          mutableType =>
          {
            var createDelegateMethod = mutableType.AllMutableMethods.Single (m => m.Name == "CreateDelegate");
            createDelegateMethod.SetBody (ctx => Expression.NewDelegate (delegateType, null, targetMethod));
          });

      var instance = (DerivedType) Activator.CreateInstance (type);
      var delegate_ = instance.CreateDelegate ();

      Assert.That (delegate_.Method, Is.EqualTo (targetMethod));
      Assert.That (delegate_.Target, Is.Null);
      Assert.That (delegate_.DynamicInvoke (), Is.EqualTo (expectedReturnValue));
    }

    // TODO 5080: think about creating a delegate (an maybe invoking) which takes parameters

    public class BaseType
    {
      public virtual string VirtualMethod () { return "base"; }
    }

    public class DerivedType : BaseType
    {
      public string Field;

      public static string StaticMethod () { return "static method"; }
      public string Method () { return "method"; }
      public void VoidMethod () { Field = "void method"; }
      public override string VirtualMethod () { return "derived"; }

      public virtual Delegate CreateDelegate () { throw new NotImplementedException(); }
    }
  }
}