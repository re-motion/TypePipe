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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.Serialization;
using Remotion.TypePipe.Serialization;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class AssembledTypeIDDataTest
  {
    private AssembledTypeIDData _data;

    private Type _type;
    private FlatValueStub _flatValueStub;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType();
      _flatValueStub = new FlatValueStub();

      _data = new AssembledTypeIDData (_type.AssemblyQualifiedName, new IFlatValue[] { _flatValueStub, null });
    }

    [Test]
    public void IsSerializable ()
    {
      var result = Serializer.SerializeAndDeserialize (_data);

      Assert.That (result.RequestedTypeAssemblyQualifiedName, Is.EqualTo (_data.RequestedTypeAssemblyQualifiedName));
      Assert.That (result.FlattenedSerializableIDParts, Is.EqualTo (_data.FlattenedSerializableIDParts));
    }

    [Test]
    public void CreateTypeID ()
    {
      var deserializedIdPart = new object();
      _flatValueStub.RealValue = deserializedIdPart;

      var result = _data.CreateTypeID();

      Assert.That (result.RequestedType, Is.SameAs (_type));
      Assert.That (result.Parts, Is.EqualTo (new[] { deserializedIdPart, null }));
    }

    [Test]
    public void GetRealObject_RequestedTypeNotFound ()
    {
      var data = new AssembledTypeIDData ("UnknownType", new IFlatValue[0]);
      Assert.That (
          () => data.CreateTypeID(),
          Throws.InstanceOf<TypeLoadException>()
#if NETFRAMEWORK || NET6_0 || NET7_0
              .With.Message.StartsWith ("Could not load type 'UnknownType' from assembly 'Remotion.TypePipe, Version="));
#else
              .With.Message.StartsWith ("Could not resolve type 'UnknownType' in assembly 'Remotion.TypePipe, Version="));
#endif
    }
  }
}