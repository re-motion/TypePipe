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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [Ignore ("TODO 5227")]
  [TestFixture]
  public class InterfaceMappingHelperTest
  {
    private InterfaceMappingComputer _computer;

    private MutableType _mutableType;
    private MethodInfo _existingInterfaceMethod;
    private MethodInfo _addedInterfaceMethod;

    [SetUp]
    public void SetUp ()
    {
      _computer = new InterfaceMappingComputer();

      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));
      _existingInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.Method1());
      _addedInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.Method2());
    }

    [Test]
    public void ComputeMapping_ExistingInterface ()
    {
      var underlyingImplementation = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method1());
      var mappingProvider = CreateMappingProvider (_existingInterfaceMethod, underlyingImplementation);

      var result = _computer.ComputeMapping (_mutableType, mappingProvider, typeof (IExistingInterface));

      var implementation = _mutableType.AllMutableMethods.Single (m => m.Name == "Method1");
      CheckInterfaceMapping (result, implementation);
    }

    [Test]
    public void ComputeMapping_ExistingInterface_ExplicitReplacesImplicit ()
    {
      var implementation = _mutableType.AllMutableMethods.Single (m => m.Name == "UnrelatedMethod");
      implementation.AddExplicitBaseDefinition (_existingInterfaceMethod);

      var result = _computer.ComputeMapping (_mutableType, CreateMappingProvider (_existingInterfaceMethod), typeof (IExistingInterface));

      CheckInterfaceMapping (result, implementation);
    }

    [Test]
    public void ComputeMapping_AddedInterface_Implicit ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));

      var result = _computer.ComputeMapping (_mutableType, CreateMappingProvider (_addedInterfaceMethod), typeof (IAddedInterface));

      var implementation = _mutableType.AllMutableMethods.Single (m => m.Name == "Method2");
      CheckInterfaceMapping (result, implementation);
    }

    [Test]
    public void ComputeMapping_AddedInterface_Explicit ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementation = _mutableType.AllMutableMethods.Single (m => m.Name == "UnrelatedMethod");
      implementation.AddExplicitBaseDefinition (_addedInterfaceMethod);

      var result = _computer.ComputeMapping (_mutableType, CreateMappingProvider (_addedInterfaceMethod), typeof (IAddedInterface));

      CheckInterfaceMapping (result, implementation);
    }

    private Func<Type, InterfaceMapping> CreateMappingProvider (MethodInfo interfaceMethod, MethodInfo underlyingImplementationMethod = null)
    {
      return interfaceType =>
      {
        Assert.That (interfaceType, Is.SameAs (interfaceMethod.DeclaringType));

        return new InterfaceMapping
               {
                   InterfaceType = interfaceType,
                   InterfaceMethods = new[] { interfaceMethod },
                   TargetType = typeof (DomainType),
                   TargetMethods = new[] { underlyingImplementationMethod }
               };
      };
    }

    private void CheckInterfaceMapping (InterfaceMapping mapping, MethodInfo expectedImplementationMethod)
    {
      Assert.That (mapping.TargetType, Is.SameAs (_mutableType));
      Assert.That (mapping.TargetMethods, Is.EqualTo (new[] { expectedImplementationMethod }));
    }

    class DomainType : IExistingInterface
    {
      public void Method1 () { }
      public void Method2 () { }
      public void UnrelatedMethod () { }
    }

    interface IExistingInterface
    {
      void Method1 ();
    }
    interface IAddedInterface
    {
      void Method2 ();
    }
  }
}