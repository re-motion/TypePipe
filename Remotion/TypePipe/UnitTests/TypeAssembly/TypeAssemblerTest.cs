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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    private IProxyTypeModelFactory _proxyTypeModelFactoryMock;
    private ISubclassProxyCreator _subclassProxyCreatorMock;
    
    private Type _requestedType;

    [SetUp]
    public void SetUp ()
    {
      _proxyTypeModelFactoryMock = MockRepository.GenerateStrictMock<IProxyTypeModelFactory>();
      _subclassProxyCreatorMock = MockRepository.GenerateStrictMock<ISubclassProxyCreator> ();

      _requestedType = typeof (object);
    }

    [Test]
    public void Initialization ()
    {
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      var participantWithCacheProviderStub = MockRepository.GenerateStub<IParticipant>();
      var cachKeyProviderStub = MockRepository.GenerateStub<ICacheKeyProvider>();
      participantWithCacheProviderStub.Stub (stub => stub.PartialCacheKeyProvider).Return (cachKeyProviderStub);

      var participants = new[] { participantStub, participantWithCacheProviderStub };
      var typeAssembler = new TypeAssembler (participants.AsOneTime (), _proxyTypeModelFactoryMock, _subclassProxyCreatorMock);

      Assert.That (typeAssembler.CacheKeyProviders, Is.EqualTo (new[] { cachKeyProviderStub }));
    }

    [Test]
    public void CodeGenerator ()
    {
      var fakeCodeGenerator = MockRepository.GenerateStub<ICodeGenerator>();
      _subclassProxyCreatorMock.Expect (mock => mock.CodeGenerator).Return (fakeCodeGenerator);
      var typeAssembler = CreateTypeAssembler();

      Assert.That (typeAssembler.CodeGenerator, Is.SameAs (fakeCodeGenerator));
    }

    [Test]
    public void AssembleType ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<IParticipant>();
      var participantMock2 = mockRepository.StrictMock<IParticipant>();
      var proxyTypeModelFactoryMock = mockRepository.StrictMock<IProxyTypeModelFactory>();
      var subclassProxyBuilderMock = mockRepository.StrictMock<ISubclassProxyCreator>();

      var fakeResult = ReflectionObjectMother.GetSomeType();
      using (mockRepository.Ordered())
      {
        participantMock1.Expect (mock => mock.PartialCacheKeyProvider);
        participantMock2.Expect (mock => mock.PartialCacheKeyProvider);

        var fakeProxyType = ProxyTypeObjectMother.Create();
        proxyTypeModelFactoryMock.Expect (mock => mock.CreateProxyType (_requestedType)).Return (fakeProxyType);
        participantMock1.Expect (mock => mock.ModifyType (fakeProxyType));
        participantMock2.Expect (mock => mock.ModifyType (fakeProxyType));

        subclassProxyBuilderMock.Expect (mock => mock.CreateProxy (fakeProxyType)).Return (fakeResult);
      }
      mockRepository.ReplayAll();

      var typeAssembler = CreateTypeAssembler (
          proxyTypeModelFactoryMock, subclassProxyBuilderMock, participants: new[] { participantMock1, participantMock2 });

      var result = typeAssembler.AssembleType (_requestedType);

      mockRepository.VerifyAll();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void AssembleType_ExceptionInCodeGeneraton ()
    {
      _proxyTypeModelFactoryMock.Stub (stub => stub.CreateProxyType (_requestedType)).Return (ProxyTypeObjectMother.Create (name: "ProxyName"));
      var exception1 = new InvalidOperationException ("blub");
      var exception2 = new NotSupportedException ("blub");
      var exception3 = new Exception();
      _subclassProxyCreatorMock.Expect (mock => mock.CreateProxy (Arg<ProxyType>.Is.Anything)).Throw (exception1);
      _subclassProxyCreatorMock.Expect (mock => mock.CreateProxy (Arg<ProxyType>.Is.Anything)).Throw (exception2);
      _subclassProxyCreatorMock.Expect (mock => mock.CreateProxy (Arg<ProxyType>.Is.Anything)).Throw (exception3);
      var typeAssembler = CreateTypeAssembler (participants: MockRepository.GenerateStub<IParticipant>());

      var expectedMessageRegex = "An error occurred during code generation for 'ProxyName': blub "
                                 + @"The following participants are currently configured and may have caused the error: 'IParticipantProxy.*'\.";
      Assert.That (
          () => typeAssembler.AssembleType (_requestedType),
          Throws.InvalidOperationException.With.InnerException.SameAs (exception1).And.With.Message.Matches (expectedMessageRegex));
      Assert.That (
          () => typeAssembler.AssembleType (_requestedType),
          Throws.TypeOf<NotSupportedException>().With.InnerException.SameAs (exception2).And.With.Message.Matches (expectedMessageRegex));
      Assert.That (() => typeAssembler.AssembleType (_requestedType), Throws.Exception.SameAs (exception3));
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

    private TypeAssembler CreateTypeAssembler (
        IProxyTypeModelFactory proxyTypeModelFactory = null, ISubclassProxyCreator subclassProxyCreator = null, params IParticipant[] participants)
    {
      proxyTypeModelFactory = proxyTypeModelFactory ?? _proxyTypeModelFactoryMock;
      subclassProxyCreator = subclassProxyCreator ?? _subclassProxyCreatorMock;

      return new TypeAssembler (participants.AsOneTime(), proxyTypeModelFactory, subclassProxyCreator);
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