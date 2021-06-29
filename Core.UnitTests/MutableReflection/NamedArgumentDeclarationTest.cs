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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class NamedArgumentDeclarationTest
  {
    [Test]
    public void Initialization_Property ()
    {
      var propertyInfo = CustomAttributeReflectionObjectMother.GetPropertyWithType (typeof (ValueType));
      int value = 7;

      var declaration = new NamedArgumentDeclaration (propertyInfo, value);

      Assert.That (declaration.MemberInfo, Is.SameAs (propertyInfo));
      Assert.That (declaration.MemberType, Is.SameAs (typeof(ValueType)));
      Assert.That (declaration.Value, Is.EqualTo (value));
    }

    [Test]
    public void Initialization_Property_ValueNotAssignable ()
    {
      var propertyInfo = CustomAttributeReflectionObjectMother.GetPropertyWithType (typeof (ValueType));
      string value = "not assignable";
      Assert.That (
          () => new NamedArgumentDeclaration (propertyInfo, value),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Parameter 'value' has type 'System.String' when type 'System.ValueType' was expected.", "value"));
    }

    [Test]
    public void Initialization_Property_ValueAssignable ()
    {
      var propertyInfo = CustomAttributeReflectionObjectMother.GetPropertyWithType (typeof (ValueType));
      int value = 7;

      new NamedArgumentDeclaration (propertyInfo, value);
    }

    [Test]
    public void Initialization_Property_WithNullArgument ()
    {
      var nullableMember2 = CustomAttributeReflectionObjectMother.GetPropertyWithType (typeof (object));
      var nullableMember1 = CustomAttributeReflectionObjectMother.GetPropertyWithType (typeof (int?));

      new NamedArgumentDeclaration (nullableMember1, null);
      new NamedArgumentDeclaration (nullableMember2, null);
    }

    [Test]
    public void Initialization_Property_WithInvalidNullArgument ()
    {
      var nonNullMember = CustomAttributeReflectionObjectMother.GetPropertyWithType (typeof (int));
      Assert.That (
          () => new NamedArgumentDeclaration (nonNullMember, null),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Parameter 'value' has type '<null>' when type 'System.Int32' was expected.", "value"));
    }

    [Test]
    public void Initialization_Property_MustBeWritable ()
    {
      var property = typeof (string).GetProperty ("Length");
      Assert.That (
          () => new NamedArgumentDeclaration (property, 0),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Property 'Length' has no public setter.", "propertyInfo"));
    }

    [Test]
    public void Initialization_Property_MustBePublic ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty (() => PrivateProperty);
      Assert.That (
          () => new NamedArgumentDeclaration (property, ""),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Property 'PrivateProperty' has no public setter.", "propertyInfo"));
    }

    [Test]
    public void Initialization_Property_MustNotBeStatic ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty (() => StaticProperty);
      Assert.That (
          () => new NamedArgumentDeclaration (property, ""),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Property 'StaticProperty' is not an instance property.", "propertyInfo"));
    }

    [Test]
    public void Initialization_Field ()
    {
      var fieldInfo = CustomAttributeReflectionObjectMother.GetFieldWithType (typeof (ValueType));
      int value = 7;

      var declaration = new NamedArgumentDeclaration (fieldInfo, value);

      Assert.That (declaration.MemberInfo, Is.SameAs (fieldInfo));
      Assert.That (declaration.MemberType, Is.SameAs (typeof (ValueType)));
      Assert.That (declaration.Value, Is.EqualTo (value));
    }

    [Test]
    public void Initialization_Field_ValueNotAssignable ()
    {
      var propertyInfo = CustomAttributeReflectionObjectMother.GetFieldWithType (typeof (ValueType));
      string value = "not assignable";
      Assert.That (
          () => new NamedArgumentDeclaration (propertyInfo, value),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Parameter 'value' has type 'System.String' when type 'System.ValueType' was expected.", "value"));
    }

    [Test]
    public void Initialization_Field_ValueAssignable ()
    {
      var propertyInfo = CustomAttributeReflectionObjectMother.GetFieldWithType (typeof (ValueType));
      int value = 7;

      new NamedArgumentDeclaration (propertyInfo, value);
    }

    [Test]
    public void Initialization_Field_WithNullArgument ()
    {
      var nullableMember2 = CustomAttributeReflectionObjectMother.GetFieldWithType (typeof (object));
      var nullableMember1 = CustomAttributeReflectionObjectMother.GetFieldWithType (typeof (int?));

      new NamedArgumentDeclaration (nullableMember1, null);
      new NamedArgumentDeclaration (nullableMember2, null);
    }

    [Test]
    public void Initialization_Field_WithInvalidNullArgument ()
    {
      var nonNullMember = CustomAttributeReflectionObjectMother.GetFieldWithType (typeof (int));
      Assert.That (
          () => new NamedArgumentDeclaration (nonNullMember, null),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Parameter 'value' has type '<null>' when type 'System.Int32' was expected.", "value"));
    }

    [Test]
    public void Initialization_Field_MustBeWritable_Readonly ()
    {
      var readonlyField = typeof (Type).GetField ("EmptyTypes");
      Assert.That (
          () => new NamedArgumentDeclaration (readonlyField, Type.EmptyTypes),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Field 'EmptyTypes' is not writable.", "fieldInfo"));
    }

    [Test]
    public void Initialization_Field_MustBeWritable_Literal ()
    {
      var literalField = GetType ().GetField ("c_string");
      Assert.That (
          () => new NamedArgumentDeclaration (literalField, "value"),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Field 'c_string' is not writable.", "fieldInfo"));
    }

    [Test]
    public void Initialization_Fied_MustBePublic ()
    {
      var privateField = NormalizingMemberInfoFromExpressionUtility.GetField (() => _privateFied);
      Assert.That (
          () => new NamedArgumentDeclaration (privateField, ""),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Field '_privateFied' is not public.", "fieldInfo"));
    }

    [Test]
    public void Initialization_Field_MustNotBeStatic ()
    {
      var staticField = NormalizingMemberInfoFromExpressionUtility.GetField (() => StaticField);
      Assert.That (
          () => new NamedArgumentDeclaration (staticField, ""),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Field 'StaticField' is not an instance field.", "fieldInfo"));
    }

    public const string c_string = "string";
// ReSharper disable UnusedAutoPropertyAccessor.Local
    private string PrivateProperty { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local
    public static string StaticProperty { get; set; }
    private string _privateFied = "string";
    public static string StaticField = "string";
  }
}