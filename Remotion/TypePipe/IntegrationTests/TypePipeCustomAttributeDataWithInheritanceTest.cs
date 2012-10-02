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
    public void InheritedAttributes_WithAllowMultipleFiltering_AttributesOnBaseAndDerived ()
    {
      var member = NormalizingMemberInfoFromExpressionUtility.GetMember ((DerivedClass obj) => obj.OverriddenMethodWithAttributesOnBaseAndDerived ());

      var attributes = TypePipeCustomAttributeData.GetCustomAttributes (member, true);

      var attributeTypesAndCtorArgs = attributes
          .Select (d => new { Type = d.Constructor.DeclaringType, Arg = (string) d.ConstructorArguments.Single () })
          .ToArray ();
      var expectedAttributeTypesAndCtorArgs = 
          new[] 
          {
              new { Type = typeof (InheritableNonMultipleAttribute), Arg = "derived" },
              new { Type = typeof (InheritableAllowMultipleAttribute), Arg = "base" },
              new { Type = typeof (InheritableAllowMultipleAttribute), Arg = "derived" }
          };
      Assert.That (attributeTypesAndCtorArgs, Is.EquivalentTo (expectedAttributeTypesAndCtorArgs));
    }

    [Test]
    public void InheritedAttributes_WithAllowMultipleFiltering_AttributesOnBaseOnly ()
    {
      var member = NormalizingMemberInfoFromExpressionUtility.GetMember ((DerivedClass obj) => obj.OverriddenMethodWithAttributesOnBaseOnly());
      
      var attributes = TypePipeCustomAttributeData.GetCustomAttributes (member, true);
      
      var attributeTypes = attributes.Select (d => d.Constructor.DeclaringType).ToArray();
      Assert.That (attributeTypes, Is.EquivalentTo (new[] { typeof (InheritableAllowMultipleAttribute), typeof (InheritableNonMultipleAttribute) }));
    }

    private void CheckAttributeDataInheritance (MemberInfo member)
    {
      var customAttributesWithoutInheritance = TypePipeCustomAttributeData.GetCustomAttributes (member, false).ToArray();
      Assert.That (customAttributesWithoutInheritance, Is.Empty);

      var customAttributesWithInheritance = TypePipeCustomAttributeData.GetCustomAttributes (member, true).ToArray();
      Assert.That (customAttributesWithInheritance, Is.Not.Empty);

      var customAttributeTypesWithInheritance = customAttributesWithInheritance.Select (d => d.Constructor.DeclaringType).ToArray ();
      Assert.That (customAttributeTypesWithInheritance, Is.EquivalentTo (new[] { typeof (InheritableAttribute), typeof (NonInheritableAttribute) }));
    }

    [Inheritable, NonInheritable]
    class BaseClass
    {
      [Inheritable, NonInheritable]
      public virtual void OverriddenMethod () { }

      [Inheritable, NonInheritable]
      public virtual string OverriddenProperty { get; set; }

      // ReSharper disable EventNeverInvoked.Global
      [Inheritable, NonInheritable]
      public virtual event EventHandler OverriddenEvent;
      // ReSharper restore EventNeverInvoked.Global

      [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
      public virtual void OverriddenMethodWithAttributesOnBaseAndDerived () { }

      [InheritableAllowMultiple ("base"), InheritableNonMultiple ("base")]
      public virtual void OverriddenMethodWithAttributesOnBaseOnly () { }
    }

    class DerivedClass : BaseClass
    {
      public override void OverriddenMethod () { }
      public override string OverriddenProperty { get; set; }
      public override event EventHandler OverriddenEvent;

      [InheritableAllowMultiple ("derived"), InheritableNonMultiple ("derived")]
      public override void OverriddenMethodWithAttributesOnBaseAndDerived () { }
      public override void OverriddenMethodWithAttributesOnBaseOnly () { }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true)]
    public sealed class InheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = false)]
    public sealed class NonInheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public sealed class InheritableAllowMultipleAttribute : Attribute 
    {
      public InheritableAllowMultipleAttribute (string arg) { Dev.Null = arg; }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class InheritableNonMultipleAttribute : Attribute 
    { 
      public InheritableNonMultipleAttribute (string arg) { Dev.Null = arg; }
    }
  }
}