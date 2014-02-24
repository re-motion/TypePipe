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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class RelatedPropertyFinderTest
  {
    private RelatedPropertyFinder _finder;

    [SetUp]
    public void SetUp ()
    {
      _finder = new RelatedPropertyFinder ();
    }

    [Test]
    public void GetBaseProperty_NoBaseMethod ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseBaseType obj) => obj.OverridingProperty);

      var result = _finder.GetBaseProperty (property);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseProperty_Overridden ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.OverridingProperty);

      var result = _finder.GetBaseProperty(property);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseType obj) => obj.OverridingProperty);
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseProperty_Shadowed ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.ShadowingProperty);

      var result = _finder.GetBaseProperty (property);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseProperty_SkippingMiddleClass ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.OverridingPropertySkippingMiddleClass);

      var result = _finder.GetBaseProperty (property);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseBaseType obj) => obj.OverridingPropertySkippingMiddleClass);
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseProperty_NonPublic ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.NonPublicOverridingProperty);

      var result = _finder.GetBaseProperty (property);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseType obj) => obj.NonPublicOverridingProperty);
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseProperty_NoGetter ()
    {
      var property = typeof (DomainType).GetProperty ("OnlySetterOverridingProperty");

      var result = _finder.GetBaseProperty (property);

      var expected = typeof (BaseType).GetProperty ("OnlySetterOverridingProperty");
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseProperty_NoGetter_NonPublicSetter ()
    {
      var property = typeof (DomainType).GetProperty ("OnlyNonPublicSetterOverridingProperty", BindingFlags.NonPublic | BindingFlags.Instance);

      var result = _finder.GetBaseProperty (property);

      var expected = typeof (BaseType).GetProperty ("OnlyNonPublicSetterOverridingProperty", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (result, Is.EqualTo (expected));
    }

    public class BaseBaseType
    {
      public virtual string OverridingProperty { get; set; }
      public virtual string OverridingPropertySkippingMiddleClass { get; set; }
    }

    public class BaseType : BaseBaseType
    {
      public override string OverridingProperty { get; set; }
      public virtual string ShadowingProperty { get; set; }
      
      internal virtual string NonPublicOverridingProperty { get; set; }
      public virtual string OnlySetterOverridingProperty { set { Dev.Null = value; } }
      internal virtual string OnlyNonPublicSetterOverridingProperty { set { Dev.Null = value; } } 
    }

    public class DomainType : BaseType
    {
      public override string OverridingProperty { get; set; }
      public new virtual string ShadowingProperty { get; set; }
      public override string OverridingPropertySkippingMiddleClass { get; set; }

      internal override string NonPublicOverridingProperty { get; set; }
      public override string OnlySetterOverridingProperty { set { } }
      internal override string OnlyNonPublicSetterOverridingProperty { set { } }
    }
  }
}