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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    private ITypeModifier _typeModifierMock;
    
    private Type _requestedType;

    [SetUp]
    public void SetUp ()
    {
      _typeModifierMock = MockRepository.GenerateStrictMock<ITypeModifier>();

      _requestedType = ReflectionObjectMother.GetSomeSubclassableType();
    }

    [Test]
    public void Initialization ()
    {
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      var participantWithCacheProviderStub = MockRepository.GenerateStub<IParticipant>();
      var cachKeyProviderStub = MockRepository.GenerateStub<ICacheKeyProvider>();
      participantWithCacheProviderStub.Stub (stub => stub.PartialCacheKeyProvider).Return (cachKeyProviderStub);

      var participants = new[] { participantStub, participantWithCacheProviderStub };
      var typeAssembler = new TypeAssembler (participants.AsOneTime(), _typeModifierMock);

      Assert.That (typeAssembler.CacheKeyProviders, Is.EqualTo (new[] { cachKeyProviderStub }));
    }

    [Test]
    public void CodeGenerator ()
    {
      var fakeCodeGenerator = MockRepository.GenerateStub<ICodeGenerator>();
      _typeModifierMock.Expect (mock => mock.CodeGenerator).Return (fakeCodeGenerator);
      var typeAssembler = CreateTypeAssembler (_typeModifierMock);

      Assert.That (typeAssembler.CodeGenerator, Is.SameAs (fakeCodeGenerator));
    }

    [Test]
    public void AssemblyType_StrongNaming_Unknown ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<IParticipant>();
      var participantMock2 = mockRepository.StrictMock<IParticipant>();
      var typeModifierMock = mockRepository.StrictMock<ITypeModifier>();

      MutableType mutableType = null;
      var fakeResult = ReflectionObjectMother.GetSomeType();

      using (mockRepository.Ordered())
      {
        participantMock1.Expect (mock => mock.PartialCacheKeyProvider);
        participantMock2.Expect (mock => mock.PartialCacheKeyProvider);

        participantMock1
            .Expect (mock => mock.ModifyType (Arg<MutableType>.Matches (mt => mt.UnderlyingSystemType == _requestedType)))
            .WhenCalled (mi => mutableType = (MutableType) mi.Arguments[0]);
        participantMock2.Expect (mock => mock.ModifyType (Arg<MutableType>.Matches (mt => ReferenceEquals (mt, mutableType))));

        typeModifierMock
            .Expect (mock => mock.ApplyModifications (Arg<MutableType>.Matches (mt => ReferenceEquals (mt, mutableType))))
            .Return (fakeResult);
      }
      mockRepository.ReplayAll();

      var typeAssembler = CreateTypeAssembler (typeModifierMock, participants: new[] { participantMock1, participantMock2 });

      var result = typeAssembler.AssembleType (_requestedType);

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

      var result = typeAssembler.GetCompoundCacheKey (requestedType, 3);

      Assert.That (result, Is.EqualTo (new object[] { null, null, null, requestedType, 1, "2" }));
    }

    private TypeAssembler CreateTypeAssembler (ITypeModifier typeModifier = null, params IParticipant[] participants)
    {
      typeModifier = typeModifier ?? MockRepository.GenerateStub<ITypeModifier>();

      return new TypeAssembler (participants.AsOneTime(), typeModifier);
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