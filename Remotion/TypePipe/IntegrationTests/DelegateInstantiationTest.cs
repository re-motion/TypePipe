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
using Remotion.Development.UnitTesting.Reflection;

namespace TypePipe.IntegrationTests
{
  [Ignore("TODO 5078")]
  [TestFixture]
  public class DelegateInstantiationTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void CreateNonVirtualFunc_Instance ()
    {
      var targetMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedType obj) => obj.Method());

      var type = AssembleType<DerivedType> (
          mutableType =>
          {
            var createDelegateMethod = mutableType.AllMutableMethods.Single (m => m.Name == "CreateDelegate");
            createDelegateMethod.SetBody (ctx => Expression.NewDelegate (typeof (Func<string>), ctx.This, targetMethod));
          });

      var instance = (DerivedType) Activator.CreateInstance (type);
      var delegate_ = instance.CreateDelegate();

      Assert.That (delegate_.Method, Is.EqualTo (targetMethod));
      Assert.That (delegate_.Target, Is.EqualTo (instance));
      Assert.That (delegate_.DynamicInvoke(), Is.EqualTo ("method"));
    }

    // TODO: static method
    // TODO: think about creating a delegate (an maybe invoking) which takes parameters

    class BaseType
    {
      public virtual string VirtualMethod ()
      {
        return "base";
      }
    }

    class DerivedType : BaseType
    {
      public string Field;

      public static string StaticMethod ()
      {
        return "static method";
      }

      public string Method ()
      {
        return "method";
      }

      public void VoidMethod ()
      {
        Field = "void method";
      }

      public override string VirtualMethod ()
      {
        return "derived";
      }

      public virtual Delegate CreateDelegate ()
      {
        throw new NotImplementedException();
      }
    }
  }
}