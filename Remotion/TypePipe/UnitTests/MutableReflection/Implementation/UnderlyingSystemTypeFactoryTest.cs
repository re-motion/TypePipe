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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class UnderlyingSystemTypeFactoryTest
  {
    private UnderlyingSystemTypeFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new UnderlyingSystemTypeFactory();
    }

    [Test]
    public void GetUnderlyingSystemType ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var customType = CustomTypeObjectMother.Create (baseType: baseType, interfaces: new[] { typeof (IDisposable), typeof (IComparable) });

      var result = _factory.CreateUnderlyingSystemType (customType);

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (baseType.IsAssignableFrom (result), Is.True);
      Assert.That (typeof (IDisposable).IsAssignableFrom (result), Is.True);
      Assert.That (typeof (IComparable).IsAssignableFrom (result), Is.True);

      Assert.That (_factory.CreateUnderlyingSystemType (customType), Is.SameAs (result), "Should be cached.");
    }

    [Test]
    public void GetUnderlyingSystemType_CacheKey ()
    {
      var baseType = typeof (object);
      var otherBaseType = GetType();
      var interfaceTypes = new[] { typeof (IDisposable), typeof (IComparable) };
      var otherInterfaceTypes = new[] { typeof (IDisposable), typeof (ICloneable) };
      var equivalentInterfaceTypes = new[] { typeof (IComparable), typeof (IDisposable) };

      var result1 = _factory.CreateUnderlyingSystemType (CustomTypeObjectMother.Create (baseType: baseType, interfaces: interfaceTypes));
      var result2 = _factory.CreateUnderlyingSystemType (CustomTypeObjectMother.Create (baseType: otherBaseType, interfaces: interfaceTypes));
      var result3 = _factory.CreateUnderlyingSystemType (CustomTypeObjectMother.Create (baseType: baseType, interfaces: otherInterfaceTypes));
      var result4 = _factory.CreateUnderlyingSystemType (CustomTypeObjectMother.Create (baseType: baseType, interfaces: equivalentInterfaceTypes));

      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1, Is.Not.SameAs (result3));
      Assert.That (result1, Is.SameAs (result4));
    }

    [Test]
    public void GetUnderlyingSystemType_AddEmptyDefaultCtor ()
    {
      var customType = CustomTypeObjectMother.Create (baseType: typeof (TypeWithoutDefaultCtor));

      var result = _factory.CreateUnderlyingSystemType (customType);

      var ctor = result.GetConstructor (Type.EmptyTypes);
      Assert.That (ctor, Is.Not.Null);
    }

    public class TypeWithoutDefaultCtor
    {
      public TypeWithoutDefaultCtor (int i) { Dev.Null = i; }
    }
  }
}