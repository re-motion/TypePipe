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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableFieldInfoTest
  {
    private MutableFieldInfo _field;

    private MutableFieldInfo _fieldWithAttribute;
    private bool _randomInherit;

    [SetUp]
    public void SetUp ()
    {
      _field = MutableFieldInfoObjectMother.Create();

      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => Field);
      _fieldWithAttribute = MutableFieldInfoObjectMother.CreateForExisting (underlyingField: field);
      _randomInherit = BooleanObjectMother.GetRandomBoolean ();
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var descriptor = FieldDescriptorObjectMother.Create();

      var fieldInfo = new MutableFieldInfo (declaringType, descriptor);

      Assert.That (fieldInfo.DeclaringType, Is.SameAs (declaringType));
      Assert.That (fieldInfo.FieldType, Is.SameAs (descriptor.Type));
      Assert.That (fieldInfo.Name, Is.EqualTo (descriptor.Name));
      Assert.That (fieldInfo.Attributes, Is.EqualTo (descriptor.Attributes));
      Assert.That (fieldInfo.AddedCustomAttributeDeclarations, Is.Empty);
    }

    [Test]
    public void UnderlyingSystemFieldInfo()
    {
      var originalField = ReflectionObjectMother.GetSomeField();
      var mutableField = MutableFieldInfoObjectMother.CreateForExisting (underlyingField: originalField);

      Assert.That (mutableField.UnderlyingSystemFieldInfo, Is.SameAs (originalField));
    }

    [Test]
    public void UnderlyingSystemFieldInfo_ForNull ()
    {
      var mutableField = MutableFieldInfoObjectMother.CreateForNew();

      Assert.That (mutableField.UnderlyingSystemFieldInfo, Is.SameAs (mutableField));
    }

    [Test]
    public void IsNew_True ()
    {
      var fieldInfo = MutableFieldInfoObjectMother.CreateForNew();
      Assert.That (fieldInfo.IsNew, Is.True);
    }

    [Test]
    public void IsNew_Fase ()
    {
      var fieldInfo = MutableFieldInfoObjectMother.CreateForExisting ();
      Assert.That (fieldInfo.IsNew, Is.False);
    }

    [Test]
    public void IsModified_False ()
    {
      Assert.That (_field.IsModified, Is.False);
    }

    [Test]
    public void IsModified_True ()
    {
      _field.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());

      Assert.That (_field.IsModified, Is.True);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var result = _fieldWithAttribute.GetCustomAttributeData ();

      Assert.That (result.Select (a => a.Constructor.DeclaringType), Is.EquivalentTo (new[] { typeof (DerivedAttribute) }));
      Assert.That (result, Is.SameAs (_fieldWithAttribute.GetCustomAttributeData ()), "should be cached");
    }

    [Test]
    public void AddCustomAttribute ()
    {
      Assert.That (_field.IsNew, Is.True);
      var declaration = CustomAttributeDeclarationObjectMother.Create ();

      _field.AddCustomAttribute (declaration);

      Assert.That (_field.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { declaration }));
      Assert.That (_field.GetCustomAttributeData (), Is.EqualTo (new[] { declaration }));
    }

    [Test]
    public void AddCustomAttribute_NonSerialized ()
    {
      Assert.That (_field.IsNotSerialized, Is.False);
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute());

      _field.AddCustomAttribute (new CustomAttributeDeclaration (constructor, new object[0]));

      Assert.That (_field.IsNotSerialized, Is.True);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Adding attributes to existing fields is not supported.")]
    public void AddCustomAttribute_ThrowsForExisting ()
    {
      var fieldInfo = MutableFieldInfoObjectMother.CreateForExisting ();
      Assert.That (fieldInfo.IsNew, Is.False);

      fieldInfo.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create ());
    }

    [Test]
    public void GetCustomAttributes ()
    {
      var result = _fieldWithAttribute.GetCustomAttributes (_randomInherit);

      Assert.That (result, Has.Length.EqualTo (1));
      var attribute = result.Single ();
      Assert.That (attribute, Is.TypeOf<DerivedAttribute> ());
      Assert.That (_fieldWithAttribute.GetCustomAttributes (_randomInherit).Single (), Is.Not.SameAs (attribute), "new instance");
    }

    [Test]
    public void GetCustomAttributes_Filter ()
    {
      Assert.That (_fieldWithAttribute.GetCustomAttributes (typeof (UnrelatedAttribute), _randomInherit), Is.Empty);
      Assert.That (_fieldWithAttribute.GetCustomAttributes (typeof (BaseAttribute), _randomInherit), Has.Length.EqualTo (1));
    }

    [Test]
    public void IsDefined ()
    {
      Assert.That (_fieldWithAttribute.IsDefined (typeof (UnrelatedAttribute), _randomInherit), Is.False);
      Assert.That (_fieldWithAttribute.IsDefined (typeof (BaseAttribute), _randomInherit), Is.True);
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

    [Derived]
    public string Field;

    class BaseAttribute : Attribute { }
    class DerivedAttribute : BaseAttribute { }
    class UnrelatedAttribute : Attribute { }
  }
}