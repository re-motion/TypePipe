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
    public void GetBaseMethod_DerivedTypeMethod ()
    {
      var result = _finder.GetBaseMethod ("DerivedTypeMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((DomainType obj) => obj.DerivedTypeMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_DerivedTypeMethod_NonMatchingName ()
    {
      var result = _finder.GetBaseMethod ("DoesNotExist", _methodSignature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_DerivedTypeMethod_NonMatchingSignature ()
    {
      var signature = new MethodSignature (typeof (int), Type.EmptyTypes, 0);
      Assert.That (signature, Is.Not.EqualTo (_methodSignature));
      var result = _finder.GetBaseMethod ("DerivedTypeMethod", signature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_ProtectedDerivedTypeMethod ()
    {
      var result = _finder.GetBaseMethod ("ProtectedDerivedTypeMethod", _methodSignature, _typeToStartSearch);

      var expected = typeof (DomainType).GetMethod ("ProtectedDerivedTypeMethod", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (expected, Is.Not.Null);
      Assert.That (expected.IsPublic, Is.False);

      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_BaseTypeMethod ()
    {
      var result = _finder.GetBaseMethod ("BaseTypeMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((DomainTypeBase obj) => obj.BaseTypeMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_NonVirtualMethod ()
    {
      var result = _finder.GetBaseMethod ("NonVirtualMethod", _methodSignature, _typeToStartSearch);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseMethod_OverridingMethod ()
    {
      var result = _finder.GetBaseMethod ("OverridingMethod", _methodSignature, _typeToStartSearch);

      var expected = typeof (DomainType).GetMethod ("OverridingMethod");
      Assert.That (expected, Is.Not.Null);

      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_NewMethod ()
    {
      var result = _finder.GetBaseMethod ("NewMethod", _methodSignature, _typeToStartSearch);

      var expected = typeof (DomainType).GetMethod ("NewMethod");
      Assert.That (expected, Is.Not.Null);

      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseMethod_NewMethodShadowingVirtualMethod ()
    {
      var result = _finder.GetBaseMethod ("NewMethodShadowingVirtualMethod", _methodSignature, _typeToStartSearch);

      var expected = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((DomainTypeBase obj) => obj.NewMethodShadowingVirtualMethod());
      Assert.That (result, Is.EqualTo (expected));
    }

// ReSharper disable UnusedMember.Local
    private class DomainTypeBase
    {
      public virtual void BaseTypeMethod () { }
      public virtual void OverridingMethod () { }
      public virtual void NewMethod () { }
      public virtual void NewMethodShadowingVirtualMethod () { }      
    }

    private class DomainType : DomainTypeBase
    {
      public virtual void DerivedTypeMethod () { }
      protected virtual void ProtectedDerivedTypeMethod () { }
      public void NonVirtualMethod () { }
      public override void OverridingMethod () { }
      public new virtual void NewMethod () { }
      public new void NewMethodShadowingVirtualMethod () { }
    }
// ReSharper restore UnusedMember.Local
  }
}