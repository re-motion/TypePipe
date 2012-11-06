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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection;

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

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DerivedTypeMethod());
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
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DerivedTypeMethod ());
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

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_NonVirtualMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualMethod ());
      var result = _finder.GetMostDerivedVirtualMethod (method.Name, _methodSignature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMostDerivedVirtualMethod_OverridingMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("OverridingMethod", _methodSignature, _typeToStartSearch);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod((DomainType obj) => obj.OverridingMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_VirtualMethodShadowingBaseMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("VirtualMethodShadowingBaseMethod", _methodSignature, _typeToStartSearch);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethodShadowingBaseMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedVirtualMethod_NonVirtualMethodShadowingBaseMethod ()
    {
      var result = _finder.GetMostDerivedVirtualMethod ("NonVirtualMethodShadowingBaseMethod", _methodSignature, _typeToStartSearch);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.NonVirtualMethodShadowingBaseMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedOverride_BaseTypeMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      Assert.That (result, Is.SameAs (baseDefinition));
    }

    [Test]
    public void GetMostDerivedOverride_VirtualMethodShadowingBaseMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.VirtualMethodShadowingBaseMethod ());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      Assert.That (result, Is.SameAs (baseDefinition));
    }

    [Test]
    public void GetMostDerivedOverride_OverridingMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingMethod());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedOverride_OverridingOverriddenMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.OverridingOverriddenMethod ());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingOverriddenMethod ());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetMostDerivedOverride_ShadowingOverridenMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.ShadowingOverridenMethod ());

      var result = _finder.GetMostDerivedOverride (baseDefinition, _typeToStartSearch);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.ShadowingOverridenMethod ());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_DerivedTypeMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.DerivedTypeMethod ());
      Assert.That (method.IsVirtual, Is.True);
      // Note: This method is also a NewSlot method because C# won't allow us to define a virtual reuseslot method without an overridden base method.
      // Since we implement via GetBaseDefinition(), our implementation doesn't depend on that nuance anyway.

      var result = _finder.GetBaseMethod (method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_VirtualMethodShadowingBaseMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethodShadowingBaseMethod ());
      Assert.That (method.IsVirtual, Is.True);
      Assert.That (MethodAttributesExtensions.IsSet (method.Attributes, MethodAttributes.NewSlot) , Is.True);
      Assert.That (typeof (DomainTypeBase).GetMethod ("VirtualMethodShadowingBaseMethod", Type.EmptyTypes), Is.Not.Null);

      var result = _finder.GetBaseMethod (method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_OverridingMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingMethod());

      var result = _finder.GetBaseMethod (method);

      var expectedMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingMethod ());
      Assert.That (result, Is.EqualTo (expectedMethod));
    }

    [Test]
    public void GetBaseMethod_OverridingOverriddenMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingOverriddenMethod());

      var result = _finder.GetBaseMethod (method);

      var expectedMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingOverriddenMethod ());
      Assert.That (result, Is.EqualTo (expectedMethod));
    }

    [Test]
    public void GetBaseMethod_VirtualMethod_FromObject ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ToString());

      var result = _finder.GetBaseMethod (method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void IsShadowed_BaseTypeMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.False);
    }

    [Test]
    public void IsShadowed_BaseTypeMethod_NotShadowedByItself ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod ());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainTypeBase));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.False);
    }

    [Test]
    public void IsShadowed_OverridingMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingMethod ());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));      

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.False);
    }

    [Test]
    public void IsShadowed_VirtualMethodShadowingBaseMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.VirtualMethodShadowingBaseMethod ());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsShadowed_NonVirtualMethodShadowingBaseMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.NonVirtualMethodShadowingBaseMethod());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsShadowed_ShadowingOverridenMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.ShadowingOverridenMethod ());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsShadowed_OverridingShadowingMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.OverridingShadowingMethod ());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsShadowed_VirtualMethodShadowingBaseMethod_NonMatchingSignature ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.VirtualMethodShadowingBaseMethod (7));
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.False);
    }

    [Test]
    public void IsShadowed_UnrelatedMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnrelatedType obj) => obj.UnrelatedMethod());
      var shadowingCandidates = GetDeclaredMethods (typeof (DomainType));

      var unrelatedMethod = shadowingCandidates.Single (m => m.Name == "UnrelatedMethod");
      Assert.That (MethodSignature.AreEqual(baseDefinition, unrelatedMethod), Is.True);

      var result = _finder.IsShadowed (baseDefinition, shadowingCandidates);

      Assert.That (result, Is.False);
    }

    [Test]
    public void GetOverride_VirtualMethodShadowingBaseMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.VirtualMethodShadowingBaseMethod ());
      var overrideCandidates = GetDeclaredMutableMethods (typeof (DomainType));

      var result = _finder.GetOverride (baseDefinition, overrideCandidates);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetOverride_OverridingMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.OverridingMethod());
      var overrideCandidates = GetDeclaredMutableMethods (typeof (DomainType));

      var result = _finder.GetOverride (baseDefinition, overrideCandidates);

      var expected = overrideCandidates.Single (m => m.Name == "OverridingMethod");
      Assert.That (result, Is.SameAs (expected));
    }

    [Test]
    public void GetOverride_OverridingShadowingMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBaseBase obj) => obj.OverridingShadowingMethod());
      var overrideCandidates = GetDeclaredMutableMethods (typeof (DomainType));

      var result = _finder.GetOverride (baseDefinition, overrideCandidates);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetOverride_UnrelatedMethod_ExplicitOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseTypeMethod());
      var overrideCandidates = GetDeclaredMutableMethods (typeof (DomainType));

      Assert.That (_finder.GetOverride (baseDefinition, overrideCandidates), Is.Null);
      var explicitOverride = overrideCandidates.Single (m => m.Name == "DerivedTypeMethod");
      explicitOverride.AddExplicitBaseDefinition (baseDefinition);

      var result = _finder.GetOverride (baseDefinition, overrideCandidates);

      Assert.That (result, Is.SameAs (explicitOverride));
    }

    private MethodInfo[] GetDeclaredMethods (Type type)
    {
      return type.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    private MutableMethodInfo[]  GetDeclaredMutableMethods (Type type)
    {
      return GetDeclaredMethods (type).Select (MutableMethodInfoObjectMother.CreateForExisting).ToArray();
    }

    // ReSharper disable UnusedMember.Local
    // ReSharper disable VirtualMemberNeverOverriden.Global
    private class DomainTypeBaseBase
    {
      public virtual void OverridingOverriddenMethod() { }
      public virtual void ShadowingOverridenMethod () { }
      public virtual void OverridingShadowingMethod () { }
    }

    private class DomainTypeBase : DomainTypeBaseBase
    {
      public virtual void BaseTypeMethod () { }

      public virtual void OverridingMethod () { }
      public override void OverridingOverriddenMethod () { }
      public override void ShadowingOverridenMethod () { }
      public new virtual void OverridingShadowingMethod () { }

      public virtual void VirtualMethodShadowingBaseMethod () { }
      // This method lies, it is not shadowed (but we need a method with equal name)
      public virtual void VirtualMethodShadowingBaseMethod (int i) { Dev.Null = i; }
      public virtual void NonVirtualMethodShadowingBaseMethod () { }
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
      public override void OverridingShadowingMethod () { }

      public new virtual void VirtualMethodShadowingBaseMethod () { }
      public new void NonVirtualMethodShadowingBaseMethod () { }
      
      public virtual void UnrelatedMethod () { }
    }

    private class UnrelatedType
    {
      public virtual void UnrelatedMethod () { }
    }

    // ReSharper restore VirtualMemberNeverOverriden.Global
    // ReSharper restore UnusedMember.Local
  }
}