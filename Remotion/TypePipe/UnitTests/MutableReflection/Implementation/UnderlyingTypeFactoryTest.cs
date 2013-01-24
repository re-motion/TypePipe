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
  public class UnderlyingTypeFactoryTest
  {
    private UnderlyingTypeFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new UnderlyingTypeFactory();
    }

    [Test]
    public void GetUnderlyingSystemType_GeneratesType_AssignableToBaseAndInterfaces ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var interfaces = new[] { typeof (IDisposable), typeof (IComparable) };

      var result = _factory.CreateUnderlyingSystemType (baseType, interfaces);

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (result.Name, Is.StringMatching (@"UnderlyingSystemType\d"));
      Assert.That (baseType.IsAssignableFrom (result), Is.True);
      Assert.That (typeof (IDisposable).IsAssignableFrom (result), Is.True);
      Assert.That (typeof (IComparable).IsAssignableFrom (result), Is.True);
    }

    [Test]
    public void GetUnderlyingSystemType_GeneratesMultipleTypes_AvoidsNameClash ()
    {
      _factory.CreateUnderlyingSystemType (typeof (object), Type.EmptyTypes);
      Assert.That (() => _factory.CreateUnderlyingSystemType (typeof (object), Type.EmptyTypes), Throws.Nothing);
    }

    [Test]
    public void GetUnderlyingSystemType_AddEmptyDefaultCtor ()
    {
      var baseType = typeof (TypeWithoutDefaultCtor);

      var result = _factory.CreateUnderlyingSystemType (baseType, Type.EmptyTypes);

      var ctor = result.GetConstructor (Type.EmptyTypes);
      Assert.That (ctor, Is.Not.Null);
    }

    public class TypeWithoutDefaultCtor
    {
      public TypeWithoutDefaultCtor (int i) { Dev.Null = i; }
    }
  }
}