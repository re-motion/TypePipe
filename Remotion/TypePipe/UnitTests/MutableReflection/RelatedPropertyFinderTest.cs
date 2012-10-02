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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
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
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseBaseType obj) => obj.OveriddenOverridingProperty);

      var result = _finder.GetBaseProperty (property);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetBaseProperty_Overridden ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.OveriddenOverridingProperty);

      var result = _finder.GetBaseProperty(property);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseType obj) => obj.OveriddenOverridingProperty);
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
    public void GetBaseProperty_NonPublic ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.ProtectedOverridingProperty);

      var result = _finder.GetBaseProperty (property);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseType obj) => obj.ProtectedOverridingProperty);
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void GetBaseProperty_NoGetter ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.OnlySetterOverridingProperty);

      var result = _finder.GetBaseProperty (property);

      var expected = NormalizingMemberInfoFromExpressionUtility.GetProperty ((BaseType obj) => obj.OnlySetterOverridingProperty);
      Assert.That (result, Is.EqualTo (expected));
    }

    private class BaseBaseType
    {
      public virtual string OveriddenOverridingProperty { get; set; }
    }

    private class BaseType : BaseBaseType
    {
      public override string OveriddenOverridingProperty { get; set; }
      public virtual string ShadowingProperty { get; set; }
      protected internal virtual string ProtectedOverridingProperty { get; set; }
      public virtual string OnlySetterOverridingProperty { get; set; }
      }

    private class DomainType : BaseType
    {
      public override string OveriddenOverridingProperty { get; set; }
      public new virtual string ShadowingProperty { get; set; }
      protected internal override string ProtectedOverridingProperty { get; set; }
      public override string OnlySetterOverridingProperty { set { } }
    }
  }
}