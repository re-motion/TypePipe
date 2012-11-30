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
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class GetInterfaceMapTest
  {
    private MutableType _mutableType;

    private MethodInfo _existingBaseInterfaceMethod;
    private MethodInfo _existingInterfaceMethod;
    private MethodInfo _addedInterfaceMethod;
    private MethodInfo _otherAddedInterfaceMethod;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));

      _existingBaseInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingBaseInterface obj) => obj.MethodOnExistingBaseInterface());
      _existingInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.MethodOnExistingInterface());
      _addedInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.MethodOnAddedInterface());
      _otherAddedInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IOtherAddedInterface obj) => obj.MethodOnOtherAddedInterface());
    }

    [Test]
    public void ExistingInterface_ExistingMethod ()
    {
      var implementation = _mutableType.ExistingMutableMethods.Single (m => m.Name == "MethodOnExistingInterface");

      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, implementation);
    }

    [Test]
    public void ExistingInterface_AddedMethod ()
    {
      // Although we add a method that could be used as an implementation (no override!), the existing base implementation is returned.
      AddSimiliarMethod (_mutableType, _existingBaseInterfaceMethod);
      var implementationOnBase = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.MethodOnExistingBaseInterface());
      Assert.That (implementationOnBase.DeclaringType, Is.SameAs (typeof (DomainTypeBase)));

      CheckGetInterfaceMap (_mutableType, _existingBaseInterfaceMethod, implementationOnBase);
    }

    [Test]
    public void ExistingInterface_ExistingMethod_Explicit ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (OtherDomainType));
      var implementation = GetExplicitImplementation (mutableType, _existingInterfaceMethod);

      CheckGetInterfaceMap (mutableType, _existingInterfaceMethod, implementation);
    }

    [Test]
    public void ExistingInterface_ExistingMethod_ExplicitReplacesImplicit ()
    {
      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, _mutableType.GetMethod ("MethodOnExistingInterface"));
      var implementation = (MutableMethodInfo) _mutableType.GetMethod ("UnrelatedMethod");
      implementation.AddExplicitBaseDefinition (_existingInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, implementation);
    }

    [Test]
    public void ExistingInterface_AddedMethod_Explicit ()
    {
      var implementation = AddSimiliarMethod (_mutableType, _existingInterfaceMethod, methodName: "ExplicitImplementation");
      implementation.AddExplicitBaseDefinition (_existingInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, implementation);
    }

    [Test]
    public void AddedInterface_ExistingMethod ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (OtherDomainType));
      mutableType.AddInterface (typeof (IAddedInterface));
      var implementation = mutableType.GetMethod ("MethodOnAddedInterface");

      CheckGetInterfaceMap (mutableType, _addedInterfaceMethod, implementation);
    }

    [Test]
    public void AddedInterface_AddedMethod ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementation = AddSimiliarMethod (_mutableType, _addedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _addedInterfaceMethod, implementation);
    }

    [Ignore ("TODO 5229")]
    [Test]
    public void AddInterface_ExistingMethod_ShadowedByAddedMethod ()
    {
      _mutableType.AddInterface (typeof (IOtherAddedInterface));
      var implementationOnBase = _mutableType.GetMethod ("MethodOnOtherAddedInterface");
      CheckGetInterfaceMap (_mutableType, _otherAddedInterfaceMethod, implementationOnBase);

      var shadowingImplementation = AddSimiliarMethod (_mutableType, _otherAddedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _otherAddedInterfaceMethod, shadowingImplementation);
    }

    [Test]
    public void AddedInterface_ExistingMethod_Explicit ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementation = (MutableMethodInfo) _mutableType.GetMethod ("UnrelatedMethod");
      implementation.AddExplicitBaseDefinition (_addedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _addedInterfaceMethod, implementation);
    }

    [Test]
    public void AddedInterface_AddedMethod_Explicit ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementation = AddSimiliarMethod (_mutableType, _addedInterfaceMethod, methodName: "ExplicitImplementation");
      implementation.AddExplicitBaseDefinition (_addedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _addedInterfaceMethod, implementation);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IAddedInterface' is not fully implemented. The following methods have no implementation: 'MethodOnAddedInterface'.")]
    public void AddedInterface_NotImplemented ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      _mutableType.GetInterfaceMap (typeof (IAddedInterface));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IImplementationCandidates' is not fully implemented. The following methods have no implementation: "
        + "'NonPublicMethod', 'NonVirtualMethod', 'StaticMethod'.")]
    public void AddedInterface_NotImplemented_Candidates ()
    {
      _mutableType.AddInterface (typeof (IImplementationCandidates));
      _mutableType.GetInterfaceMap (typeof (IImplementationCandidates));
    }

    private void CheckGetInterfaceMap (MutableType mutableType, MethodInfo interfaceMethod, MethodInfo expectedImplementationMethod)
    {
      var interfaceType = interfaceMethod.DeclaringType;
      Assertion.IsNotNull (interfaceType);
      Assert.That (interfaceType.IsInterface, Is.True);

      var mapping = mutableType.GetInterfaceMap (interfaceType);

      Assert.That (mapping.InterfaceType, Is.SameAs (interfaceType));
      Assert.That (mapping.TargetType, Is.SameAs (mutableType));
      var interfaceMethodIndex = Array.IndexOf (mapping.InterfaceMethods, interfaceMethod);
      var targetMethodIndex = Array.IndexOf (mapping.TargetMethods, expectedImplementationMethod);
      Assert.That (targetMethodIndex, Is.EqualTo (interfaceMethodIndex).And.Not.EqualTo (-1));
    }

    private MutableMethodInfo AddSimiliarMethod (MutableType mutableType, MethodInfo template, string methodName = null)
    {
      return mutableType.AddMethod (
          methodName ?? template.Name,
          MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot,
          template.ReturnType,
          ParameterDeclaration.CreateForEquivalentSignature (template),
          ctx => Expression.Default (template.ReturnType));
    }

    private MethodInfo GetExplicitImplementation (Type implementationType, MethodInfo interfaceMethod)
    {
      var interfaceFullName = interfaceMethod.DeclaringType.FullName.Replace ('+', '.');
      var explicitMethodName = string.Format ("{0}.{1}", interfaceFullName, interfaceMethod.Name);
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var explicitImplementation = implementationType.GetMethod (explicitMethodName, bindingFlags);
      Assertion.IsNotNull (explicitImplementation);

      return explicitImplementation;
    }

    class DomainTypeBase : IExistingBaseInterface
    {
      public void MethodOnExistingBaseInterface () { }
      public virtual void MethodOnOtherAddedInterface () { }
    }
    class DomainType : DomainTypeBase, IExistingInterface
    {
      public void MethodOnExistingInterface () { }
      public void AdditionalMethodOnExistingInterface () { }
      public virtual void ExistingMethodMatchingAddedInterfaceMethod () { }
      public virtual void UnrelatedMethod () { }

      public virtual void PublicVirtualMethod () { }
      internal virtual void NonPublicMethod () { }
      public void NonVirtualMethod () { }
      public static void StaticMethod () { }
    }
    class OtherDomainType : IExistingInterface
    {
      void IExistingInterface.MethodOnExistingInterface () { }
      public void AdditionalMethodOnExistingInterface () { }
      public virtual void MethodOnAddedInterface () { }
      public virtual void ExistingMethodMatchingAddedInterfaceMethod () { }
    }

    interface IExistingBaseInterface
    {
      void MethodOnExistingBaseInterface ();
    }
    interface IExistingInterface
    {
      void MethodOnExistingInterface ();
      void AdditionalMethodOnExistingInterface ();
    }
    interface IAddedInterface
    {
      void MethodOnAddedInterface ();
      void ExistingMethodMatchingAddedInterfaceMethod ();
    }
    interface IOtherAddedInterface
    {
      void MethodOnOtherAddedInterface ();
    }
    interface IImplementationCandidates
    {
      void PublicVirtualMethod ();
      void NonPublicMethod ();
      void NonVirtualMethod ();
      void StaticMethod ();
    }
  }
}