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

    private MethodInfo _existingInterfaceMethod;
    private MethodInfo _addedInterfaceMethod;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));

      _existingInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.MethodOnExistingInterface());
      _addedInterfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.MethodOnAddedInterface());
    }

    [Test]
    public void ExistingInterface_ExistingMethod ()
    {
      // TODO Review: Use GetMethod and check that the interface map's impl method is the same as the method returned by GetMethod. (Remove comparedAsMutable... flag.)
      var implementationMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.MethodOnExistingInterface());

      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, implementationMethod);
    }

    [Test]
    public void ExistingInterface_AddedMethod ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DerivedDomainType));
      // Although we add a method that could be used as an implementation (no override!), the existing base implementation is returned.
      AddSimiliarMethod (mutableType, _existingInterfaceMethod);
      var implementationMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedDomainType obj) => obj.MethodOnExistingInterface());

      CheckGetInterfaceMap (mutableType, _existingInterfaceMethod, implementationMethod, compareAsMutableMethods: false);
    }

    [Test]
    public void ExistingInterface_ExistingMethod_Explicit ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (OtherDomainType));
      var implementationMethod = GetExplicitImplementation (typeof (OtherDomainType), _existingInterfaceMethod);

      CheckGetInterfaceMap (mutableType, _existingInterfaceMethod, implementationMethod);
    }

    [Test]
    public void ExistingInterface_ExistingMethod_ExplicitReplacesImplicit ()
    {
      var implementationMethod = (MutableMethodInfo) _mutableType.GetMethod ("UnrelatedMethod");
      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, _mutableType.GetMethod ("MethodOnExistingInterface"));
      
      implementationMethod.AddExplicitBaseDefinition (_existingInterfaceMethod);
      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, implementationMethod);
    }

    [Test]
    public void ExistingInterface_AddedMethod_Explicit ()
    {
      var implementationMethod = AddSimiliarMethod (_mutableType, _existingInterfaceMethod, methodName: "ExplicitImplementation");
      implementationMethod.AddExplicitBaseDefinition (_existingInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _existingInterfaceMethod, implementationMethod);
    }

    [Test]
    public void AddedInterface_ExistingMethod ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (OtherDomainType));
      mutableType.AddInterface (typeof (IAddedInterface));
      var implementationMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((OtherDomainType obj) => obj.MethodOnAddedInterface());

      CheckGetInterfaceMap (mutableType, _addedInterfaceMethod, implementationMethod);
    }

    [Test]
    public void AddedInterface_AddedMethod ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementationMethod = AddSimiliarMethod (_mutableType, _addedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _addedInterfaceMethod, implementationMethod);
    }

    // TODO Review: Test where an existing method implements an added interface, then add a shadowing method, now this is the interface implementation.

    [Test]
    public void AddedInterface_ExistingMethod_Explicit ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementationMethod = (MutableMethodInfo) _mutableType.GetMethod ("UnrelatedMethod");
      implementationMethod.AddExplicitBaseDefinition (_addedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _addedInterfaceMethod, implementationMethod);
    }

    [Test]
    public void AddedInterface_AddedMethod_Explicit ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      var implementationMethod = AddSimiliarMethod (_mutableType, _addedInterfaceMethod, methodName: "ExplicitImplementation");
      implementationMethod.AddExplicitBaseDefinition (_addedInterfaceMethod);

      CheckGetInterfaceMap (_mutableType, _addedInterfaceMethod, implementationMethod);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The added interface 'IAddedInterface' is not fully implemented. The following methods have no implementation: MethodOnAddedInterface")] // TODO Review: quote method names, add a period.
    public void AddedInterface_NotImplemented ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      _mutableType.GetInterfaceMap (typeof (IAddedInterface));
    }

    // TODO Review: Test that static and non-virtual methods are not considered implicit implementations.

    private void CheckGetInterfaceMap (
        MutableType mutableType, MethodInfo interfaceMethod, MethodInfo expectedImplementationMethod, bool compareAsMutableMethods = true)
    {
      var interfaceType = interfaceMethod.DeclaringType;
      Assertion.IsNotNull (interfaceType);
      Assert.That (interfaceType.IsInterface, Is.True);

      if (compareAsMutableMethods && !(expectedImplementationMethod is MutableMethodInfo))
        expectedImplementationMethod = mutableType.AllMutableMethods.Single (m => m.UnderlyingSystemMethodInfo == expectedImplementationMethod);

      var mapping = mutableType.GetInterfaceMap (interfaceType);

      Assert.That (mapping.InterfaceType, Is.SameAs (interfaceType));
      Assert.That (mapping.TargetType, Is.SameAs (mutableType));
      // TODO Review: Use interfaces with more than one method, check that the indexes of both methods in the respective array are equal (and not -1).
      Assert.That (mapping.InterfaceMethods, Has.Length.EqualTo (1));
      Assert.That (mapping.TargetMethods, Is.EqualTo (new[] { expectedImplementationMethod }));
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

    class DomainType : IExistingInterface
    {
      public void MethodOnExistingInterface () { }
      public virtual void UnrelatedMethod () { }
    }

    class DerivedDomainType : DomainType { }

    class OtherDomainType : IExistingInterface
    {
      void IExistingInterface.MethodOnExistingInterface () { }
      public void MethodOnAddedInterface () { }
    }

    interface IExistingInterface
    {
      void MethodOnExistingInterface ();
    }

    interface IAddedInterface
    {
      void MethodOnAddedInterface ();
    }
  }
}