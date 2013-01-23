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
using Microsoft.Scripting.Ast;
using NUnit.Framework;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ReflectionWorkaroundsTest : TypeAssemblerIntegrationTestBase
  {
    // https://connect.microsoft.com/VisualStudio/feedback/details/757478/overriding-a-propertys-accessor-via-a-methodimpl-can-cause-the-property-to-disappear-from-reflection
    [Test]
    public void PreventDisappearanceOfPropertyWhenModifying ()
    {
      var modifiedType = ModifyOrOverrideProperty (typeof (DomainType), "Property");
      Assert.That (modifiedType.GetProperty ("Property"), Is.Not.Null.And.Property ("DeclaringType").SameAs (typeof (DomainType)));
    }

    [Test]
    public void PreventDisappearanceOfPropertyWhenImplicitlyOverriding ()
    {
      var modifiedType = ModifyOrOverrideProperty (typeof (DomainTypeBase), "PropertyInBaseType");
      Assert.That (modifiedType.GetProperty ("PropertyInBaseType"), Is.Not.Null.And.Property ("DeclaringType").SameAs (typeof (DomainTypeBase)));
    }

    [Test]
    public void PreventDisappearanceOfPropertyWhenExplicitlyOverriding_IsNotNecessary ()
    {
      // DomainTypeBase.Property cannot be accessed via reflection, therefore we do not need to align the visibility of explicit overrides.
      Assert.That (
          typeof (DomainType).GetProperty ("Property").DeclaringType,
          Is.SameAs (typeof (DomainType)),
          "Base property is hidden from Reflection, even without any overrides");
      Assert.That (
          typeof (DomainType).GetProperties ().Single (p => p.Name == "Property").DeclaringType,
          Is.SameAs (typeof (DomainType)),
          "Base property is hidden from Reflection, even without any overrides");

      // Even after adding an explicit override for DomainTypeBase.Property, GetProperty still returns DomainType.Property
      var modifiedType = ModifyOrOverrideProperty (typeof (DomainTypeBase), "Property");

      Assert.That (
          modifiedType.GetProperty ("Property").DeclaringType,
          Is.SameAs (typeof (DomainType)),
          "Base property is still hidden from Reflection");
      Assert.That (
          modifiedType.GetProperties ().Single (p => p.Name == "Property").DeclaringType,
          Is.SameAs (typeof (DomainType)),
          "Base property is still hidden from Reflection");
    }

    private Type ModifyOrOverrideProperty (Type declaringType, string propertyName)
    {
      var property = declaringType.GetProperty (propertyName);
      Assert.That (property, Is.Not.Null);

      return AssembleType<DomainType> (
          proxyType =>
          {
            var mutableGetter = proxyType.GetOrAddOverride (property.GetGetMethod());
            mutableGetter.SetBody (ctx => Expression.Constant (""));
          });
    }

    public class DomainTypeBase
    {
      public virtual string PropertyInBaseType
      {
        get { return ""; }
      }

      public virtual string Property
      {
        get { return ""; }
      }
    }

    public class DomainType : DomainTypeBase
    {
      public new virtual string Property
      {
        get { return ""; }
      }
    }
  }
}