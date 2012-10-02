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
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypePipeCustomAttributeDataWithInheritanceTest
  {
    [Test]
    public void GetCustomAttributes_Inheritance ()
    {
      var type = typeof (NonNestedDomainType);
      var nestedType = typeof (DomainType);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      var @event = typeof (DomainType).GetEvents().Single();

      CheckSimpleAttributeDataInheritance (type);
      CheckSimpleAttributeDataInheritance (nestedType);
      CheckSimpleAttributeDataInheritance (method);
      CheckSimpleAttributeDataInheritance (property);
      CheckSimpleAttributeDataInheritance (@event);
    }

    [Test]
    public void GetCustomAttributes_AttributesOnOriginalMemberAreNotFiltered ()
    {
      var member = NormalizingMemberInfoFromExpressionUtility.GetMember ((DomainType obj) => obj.MethodOnDomainType());
      var customAttributes = TypePipeCustomAttributeData.GetCustomAttributes (member, true);

      var customAttributeTypes = customAttributes.Select (a => a.Constructor.DeclaringType).ToArray();
      Assert.That (customAttributeTypes, Is.EquivalentTo (new[] { typeof (InheritableAttribute), typeof (NonInheritableAttribute) }));
    }

    private void CheckSimpleAttributeDataInheritance (MemberInfo member)
    {
      var customAttributesWithoutInheritance = TypePipeCustomAttributeData.GetCustomAttributes (member, false).ToArray ();
      Assert.That (customAttributesWithoutInheritance, Is.Empty);

      var customAttributesWithInheritance = TypePipeCustomAttributeData.GetCustomAttributes (member, true).ToArray ();
      Assert.That (customAttributesWithInheritance, Is.Not.Empty);

      var customAttributeTypesWithInheritance = customAttributesWithInheritance.Select (d => d.Constructor.DeclaringType).ToArray ();
      Assert.That (customAttributeTypesWithInheritance, Is.EqualTo (new[] { typeof (InheritableAttribute) }));
    }

    [Inheritable, NonInheritable]
    public class BaseType
    {
      [Inheritable, NonInheritable]
      public virtual void Method () { }

      [Inheritable, NonInheritable]
      public virtual int Property { get; set; }

      [Inheritable, NonInheritable]
      public virtual event EventHandler Event;
    }

    public class DomainType : BaseType
    {
      public override void Method () { }
      public override int Property { get; set; }
      public override event EventHandler Event;

      [Inheritable, NonInheritable]
      public void MethodOnDomainType () { }
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

  internal class NonNestedDomainType : TypePipeCustomAttributeDataWithInheritanceTest.BaseType { }
}