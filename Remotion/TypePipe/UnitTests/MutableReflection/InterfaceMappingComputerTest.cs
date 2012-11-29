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
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.UnitTests.MutableReflection
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
    private IMutableMemberProvider<MethodInfo, MutableMethodInfo> _mutableMethodProviderMock;

    private MutableType _mutableType;

    private readonly MethodInfo _existingInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method11());
    private readonly MethodInfo _existingInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method12());
    private readonly MethodInfo _existingInterfaceMethod3 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method13());
    private readonly MethodInfo _addedInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method21());
    private readonly MethodInfo _addedInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method22());

    [SetUp]
    public void SetUp ()
    {
      _computer = new InterfaceMappingComputer();

      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));

      _interfaceMapProviderMock = MockRepository.GenerateStrictMock<IInterfaceMappingProvider>();
      _mutableMethodProviderMock = MockRepository.GenerateStrictMock<IMutableMemberProvider<MethodInfo, MutableMethodInfo>>();
    }

    [Test]
    public void ComputeMapping_ExistingInterface ()
    {
      var explicitImplementation = _mutableType.AllMutableMethods.Single (m => m.Name == "UnrelatedMethod");
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
      _mutableMethodProviderMock.Expect (mock => mock.GetMutableMember (implicitImplementation1)).Return (fakeImplementation1);
      _mutableMethodProviderMock.Expect (mock => mock.GetMutableMember (implicitImplementation3)).Return (null);
      // MutableMethodProvider returns null for base methods.

      CallComputeMappingAndCheckResult (
          typeof (IExistingInterface),
          Tuple.Create (_existingInterfaceMethod1, (MethodInfo) fakeImplementation1),
          Tuple.Create (_existingInterfaceMethod2, (MethodInfo) explicitImplementation),
          Tuple.Create (_existingInterfaceMethod3, implicitImplementation3));
    }

    [Test]
    public void ComputeMapping_AddedInterface ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var explicitImplementation = _mutableType.AllMutableMethods.Single (m => m.Name == "UnrelatedMethod");
      explicitImplementation.AddExplicitBaseDefinition (_addedInterfaceMethod1);
      var implicitImplementation2 = _mutableType.GetMethod ("Method22");

      CallComputeMappingAndCheckResult (
          typeof (IAddedInterface),
          Tuple.Create (_addedInterfaceMethod1, (MethodInfo) explicitImplementation),
          Tuple.Create (_addedInterfaceMethod2, implicitImplementation2));
    }

    [Test]
    public void ComputeMapping_AddedInterface_NotFullyImplemented_AllowPartial ()
    {
      _mutableType.AddInterface (typeof (IDisposable));

      var result = _computer.ComputeMapping (
          _mutableType, _interfaceMapProviderMock.Get, _mutableMethodProviderMock, typeof (IDisposable), allowPartialInterfaceMapping: true);

      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDisposable obj) => obj.Dispose());
      Assert.That (result.InterfaceMethods, Is.EqualTo (new[] { interfaceMethod }));
      Assert.That (result.TargetMethods, Is.EqualTo (new MethodInfo[] { null }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IDisposable' is not fully implemented. The following methods have no implementation: Dispose")]
    public void ComputeMapping_AddedInterface_NotFullyImplemented ()
    {
      _mutableType.AddInterface (typeof (IDisposable));
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, _mutableMethodProviderMock, typeof (IDisposable), false);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IInterfaceWithVisibilityMethod' is not fully implemented. The following methods have no implementation: VisibilityMethod")]
    public void ComputeMapping_AddedInterface_NotFullyImplemented_NonPublicImplicitImplementation ()
    {
      _mutableType.AddInterface (typeof (IInterfaceWithVisibilityMethod));
      _computer.ComputeMapping (
          _mutableType, _interfaceMapProviderMock.Get, _mutableMethodProviderMock, typeof (IInterfaceWithVisibilityMethod), false);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Type passed must be an interface.\r\nParameter name: interfaceType")]
    public void ComputeMapping_NoInterfaceType ()
    {
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, _mutableMethodProviderMock, typeof (object), false);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Interface not found.\r\nParameter name: interfaceType")]
    public void ComputeMapping_NotImplemented ()
    {
      _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, _mutableMethodProviderMock, typeof (IDisposable), false);
    }

    // Tuple means: 1) interface method, 2) impl method, 3) expected mutable impl method
    private void CallComputeMappingAndCheckResult (Type interfaceType, params Tuple<MethodInfo, MethodInfo>[] expectedMapping)
    {
      var mapping = _computer.ComputeMapping (_mutableType, _interfaceMapProviderMock.Get, _mutableMethodProviderMock, interfaceType, false);

      _interfaceMapProviderMock.VerifyAllExpectations();
      _mutableMethodProviderMock.VerifyAllExpectations();
      Assert.That (mapping.InterfaceType, Is.SameAs (interfaceType));
      Assert.That (mapping.TargetType, Is.SameAs (_mutableType));
      // Order matters for "expectedMapping".
      Assert.That (mapping.InterfaceMethods.Zip (mapping.TargetMethods), Is.EquivalentTo (expectedMapping));
    }

    class DomainType : IExistingInterface
    {
      public void Method11 () { }
      public void Method12 () { }
      public void Method13 () { }
      public void Method21 () { }
      public void Method22 () { }
      internal void VisibilityMethod () { }
      public virtual void UnrelatedMethod () { }
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
    }
    interface IInterfaceWithVisibilityMethod
    {
      void VisibilityMethod ();
    }
  }
}