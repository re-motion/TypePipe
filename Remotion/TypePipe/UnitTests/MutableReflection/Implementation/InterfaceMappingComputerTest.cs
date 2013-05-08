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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
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

    private MutableType _mutableType;

    private readonly MethodInfo _existingInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method11());
    private readonly MethodInfo _existingInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method12());
    private readonly MethodInfo _addedInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method21());
    private readonly MethodInfo _addedInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method22());
    private readonly MethodInfo _addedInterfaceMethod3 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method23());

    [SetUp]
    public void SetUp ()
    {
      _computer = new InterfaceMappingComputer();

      _mutableType = MutableTypeObjectMother.Create (baseType: typeof (DomainType));

      _interfaceMapProviderMock = MockRepository.GenerateStrictMock<IInterfaceMappingProvider>();
    }

    [Test]
    public void ComputeMapping_ExistingInterface ()
    {
      var implicitImplementation1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method11());
      var implicitImplementation2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method12 ());
      var explicitImplementation = _mutableType.AddMethod ("ExplicitImpl", MethodAttributes.Virtual);
      explicitImplementation.AddExplicitBaseDefinition (_existingInterfaceMethod2);

      _interfaceMapProviderMock
          .Expect (mock => mock.Get (typeof (IExistingInterface)))
          .Return (
              new InterfaceMapping
              {
                  InterfaceType = typeof (IExistingInterface),
                  InterfaceMethods = new[] { _existingInterfaceMethod1, _existingInterfaceMethod2 },
                  TargetMethods = new[] { implicitImplementation1, implicitImplementation2 }
              });

      CallComputeMappingAndCheckResult (
          _mutableType,
          typeof (IExistingInterface),
          Tuple.Create (_existingInterfaceMethod1, implicitImplementation1),
          Tuple.Create (_existingInterfaceMethod2, (MethodInfo) explicitImplementation));
    }

    [Test]
    public void ComputeMapping_ExistingInterface_OverriddenBaseImplementation ()
    {
      var implicitImplementation1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method11());
      var implicitImplementation2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method12());
      var baseImplementationOverride = _mutableType.AddMethod ("Method12", MethodAttributes.Public | MethodAttributes.Virtual);

      _interfaceMapProviderMock
          .Expect (mock => mock.Get (typeof (IExistingInterface)))
          .Return (
              new InterfaceMapping
              {
                InterfaceType = typeof (IExistingInterface),
                InterfaceMethods = new[] { _existingInterfaceMethod1, _existingInterfaceMethod2 },
                TargetMethods = new[] { implicitImplementation1, implicitImplementation2 }
              });

      CallComputeMappingAndCheckResult (
          _mutableType,
          typeof (IExistingInterface),
          Tuple.Create (_existingInterfaceMethod1, implicitImplementation1),
          Tuple.Create (_existingInterfaceMethod2, (MethodInfo) baseImplementationOverride));
    }

    [Test]
    public void ComputeMapping_AddedInterface ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var explicitImplementation = _mutableType.AddMethod ("ExplicitImpl", MethodAttributes.Virtual);
      explicitImplementation.AddExplicitBaseDefinition (_addedInterfaceMethod1);

      var shadowedMethod = _mutableType.GetMethod ("Method22");
      Assert.That (shadowedMethod.DeclaringType, Is.SameAs (typeof (DomainType)));
      var implicitImplementation2 = _mutableType.AddMethod ("Method22", MethodAttributes.NewSlot | MethodAttributes.Public | MethodAttributes.Virtual);
      Assert.That (implicitImplementation2.BaseMethod, Is.Null);
      // TODO 5059: comment in
      //Assert.That (_mutableType.GetMethod ("Method22"), Is.Not.EqualTo (shadowedMethod).And.EqualTo (implicitImplementation2));

      var implicitImplementation3 = _mutableType.GetMethod ("Method23");
      Assert.That (implicitImplementation3.DeclaringType, Is.SameAs (typeof (DomainType)));

      CallComputeMappingAndCheckResult (
          _mutableType,
          typeof (IAddedInterface),
          Tuple.Create (_addedInterfaceMethod1, (MethodInfo) explicitImplementation),
          Tuple.Create (_addedInterfaceMethod2, (MethodInfo) implicitImplementation2),
          Tuple.Create (_addedInterfaceMethod3, implicitImplementation3));
    }

    [Test]
    public void ComputeMapping_AddedInterface_CandidateOrder ()
    {
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      var mutableType = MutableTypeObjectMother.Create (baseType: typeof (DomainType), memberSelector: memberSelectorMock);
      memberSelectorMock.Stub (stub => stub.SelectMethods<MethodInfo> (null, 0, null)).IgnoreArguments().Return (new MethodInfo[0]).Repeat.Once();
      mutableType.AddMethod ("Method21", MethodAttributes.Public | MethodAttributes.Virtual);

      // TODO 5059: fix (use simple GetMethods with name)
      var baseMethod = typeof (DomainType).GetMethods().Single (m => m.Name == "Method23" && m.DeclaringType == typeof (DomainType));
      var methods = GetAllMethods (mutableType).ToArray();
      var baseMethodIndex = Array.IndexOf (methods, baseMethod);
      // Change sequence so that base method comes at start.
      var mixedMethods = methods.Skip (baseMethodIndex).Concat (methods.Take (baseMethodIndex)).ToArray();
      Assert.That (mixedMethods[0], Is.SameAs (baseMethod));
      Assert.That (mixedMethods, Is.EquivalentTo (methods));

      mutableType.AddInterface (typeof (IAddedInterface));
      memberSelectorMock
          .Expect (
              mock =>
              mock.SelectMethods (
                  Arg<IEnumerable<MethodInfo>>.List.Equal (methods), Arg.Is (BindingFlags.Public | BindingFlags.Instance), Arg.Is (mutableType)))
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
      _mutableType.AddInterface (typeof (IDisposable));
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDisposable obj) => obj.Dispose ());

      CallComputeMappingAndCheckResult (_mutableType, typeof (IDisposable), Tuple.Create (interfaceMethod, (MethodInfo) null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IDisposable' is not fully implemented. The following methods have no implementation: 'Dispose'.")]
    public void ComputeMapping_AddedInterface_NotFullyImplemented_Throws ()
    {
      _mutableType.AddInterface (typeof (IDisposable));
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, typeof (IDisposable), false);
    }

    [Test]
    public void ComputeMapping_AddedInterface_Candidates_AllowPartial ()
    {
      _mutableType.AddInterface (typeof (IImplementationCandidates));
      var interfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IImplementationCandidates obj) => obj.NonPublicMethod());
      var interfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IImplementationCandidates obj) => obj.NonVirtualMethod());

      CallComputeMappingAndCheckResult (
          _mutableType,
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
      _mutableType.AddInterface (typeof (IImplementationCandidates));
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, typeof (IImplementationCandidates), false);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type passed must be an interface.\r\nParameter name: interfaceType")]
    public void ComputeMapping_NoInterfaceType ()
    {
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, typeof (object), false);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Interface not found.\r\nParameter name: interfaceType")]
    public void ComputeMapping_NotImplemented ()
    {
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, typeof (IDisposable), false);
    }

    private IEnumerable<MethodInfo> GetAllMethods (MutableType mutableType)
    {
      return (IEnumerable<MethodInfo>) PrivateInvoke.InvokeNonPublicMethod (mutableType, "GetAllMethods");
    }

    // Tuple means: 1) interface method, 2) implementation method
    private void CallComputeMappingAndCheckResult (MutableType mutableType, Type interfaceType, params Tuple<MethodInfo, MethodInfo>[] expectedMapping)
    {
      var mapping = _computer.ComputeMapping (mutableType, _interfaceMapProviderMock.Get, interfaceType, true);

      _interfaceMapProviderMock.VerifyAllExpectations();
      Assert.That (mapping.InterfaceType, Is.SameAs (interfaceType));
      Assert.That (mapping.TargetType, Is.SameAs (mutableType));
      // Order matters for "expectedMapping".
      var enumerable = mapping.InterfaceMethods.Zip (mapping.TargetMethods).ToArray();
      Assert.That (enumerable, Is.EquivalentTo (expectedMapping));
    }

    public class DomainType : IExistingInterface
    {
      public void Method11 () { }
      public virtual void Method12 () { }

      // This methods can be shadowed in the proxy type (via added methods), unordered implicit matching (without considering the type hierarchy)
      // would result in an ambigous match. 
      // Method21 is added by tests.
      public virtual void Method22 () { } // Shadowed.
      public virtual void Method23 () { } // Not shadowed.

      internal virtual void NonPublicMethod () { }
      [UsedImplicitly] public void NonVirtualMethod () { }
    }

    public interface IExistingInterface
    {
      void Method11 ();
      void Method12 ();
    }
    public interface IAddedInterface
    {
      void Method21 ();
      void Method22 ();
      void Method23 ();
    }
    public interface IImplementationCandidates
    {
      void NonPublicMethod ();
      void NonVirtualMethod ();
    }
  }
}