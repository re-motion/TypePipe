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
      var declaringType = ReflectionObjectMother.GetSomeType();
      var fieldType = ReflectionObjectMother.GetSomeType ();
      var name = "_fieldName";
      var attributes = FieldAttributes.InitOnly;

      var fieldInfo = new MutableFieldInfo (declaringType, fieldType, name, attributes);
      
      Assert.That (fieldInfo.DeclaringType, Is.SameAs (declaringType));
      Assert.That (fieldInfo.FieldType, Is.SameAs (fieldType));
      Assert.That (fieldInfo.Name, Is.EqualTo (name));
      Assert.That (fieldInfo.Attributes, Is.EqualTo (attributes));
      Assert.That (fieldInfo.AddedCustomAttributeDeclarations, Is.Empty);
    }

    [Test]
    public void IsNewField ()
    {
      Assert.That (_fieldInfo.IsNewField, Is.True);
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
    public void AddCustomAttribute ()
    {
      var customAttribute = CustomAttributeDeclarationObjectMother.Create();

      _fieldInfo.AddCustomAttribute (customAttribute);

      Assert.That (_fieldInfo.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { customAttribute }));
    }

    [Test]
    public void GetCustomAttributes_WithoutFilter ()
    {
      var customAttribute = CustomAttributeDeclarationObjectMother.Create ();
      _fieldInfo.AddCustomAttribute (customAttribute);
      
      var result = _fieldInfo.GetCustomAttributes (false);
      
      Assert.That (result, Has.Length.EqualTo (1).And.Some.TypeOf (customAttribute.AttributeConstructorInfo.DeclaringType));
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
  }
}