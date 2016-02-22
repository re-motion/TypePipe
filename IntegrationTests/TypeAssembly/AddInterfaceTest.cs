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
  public class AddInterfaceTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void MarkerInterface ()
    {
      Assert.That (typeof (DomainType).GetInterfaces(), Is.EqualTo (new[] { typeof (IOriginalInterface) }));

      var type = AssembleType<DomainType> (proxyType => proxyType.AddInterface (typeof (IMarkerInterface)));

      Assert.That (type.GetInterfaces(), Is.EquivalentTo (new[] { typeof (IOriginalInterface), typeof (IMarkerInterface) }));
    }

    [Test]
    public void AddMethodAsExplicitInterfaceImplementation ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IInterfaceWithMethod o) => o.Method());
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            proxyType.AddInterface (typeof (IInterfaceWithMethod));
            var mutableMethod = proxyType.AddMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  return Expression.Constant ("explicitly implemented");
                });
            mutableMethod.AddExplicitBaseDefinition (interfaceMethod);

            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { interfaceMethod }));
            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.GetBaseDefinition(), Is.EqualTo (mutableMethod));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      Assert.That (instance, Is.AssignableTo<IInterfaceWithMethod>());

      var method = GetDeclaredMethod (type, "DifferentName");

      // Reflection doesn't handle explicit overrides in GetBaseDefinition.
      // If this changes, MutableMethodInfo.GetBaseDefinition() must be changed as well.
      Assert.That (method.GetBaseDefinition (), Is.EqualTo (method));

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("explicitly implemented"));
      Assert.That (((IInterfaceWithMethod) instance).Method (), Is.EqualTo ("explicitly implemented"));
    }

    [Test]
    public void ReImplement ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IInterfaceWithMethod o) => o.Method());
      var type = AssembleType<DomainTypeWithMethod> (
          proxyType =>
          {
            proxyType.AddInterface (typeof (IInterfaceWithMethod));
            var mutableMethod = AddEquivalentMethod (
                proxyType,
                interfaceMethod,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  return Expression.Constant ("new implementation");
                });

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.GetBaseDefinition(), Is.EqualTo (mutableMethod));
          });

      var instance = (DomainTypeWithMethod) Activator.CreateInstance (type);

      Assert.That (instance.Method(), Is.EqualTo ("original implementation"));
      Assert.That (((IInterfaceWithMethod) instance).Method(), Is.EqualTo ("new implementation"));
    }

    public class DomainType : IOriginalInterface { }
    public class DomainTypeWithMethod : IInterfaceWithMethod
    {
      public string Method () { return "original implementation"; }
    }

    public interface IOriginalInterface { }
    public interface IMarkerInterface { }
    public interface IInterfaceWithMethod
    {
      string Method ();
    }
  }
}