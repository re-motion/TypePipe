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
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class InterfaceMappingHelperTest
  {
    public interface IInterfaceMappingProvider
    {
      InterfaceMapping Get (Type interfaceType);
    }

    private InterfaceMappingComputer _computer;

    private IInterfaceMappingProvider _interfaceMappingProviderMock;
    private IMutableMemberProvider<MethodInfo, MutableMethodInfo> _mutableMethodProviderMock;

    private MutableType _mutableType;

    private readonly MethodInfo _existingInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method11());
    private readonly MethodInfo _existingInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method12());
    private readonly MethodInfo _addedInterfaceMethod1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method21());
    private readonly MethodInfo _addedInterfaceMethod2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method22());
    private readonly MutableMethodInfo _fakeResult1 = MutableMethodInfoObjectMother.Create();
    private readonly MutableMethodInfo _fakeResult2 = MutableMethodInfoObjectMother.Create();

    [SetUp]
    public void SetUp ()
    {
      _computer = new InterfaceMappingComputer();

      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));

      _interfaceMappingProviderMock = MockRepository.GenerateStrictMock<IInterfaceMappingProvider>();
      _mutableMethodProviderMock = MockRepository.GenerateStrictMock<IMutableMemberProvider<MethodInfo, MutableMethodInfo>>();
    }

    [Test]
    public void ComputeMapping_ExistingInterface ()
    {
      var explicitImplementation = _mutableType.AllMutableMethods.Single (m => m.Name == "UnrelatedMethod");
      explicitImplementation.AddExplicitBaseDefinition (_existingInterfaceMethod2);
      var implicitImplementation1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method11());
      _interfaceMappingProviderMock
          .Expect (mock => mock.Get (typeof (IExistingInterface)))
          .Return (
              new InterfaceMapping
              {
                  InterfaceMethods = new[] { _existingInterfaceMethod1, _existingInterfaceMethod2 },
                  TargetMethods = new[] { implicitImplementation1, null /* not used */ }
              });

      CallComputeMappingAndCheckResult (
          typeof (IExistingInterface),
          Tuple.Create (_existingInterfaceMethod1, implicitImplementation1, _fakeResult1, (MethodInfo) _fakeResult1),
          Tuple.Create (_existingInterfaceMethod2, (MethodInfo) explicitImplementation, _fakeResult2, (MethodInfo) _fakeResult2));
    }

    [Ignore("TODO")]
    [Test]
    public void ComputeMapping_AddedInterface ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var explicitImplementation = _mutableType.AllMutableMethods.Single (m => m.Name == "UnrelatedMethod");
      explicitImplementation.AddExplicitBaseDefinition (_addedInterfaceMethod1);
      var implicitImplementation2 = _mutableType.GetMethod ("Method2");

      CallComputeMappingAndCheckResult (
          typeof (IAddedInterface),
          Tuple.Create (_existingInterfaceMethod1, (MethodInfo) explicitImplementation, _fakeResult1, (MethodInfo) _fakeResult1),
          Tuple.Create (_existingInterfaceMethod2, implicitImplementation2, _fakeResult2, (MethodInfo) _fakeResult2));
    }

    // Tuple means: 1) interface method, 2) impl method, 3) mutable impl method, 4) expected result impl method
    private void CallComputeMappingAndCheckResult (
        Type interfaceType, params Tuple<MethodInfo, MethodInfo, MutableMethodInfo, MethodInfo>[] expectedMapping)
    {
      foreach (var entry in expectedMapping)
      {
        var e = entry;
        _mutableMethodProviderMock.Expect (mock => mock.GetMutableMember (e.Item2)).Return (e.Item3).Repeat.Once();
      }

      var mapping = _computer.ComputeMapping (_mutableType, _interfaceMappingProviderMock.Get, interfaceType, _mutableMethodProviderMock);

      _interfaceMappingProviderMock.VerifyAllExpectations();
      _mutableMethodProviderMock.VerifyAllExpectations();
      Assert.That (mapping.InterfaceType, Is.SameAs (interfaceType));
      Assert.That (mapping.TargetType, Is.SameAs (_mutableType));

      // Order matters for "expectedMapping".
      var comparableMapping = mapping.InterfaceMethods.Zip (mapping.TargetMethods).OrderBy (t => t.Item1.Name);
      var expectedComparableMapping = expectedMapping.Select (t => Tuple.Create (t.Item1, t.Item4));
      Assert.That (comparableMapping, Is.EqualTo (expectedComparableMapping));
    }

    class DomainType : IExistingInterface
    {
      public void Method11 () { }
      public void Method12 () { }
      public void Method21 () { }
      public void Method22 () { }
      public virtual void UnrelatedMethod () { }
    }

    interface IExistingInterface
    {
      void Method11 ();
      void Method12 ();
    }
    interface IAddedInterface
    {
      void Method21 ();
      void Method22 ();
    }
  }
}