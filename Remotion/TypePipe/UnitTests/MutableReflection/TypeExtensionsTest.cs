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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Generics;
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
    public void IsGenericTypeInstantiation ()
    {
      var nonGenericType = ReflectionObjectMother.GetSomeNonGenericType();
      var genericTypeDefinition = typeof (List<>);
      var typeInstantiation = typeof (List<string>);

      Assert.That (nonGenericType.IsGenericTypeInstantiation(), Is.False);
      Assert.That (genericTypeDefinition.IsGenericTypeInstantiation(), Is.False);
      Assert.That (typeInstantiation.IsGenericTypeInstantiation(), Is.True);
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
    public void IsAssignableFromFast_CustomType_OnRightSide_CustomBaseTypeAndInterfaces ()
    {
      var customBaseType = CustomTypeObjectMother.Create();
      var customInterfaceType = CustomTypeObjectMother.Create();
      var customType = CustomTypeObjectMother.Create (baseType: customBaseType, interfaces: new[] { customInterfaceType });

      Assert.That (customType.IsAssignableFromFast (customType), Is.True);
      Assert.That (customBaseType.IsAssignableFromFast (customType), Is.True);
      Assert.That (customInterfaceType.IsAssignableFromFast (customType), Is.True);
    }

    [Test]
    public void IsAssignableFromFast_TypeInstantiations ()
    {
      var genericTypeDefinition = typeof (List<>);
      var instantiation1 = TypeInstantiationObjectMother.Create (genericTypeDefinition, new[] { typeof (int) });
      var instantiation2 = TypeInstantiationObjectMother.Create (genericTypeDefinition, new[] { typeof (double) });
      var instantiation3 = TypeInstantiationObjectMother.Create (genericTypeDefinition, new[] { typeof (int) });

      Assert.That (instantiation1.IsAssignableFromFast (instantiation2), Is.False);
      Assert.That (instantiation1.IsAssignableFromFast (instantiation3), Is.True);
    }

    [Test]
    public void IsAssignableFromFast_GenericParameterConstraints ()
    {
      var genericParameter1 = MutableGenericParameterObjectMother.Create (constraints: new[] { typeof (IDisposable) });
      var genericParameter2 = MutableGenericParameterObjectMother.Create (constraints: new[] { typeof (TypeExtensionsTest) });
      var genericParameter3 = MutableGenericParameterObjectMother.Create (constraints: new[] { typeof (IDisposable) });

      Assert.That (typeof (object).IsAssignableFromFast (genericParameter1), Is.True);
      Assert.That (typeof (TypeExtensionsTest).IsAssignableFromFast (genericParameter1), Is.False);
      Assert.That (typeof (TypeExtensionsTest).IsAssignableFromFast (genericParameter2), Is.True);

      Assert.That (typeof (IDisposable).IsAssignableFromFast (genericParameter1), Is.True);
      Assert.That (typeof (IDisposable).IsAssignableFromFast (genericParameter2), Is.False);

      Assert.That (genericParameter1.IsAssignableFromFast (typeof (object)), Is.False);
      Assert.That (genericParameter1.IsAssignableFromFast (typeof (IDisposable)), Is.False);
      Assert.That (genericParameter1.IsAssignableFromFast (genericParameter1), Is.True);
      Assert.That (genericParameter1.IsAssignableFromFast (genericParameter2), Is.False);
      Assert.That (genericParameter1.IsAssignableFromFast (genericParameter3), Is.False);
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
        ExpectedMessage = "The generic definition 'Dictionary`2' has 2 generic parameter(s), but 1 generic argument(s) were provided. "
                          + "A generic argument must be provided for each generic parameter.\r\nParameter name: typeArguments")]
    public void MakeTypePipeGenericType_UsesGenericArgumentUtilityToValidateGenericArguments ()
    {
      typeof (Dictionary<,>).MakeTypePipeGenericType (typeof (int));
    }
  }
}