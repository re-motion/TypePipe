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
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypeExtensionsTest
  {
    [Test]
    public void IsRuntimeType ()
    {
      var runtimeType = ReflectionObjectMother.GetSomeType();
      var customType = CustomTypeObjectMother.Create();

      Assert.That (runtimeType.IsRuntimeType(), Is.True);
      Assert.That (customType.IsRuntimeType(), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_NoCustomTypes ()
    {
      Assert.That (typeof (string).IsAssignableFromFast (typeof (string)), Is.True);
      Assert.That (typeof (object).IsAssignableFromFast (typeof (string)), Is.True);
      Assert.That (typeof (string).IsAssignableFromFast (typeof (object)), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_CustomType_OnLeftSide ()
    {
      var customType = CustomTypeObjectMother.Create();

      Assert.That (customType.IsAssignableFromFast (customType), Is.True);
      Assert.That (customType.IsAssignableFromFast (customType.BaseType), Is.False);
      Assert.That (customType.IsAssignableFromFast (typeof (object)), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_CustomType_OnRightSide ()
    {
      var customType = CustomTypeObjectMother.Create (baseType: typeof (List<int>), interfaces: new[] { typeof (IDisposable) });

      Assert.That (customType.IsAssignableFromFast (customType), Is.True);

      Assert.That (typeof (List<int>).IsAssignableFromFast (customType), Is.True);
      Assert.That (typeof (object).IsAssignableFromFast (customType), Is.True);

      Assert.That (typeof (IDisposable).IsAssignableFromFast (customType), Is.True);

      var unrelatedType = ReflectionObjectMother.GetSomeType();
      Assert.That (unrelatedType.IsAssignableFromFast (customType), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_NullFromType ()
    {
      Assert.That (typeof (object).IsAssignableFromFast (null), Is.False);
    }

    [Test]
    public void GetTypeCodeFast ()
    {
      var runtimeType = ReflectionObjectMother.GetSomeType();
      var customType = CustomTypeObjectMother.Create();

      Assert.That (runtimeType.GetTypeCodeFast(), Is.EqualTo (Type.GetTypeCode (runtimeType)));
      Assert.That (customType.GetTypeCodeFast(), Is.EqualTo (TypeCode.Object));
    }

    [Test]
    public void MakeTypePipeGenericType_MakesGenericTypeWithCustomTypeArgument ()
    {
      var genericTypeDefinition = typeof (Dictionary<,>);
      var runtimeType = ReflectionObjectMother.GetSomeType();
      var customType = CustomTypeObjectMother.Create();

      var result = genericTypeDefinition.MakeTypePipeGenericType (runtimeType, customType);

      Assert.That (result.IsGenericType, Is.True);
      Assert.That (result.IsGenericTypeDefinition, Is.False);
      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { runtimeType, customType }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "'Int32' is not a generic type definition. MakeTypePipeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true.")]
    public void MakeTypePipeGenericType_NoGenericTypeDefinition ()
    {
      typeof (int).MakeTypePipeGenericType (typeof (int));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "The type has 2 generic parameter(s), but 1 generic argument(s) were provided. "
                          + "A generic argument must be provided for each generic parameter.\r\nParameter name: typeArguments")]
    public void MakeTypePipeGenericType_WrongNumberOfTypeArguments ()
    {
      typeof (Dictionary<,>).MakeTypePipeGenericType (typeof (int));
    }

    [Test]
    public void MakeTypePipeGenericType_GenericParameterConstraints ()
    {
      var staticType = typeof (TypeExtensions);
      var typeWithPublicDefaultCtor = typeof (object);
      var typeWithoutDefaultCtor = typeof (string);
      var typeWithoutPublicDefaultCtor = typeof (TypeWithNonPublicDefaultCtor);
      var classType = typeof (string);
      var valueType = typeof (int);
      var nullableValueType = typeof (int?);
      var disposableType = typeof (Stream);

      CheckArgumentCheck (
          typeof (NewConstraint<>),
          typeWithPublicDefaultCtor,
          typeWithoutDefaultCtor,
          "Generic argument 'String' at position 0 on 'NewConstraint`1' violates a constraint of type parameter 'TNew'.");
      CheckArgumentCheck (
          typeof (NewConstraint<>),
          typeWithPublicDefaultCtor,
          typeWithoutPublicDefaultCtor,
          "Generic argument 'TypeWithNonPublicDefaultCtor' at position 0 on 'NewConstraint`1' violates a constraint of type parameter 'TNew'.");
      CheckArgumentCheck (
          typeof (ClassConstraint<>),
          classType,
          valueType,
          "Generic argument 'Int32' at position 0 on 'ClassConstraint`1' violates a constraint of type parameter 'TClass'.");
      CheckArgumentCheck (
          typeof (StructConstraint<>),
          valueType,
          classType,
          "Generic argument 'String' at position 0 on 'StructConstraint`1' violates a constraint of type parameter 'TStruct'.");
      CheckArgumentCheck (
          typeof (StructConstraint<>),
          valueType,
          nullableValueType,
          "Generic argument 'Nullable`1' at position 0 on 'StructConstraint`1' violates a constraint of type parameter 'TStruct'.");
      CheckArgumentCheck (
          typeof (BaseTypeConstraint<>),
          typeof (TypeExtensionsTest),
          classType,
          "Generic argument 'String' at position 0 on 'BaseTypeConstraint`1' violates a constraint of type parameter 'T'.");
      CheckArgumentCheck (
          typeof (InterfaceConstraint<>),
          disposableType,
          classType,
          "Generic argument 'String' at position 0 on 'InterfaceConstraint`1' violates a constraint of type parameter 'T'.");

      Assert.That (() => typeof (GenericType<>).MakeGenericType (staticType), Throws.Nothing, "Assert original reflection behavior.");
      Assert.That (
          () => typeof (DependentConstraint<,>).MakeGenericType (typeof (string), typeof (object)),
          Throws.ArgumentException,
          "Assert original reflection behavior.");

      Assert.That (() => typeof (GenericType<>).MakeTypePipeGenericType (staticType), Throws.Nothing);
      Assert.That (() => typeof (DependentConstraint<,>).MakeTypePipeGenericType (typeof (object), typeof (string)), Throws.Nothing);
      Assert.That (
          () => typeof (DependentConstraint<,>).MakeTypePipeGenericType (typeof (string), typeof (object)),
          Throws.ArgumentException,
          "Only throws (correct) exception because all participating types are runtime types.");
      Assert.That (() => typeof (DependentRecursiveConstraint<,>).MakeTypePipeGenericType (typeof (int), typeof (IComparable<int>)), Throws.Nothing);
      Assert.That (
          () => typeof (DependentRecursiveConstraint<,>).MakeTypePipeGenericType (typeof (int), typeof (IComparable<string>)),
          Throws.ArgumentException,
          "Only throws (correct) exception because all participating types are runtime types.");
    }

    private void CheckArgumentCheck (Type genericTypeDefinition, Type validTypeArgument, Type invalidTypeArgument, string message)
    {
      Assert.That (() => genericTypeDefinition.MakeTypePipeGenericType (validTypeArgument), Throws.Nothing);
      Assert.That (
          () => genericTypeDefinition.MakeTypePipeGenericType (invalidTypeArgument),
          Throws.ArgumentException.With.Message.EqualTo (message + "\r\nParameter name: typeArguments"));
    }

    class TypeWithNonPublicDefaultCtor
    {
      protected internal TypeWithNonPublicDefaultCtor () { }
    }
    class GenericType<T> { }
    class NewConstraint<TNew> where TNew : new () { }
    class ClassConstraint<TClass> where TClass : class { }
    class StructConstraint<TStruct> where TStruct : struct { }
    class BaseTypeConstraint<T> where T : TypeExtensionsTest { }
    class InterfaceConstraint<T> where T : IDisposable { }
    class DependentConstraint<T1, T2> where T2 : T1 { }
    class DependentRecursiveConstraint<T1, T2> where T2 : IComparable<T1> { }
  }
}