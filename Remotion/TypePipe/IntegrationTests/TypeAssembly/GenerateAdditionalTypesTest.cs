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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenerateAdditionalTypesTest : TypeAssemblerIntegrationTestBase
  {
    [Ignore ("TODO 5475")]
    [Test]
    public void ProxyImplementsGeneratedInterface ()
    {
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var ifc = typeContext.CreateType ("INewInterface", "MyNamespace", TypeAttributes.Interface, null);
            var ifcMethod = ifc.AddAbstractMethod ("InterfaceMethod", returnType: typeof (string));

            typeContext.ProxyType.AddInterface (ifc);
            typeContext.ProxyType.GetOrAddOverride (ifcMethod).SetBody (ctx => Expression.Constant ("new interface implemented"));
          });

      var newInterface = type.GetInterfaces().Single();
      Assert.That (newInterface.FullName, Is.EqualTo ("MyNamespace.INewInterface"));
      var interfaceMethod = newInterface.GetMethods().Single();
      Assert.That (interfaceMethod.Name, Is.EqualTo ("InterfaceMethod"));

      var instance = Activator.CreateInstance (type);
      var result = interfaceMethod.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("new interface implemented"));
    }

    [Ignore ("TODO 5475")]
    [Test]
    public void ProxyIsBaseTypeOfNewClass ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.Method());
      string newClassName = null;

      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var proxy = typeContext.ProxyType;
            proxy.GetOrAddOverride (method).SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" Proxy")));

            var proxyProxy = typeContext.CreateProxyType (proxy);
            proxyProxy.GetOrAddOverride (method).SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" ProxyProxy")));
            newClassName = proxyProxy.FullName;
          });

      var proxyProxyType = type.Assembly.GetType (newClassName, throwOnError: true);
      var instance = (DomainType) Activator.CreateInstance (proxyProxyType);

      Assert.That (instance.Method(), Is.EqualTo ("DomainType Proxy ProxyProxy"));
    }

    [Test]
    public void TypesRequiringForwardDeclarations ()
    {
      // public class Proxy : DomainType {
      //   public int Method1 (NewClass x, int i) {
      //     if (i <= 0)
      //       return i;
      //     else
      //       return x.Method2 (this, i);
      //   }
      // }
      // public class NewClass {
      //   public int Method2 (Proxy x, int i) {
      //     return x.Method1 (this, i - 1);
      //   }
      // }
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var proxyType = typeContext.ProxyType;
            var newClass = typeContext.CreateType ("NewClass", null, TypeAttributes.Public | TypeAttributes.Class, typeof (object));

            var method1 = proxyType.AddAbstractMethod (
                "Method1",
                MethodAttributes.Public,
                typeof (int),
                new[] { new ParameterDeclaration (newClass, "x"), new ParameterDeclaration (typeof (int), "i") });
            var method2 = newClass.AddMethod (
                "Method2",
                MethodAttributes.Public,
                typeof (int),
                new[] { new ParameterDeclaration (proxyType, "x"), new ParameterDeclaration (typeof (int), "i") },
                ctx => Expression.Call (ctx.Parameters[0], method1, ctx.This, Expression.Decrement (ctx.Parameters[1])));

            method1.SetBody (
                ctx => Expression.Condition (
                    Expression.LessThanOrEqual (ctx.Parameters[1], Expression.Constant (0)),
                    ctx.Parameters[1],
                    Expression.Call (ctx.Parameters[0], method2, ctx.This, ctx.Parameters[1])));
          });

      var method = type.GetMethod ("Method1");
      var newClassType = type.Assembly.GetType ("NewClass", throwOnError: true);
      var proxyInstance = Activator.CreateInstance (type);
      var newClassInstance = Activator.CreateInstance (newClassType);

      Assert.That (method.Invoke (proxyInstance, new[] { newClassInstance, 7 }), Is.EqualTo (0));
      Assert.That (method.Invoke (proxyInstance, new[] { newClassInstance, -8 }), Is.EqualTo (-8));
    }

    public class DomainType
    {
      public virtual string Method () { return "DomainType"; }
    }
  }
}