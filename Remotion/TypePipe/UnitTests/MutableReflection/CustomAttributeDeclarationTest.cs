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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class CustomAttributeDeclarationTest
  {
    [Test]
    public void Initialization ()
    {
      var constructor = typeof (CustomAttribute).GetConstructor (new[] { typeof (ValueType) });
      var property = typeof (CustomAttribute).GetProperty ("Property");
      var field = typeof (CustomAttribute).GetField ("Field");

      var declaration = new CustomAttributeDeclaration (
          constructor,
          new object[] { 7 },
          new NamedAttributeArgumentDeclaration (property, 7),
          new NamedAttributeArgumentDeclaration (field, "value"));

      Assert.That (declaration.AttributeConstructorInfo, Is.SameAs (constructor));
      Assert.That (declaration.ConstructorArguments, Is.EqualTo(new[] {7}));
      var actualNamedArguments = declaration.NamedArguments.Select (na => new { na.MemberInfo, na.Value });
      var expectedNamedArguments =
          new[]
          {
              new { MemberInfo = (MemberInfo) property, Value = (object) 7 },
              new { MemberInfo = (MemberInfo) field, Value = (object) "value" }
          };
      Assert.That (actualNamedArguments, Is.EqualTo (expectedNamedArguments));
    }

    [Test]
    public void Initialization_WithNullArgument ()
    {
      var constructor = typeof (CustomAttribute).GetConstructor (new[] { typeof (ValueType) });

      var declaration = new CustomAttributeDeclaration (constructor, new object[] { null });

      Assert.That (declaration.ConstructorArguments[0], Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Expected 1 constructor argument(s), but was 2.\r\nParameter name: constructorArguments")]
    public void Initialization_InvalidConstructorArgumentCount ()
    {
      var constructor = typeof (CustomAttribute).GetConstructor (new[] { typeof (ValueType) });

      new CustomAttributeDeclaration (constructor, new object[] { 7, 8 });
    }

    [Test]
    [ExpectedException (typeof (ArgumentItemTypeException), ExpectedMessage =
      "Item 0 of argument constructorArguments has the type System.String instead of System.ValueType.")]
    public void Initialization_InvalidConstructorArgumentType ()
    {
      var constructor = typeof (CustomAttribute).GetConstructor (new[] { typeof (ValueType) });

      new CustomAttributeDeclaration (constructor, new object[] { "string" });
    }

    [Test]
    [ExpectedException (typeof (ArgumentItemNullException), ExpectedMessage =
      "Constructor parameter at 0 of type 'System.Int32' cannot be null.\r\nParameter name: constructorArguments")]
    public void Initialization_InvalidNullArgument ()
    {
      var constructor = typeof (CustomAttribute).GetConstructor (new[] { typeof (int) });

      new CustomAttributeDeclaration (constructor, new object[] { null });
    }

    [Ignore("TODO 4672")]
    [Test]
    public void Initialization_InvalidMemberDeclaringType ()
    {
      // Constructor.DeclaringType must be assignable to all fields and properties.DeclaringType
    }

    private class CustomAttribute
    {
      public string Field = null;

      public CustomAttribute (ValueType valueType)
      {
      }

      public CustomAttribute (int arg)
      {
      }

      public int Property { get; set;}
    }
  }
}