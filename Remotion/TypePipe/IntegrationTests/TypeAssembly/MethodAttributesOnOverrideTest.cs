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

using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  public class MethodAttributesOnOverrideTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Override ()
    {
      var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.Method1());

      var type = AssembleType<DomainType> (
          proxy =>
          {
            var override1 = proxy.GetOrAddOverride (overriddenMethod);
            CheckMethodAttributes (override1);
          });

      var method = GetDeclaredMethod (type, "Method1");
      CheckMethodAttributes (method);
    }

    [Test]
    public void Implementation ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface o) => o.AddedInterfaceMethod());

      var type = AssembleType<DomainType> (
          proxy =>
          {
            proxy.AddInterface (typeof (IAddedInterface));
            var implementation = proxy.GetOrAddImplementation (interfaceMethod);
            CheckMethodAttributes (implementation);
          });

      var method = GetDeclaredMethod (type, "AddedInterfaceMethod");
      CheckMethodAttributes (method);
    }

    [Test]
    public void ReImplementation ()
    {
      var interfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.InterfaceMethod1());
      var interfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IMyInterface o) => o.InterfaceMethod2());

      var type = AssembleType<DomainType> (
          proxy =>
          {
            var implementation1 = proxy.GetOrAddImplementation (interfaceMethod1);
            var implementation2 = proxy.GetOrAddImplementation (interfaceMethod2);

            // Interface method is implemented by a virtual method which will be overidden (and is not marked [SpecialName]).
            Assert.That (implementation1.Attributes.IsSet (MethodAttributes.SpecialName), Is.False);
            CheckMethodAttributes (implementation2);
          });

      var method1 = GetDeclaredMethod (type, "InterfaceMethod1");
      var method2 = GetDeclaredMethod (type, "InterfaceMethod2");

      // Interface method is implemented by a virtual method which will be overidden (and is not marked [SpecialName]).
      Assert.That (method1.Attributes.IsSet (MethodAttributes.SpecialName), Is.False);
      CheckMethodAttributes (method2);
    }

    private void CheckMethodAttributes (MethodInfo method)
    {
      Assert.That (method.Attributes.IsSet (MethodAttributes.SpecialName), Is.True);
    }

    public class DomainType : IMyInterface
    {
      [SpecialName] public virtual void Method1 () { }

      public virtual void InterfaceMethod1 () { }
      public /*not-virtual*/ void InterfaceMethod2 () { }
    }

    public interface IMyInterface
    {
      [SpecialName] void InterfaceMethod1 ();
      [SpecialName] void InterfaceMethod2 ();
    }
    public interface IAddedInterface
    {
      [SpecialName] void AddedInterfaceMethod ();
    }
  }
}