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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Implementation;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    public interface ICachKeyProviderMethod
    {
      object M (ICacheKeyProvider cacheKeyProvider, ITypeAssembler typeAssembler, Type fromType);
    }

    private IMutableTypeFactory _mutableTypeFactoryMock;

    private Type _requestedType;
    private IDictionary<string, object> _participantState;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeFactoryMock = MockRepository.GenerateStrictMock<IMutableTypeFactory>();

      _requestedType = ReflectionObjectMother.GetSomeSubclassableType();
      _participantState = new Dictionary<string, object>();
    }

    [Test]
    public void Initialization ()
    {
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      var participantWithCacheProviderStub = MockRepository.GenerateStub<IParticipant>();
      var cachKeyProviderStub = MockRepository.GenerateStub<ICacheKeyProvider>();
      participantWithCacheProviderStub.Stub (stub => stub.PartialCacheKeyProvider).Return (cachKeyProviderStub);
      var participants = new[] { participantStub, participantWithCacheProviderStub };

      var typeAssembler = new TypeAssembler ("configId", participants.AsOneTime(), _mutableTypeFactoryMock);

      var cacheKeyProviders = PrivateInvoke.GetNonPublicField (typeAssembler, "_cacheKeyProviders");
      Assert.That (cacheKeyProviders, Is.EqualTo (new[] { cachKeyProviderStub }));
      Assert.That (typeAssembler.ParticipantConfigurationID, Is.EqualTo ("configId"));
      Assert.That (typeAssembler.Participants, Is.EqualTo (participants));
    }

    [Test]
    public void IsAssembledType ()
    {
      var assembledType = typeof (AssembledType);
      var assembledTypeSubclass = typeof (AssembledTypeSubclass);
      var otherType = ReflectionObjectMother.GetSomeType();

      var typeAssembler = CreateTypeAssembler();

      Assert.That (typeAssembler.IsAssembledType (assembledType), Is.True);
      Assert.That (typeAssembler.IsAssembledType (assembledTypeSubclass), Is.False);
      Assert.That (typeAssembler.IsAssembledType (otherType), Is.False);
    }

    [Test]
    public void GetRequestedType ()
    {
      var assembledType = typeof (AssembledType);
      var assembledTypeSubclass = typeof (AssembledTypeSubclass);
      var otherType = ReflectionObjectMother.GetSomeType();

      var typeAssembler = CreateTypeAssembler();

      Assert.That (typeAssembler.GetRequestedType (assembledType), Is.SameAs (typeof (RequestedType)));
      var message = "The argument type is not an assembled type.\r\nParameter name: assembledType";
      Assert.That (() => typeAssembler.GetRequestedType (assembledTypeSubclass), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => typeAssembler.GetRequestedType (otherType), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void GetCompoundCacheKey ()
    {
      var participantMock1 = MockRepository.GenerateStrictMock<IParticipant>();
      var participantMock2 = MockRepository.GenerateStrictMock<IParticipant>();
      var partialCacheKeyProviderMock1 = MockRepository.GenerateStrictMock<ICacheKeyProvider>();
      var partialCacheKeyProviderMock2 = MockRepository.GenerateStrictMock<ICacheKeyProvider>();
      participantMock1.Expect (mock => mock.PartialCacheKeyProvider).Return (partialCacheKeyProviderMock1);
      participantMock2.Expect (mock => mock.PartialCacheKeyProvider).Return (partialCacheKeyProviderMock2);
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1, participantMock2 });

      var cachKeyProviderMethod = MockRepository.GenerateStrictMock<ICachKeyProviderMethod>();
      cachKeyProviderMethod.Expect (mock => mock.M (partialCacheKeyProviderMock1, typeAssembler, _requestedType)).Return (1);
      cachKeyProviderMethod.Expect (mock => mock.M (partialCacheKeyProviderMock2, typeAssembler, _requestedType)).Return ("2");

      var result = typeAssembler.GetCompoundCacheKey (cachKeyProviderMethod.M, _requestedType, 2);

      Assert.That (result, Is.EqualTo (new object[] { null, null, 1, "2" }));
    }

    [Test]
    public void AssembleType ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<IParticipant>();
      var participantMock2 = mockRepository.StrictMock<IParticipant>();
      var mutableTypeFactoryMock = mockRepository.StrictMock<IMutableTypeFactory>();
      var codeGeneratorMock = mockRepository.StrictMock<IMutableTypeBatchCodeGenerator>();

      var generationCompletedEventRaised = false;
      var fakeGeneratedType = ReflectionObjectMother.GetSomeType();
      using (mockRepository.Ordered())
      {
        participantMock1.Expect (mock => mock.PartialCacheKeyProvider);
        participantMock2.Expect (mock => mock.PartialCacheKeyProvider);

        var proxyType = MutableTypeObjectMother.Create();
        var typeModificationContextMock = mockRepository.StrictMock<ITypeModificationTracker>();
        mutableTypeFactoryMock.Expect (mock => mock.CreateProxy (_requestedType)).Return (typeModificationContextMock);
        typeModificationContextMock.Stub (stub => stub.Type).Return (proxyType);

        var additionalType = MutableTypeObjectMother.Create();
        ITypeAssemblyContext typeAssemblyContext = null;
        participantMock1.Expect (mock => mock.Participate (Arg<ITypeAssemblyContext>.Is.Anything)).WhenCalled (
            mi =>
            {
              typeAssemblyContext = (ITypeAssemblyContext) mi.Arguments[0];
              Assert.That (typeAssemblyContext.ParticipantConfigurationID, Is.EqualTo ("participant configuration id"));
              Assert.That (typeAssemblyContext.RequestedType, Is.SameAs (_requestedType));
              Assert.That (typeAssemblyContext.ProxyType, Is.SameAs (proxyType));
              Assert.That (typeAssemblyContext.State, Is.SameAs (_participantState));

              typeAssemblyContext.CreateType ("AdditionalType", null, 0, typeof (int));

              typeAssemblyContext.GenerationCompleted += ctx =>
              {
                Assert.That (ctx.GetGeneratedType (proxyType), Is.SameAs (fakeGeneratedType));
                generationCompletedEventRaised = true;
              };
            });
        mutableTypeFactoryMock.Expect (mock => mock.CreateType ("AdditionalType", null, 0, typeof (int))).Return (additionalType);
        participantMock2.Expect (mock => mock.Participate (Arg<ITypeAssemblyContext>.Matches (ctx => ctx == typeAssemblyContext)));

        typeModificationContextMock.Expect (mock => mock.IsModified()).Return (true);

        codeGeneratorMock
            .Expect (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.List.Equal (new[] { additionalType, proxyType })))
            .Return (new[] { new KeyValuePair<MutableType, Type> (proxyType, fakeGeneratedType) })
            .WhenCalled (
                mi =>
                {
                  Assert.That (generationCompletedEventRaised, Is.False);

                  var proxyAttribute = proxyType.AddedCustomAttributes.Single();
                  Assert.That (proxyAttribute.Type, Is.SameAs (typeof (AssembledTypeAttribute)));
                  Assert.That (proxyAttribute.ConstructorArguments, Is.Empty);
                  Assert.That (proxyAttribute.NamedArguments, Is.Empty);
                });
      }
      mockRepository.ReplayAll();

      var typeAssembler = CreateTypeAssembler (mutableTypeFactoryMock, "participant configuration id", new[] { participantMock1, participantMock2 });

      var result = typeAssembler.AssembleType (_requestedType, _participantState, codeGeneratorMock);

      mockRepository.VerifyAll();
      Assert.That (generationCompletedEventRaised, Is.True);
      Assert.That (result, Is.SameAs (fakeGeneratedType));
    }

    [Test]
    public void AssembleType_NonSubclassableType_LetParticipantReportErrors_AndReturnsRequestedType ()
    {
      var participantMock = MockRepository.GenerateMock<IParticipant>();
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock });
      var nonSubclassableType = ReflectionObjectMother.GetSomeNonSubclassableType();
      var codeGenerator = MockRepository.GenerateStub<IMutableTypeBatchCodeGenerator>();

      var result = typeAssembler.AssembleType (nonSubclassableType, _participantState, codeGenerator);

      Assert.That (result, Is.SameAs (nonSubclassableType));
      participantMock.AssertWasCalled (mock => mock.HandleNonSubclassableType (nonSubclassableType));
      participantMock.AssertWasNotCalled (mock => mock.Participate (Arg<ITypeAssemblyContext>.Is.Anything));
    }

    [Test]
    public void AssembleType_NoModifications_ReturnsRequestedType ()
    {
      var mutableTypeFactoryStub = MockRepository.GenerateStub<IMutableTypeFactory>();
      var typeModificationContextStub = MockRepository.GenerateStub<ITypeModificationTracker>();
      mutableTypeFactoryStub.Stub (_ => _.CreateProxy (_requestedType)).Return (typeModificationContextStub);
      typeModificationContextStub.Stub (_ => _.Type).Return (MutableTypeObjectMother.Create());
      var typeAssembler = CreateTypeAssembler (mutableTypeFactoryStub);
      var codeGeneratorMock = MockRepository.GenerateMock<IMutableTypeBatchCodeGenerator>();

      var result = typeAssembler.AssembleType (_requestedType, _participantState, codeGeneratorMock);

      Assert.That (result, Is.SameAs (_requestedType));
      codeGeneratorMock.AssertWasNotCalled (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.Is.Anything));
    }

    [Test]
    public void AssembleType_ExceptionInCodeGeneraton ()
    {
      var typeModificationContextStub = MockRepository.GenerateStub<ITypeModificationTracker>();
      typeModificationContextStub.Stub (_ => _.Type).Do ((Func<MutableType>) (() => MutableTypeObjectMother.Create()));
      typeModificationContextStub.Stub (_ => _.IsModified()).Return (true);
      _mutableTypeFactoryMock.Stub (_ => _.CreateProxy (_requestedType)).Return (typeModificationContextStub);
      var typeAssemblyContextCodeGeneratorMock = MockRepository.GenerateStrictMock<IMutableTypeBatchCodeGenerator>();
      var exception1 = new InvalidOperationException ("blub");
      var exception2 = new NotSupportedException ("blub");
      var exception3 = new Exception();
      typeAssemblyContextCodeGeneratorMock.Expect (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.Is.Anything)).Throw (exception1);
      typeAssemblyContextCodeGeneratorMock.Expect (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.Is.Anything)).Throw (exception2);
      typeAssemblyContextCodeGeneratorMock.Expect (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.Is.Anything)).Throw (exception3);
      var typeAssembler = CreateTypeAssembler (participants: MockRepository.GenerateStub<IParticipant>());

      var expectedMessageRegex = "An error occurred during code generation for '" + _requestedType.Name + "':\r\nblub\r\n"
                                 + @"The following participants are currently configured and may have caused the error: 'IParticipantProxy.*'\.";
      Assert.That (
          () => typeAssembler.AssembleType (_requestedType, _participantState, typeAssemblyContextCodeGeneratorMock),
          Throws.InvalidOperationException.With.InnerException.SameAs (exception1).And.With.Message.Matches (expectedMessageRegex));
      Assert.That (
          () => typeAssembler.AssembleType (_requestedType, _participantState, typeAssemblyContextCodeGeneratorMock),
          Throws.TypeOf<NotSupportedException>().With.InnerException.SameAs (exception2).And.With.Message.Matches (expectedMessageRegex));

      Assert.That (
          () => typeAssembler.AssembleType (_requestedType, _participantState, typeAssemblyContextCodeGeneratorMock),
          Throws.Exception.SameAs (exception3));
    }

    [Test]
    public void RebuildParticipantState ()
    {
      var loadedTypesContext = LoadedTypesContextObjectMother.Create();
      var participantMock = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock.Stub (stub => stub.PartialCacheKeyProvider);
      participantMock.Expect (mock => mock.RebuildState (loadedTypesContext));
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock });

      typeAssembler.RebuildParticipantState (loadedTypesContext);

      participantMock.VerifyAllExpectations();
    }

    private TypeAssembler CreateTypeAssembler (
        IMutableTypeFactory mutableTypeFactory = null, string configurationId = "id", params IParticipant[] participants)
    {
      mutableTypeFactory = mutableTypeFactory ?? _mutableTypeFactoryMock;

      return new TypeAssembler (configurationId, participants.AsOneTime(), mutableTypeFactory);
    }

    private class RequestedType {}
    [AssembledType] private class AssembledType : RequestedType {}
    private class AssembledTypeSubclass {}
  }
}