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
using Remotion.Collections;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class InterfaceMappingComputerTest
  {
    public interface IInterfaceMappingProvider
    {
      InterfaceMapping Get (Type interfaceType);
    }

    private InterfaceMappingComputer _computer;

    private IInterfaceMappingProvider _interfaceMapProviderMock;

    private ProxyType _proxyType;

    private readonly MethodInfo _existingInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method11());
    private readonly MethodInfo _existingInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method12());
    private readonly MethodInfo _existingInterfaceMethod3 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method13());
    private readonly MethodInfo _addedInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method21());
    private readonly MethodInfo _addedInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method22());
    private readonly MethodInfo _addedInterfaceMethod3 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method23());

    [SetUp]
    public void SetUp ()
    {
      _computer = new InterfaceMappingComputer();

      _proxyType = MutableTypeObjectMother.Create (
          typeof (DomainType),
          memberSelector: null,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);

      _interfaceMapProviderMock = MockRepository.GenerateStrictMock<IInterfaceMappingProvider>();
    }

    [Test]
    public void ComputeMapping_ExistingInterface ()
    {
      var explicitImplementation = CreateVirtualMethod(_proxyType);
      explicitImplementation.AddExplicitBaseDefinition (_existingInterfaceMethod2);
      var implicitImplementation1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method11());
      var implicitImplementation3 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method13());
      var fakeImplementation1 = MutableMethodInfoObjectMother.Create();

      _interfaceMapProviderMock
          .Expect (mock => mock.Get (typeof (IExistingInterface)))
          .Return (
              new InterfaceMapping
              {
                  InterfaceType = typeof (IExistingInterface),
                  InterfaceMethods = new[] { _existingInterfaceMethod1, _existingInterfaceMethod2, _existingInterfaceMethod3 },
                  TargetMethods = new[] { implicitImplementation1, null /* not used */, implicitImplementation3 }
              });

      CallComputeMappingAndCheckResult (
          _proxyType,
          typeof (IExistingInterface),
          Tuple.Create (_existingInterfaceMethod1, (MethodInfo) fakeImplementation1),
          Tuple.Create (_existingInterfaceMethod2, (MethodInfo) explicitImplementation),
          Tuple.Create (_existingInterfaceMethod3, implicitImplementation3));
    }

    [Test]
    public void ComputeMapping_AddedInterface ()
    {
      _proxyType.AddInterface (typeof (IAddedInterface));
      var explicitImplementation = _proxyType.AddMethod (
          "UnrelatedMethod", MethodAttributes.Virtual, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());
      explicitImplementation.AddExplicitBaseDefinition (_addedInterfaceMethod1);
      var implicitImplementation2 = _proxyType.GetMethod ("Method22", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
      var implicitImplementation3 = _proxyType.GetMethod ("Method23");
      Assert.That (implicitImplementation2.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (implicitImplementation3.DeclaringType, Is.SameAs (typeof (DomainTypeBase)));

      CallComputeMappingAndCheckResult (
          _proxyType,
          typeof (IAddedInterface),
          Tuple.Create (_addedInterfaceMethod1, (MethodInfo) explicitImplementation),
          Tuple.Create (_addedInterfaceMethod2, implicitImplementation2),
          Tuple.Create (_addedInterfaceMethod3, implicitImplementation3));
    }

    [Test]
    public void ComputeMapping_AddedInterface_CandidateOrder ()
    {
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      var mutableType = MutableTypeObjectMother.Create (
          typeof (DomainType),
          memberSelector: memberSelectorMock,
          relatedMethodFinder: null,
          interfaceMappingComputer: null,
          mutableMemberFactory: null);

      var baseMethod = typeof (DomainType).GetMethods().Single (m => m.Name == "Method22" && m.DeclaringType == typeof (DomainTypeBase));
      var methods = GetAllMethods (mutableType).ToArray();
      var baseMethodIndex = Array.IndexOf (methods, baseMethod);
      // Change sequence so that base method comes at start.
      var mixedMethods = methods.Skip (baseMethodIndex).Concat (methods.Take (baseMethodIndex)).ToArray();
      Assert.That (mixedMethods[0], Is.SameAs (baseMethod));
      Assert.That (mixedMethods, Is.EquivalentTo (methods));

      mutableType.AddInterface (typeof (IAddedInterface));
      memberSelectorMock
          .Expect (mock => mock.SelectMethods (GetAllMethods (mutableType), BindingFlags.Public | BindingFlags.Instance, mutableType))
          .Return (mixedMethods);

      CallComputeMappingAndCheckResult (
          mutableType,
          typeof (IAddedInterface),
          Tuple.Create (_addedInterfaceMethod1, methods.First (m => m.Name == "Method21")),
          Tuple.Create (_addedInterfaceMethod2, methods.First (m => m.Name == "Method22")),
          Tuple.Create (_addedInterfaceMethod3, methods.First (m => m.Name == "Method23")));
      memberSelectorMock.VerifyAllExpectations();
    }

    [Test]
    public void ComputeMapping_AddedInterface_NotFullyImplemented_AllowPartial ()
    {
      _proxyType.AddInterface (typeof (IDisposable));
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDisposable obj) => obj.Dispose ());

      CallComputeMappingAndCheckResult (_proxyType, typeof (IDisposable), Tuple.Create (interfaceMethod, (MethodInfo) null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IDisposable' is not fully implemented. The following methods have no implementation: 'Dispose'.")]
    public void ComputeMapping_AddedInterface_NotFullyImplemented_Throws ()
    {
      _proxyType.AddInterface (typeof (IDisposable));
      _computer.ComputeMapping (_proxyType, _interfaceMapProviderMock.Get, typeof (IDisposable), false);
    }

    [Test]
    public void ComputeMapping_AddedInterface_Candidates_AllowPartial ()
    {
      _proxyType.AddInterface (typeof (IImplementationCandidates));
      var interfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IImplementationCandidates obj) => obj.NonPublicMethod());
      var interfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IImplementationCandidates obj) => obj.NonVirtualMethod());

      CallComputeMappingAndCheckResult (
          _proxyType,
          typeof (IImplementationCandidates),
          Tuple.Create (interfaceMethod1, (MethodInfo) null),
          Tuple.Create (interfaceMethod2, (MethodInfo) null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IImplementationCandidates' is not fully implemented. The following methods have no implementation: " +
        "'NonPublicMethod', 'NonVirtualMethod'.")]
    public void ComputeMapping_AddedInterface_Candidates_Throws ()
    {
      _proxyType.AddInterface (typeof (IImplementationCandidates));
      _computer.ComputeMapping (_proxyType, _interfaceMapProviderMock.Get, typeof (IImplementationCandidates), false);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type passed must be an interface.\r\nParameter name: interfaceType")]
    public void ComputeMapping_NoInterfaceType ()
    {
      _computer.ComputeMapping (_proxyType, _interfaceMapProviderMock.Get, typeof (object), false);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Interface not found.\r\nParameter name: interfaceType")]
    public void ComputeMapping_NotImplemented ()
    {
      _computer.ComputeMapping (_proxyType, _interfaceMapProviderMock.Get, typeof (IDisposable), false);
    }

    private MutableMethodInfo CreateVirtualMethod (ProxyType proxyType)
    {
      return proxyType.AddMethod ("m", MethodAttributes.Virtual, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());
    }

    private MutableTypeMethodCollection GetAllMethods (ProxyType proxyType)
    {
      return (MutableTypeMethodCollection) PrivateInvoke.GetNonPublicField (proxyType, "_methods");
    }

    // Tuple means: 1) interface method, 2) implementation method
    private void CallComputeMappingAndCheckResult (ProxyType proxyType, Type interfaceType, params Tuple<MethodInfo, MethodInfo>[] expectedMapping)
    {
      var mapping = _computer.ComputeMapping (proxyType, _interfaceMapProviderMock.Get, interfaceType, true);

      _interfaceMapProviderMock.VerifyAllExpectations();
      Assert.That (mapping.InterfaceType, Is.SameAs (interfaceType));
      Assert.That (mapping.TargetType, Is.SameAs (proxyType));
      // Order matters for "expectedMapping".
      Assert.That (mapping.InterfaceMethods.Zip (mapping.TargetMethods), Is.EquivalentTo (expectedMapping));
    }

    class DomainTypeBase
    {
      // This methods can be shadowed in 'DomainType', unordered implicit matching (without considering the type hierarchy)
      // would result in an ambigous match.
      public virtual void Method22 () { } // Shadowed.
      public virtual void Method23 () { } // Not shadowed.
    }
    class DomainType : DomainTypeBase, IExistingInterface
    {
      public void Method11 () { }
      public void Method12 () { }
      public void Method13 () { }
      public virtual void Method21 () { }
      public new virtual void Method22 () { }

      internal virtual void NonPublicMethod () { }
      public void NonVirtualMethod () { }
    }

    interface IExistingInterface
    {
      void Method11 ();
      void Method12 ();
      void Method13 ();
    }
    interface IAddedInterface
    {
      void Method21 ();
      void Method22 ();
      void Method23 ();
    }
    interface IImplementationCandidates
    {
      void NonPublicMethod ();
      void NonVirtualMethod ();
    }
  }
}