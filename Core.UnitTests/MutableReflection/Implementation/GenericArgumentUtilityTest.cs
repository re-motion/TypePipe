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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class GenericArgumentUtilityTest
  {
    [Test]
    public void ValidateGenericArguments_WrongNumberOfTypeArguments ()
    {
      Assert.That (
          () => CallValidateGenericArguments (typeof (Dictionary<,>), typeof (int)),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "The generic definition 'xxx' has 2 generic parameter(s), but 1 generic argument(s) were provided. "
                  + "A generic argument must be provided for each generic parameter.", "typeArguments"));
    }

    [Test]
    public void ValidateGenericArguments_GenericParameterConstraints ()
    {
      var staticType = typeof (TypeExtensions);
      var typeWithPublicDefaultCtor = typeof (object);
      var typeWithoutDefaultCtor = typeof (string);
      var typeWithoutPublicDefaultCtor = typeof (TypeWithNonPublicDefaultCtor);
      var classType = typeof (string);
      var valueType = typeof (int);
      var nullableValueType = typeof (int?);
      var disposableType = typeof (Stream);

      Assert.That (() => typeof (GenericType<>).MakeGenericType (staticType), Throws.Nothing, "Assert original reflection behavior.");
      Assert.That (() => CallValidateGenericArguments (typeof (GenericType<>), staticType), Throws.Nothing);

      CallAndCheckValidateGenericArguments (
          typeof (NewConstraint<>),
          typeWithPublicDefaultCtor,
          typeWithoutDefaultCtor,
          "Generic argument 'String' at position 0 on 'xxx' violates a constraint of type parameter 'TNew'.");
      CallAndCheckValidateGenericArguments (
          typeof (NewConstraint<>),
          typeWithPublicDefaultCtor,
          typeWithoutPublicDefaultCtor,
          "Generic argument 'TypeWithNonPublicDefaultCtor' at position 0 on 'xxx' violates a constraint of type parameter 'TNew'.");
      CallAndCheckValidateGenericArguments (
          typeof (ClassConstraint<>),
          classType,
          valueType,
          "Generic argument 'Int32' at position 0 on 'xxx' violates a constraint of type parameter 'TClass'.");
      CallAndCheckValidateGenericArguments (
          typeof (StructConstraint<>),
          valueType,
          classType,
          "Generic argument 'String' at position 0 on 'xxx' violates a constraint of type parameter 'TStruct'.");
      CallAndCheckValidateGenericArguments (
          typeof (StructConstraint<>),
          valueType,
          nullableValueType,
          "Generic argument 'Nullable`1' at position 0 on 'xxx' violates a constraint of type parameter 'TStruct'.");
      CallAndCheckValidateGenericArguments (
          typeof (BaseTypeConstraint<>),
          typeof (TypeExtensionsTest),
          classType,
          "Generic argument 'String' at position 0 on 'xxx' violates a constraint of type parameter 'T'.");
      CallAndCheckValidateGenericArguments (
          typeof (InterfaceConstraint<>),
          disposableType,
          classType,
          "Generic argument 'String' at position 0 on 'xxx' violates a constraint of type parameter 'T'.");
    }

    [Test]
    public void ValidateGenericArguments_SelfReferencingGenericParameterConstraints ()
    {
      // TODO: Fully implement and remove GenericArgumentUtility.SkipValidation
      Assert.That (() => CallValidateGenericArguments (typeof (DependentConstraint<,>), typeof (object), typeof (string)), Throws.Nothing);
      // Only throws (correct) exception because all participating types are runtime types.
      //Assert.That (() => CallValidateGenericArguments (typeof (DependentConstraint<,>), typeof (string), typeof (object)), Throws.ArgumentException);

      Assert.That (() => CallValidateGenericArguments (typeof (DependentRecursiveConstraint<,>), typeof (int), typeof (IComparable<int>)), Throws.Nothing);
      // Only throws (correct) exception because all participating types are runtime types.
      //Assert.That (() => CallValidateGenericArguments (typeof (DependentRecursiveConstraint<,>), typeof (int), typeof (IComparable<string>)), Throws.ArgumentException);
    }

    private void CallAndCheckValidateGenericArguments (Type genericTypeDefinition, Type validTypeArgument, Type invalidTypeArgument, string message)
    {
      Assert.That (() => CallValidateGenericArguments (genericTypeDefinition, validTypeArgument), Throws.Nothing);
      Assert.That (
          () => CallValidateGenericArguments (genericTypeDefinition, invalidTypeArgument),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (message, "typeArguments"));
    }

    private void CallValidateGenericArguments (Type genericMethodDefinition, params Type[] typeArguments)
    {
      GenericArgumentUtility.ValidateGenericArguments (genericMethodDefinition.GetGenericArguments(), typeArguments, "xxx");
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