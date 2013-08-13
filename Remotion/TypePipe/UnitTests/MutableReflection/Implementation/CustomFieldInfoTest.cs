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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomFieldInfoTest
  {
    private CustomType _declaringType;
    private string _name;
    private Type _type;
    private FieldAttributes _attributes;

    private TestableCustomFieldInfo _field;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create();
      _name = "abc";
      _type = ReflectionObjectMother.GetSomeType ();
      _attributes = (FieldAttributes) 7;

      _field = new TestableCustomFieldInfo (_declaringType, _name, _type, _attributes);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_field.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_field.Name, Is.EqualTo (_name));
      Assert.That (_field.FieldType, Is.SameAs (_type));
      Assert.That (_field.Attributes, Is.EqualTo (_attributes));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      _field.CustomAttributeDatas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };

      Assert.That (_field.GetCustomAttributes (false).Select (a => a.GetType ()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (_field.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_field.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_field.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_field.ToString(), Is.EqualTo (_type.Name + " abc"));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString is defined in CustomFieldInfo base class.
      var declaringTypeName = _field.DeclaringType.Name;
      var fieldTypeName = _field.FieldType.Name;
      var fieldName = _field.Name;
      var expected = "TestableCustomField = \"" + fieldTypeName + " " + fieldName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (_field.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => _field.FieldHandle, "FieldHandle");
      UnsupportedMemberTestHelper.CheckProperty (() => _field.ReflectedType, "ReflectedType");

      UnsupportedMemberTestHelper.CheckMethod (() => _field.GetValue (null), "GetValue");
      UnsupportedMemberTestHelper.CheckMethod (() => _field.SetValue (null, null), "SetValue");
    }
  }
}