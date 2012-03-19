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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class NamedAttributeArgumentDeclarationTest
  {
    [Test]
    public void Initialization_Property ()
    {
      var propertyInfo = ReflectionObjectMother.GetReadWritePropertyWithType(typeof(ValueType));
      int value = 7;

      var declaration = new NamedAttributeArgumentDeclaration (propertyInfo, value);

      Assert.That (declaration.MemberInfo, Is.SameAs (propertyInfo));
      Assert.That (declaration.Value, Is.EqualTo (value));
    }

    [Test]
    public void Initialization_Field ()
    {
      var fieldInfo = ReflectionObjectMother.GetFieldWithType (typeof (ValueType));
      int value = 7;

      var declaration = new NamedAttributeArgumentDeclaration (fieldInfo, value);

      Assert.That (declaration.MemberInfo, Is.SameAs (fieldInfo));
      Assert.That (declaration.Value, Is.EqualTo (value));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
      "Argument value has type System.String when type System.ValueType was expected.\r\nParameter name: value")]
    public void Initialization_ValueNotAssignable ()
    {
      var propertyInfo = ReflectionObjectMother.GetReadWritePropertyWithType (typeof (ValueType));
      string value = "not assignable";

      new NamedAttributeArgumentDeclaration (propertyInfo, value);
    }

    [Test]
    public void Initialization_ValueAssignable ()
    {
      var propertyInfo = ReflectionObjectMother.GetReadWritePropertyWithType (typeof (ValueType));
      int value = 7;

      new NamedAttributeArgumentDeclaration (propertyInfo, value);
    }

    [Test]
    public void Initialization_WithNullArgument ()
    {
      var nullableMember2 = ReflectionObjectMother.GetReadWritePropertyWithType (typeof (object));
      var nullableMember1 = ReflectionObjectMother.GetReadWritePropertyWithType (typeof (int?));

      new NamedAttributeArgumentDeclaration (nullableMember1, null);
      new NamedAttributeArgumentDeclaration (nullableMember2, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
      "Argument value has type <null> when type System.Int32 was expected.\r\nParameter name: value")]
    public void Initialization_WithInvalidNullArgument ()
    {
      var nonNullMember = ReflectionObjectMother.GetReadWritePropertyWithType (typeof (int));

      new NamedAttributeArgumentDeclaration (nonNullMember, null);
    }

    [Test]
    [ExpectedException (typeof(ArgumentException), ExpectedMessage =
      "Property 'DoubleReadOnlyProperty' is not writable.\r\nParameter name: propertyInfo")]
    public void Initialization_Property_MustBeWritable ()
    {
      var property = ReflectionObjectMother.GetReadonlyProperyWithType (typeof (double));

      new NamedAttributeArgumentDeclaration (property, 7.7);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Field 'EmptyTypes' is not writable.\r\nParameter name: fieldInfo")]
    public void Initialization_Field_MustBeWritable_Readonly ()
    {
      var readonlyField = typeof (Type).GetField ("EmptyTypes");

      new NamedAttributeArgumentDeclaration (readonlyField, Type.EmptyTypes);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Field 'c_string' is not writable.\r\nParameter name: fieldInfo")]
    public void Initialization_Field_MustBeWritable_Literal ()
    {
      var literalField = GetType ().GetField ("c_string");

      new NamedAttributeArgumentDeclaration (literalField, "value");
    }

    public const string c_string = "string";
  }
}