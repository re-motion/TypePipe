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
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableFieldInfoTest
  {
    private MutableFieldInfo _field;

    [SetUp]
    public void SetUp ()
    {
      _field = MutableFieldInfoObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var descriptor = FieldDescriptorObjectMother.Create();

      var field = new MutableFieldInfo (declaringType, descriptor);

      Assert.That (field.DeclaringType, Is.SameAs (declaringType));
      Assert.That (field.FieldType, Is.SameAs (descriptor.Type));
      Assert.That (field.Name, Is.EqualTo (descriptor.Name));
      Assert.That (field.Attributes, Is.EqualTo (descriptor.Attributes));
      Assert.That (field.AddedCustomAttributes, Is.Empty);
    }

    [Test]
    public void IsNew ()
    {
      var field1 = MutableFieldInfoObjectMother.CreateForExisting();
      var field2 = MutableFieldInfoObjectMother.CreateForNew();

      Assert.That (field1.IsNew, Is.False);
      Assert.That (field2.IsNew, Is.True);
    }

    [Test]
    public void IsModified ()
    {
      Assert.That (_field.IsModified, Is.False);
      _field.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());

      Assert.That (_field.IsModified, Is.True);
    }

    [Test]
    public void CanAddCustomAttributes ()
    {
      var field1 = MutableFieldInfoObjectMother.CreateForExisting();
      var field2 = MutableFieldInfoObjectMother.CreateForNew();

      Assert.That (field1.CanAddCustomAttributes, Is.False);
      Assert.That (field2.CanAddCustomAttributes, Is.True);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      Assert.That (_field.CanAddCustomAttributes, Is.True);
      _field.AddCustomAttribute (declaration);

      Assert.That (_field.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));

      Assert.That (_field.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));

      Assert.That (_field.GetCustomAttributes (false).Single(), Is.TypeOf<ObsoleteAttribute>());
      Assert.That (_field.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_field.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_field.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public void AddCustomAttribute_NonSerialized ()
    {
      Assert.That (_field.IsNotSerialized, Is.False);

      _field.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (NonSerializedAttribute)));

      Assert.That (_field.IsNotSerialized, Is.True);
    }

    [Test]
    public new void ToString ()
    {
      var field = MutableFieldInfoObjectMother.Create (name: "_field", type: typeof (MutableFieldInfoTest));

      Assert.That (field.ToString(), Is.EqualTo ("MutableFieldInfoTest _field"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringTypeName = _field.DeclaringType.Name;
      var fieldTypeName = _field.FieldType.Name;
      var fieldName = _field.Name;
      var expected = "MutableField = \"" + fieldTypeName + " " + fieldName + "\", DeclaringType = \"" + declaringTypeName + "\"";

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