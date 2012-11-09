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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    [Test]
    public void Initialization ()
    {
      var participantWithCacheProviderStub = MockRepository.GenerateStub<IParticipant>();
      var cachKeyProviderStub = MockRepository.GenerateStub<ICacheKeyProvider>();
      participantWithCacheProviderStub.Stub (stub => stub.PartialCacheKeyProvider).Return (cachKeyProviderStub);

      var participants = new[] { MockRepository.GenerateStub<IParticipant>(), participantWithCacheProviderStub };
      var typeModifier = MockRepository.GenerateStub<ITypeModifier>();

      var typeAssembler = new TypeAssembler (participants.AsOneTime(), typeModifier);

      Assert.That (typeAssembler.Participants, Is.EqualTo (participants));
      // Make sure that participants are iterated only once
      Assert.That (typeAssembler.Participants, Is.EqualTo (participants));
      Assert.That (typeAssembler.TypeModifier, Is.SameAs (typeModifier));
      Assert.That (typeAssembler.CacheKeyProviders, Is.EqualTo (new[] { cachKeyProviderStub }));
    }

    [Test]
    public void AssemblyType ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<IParticipant>();
      var participantMock2 = mockRepository.StrictMock<IParticipant>();
      var typeModifierMock = mockRepository.StrictMock<ITypeModifier> ();

      var requestedType = ReflectionObjectMother.GetSomeSubclassableType();
      MutableType mutableType = null;
      var fakeResult = ReflectionObjectMother.GetSomeType();

      using (mockRepository.Ordered())
      {
        participantMock1.Expect (mock => mock.PartialCacheKeyProvider);
        participantMock2.Expect (mock => mock.PartialCacheKeyProvider);

        participantMock1
            .Expect (mock => mock.ModifyType (Arg<MutableType>.Matches (mt => mt.UnderlyingSystemType == requestedType)))
            .WhenCalled (mi => mutableType = (MutableType) mi.Arguments[0]);
        participantMock2
            .Expect (mock => mock.ModifyType (Arg<MutableType>.Matches (mt => mt == mutableType)))
            .WhenCalled (mi => Assert.That (mi.Arguments[0], Is.SameAs (mutableType)));

        typeModifierMock
            .Expect (mock => mock.ApplyModifications (Arg<MutableType>.Matches (mt => mt == mutableType)))
            .Return (fakeResult);
      }
      mockRepository.ReplayAll();

      var typeAssembler = CreateTypeAssembler (typeModifierMock, participantMock1, participantMock2);

      var result = typeAssembler.AssembleType (requestedType);

      mockRepository.VerifyAll();
      Assert.That (mutableType, Is.Not.Null);
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetCompoundCacheKey ()
    {
      var requestedType = ReflectionObjectMother.GetSomeSubclassableType();
      var participantMock1 = CreateCacheKeyReturningParticipantMock (requestedType, 1);
      var participantMock2 = CreateCacheKeyReturningParticipantMock (requestedType, "2");
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1, participantMock2 });

      var result = typeAssembler.GetCompoundCacheKey (requestedType);

      Assert.That (result, Is.EqualTo (new object[] { requestedType, 1, "2" }));
    }

    private TypeAssembler CreateTypeAssembler (ITypeModifier typeModifier = null, params IParticipant[] participants)
    {
      return new TypeAssembler (participants, typeModifier ?? MockRepository.GenerateStub<ITypeModifier>());
    }

    private IParticipant CreateCacheKeyReturningParticipantMock (Type requestedType, object cacheKey)
    {
      var participantMock = MockRepository.GenerateStrictMock<IParticipant>();
      var cacheKeyProviderMock = MockRepository.GenerateStrictMock<ICacheKeyProvider>();

      participantMock.Expect (mock => mock.PartialCacheKeyProvider).Return (cacheKeyProviderMock);
      cacheKeyProviderMock.Expect (mock => mock.GetCacheKey (requestedType)).Return (cacheKey);

      return participantMock;
    }
  }
}