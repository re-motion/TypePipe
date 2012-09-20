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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
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
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ((ValueType) null));
      var property = typeof (CustomAttribute).GetProperty ("Property");
      var field = typeof (CustomAttribute).GetField ("Field");

      var declaration = new CustomAttributeDeclaration (
          constructor,
          new object[] { 7 },
          new NamedAttributeArgumentDeclaration (property, 7),
          new NamedAttributeArgumentDeclaration (field, "value"));

      Assert.That (declaration.Constructor, Is.SameAs (constructor));
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
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ((ValueType) null));

      var declaration = new CustomAttributeDeclaration (constructor, new object[] { null });

      Assert.That (declaration.ConstructorArguments[0], Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Type 'Remotion.TypePipe.UnitTests.MutableReflection.CustomAttributeDeclarationTest+NonAttributeClass' does not derive from 'System.Attribute'."
        + "\r\nParameter name: constructor")]
    public void Initialization_NoAttributeType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonAttributeClass());

      new CustomAttributeDeclaration (constructor, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "The attribute constructor 'Void .ctor(System.String)' is not a public instance constructor.\r\nParameter name: constructor")]
    public void Initialization_NonPublicConstructor ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ("internal"));

      new CustomAttributeDeclaration (constructor, new object[] { "ctorArg" });
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "The attribute constructor 'Void .cctor()' is not a public instance constructor.\r\nParameter name: constructor")]
    public void Initialization_TypeInitializer ()
    {
      var constructor = typeof (CustomAttribute).GetConstructor (BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
      
      new CustomAttributeDeclaration (constructor, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The attribute type 'Remotion.TypePipe.UnitTests.MutableReflection.CustomAttributeDeclarationTest+PrivateCustomAttribute' is not publicly "
        + "visible.\r\nParameter name: constructor")]
    public void Initialization_NonVisibleCustomAttributeType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new PrivateCustomAttribute ());

      new CustomAttributeDeclaration (constructor, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Expected 1 constructor argument(s), but was 2.\r\nParameter name: constructorArguments")]
    public void Initialization_InvalidConstructorArgumentCount ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ((ValueType) null));

      new CustomAttributeDeclaration (constructor, new object[] { 7, 8 });
    }

    [Test]
    [ExpectedException (typeof (ArgumentItemTypeException), ExpectedMessage =
      "Item 0 of argument constructorArguments has the type System.String instead of System.ValueType.")]
    public void Initialization_InvalidConstructorArgumentType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ((ValueType) null));

      new CustomAttributeDeclaration (constructor, new object[] { "string" });
    }

    [Test]
    [ExpectedException (typeof (ArgumentItemNullException), ExpectedMessage =
      "Constructor parameter at 0 of type 'System.Int32' cannot be null.\r\nParameter name: constructorArguments")]
    public void Initialization_InvalidNullArgument ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute (0));

      new CustomAttributeDeclaration (constructor, new object[] { null });
    }

    [Test]
    [ExpectedException (typeof(ArgumentException), ExpectedMessage =
      "Named argument 'PropertyInDerivedType' cannot be used with custom attribute type "
      + "'Remotion.TypePipe.UnitTests.MutableReflection.CustomAttributeDeclarationTest+CustomAttribute'."
      + "\r\nParameter name: namedArguments")]
    public void Initialization_InvalidMemberDeclaringType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ());
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedCustomAttribute attr) => attr.PropertyInDerivedType);

      new CustomAttributeDeclaration (constructor, new object[0], new NamedAttributeArgumentDeclaration(property, 7));
    }

    [Test]
    public void Initialization_MemberDeclaringTypesAreAssignable ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ());
      var property = typeof (CustomAttribute).GetProperty ("Property");

      new CustomAttributeDeclaration (constructor, new object[0], new NamedAttributeArgumentDeclaration (property, 7));
    }

    [Test]
    public void CreateInstance ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ());
      var declaration = new CustomAttributeDeclaration (constructor, new object[0]);

      var instance = declaration.CreateInstance();

      Assert.That (instance, Is.TypeOf<CustomAttribute>());
    }

    [Test]
    public void CreateInstance_WithCtorArgs ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute (0));
      var declaration = new CustomAttributeDeclaration (constructor, new object[] { 7 });

      var instance = (CustomAttribute) declaration.CreateInstance ();

      Assert.That (instance.CtorIntArg, Is.EqualTo (7));
    }

    [Test]
    public void CreateInstance_WithNamedArgs ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute ());
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((CustomAttribute attr) => attr.Property);
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((CustomAttribute attr) => attr.Field);
      var declaration = new CustomAttributeDeclaration (
          constructor, 
          new object[0], 
          new NamedAttributeArgumentDeclaration (property, 4711),
          new NamedAttributeArgumentDeclaration (field, "1676"));

      var instance = (CustomAttribute) declaration.CreateInstance ();

      Assert.That (instance.Property, Is.EqualTo (4711));
      Assert.That (instance.Field, Is.EqualTo ("1676"));
    }

    public class CustomAttribute : Attribute
    {
      public string Field;

      static CustomAttribute ()
      {
        Dev.Null = null;
      }

      public CustomAttribute ()
      {
      }

      public CustomAttribute (ValueType valueType)
      {
        Dev.Null = valueType;
      }

      public CustomAttribute (int arg)
      {
        CtorIntArg = arg;
      }

      internal CustomAttribute (string arg)
      {
        Dev.Null = arg;
      }

      public int CtorIntArg { get; set; }
      public int Property { get; set; }
    }

    private class DerivedCustomAttribute : CustomAttribute
    {
// ReSharper disable UnusedAutoPropertyAccessor.Local
      public int PropertyInDerivedType { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local
    }

    private class PrivateCustomAttribute : Attribute { }

    public class NonAttributeClass /* does not derive from Attribute */ { }
  }
}