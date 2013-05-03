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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Serialization;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class AssembledTypeIDDataTest
  {
    private AssembledTypeIDData _data;

    private Type _type;
    private object _idPart;

    [SetUp]
    public void SetUp ()
    {
      _type = ReflectionObjectMother.GetSomeType();
      _idPart = "id part";

      _data = new AssembledTypeIDData (_type.AssemblyQualifiedName, new[] { _idPart });
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
      var idProviderMock = MockRepository.GenerateStrictMock<ITypeIdentifierProvider>();
      var deserializedIdPart = new object();
      idProviderMock.Expect (_ => _.DeserializeFlattenedID (_idPart)).Return (deserializedIdPart);
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      participantStub.Stub (_ => _.PartialTypeIdentifierProvider).Return (idProviderMock);
      var pipeline = CreatePipelineStub (participantStub);

      var result = _data.CreateTypeID (pipeline);

      idProviderMock.VerifyAllExpectations();
      Assert.That (result.RequestedType, Is.SameAs (_type));
      Assert.That (result.Parts, Is.EqualTo (new[] { deserializedIdPart }));
    }

    [Test]
    [ExpectedException (typeof (TypeLoadException), MatchType = MessageMatch.StartsWith,
        ExpectedMessage = "Could not load type 'UnknownType' from assembly 'Remotion.TypePipe, ")]
    public void GetRealObject_RequestedTypeNotFound ()
    {
      var pipeline = CreatePipelineStub();
      var data = new AssembledTypeIDData ("UnknownType", new object[0]);

      data.CreateTypeID (pipeline);
    }

    private static IPipeline CreatePipelineStub (params IParticipant[] participants)
    {
      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      pipelineStub.Stub (_ => _.Participants).Return (participants.ToList().AsReadOnly());

      return pipelineStub;
    }
  }
}