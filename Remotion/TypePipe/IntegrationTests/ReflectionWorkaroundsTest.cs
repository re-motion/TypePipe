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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Utilities;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ReflectionWorkaroundsTest : TypeAssemblerIntegrationTestBase
  {
    // https://connect.microsoft.com/VisualStudio/feedback/details/757478/overriding-a-propertys-accessor-via-a-methodimpl-can-cause-the-property-to-disappear-from-reflection
    [Test]
    public void PreventDisappearanceOfPropertyWhenModifying ()
    {
      ModifyOrOverrideProperty (typeof (DomainType), "Property");
    }

    [Test]
    public void PreventDisappearanceOfPropertyWhenImplicitlyOverriding ()
    {
      ModifyOrOverrideProperty (typeof (DomainTypeBase), "PropertyInBaseType");
    }

    [Test]
    public void PreventDisappearanceOfPropertyWhenExplicitlyOverriding ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      var comparer = MemberInfoEqualityComparer<PropertyInfo>.Instance;
      Assert.That (comparer.Equals (typeof (DomainType).GetProperty ("Property"), property), Is.True);
      Assert.That (typeof (DomainType).GetProperties().Single(p => p.Name == "Property"), Is.EqualTo (property));

      // DomainTypeBase.Property cannot be accessed via reflection, therefore we do not need to align the visibility of explicit overrides.
    }

    private void ModifyOrOverrideProperty (Type declaringType, string propertyName)
    {
      var property = declaringType.GetProperty (propertyName);
      Assert.That (property, Is.Not.Null);

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableGetter = mutableType.GetOrAddMutableMethod (property.GetGetMethod());
            mutableGetter.SetBody (ctx => Expression.Constant (""));
          });

      var comparer = MemberInfoEqualityComparer<PropertyInfo>.Instance;
      Assert.That (comparer.Equals (type.GetProperty (property.Name), property), Is.True);
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