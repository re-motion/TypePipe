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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenerateAdditionalTypesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ProxyImplementsGeneratedInterface ()
    {
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var ifc = typeContext.CreateInterface ("INewInterface", "MyNamespace");
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

    [Test]
    public void ProxyIsBaseTypeOfNewClass ()
    {
      // public class Proxy : DomainType {
      //   public override string Method () { return base.Method() + " Proxy"; }
      // }
      // public class ProxyProxy : Proxy {
      //   public override string Method () { return base.Method() + " ProxyProxy"; }
      // }
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.Method());
      string newClassName = null;

      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var proxy = typeContext.ProxyType;
            proxy.GetOrAddOverride (method).SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" Proxy")));

            var proxyProxy = typeContext.CreateProxy (proxy);
            proxyProxy.GetOrAddOverride (method).SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" ProxyProxy")));
            newClassName = proxyProxy.FullName;
          });

      var proxyProxyType = type.Assembly.GetType (newClassName, throwOnError: true);
      var proxyInstance = (DomainType) Activator.CreateInstance (type);
      var proxyProxyInstance = (DomainType) Activator.CreateInstance (proxyProxyType);

      Assert.That (proxyInstance.Method(), Is.EqualTo ("DomainType Proxy"));
      Assert.That (proxyProxyInstance.Method(), Is.EqualTo ("DomainType Proxy ProxyProxy"));
    }

    [Test]
    public void TypesRequiringDependencySorting ()
    {
      // public class interface IInterface1 : IComparable<IInterface2> {}
      // public class interface IInterface2 {}
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var addedInterface1 = typeContext.CreateInterface ("IInterface1", "ns");
            var addedInterface2 = typeContext.CreateInterface ("IInterface2", "ns");
            addedInterface1.AddInterface (typeof (IComparable<>).MakeTypePipeGenericType (addedInterface2));

            Assert.That (typeContext.AdditionalTypes, Is.EqualTo (new[] { addedInterface1, addedInterface2 }));
          });
      var assembly = type.Assembly;
      var interface1 = assembly.GetType ("ns.IInterface1");
      var interface2 = assembly.GetType ("ns.IInterface2");

      Assert.That (interface1.GetInterfaces().Single(), Is.SameAs (typeof (IComparable<>).MakeGenericType (interface2)));
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

    [Test]
    public void ValueType ()
    {
      var type = AssembleType<DomainType> (
          typeContext =>
          {
            var newValueType = typeContext.CreateType ("MyValueType", null, TypeAttributes.Sealed, typeof (ValueType));
            newValueType.AddField ("dummy", FieldAttributes.Private, typeof (int));
          });
      var assembly = type.Assembly;

      var valueType = assembly.GetType ("MyValueType", throwOnError: true);

      Assert.That (valueType.IsValueType, Is.True);
      Assert.That (valueType.BaseType, Is.SameAs (typeof (ValueType)));
      Assert.That (valueType.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), Is.Empty);
      Assert.That (Activator.CreateInstance (valueType), Is.Not.Null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "An error occurred during code generation for 'DomainType':\r\n"
        + "MutableTypes must not contain cycles in their dependencies, i.e., an algorithm that recursively follows the types "
        + "returned by Type.BaseType and Type.GetInterfaces must terminate.\r\n"
        + "At least one of the following types is causing the dependency cycle: 'IInterface1', 'IInterface2'.\r\n"
        + "The following participants are currently configured and may have caused the error: 'ParticipantStub'.")]
    public void CircularDependency_Throws ()
    {
      // public interface IInterface1 : IInterface2 { }
      // public interface IInterface2 : IInterface1 { }
      AssembleType<DomainType> (
          typeContext =>
          {
            var interface1 = typeContext.CreateInterface ("IInterface1", "ns");
            var interface2 = typeContext.CreateInterface ("IInterface2", "ns");

            interface1.AddInterface (interface2);
            interface2.AddInterface (interface1);
          });
    }

    public class DomainType
    {
      public virtual string Method () { return "DomainType"; }
    }
  }
}