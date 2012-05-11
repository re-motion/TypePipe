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
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class RelatedMethodFinderTest
  {
    private RelatedMethodFinder _finder;

    private MethodSignature _methodSignature;
    private Type _typeToStartSearch;

    [SetUp]
    public void SetUp ()
    {
      _finder = new RelatedMethodFinder();

      _methodSignature = new MethodSignature (typeof (void), Type.EmptyTypes, 0);
      _typeToStartSearch = typeof (DomainType);
    }

    [Test]
    public void GetMostDerivedVirtualMethod_DerivedTypeMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("DerivedTypeMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DerivedTypeMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_DerivedTypeMethod_NonMatchingName ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("DoesNotExist", _methodSignature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMostDerivedVirtualMethod_DerivedTypeMethod_NonMatchingSignature ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DerivedTypeMethod ());
      var signature = new MethodSignature (typeof (int), Type.EmptyTypes, 0);
      Assert.That (signature, Is.Not.EqualTo (MethodSignature.Create (method)));

      var result = _finder.GetMostDerivedVirtualMethod (method.Name, signature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMostDerivedVirtualMethod_ProtectedDerivedTypeMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("ProtectedDerivedTypeMethod", _methodSignature, _typeToStartSearch);

      var expected = typeof (DomainType).GetMethod ("ProtectedDerivedTypeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (expected, Is.Not.Null);
      Assert.That (expected.IsPublic, Is.False);

      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_BaseTypeMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("BaseTypeMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_NonVirtualMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualMethod ());
      var result = _finder.GetMostDerivedVirtualMethod (method.Name, _methodSignature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMostDerivedVirtualMethod_OverridingMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("OverridingMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod((DomainType obj) => obj.OverridingMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_VirtualMethodShadowingBaseMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("VirtualMethodShadowingBaseMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethodShadowingBaseMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_NonVirtualMethodShadowingVirtualMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("NonVirtualMethodShadowingVirtualMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.NonVirtualMethodShadowingVirtualMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedOverride_BaseTypeMethod ()
    {
      var baseDefinition = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMostDerivedOverride_VirtualMethodShadowingBaseMethod ()
    {
      var baseDefinition = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.VirtualMethodShadowingBaseMethod ());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMostDerivedOverride_OverridingMethod ()
    {
      var baseDefinition = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingMethod());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedOverride_OverridingOverriddenMethod ()
    {
      var baseDefinition = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.OverridingOverriddenMethod ());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingOverriddenMethod ());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedOverride_ShadowingOverridenMethod ()
    {
      var baseDefinition = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.ShadowingOverridenMethod ());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.ShadowingOverridenMethod ());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_DerivedTypeMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DerivedTypeMethod ());
      Assert.That (method.IsVirtual, Is.True);
      // Note: This method is also a NewSlot method because C# won't allow us to define a virtual reuseslot method without an overridden base method.
      // Since we implement via GetBaseDefinition(), our implementation doesn't depend on that nuance anyway.

      var result = _finder.GetBaseMethod (method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_VirtualMethodShadowingBaseMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethodShadowingBaseMethod ());
      Assert.That (method.IsVirtual, Is.True);
      Assert.That (MethodAttributeUtility.IsSet (method.Attributes, MethodAttributes.NewSlot) , Is.True);
      Assert.That (typeof (DomainTypeBase).GetMethod ("VirtualMethodShadowingBaseMethod"), Is.Not.Null);

      var result = _finder.GetBaseMethod (method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_OverridingMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingMethod());

      var result = _finder.GetBaseMethod (method);

      var expectedMethod = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingMethod ());
      Assert.That (result, Is.EqualTo (expectedMethod));
    }

    [Test]
    public void GetBaseMethod_OverridingOverriddenMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingOverriddenMethod());

      var result = _finder.GetBaseMethod (method);

      var expectedMethod = MemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingOverriddenMethod ());
      Assert.That (result, Is.EqualTo (expectedMethod));
    }

    // ReSharper disable UnusedMember.Local
    // ReSharper disable VirtualMemberNeverOverriden.Global
    private class DomainTypeBaseBase
    {
      public virtual void OverridingOverriddenMethod() { }
      public virtual void ShadowingOverridenMethod () { }
    }

    private class DomainTypeBase : DomainTypeBaseBase
    {
      public virtual void BaseTypeMethod () { }

      public virtual void OverridingMethod () { }
      public override void OverridingOverriddenMethod () { }
      public override void ShadowingOverridenMethod () { }

      public virtual void VirtualMethodShadowingBaseMethod () { }
      public virtual void NonVirtualMethodShadowingVirtualMethod () { }
    }

    // ReSharper disable ClassWithVirtualMembersNeverInherited.Local
    private class DomainType : DomainTypeBase
    // ReSharper restore ClassWithVirtualMembersNeverInherited.Local
    {
      public virtual void DerivedTypeMethod () { }
      protected virtual void ProtectedDerivedTypeMethod () { }

      public void NonVirtualMethod () { }

      public override void OverridingMethod () { }
      public override void OverridingOverriddenMethod () { }
      public new virtual void ShadowingOverridenMethod () { }

      public new virtual void VirtualMethodShadowingBaseMethod () { }
      public new void NonVirtualMethodShadowingVirtualMethod () { }
    }
    // ReSharper restore VirtualMemberNeverOverriden.Global
    // ReSharper restore UnusedMember.Local
  }
}