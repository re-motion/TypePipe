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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;

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
      var name = "abc";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;

      var field = new MutableFieldInfo (declaringType, name, type, attributes);

      Assert.That (field.DeclaringType, Is.SameAs (declaringType));
      Assert.That (field.MutableDeclaringType, Is.SameAs (declaringType));
      Assert.That (field.Name, Is.EqualTo (name));
      Assert.That (field.FieldType, Is.SameAs (type));
      Assert.That (field.Attributes, Is.EqualTo (attributes));
      Assert.That (field.AddedCustomAttributes, Is.Empty);
    }

    [Test]
    public void Attributes_NonSerialized ()
    {
      Assert.That (_field.IsNotSerialized, Is.False);

      _field.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create (typeof (NonSerializedAttribute)));

      Assert.That (_field.IsNotSerialized, Is.True);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _field.AddCustomAttribute (declaration);

      Assert.That (_field.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_field.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString is defined in CustomFieldInfo base class.
      var declaringTypeName = _field.DeclaringType.Name;
      var fieldTypeName = _field.FieldType.Name;
      var fieldName = _field.Name;
      var expected = "MutableField = \"" + fieldTypeName + " " + fieldName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (_field.ToDebugString(), Is.EqualTo (expected));
    }
  }
}