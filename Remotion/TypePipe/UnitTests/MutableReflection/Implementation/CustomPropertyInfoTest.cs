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
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomPropertyInfoTest
  {
    [Test]
    public void Initialization ()
    {
      var declaringType = CustomTypeObjectMother.Create();
      var name = "Property";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (PropertyAttributes) 7;
      var getMethod = ReflectionObjectMother.GetSomeMethod();
      var setMethod = ReflectionObjectMother.GetSomeMethod();
      var indexParameters = new[] { ReflectionObjectMother.GetSomeParameter(), ReflectionObjectMother.GetSomeParameter() };

      var property = new TestableCustomPropertyInfo (declaringType, name, type, attributes, getMethod, setMethod, indexParameters);

      Assert.That (property.Attributes, Is.EqualTo (attributes));
      Assert.That (property.DeclaringType, Is.EqualTo (declaringType));
      Assert.That (property.Name, Is.EqualTo (name));
      Assert.That (property.GetGetMethod(), Is.SameAs (getMethod));
      Assert.That (property.GetSetMethod(), Is.SameAs (setMethod));
      Assert.That (property.GetIndexParameters(), Is.EqualTo (indexParameters));
    }

    [Test]
    public void GetGetMethod ()
    {
      var nonPublicMethod = ReflectionObjectMother.GetSomeNonPublicMethod();
      var publicMethod = ReflectionObjectMother.GetSomePublicMethod();
      var property1 = CustomPropertyInfoObjectMother.Create (getMethod: nonPublicMethod);
      var property2 = CustomPropertyInfoObjectMother.Create (getMethod: publicMethod);

      Assert.That (property1.GetGetMethod (true), Is.SameAs (nonPublicMethod));
      Assert.That (property1.GetGetMethod (false), Is.Null);
      Assert.That (property2.GetGetMethod (true), Is.SameAs (publicMethod));
      Assert.That (property2.GetGetMethod (false), Is.SameAs (publicMethod));
    }

    [Test]
    public void GetSetMethod ()
    {
      var nonPublicMethod = ReflectionObjectMother.GetSomeNonPublicMethod();
      var publicMethod = ReflectionObjectMother.GetSomePublicMethod();
      var property1 = CustomPropertyInfoObjectMother.Create (setMethod: nonPublicMethod);
      var property2 = CustomPropertyInfoObjectMother.Create (setMethod: publicMethod);

      Assert.That (property1.GetSetMethod (true), Is.SameAs (nonPublicMethod));
      Assert.That (property1.GetSetMethod (false), Is.Null);
      Assert.That (property2.GetSetMethod (true), Is.SameAs (publicMethod));
      Assert.That (property2.GetSetMethod (false), Is.SameAs (publicMethod));
    }

    [Test]
    public void GetAccessors ()
    {
      var getMethod = ReflectionObjectMother.GetSomePublicMethod();
      var setMethod = ReflectionObjectMother.GetSomeNonPublicMethod();
      var property = CustomPropertyInfoObjectMother.Create (getMethod: getMethod, setMethod: setMethod);

      Assert.That (property.GetAccessors (true), Is.EquivalentTo (new[] { getMethod, setMethod }));
      Assert.That (property.GetAccessors (false), Is.EquivalentTo (new[] { getMethod }));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var customAttribute = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      var property = CustomPropertyInfoObjectMother.Create (customAttributes: new[] { customAttribute });

      Assert.That (property.GetCustomAttributes (false).Select (a => a.GetType()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (property.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (property.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (property.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var name = "MyProperty";
      var property = CustomPropertyInfoObjectMother.Create (name: name, type: type);

      Assert.That (property.ToString(), Is.EqualTo (type.Name + " MyProperty"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = CustomTypeObjectMother.Create();
      var type = ReflectionObjectMother.GetSomeType ();
      var name = "MyProperty";
      var property = CustomPropertyInfoObjectMother.Create (declaringType, name, type);

      // Note: ToDebugString is defined in CustomFieldInfo base class.
      Assertion.IsNotNull (property.DeclaringType);
      var declaringTypeName = property.DeclaringType.Name;
      var propertyTypeName = property.PropertyType.Name;
      var propertyName = property.Name;
      var expected = "TestableCustomProperty = \"" + propertyTypeName + " " + propertyName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (property.ToDebugString (), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      var property = CustomPropertyInfoObjectMother.Create();

      UnsupportedMemberTestHelper.CheckProperty (() => property.ReflectedType, "ReflectedType");
      UnsupportedMemberTestHelper.CheckProperty (() => property.CanRead, "CanRead");
      UnsupportedMemberTestHelper.CheckProperty (() => property.CanWrite, "CanWrite");

      UnsupportedMemberTestHelper.CheckMethod (() => property.SetValue (null, null, null), "SetValue");
      UnsupportedMemberTestHelper.CheckMethod (() => property.GetValue (null, null), "GetValue");
    }
  }
}