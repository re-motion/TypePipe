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

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore("TODO 5072")]
  public class TypePipeCustomAttributeDataWithInheritanceTest
  {
    [Test]
    public void InheritedAttributes ()
    {
      var type = typeof (DerivedClass);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DerivedClass obj) => obj.OverriddenMethod());
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedClass obj) => obj.OverriddenProperty);
      var @event = type.GetEvents().Single();

      CheckAttributeDataInheritance (type);
      CheckAttributeDataInheritance (method);
      CheckAttributeDataInheritance (property);
      CheckAttributeDataInheritance (@event);
    }

    [Test]
    public void InheritedAttributes_WithAllowMultipleTrue ()
    {
      // Methods only: AllowMultiple true
      // Attributes on derived and base should both be included
    }

    [Test]
    public void InheritedAttributes_WithAllowMultipleFalse ()
    {
      // Methods only: AllowMultiple false
      // Attributes on derived hide attributes on base
      // When no attributes on derived, base attributes are returned
    }

    private void CheckAttributeDataInheritance (MemberInfo member)
    {
      var inheritableType = typeof (InheritableAttribute);
      var nonInheritableType = typeof (NonInheritableAttribute);

      var customAttributesWithoutInheritance = TypePipeCustomAttributeData.GetCustomAttributes (member, false).ToArray();
      Assert.That (customAttributesWithoutInheritance, Is.Empty);

      var customAttributesWithInheritance = TypePipeCustomAttributeData.GetCustomAttributes (member, true).ToArray();
      Assert.That (customAttributesWithInheritance, Is.Not.Empty);
      Assert.That (customAttributesWithInheritance, Has.Some.Matches<ICustomAttributeData> (d => d.Constructor.DeclaringType == inheritableType));
      Assert.That (customAttributesWithInheritance, Has.None.Matches<ICustomAttributeData> (d => d.Constructor.DeclaringType == nonInheritableType));
    }

    [Inheritable, NonInheritable]
    class BaseClass
    {
      [Inheritable, NonInheritable]
      public virtual void OverriddenMethod () { }

      [Inheritable, NonInheritable]
      public virtual string OverriddenProperty { get; set; }

      [Inheritable, NonInheritable]
// ReSharper disable EventNeverInvoked.Global
      public virtual event EventHandler OverriddenEvent;
// ReSharper restore EventNeverInvoked.Global
    }

    class DerivedClass : BaseClass
    {
      public override void OverriddenMethod () { }
      public override string OverriddenProperty { get; set; }
      public override event EventHandler OverriddenEvent;
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true)]
    public sealed class InheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = false)]
    public sealed class NonInheritableAttribute : Attribute { }

  }
}