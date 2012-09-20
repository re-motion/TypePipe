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

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableFieldInfoTest
  {
    private MutableFieldInfo _fieldInfo;

    [SetUp]
    public void SetUp ()
    {
      _fieldInfo = MutableFieldInfoObjectMother.Create ();
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var descriptor = UnderlyingFieldInfoDescriptor.Create (ReflectionObjectMother.GetSomeType (), "_fieldName", FieldAttributes.InitOnly);

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
      var mutableField = MutableFieldInfoObjectMother.CreateForExisting (originalField: originalField);

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
      Assert.That (_fieldInfo.IsModified, Is.False);
    }

    [Test]
    public void IsModified_True ()
    {
      _fieldInfo.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());

      Assert.That (_fieldInfo.IsModified, Is.True);
    }

    [Test]
    public new void ToString ()
    {
      var field = MutableFieldInfoObjectMother.Create (fieldType: typeof (MutableFieldInfoTest), name: "_field");

      Assert.That (field.ToString (), Is.EqualTo ("MutableFieldInfoTest _field"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringTypeName = _fieldInfo.DeclaringType.Name;
      var fieldTypeName = _fieldInfo.FieldType.Name;
      var fieldName = _fieldInfo.Name;
      var expected = "MutableField = \"" + fieldTypeName + " " + fieldName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (_fieldInfo.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => Field);
      var mutableField = MutableFieldInfoObjectMother.CreateForExisting (originalField:field);

      var result = mutableField.GetCustomAttributeData ();

      Assert.That (result.Select (a => a.Constructor.DeclaringType), Is.EquivalentTo (new[] { typeof (AbcAttribute) }));
    }

    [Test]
    public void GetCustomAttributeData_Lazy ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => Field);
      var mutableField = MutableFieldInfoObjectMother.CreateForExisting (originalField: field);

      var result1 = mutableField.GetCustomAttributeData ();
      var result2 = mutableField.GetCustomAttributeData ();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void AddCustomAttribute ()
    {
      Assert.That (_fieldInfo.IsNew, Is.True);
      var declaration = CustomAttributeDeclarationObjectMother.Create();

      _fieldInfo.AddCustomAttribute (declaration);

      Assert.That (_fieldInfo.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { declaration }));
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Adding attributes to existing fields is not supported.")]
    public void AddCustomAttribute_ThrowsForExisting ()
    {
      var fieldInfo = MutableFieldInfoObjectMother.CreateForExisting ();
      Assert.That (fieldInfo.IsNew, Is.False);

      fieldInfo.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create ());
    }

    [Test]
    public void GetCustomAttributes_WithoutFilter ()
    {
      var customAttribute = CustomAttributeDeclarationObjectMother.Create ();
      _fieldInfo.AddCustomAttribute (customAttribute);
      
      var result = _fieldInfo.GetCustomAttributes (false);
      
      Assert.That (result, Has.Length.EqualTo (1).And.Some.TypeOf (customAttribute.Constructor.DeclaringType));
    }

    [Test]
    public void GetCustomAttributes_WithFilter ()
    {
      var baseCustomAttribute = CustomAttributeDeclarationObjectMother.Create (typeof (CustomAttribute));
      var derivedCustomAttribute = CustomAttributeDeclarationObjectMother.Create (typeof (DerivedCustomAttribute));

      _fieldInfo.AddCustomAttribute (baseCustomAttribute);
      _fieldInfo.AddCustomAttribute (derivedCustomAttribute);

      var resultWithBaseFilter = _fieldInfo.GetCustomAttributes (typeof (CustomAttribute), false);
      var resultWithDerivedFilter = _fieldInfo.GetCustomAttributes (typeof (DerivedCustomAttribute), false);

      Assert.That (resultWithBaseFilter, Has.Length.EqualTo (2).And.Some.TypeOf<CustomAttribute> ().And.Some.TypeOf<DerivedCustomAttribute> ());
      Assert.That (resultWithDerivedFilter, Has.Length.EqualTo (1).And.Some.TypeOf<DerivedCustomAttribute> ());
    }

    public class CustomAttribute : Attribute
    {
    }

    public class DerivedCustomAttribute : CustomAttribute
    {
    }

    [Abc]
    public string Field;

    public class AbcAttribute : Attribute { }
  }
}