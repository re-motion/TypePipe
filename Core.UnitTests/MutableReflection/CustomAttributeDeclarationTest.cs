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
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class CustomAttributeDeclarationTest
  {
    [Test]
    public void Initialization ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ((ValueType) null));
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainAttribute obj) => obj.Property);
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.Field);

      var declaration = new CustomAttributeDeclaration (
          constructor,
          new object[] { 7 },
          new NamedArgumentDeclaration (property, 7),
          new NamedArgumentDeclaration (field, "value"));

      Assert.That (declaration.Type, Is.SameAs (typeof (DomainAttribute)));
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
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ((ValueType) null));

      var declaration = new CustomAttributeDeclaration (constructor, new object[] { null });

      Assert.That (declaration.ConstructorArguments[0], Is.Null);
    }

    [Test]
    public void Initialization_MemberDeclaringTypesAreAssignable ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ());
      var property = typeof (DomainAttribute).GetProperty ("Property");

      new CustomAttributeDeclaration (constructor, new object[0], new NamedArgumentDeclaration (property, 7));
    }

    [Test]
    public void Initialization_NoAttributeType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonAttributeClass());
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[0]),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Type 'Remotion.TypePipe.UnitTests.MutableReflection.CustomAttributeDeclarationTest+NonAttributeClass' does not derive from 'System.Attribute'.",
                  "constructor"));
    }

    [Test]
    public void Initialization_NonPublicConstructor ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ("internal"));
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[] { "ctorArg" }),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "The attribute constructor 'Void .ctor(System.String)' is not a public instance constructor.", "constructor"));
    }

    [Test]
    public void Initialization_TypeInitializer ()
    {
      var constructor = typeof (DomainAttribute).GetConstructor (BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[0]),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("The attribute constructor 'Void .cctor()' is not a public instance constructor.", "constructor"));
    }

    [Test]
    public void Initialization_NonVisibleCustomAttributeType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new PrivateCustomAttribute ());
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[0]),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "The attribute type 'Remotion.TypePipe.UnitTests.MutableReflection.CustomAttributeDeclarationTest+PrivateCustomAttribute' is not publicly "
                  + "visible.", "constructor"));
    }

    [Test]
    public void Initialization_InvalidConstructorArgumentCount ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ((ValueType) null));
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[] { 7, 8 }),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Expected 1 constructor argument(s), but was 2.", "constructorArguments"));
    }

    [Test]
    public void Initialization_InvalidConstructorArgumentType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ((ValueType) null));
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[] { "string" }),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Item 0 of parameter 'constructorArguments' has the type 'System.String' instead of 'System.ValueType'.",
                  "constructorArguments"));
    }

    [Test]
    public void Initialization_InvalidNullArgument ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute (0));
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[] { null }),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Constructor parameter at position 0 of type 'System.Int32' cannot be null.", "constructorArguments"));
    }

    [Test]
    public void Initialization_InvalidMemberDeclaringType ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ());
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DerivedAttribute attr) => attr.PropertyInDerivedType);
      Assert.That (
          () => new CustomAttributeDeclaration (constructor, new object[0], new NamedArgumentDeclaration(property, 7)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Named argument 'PropertyInDerivedType' cannot be used with custom attribute type "
                  + "'Remotion.TypePipe.UnitTests.MutableReflection.CustomAttributeDeclarationTest+DomainAttribute'.",
                  "namedArguments"));
    }

    [Test]
    public void PropertiesCreateNewInstances ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ((object) null));
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.ObjectField);

      var declaration = new CustomAttributeDeclaration (
          constructor,
          new object[] { new object[] { 1, new[] { 2, 3 } } },
          new NamedArgumentDeclaration (field, new object[] { new[] { 4 }, 5, 6 }));

      Assert.That (declaration.ConstructorArguments, Is.Not.SameAs (declaration.ConstructorArguments));
      Assert.That (declaration.ConstructorArguments.Single (), Is.Not.SameAs (declaration.ConstructorArguments.Single ()));
      Assert.That (((object[]) declaration.ConstructorArguments.Single ())[1], Is.Not.SameAs (((object[]) declaration.ConstructorArguments.Single ())[1]));

      Assert.That (declaration.NamedArguments, Is.Not.SameAs (declaration.NamedArguments));
      Assert.That (declaration.NamedArguments.Single ().Value, Is.Not.SameAs (declaration.NamedArguments.Single ()));
      Assert.That (((object[]) declaration.NamedArguments.Single ().Value)[0], Is.Not.SameAs (((object[]) declaration.NamedArguments.Single ().Value)[0]));
    }

    public class DomainAttribute : Attribute
    {
      static DomainAttribute ()
      {
        Dev.Null = null;
      }

      public string Field;
      public object ObjectField;

      public DomainAttribute () { }

      public DomainAttribute (object arg)
      {
        Dev.Null = arg;
      }

      public DomainAttribute (ValueType valueType)
      {
        Dev.Null = valueType;
      }

      public DomainAttribute (int arg)
      {
        CtorIntArg = arg;
      }

      internal DomainAttribute (string arg)
      {
        Dev.Null = arg;
      }

      public int CtorIntArg { get; set; }
      public int Property { get; set; }
    }

    private class DerivedAttribute : DomainAttribute
    {
// ReSharper disable UnusedAutoPropertyAccessor.Local
      public int PropertyInDerivedType { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local
    }

    private class PrivateCustomAttribute : Attribute { }

    public class NonAttributeClass /* does not derive from Attribute */ { }
  }
}