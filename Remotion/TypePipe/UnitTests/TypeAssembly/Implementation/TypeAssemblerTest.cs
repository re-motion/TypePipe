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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.TypeAssembly;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    private IMutableTypeFactory _mutableTypeFactoryMock;
    private IComplexSerializationEnabler _complexSerializationEnablerMock;
    private IParticipantState _participantStateMock;

    private AssembledTypeID _typeID;
    private Type _requestedType;
    private Type _assembledType;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeFactoryMock = MockRepository.GenerateStrictMock<IMutableTypeFactory>();
      _complexSerializationEnablerMock = MockRepository.GenerateStrictMock<IComplexSerializationEnabler>();
      _participantStateMock = MockRepository.GenerateStrictMock<IParticipantState>();

      _requestedType = typeof (RequestedType);
      _typeID = AssembledTypeIDObjectMother.Create (_requestedType);
      _assembledType = typeof (AssembledType);
    }

    [Test]
    public void Initialization ()
    {
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      var participantWithCacheProviderStub = MockRepository.GenerateStub<IParticipant>();
      var identifierProviderStub = MockRepository.GenerateStub<ITypeIdentifierProvider>();
      participantWithCacheProviderStub.Stub (stub => stub.PartialTypeIdentifierProvider).Return (identifierProviderStub);
      var participants = new[] { participantStub, participantWithCacheProviderStub };

      var typeAssembler = new TypeAssembler ("configId", participants.AsOneTime(), _mutableTypeFactoryMock, _complexSerializationEnablerMock);

      Assert.That (typeAssembler.ParticipantConfigurationID, Is.EqualTo ("configId"));
      Assert.That (typeAssembler.Participants, Is.EqualTo (participants));

      var assembledTypeIdentifierProvider = PrivateInvoke.GetNonPublicField (typeAssembler, "_assembledTypeIdentifierProvider");
      Assert.That (assembledTypeIdentifierProvider, Is.TypeOf<AssembledTypeIdentifierProvider>());
      var identifierProviders = PrivateInvoke.GetNonPublicField (assembledTypeIdentifierProvider, "_identifierProviders");
      Assert.That (identifierProviders, Is.EqualTo (new[] { identifierProviderStub }));
    }

    [Test]
    public void IsAssembledType ()
    {
      Assert.That (typeof (AssembledTypeSubclass).BaseType, Is.SameAs (typeof (AssembledType)));
      var typeAssembler = CreateTypeAssembler();

      Assert.That (typeAssembler.IsAssembledType (typeof (AssembledType)), Is.True);
      Assert.That (typeAssembler.IsAssembledType (typeof (AssembledTypeSubclass)), Is.False);
      Assert.That (typeAssembler.IsAssembledType (typeof (object)), Is.False);
    }

    [Test]
    public void GetRequestedType ()
    {
      var otherType = typeof (int);

      var typeAssembler = CreateTypeAssembler();

      Assert.That (typeAssembler.GetRequestedType (_assembledType), Is.SameAs (typeof (RequestedType)));
      Assert.That (
          () => typeAssembler.GetRequestedType (otherType),
          Throws.ArgumentException.With.Message.EqualTo ("The argument type 'Int32' is not an assembled type.\r\nParameter name: assembledType"));
    }

    [Test]
    public void ComputeTypeID ()
    {
      var participantMock1 = MockRepository.GenerateStrictMock<IParticipant>();
      var participantMock2 = MockRepository.GenerateStrictMock<IParticipant>();
      var idProviderMock1 = MockRepository.GenerateStrictMock<ITypeIdentifierProvider>();
      var idProviderMock2 = MockRepository.GenerateStrictMock<ITypeIdentifierProvider>();
      participantMock1.Expect (mock => mock.PartialTypeIdentifierProvider).Return (idProviderMock1);
      participantMock2.Expect (mock => mock.PartialTypeIdentifierProvider).Return (idProviderMock2);
      idProviderMock1.Expect (mock => mock.GetID (_requestedType)).Return (1);
      idProviderMock2.Expect (mock => mock.GetID (_requestedType)).Return ("2");
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1, participantMock2 });

      var result = typeAssembler.ComputeTypeID (_requestedType);

      Assert.That (result, Is.EqualTo (new AssembledTypeID (_requestedType, new object[] { 1, "2" })));
    }

    [Test]
    public void ExtractTypeID ()
    {
      var otherType = typeof (int);
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      var assembledTypeIdentifierProviderStub = MockRepository.GenerateStub<IAssembledTypeIdentifierProvider>();
      assembledTypeIdentifierProviderStub.Stub (_ => _.ExtractTypeID (_assembledType)).Return (fakeTypeID);

      var typeAssembler = CreateTypeAssembler (assembledTypeIdentifierProvider: assembledTypeIdentifierProviderStub);

      Assert.That (typeAssembler.ExtractTypeID (_assembledType), Is.EqualTo (fakeTypeID));
      Assert.That (
          () => typeAssembler.ExtractTypeID (otherType),
          Throws.ArgumentException.With.Message.EqualTo ("The argument type 'Int32' is not an assembled type.\r\nParameter name: assembledType"));
    }

    [Test]
    public void AssembleType ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<IParticipant>();
      var participantMock2 = mockRepository.StrictMock<IParticipant>();
      var mutableTypeFactoryMock = mockRepository.StrictMock<IMutableTypeFactory>();
      var assembledTypeIdentifierProviderMock = mockRepository.StrictMock<IAssembledTypeIdentifierProvider>();
      var complexSerializationEnablerMock = mockRepository.StrictMock<IComplexSerializationEnabler>();
      var codeGeneratorMock = mockRepository.StrictMock<IMutableTypeBatchCodeGenerator>();

      var participantConfigurationID = "participant configuration id";
      var typeID = AssembledTypeIDObjectMother.Create (_requestedType, new object[] { "type id part" });
      var additionalTypeID = new object();
      var generationCompletedEventRaised = false;
      var fakeGeneratedType = ReflectionObjectMother.GetSomeType();
      var fakeGeneratedAdditionalType = ReflectionObjectMother.GetSomeType();

      using (mockRepository.Ordered())
      {
        var typeIdentifierProviderMock = MockRepository.GenerateStrictMock<ITypeIdentifierProvider>();
        participantMock1.Expect (mock => mock.PartialTypeIdentifierProvider).Return (typeIdentifierProviderMock);
        participantMock2.Expect (mock => mock.PartialTypeIdentifierProvider).Return (null);

        var proxyType = MutableTypeObjectMother.Create();
        var typeModificationTrackerMock = mockRepository.StrictMock<ITypeModificationTracker>();
        mutableTypeFactoryMock.Expect (mock => mock.CreateProxy (_requestedType)).Return (typeModificationTrackerMock);
        typeModificationTrackerMock.Stub (stub => stub.Type).Return (proxyType);

        var idPart = new object();
        assembledTypeIdentifierProviderMock
            .Expect (mock => mock.GetPart (Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)), Arg.Is (participantMock1)))
            .Return (idPart);

        IProxyTypeAssemblyContext proxyTypeAssemblyContext = null;
        participantMock1.Expect (mock => mock.Participate (Arg.Is (idPart), Arg<IProxyTypeAssemblyContext>.Is.Anything)).WhenCalled (
            mi =>
            {
              proxyTypeAssemblyContext = (IProxyTypeAssemblyContext) mi.Arguments[1];
              Assert.That (proxyTypeAssemblyContext.ParticipantConfigurationID, Is.EqualTo (participantConfigurationID));
              Assert.That (proxyTypeAssemblyContext.ParticipantState, Is.SameAs (_participantStateMock));
              Assert.That (proxyTypeAssemblyContext.RequestedType, Is.SameAs (_requestedType));
              Assert.That (proxyTypeAssemblyContext.ProxyType, Is.SameAs (proxyType));

              proxyTypeAssemblyContext.CreateAdditionalType (additionalTypeID, "AdditionalType", null, 0, typeof (int));

              proxyTypeAssemblyContext.GenerationCompleted += ctx =>
              {
                Assert.That (ctx.GetGeneratedType (proxyType), Is.SameAs (fakeGeneratedType));
                generationCompletedEventRaised = true;
              };
            });
        var additionalType = MutableTypeObjectMother.Create();
        mutableTypeFactoryMock.Expect (mock => mock.CreateType ("AdditionalType", null, 0, typeof (int), null)).Return (additionalType);

        assembledTypeIdentifierProviderMock
            .Expect (mock => mock.GetPart (Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)), Arg.Is (participantMock2)))
            .Return (null);
        participantMock2.Expect (mock => mock.Participate (Arg.Is<object> (null), Arg<IProxyTypeAssemblyContext>.Matches (ctx => ctx == proxyTypeAssemblyContext)));

        typeModificationTrackerMock.Expect (mock => mock.IsModified()).Return (true);

        assembledTypeIdentifierProviderMock.Expect (mock => mock.AddTypeID (Arg.Is (proxyType), Arg<AssembledTypeID>.Matches (id => id.Equals (typeID))));
        complexSerializationEnablerMock
            .Expect (
                mock => mock.MakeSerializable (
                    Arg.Is(proxyType),
                    Arg.Is(participantConfigurationID),
                    Arg.Is (assembledTypeIdentifierProviderMock),
                    Arg<AssembledTypeID>.Matches (id => id.Equals (typeID))));

        codeGeneratorMock
            .Expect (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.List.Equal (new[] { additionalType, proxyType })))
            .Return (
                new[]
                {
                    new KeyValuePair<MutableType, Type> (proxyType, fakeGeneratedType),
                    new KeyValuePair<MutableType, Type> (additionalType, fakeGeneratedAdditionalType)
                })
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

      var typeAssembler = CreateTypeAssembler (
          mutableTypeFactoryMock,
          assembledTypeIdentifierProviderMock,
          complexSerializationEnablerMock,
          participantConfigurationID,
          new[] { participantMock1, participantMock2 });

      var result = typeAssembler.AssembleType (typeID, _participantStateMock, codeGeneratorMock);

      mockRepository.VerifyAll();
      Assert.That (generationCompletedEventRaised, Is.True);
      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (fakeGeneratedType));
      Assert.That (result.AdditionalTypes.Count, Is.EqualTo (1));
      Assert.That (result.AdditionalTypes[additionalTypeID], Is.SameAs (fakeGeneratedAdditionalType));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The provided requested type 'AssembledType' is already an assembled type.")]
    public void AssembleType_RequestAssembledType ()
    {
      var typeAssembler = CreateTypeAssembler();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType: _assembledType);
      var codeGeneratorStub = MockRepository.GenerateStub<IMutableTypeBatchCodeGenerator>();

      typeAssembler.AssembleType (typeID, _participantStateMock, codeGeneratorStub);
    }

    [Test]
    public void AssembleType_NonSubclassableType_LetParticipantReportErrors_AndReturnsRequestedType ()
    {
      var participantMock = MockRepository.GenerateMock<IParticipant>();
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock });
      var nonSubclassableType = ReflectionObjectMother.GetSomeNonSubclassableType();
      var typeID = AssembledTypeIDObjectMother.Create (nonSubclassableType);
      var codeGeneratorStub = MockRepository.GenerateStub<IMutableTypeBatchCodeGenerator>();

      var result = typeAssembler.AssembleType (typeID, _participantStateMock, codeGeneratorStub);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (nonSubclassableType));
      participantMock.AssertWasCalled (mock => mock.HandleNonSubclassableType (nonSubclassableType));
      participantMock.AssertWasNotCalled (mock => mock.Participate (Arg.Is<object> (null), Arg<IProxyTypeAssemblyContext>.Is.Anything));
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

      var result = typeAssembler.AssembleType (_typeID, _participantStateMock, codeGeneratorMock);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (_requestedType));
      codeGeneratorMock.AssertWasNotCalled (mock => mock.GenerateTypes (Arg<IEnumerable<MutableType>>.Is.Anything));
    }

    [Test]
    public void AssembleType_ExceptionInCodeGeneraton ()
    {
      var typeModificationContextStub = MockRepository.GenerateStub<ITypeModificationTracker>();
      typeModificationContextStub.Stub (_ => _.Type).Do ((Func<MutableType>) (() => MutableTypeObjectMother.Create()));
      typeModificationContextStub.Stub (_ => _.IsModified()).Return (true);
      _mutableTypeFactoryMock.Stub (_ => _.CreateProxy (_requestedType)).Return (typeModificationContextStub);
      _complexSerializationEnablerMock.Stub (_ => _.MakeSerializable (null, null, null, new AssembledTypeID())).IgnoreArguments();
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
          () => typeAssembler.AssembleType (_typeID, _participantStateMock, typeAssemblyContextCodeGeneratorMock),
          Throws.InvalidOperationException.With.InnerException.SameAs (exception1).And.With.Message.Matches (expectedMessageRegex));
      Assert.That (
          () => typeAssembler.AssembleType (_typeID, _participantStateMock, typeAssemblyContextCodeGeneratorMock),
          Throws.TypeOf<NotSupportedException>().With.InnerException.SameAs (exception2).And.With.Message.Matches (expectedMessageRegex));

      Assert.That (
          () => typeAssembler.AssembleType (_typeID, _participantStateMock, typeAssemblyContextCodeGeneratorMock),
          Throws.Exception.SameAs (exception3));
    }

    [Test]
    public void AssembleAdditionalType_ParticipantReturnsMutableType ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<IParticipant>();
      var participantMock2 = mockRepository.StrictMock<IParticipant>();
      var participantMock3 = mockRepository.StrictMock<IParticipant>();
      var mutableTypeFactoryMock = mockRepository.StrictMock<IMutableTypeFactory>();
      var codeGeneratorMock = mockRepository.StrictMock<IMutableTypeBatchCodeGenerator>();
      participantMock1.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock2.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock3.Stub (_ => _.PartialTypeIdentifierProvider);

      var participantConfigurationID = "participant configuration ID";
      var additionalTypeID = new object();
      bool generationCompletedEventRaised = false;
      var fakeAdditionalType = ReflectionObjectMother.GetSomeType();
      var otherAdditionalTypeID = new object();
      var otherFakeAdditionalType = ReflectionObjectMother.GetSomeType();

      using (mockRepository.Ordered())
      {
        IAdditionalTypeAssemblyContext additionalTypeAssemblyContext = null;
        var additionalMutableType = MutableTypeObjectMother.Create();
        var otherAdditionalType = MutableTypeObjectMother.Create();
        participantMock1
            .Expect (mock => mock.GetOrCreateAdditionalType (Arg.Is (additionalTypeID), Arg<IAdditionalTypeAssemblyContext>.Is.Anything))
            .Return (null)
            .WhenCalled (
                mi =>
                {
                  additionalTypeAssemblyContext = (IAdditionalTypeAssemblyContext) mi.Arguments[1];
                  Assert.That (additionalTypeAssemblyContext.ParticipantConfigurationID, Is.EqualTo (participantConfigurationID));
                  Assert.That (additionalTypeAssemblyContext.ParticipantState, Is.SameAs (_participantStateMock));

                  additionalTypeAssemblyContext.CreateAdditionalType (additionalTypeID, "AdditionalType", null, 0, typeof (int));
                  additionalTypeAssemblyContext.CreateAdditionalType (otherAdditionalTypeID, "OtherAdditionalType", null, 0, typeof (int));

                  additionalTypeAssemblyContext.GenerationCompleted += ctx =>
                  {
                    Assert.That (ctx.GetGeneratedType (additionalMutableType), Is.SameAs (fakeAdditionalType));
                    Assert.That (ctx.GetGeneratedType (otherAdditionalType), Is.SameAs (otherFakeAdditionalType));
                    generationCompletedEventRaised = true;
                  };
                });
        mutableTypeFactoryMock.Expect (mock => mock.CreateType ("AdditionalType", null, 0, typeof (int), null)).Return (additionalMutableType);
        mutableTypeFactoryMock.Expect (mock => mock.CreateType ("OtherAdditionalType", null, 0, typeof (int), null)).Return (otherAdditionalType);

        participantMock2
            .Expect (
                mock => mock.GetOrCreateAdditionalType (
                    Arg.Is (additionalTypeID), Arg<IAdditionalTypeAssemblyContext>.Matches (ctx => ctx == additionalTypeAssemblyContext)))
            .Return (additionalMutableType);
        // Participant 3 is not invoked.

        codeGeneratorMock
            .Expect (mock => mock.GenerateTypes (new[] { additionalMutableType, otherAdditionalType }))
            .Return (
                new[]
                {
                    new KeyValuePair<MutableType, Type> (additionalMutableType, fakeAdditionalType),
                    new KeyValuePair<MutableType, Type> (otherAdditionalType, otherFakeAdditionalType)
                })
            .WhenCalled (mi => Assert.That (generationCompletedEventRaised, Is.False));
      }
      mockRepository.ReplayAll();
      var typeAssembler = CreateTypeAssembler (
          mutableTypeFactoryMock,
          configurationId: participantConfigurationID,
          participants: new[] { participantMock1, participantMock2, participantMock3 });

      var result = typeAssembler.AssembleAdditionalType (additionalTypeID, _participantStateMock, codeGeneratorMock);

      mockRepository.VerifyAll();
      Assert.That (generationCompletedEventRaised, Is.True);
      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (fakeAdditionalType));
      Assert.That (result.AdditionalTypes.Count, Is.EqualTo (2));
      Assert.That (result.AdditionalTypes[additionalTypeID], Is.SameAs (fakeAdditionalType));
      Assert.That (result.AdditionalTypes[otherAdditionalTypeID], Is.SameAs (otherFakeAdditionalType));
    }

    [Test]
    public void AssembleAdditionalType_ParticipantReturnsNonMutableType ()
    {
      var fakeType = ReflectionObjectMother.GetSomeType();
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      var codeGeneratorStub = MockRepository.GenerateStub<IMutableTypeBatchCodeGenerator>();
      participantStub.Stub (_ => _.GetOrCreateAdditionalType (null, null)).IgnoreArguments().Return (fakeType);
      codeGeneratorStub.Stub (_ => _.GenerateTypes (new MutableType[0])).Return (new KeyValuePair<MutableType, Type>[0]);
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantStub });

      var result = typeAssembler.AssembleAdditionalType (new object(), _participantStateMock, codeGeneratorStub);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (fakeType));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "No participant provided an additional type for the given identifier.")]
    public void AssembleAdditionalType_NoParticipantReturnsType ()
    {
      var codeGeneratorStub = MockRepository.GenerateStub<IMutableTypeBatchCodeGenerator>();
      var typeAssembler = CreateTypeAssembler();

      typeAssembler.AssembleAdditionalType (new object(), _participantStateMock, codeGeneratorStub);
    }

    [Test]
    public void GetAdditionalTypeID_OneParticpantProvidesID_ReturnsResultFromParticipant ()
    {
      var additionalType = ReflectionObjectMother.GetSomeOtherType();
      var expectedAdditionalTypeID = new object();

      var participantMock1 = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock1.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock1.Expect (_ => _.GetAdditionalTypeID (additionalType)).Return (null);

      var participantMock2 = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock2.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock2.Expect (_ => _.GetAdditionalTypeID (additionalType)).Return (expectedAdditionalTypeID);

      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1, participantMock2 });
      var additionalTypeID = typeAssembler.GetAdditionalTypeID (additionalType);

      Assert.That (additionalTypeID, Is.SameAs (expectedAdditionalTypeID));
      participantMock1.VerifyAllExpectations();
      participantMock2.VerifyAllExpectations();
    }

    [Test]
    public void GetAdditionalTypeID_NoParticpantProvidesID_ReturnsNull ()
    {
      var additionalType = ReflectionObjectMother.GetSomeOtherType();

      var participantMock = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock.Expect (_ => _.GetAdditionalTypeID (additionalType)).Return (null);

      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock });
      var additionalTypeID = typeAssembler.GetAdditionalTypeID (additionalType);

      Assert.That (additionalTypeID, Is.Null);
    }

    [Test]
    public void GetAdditionalTypeID_MultipleParticpantsProvidesID_ThrowsInvalidOperationException ()
    {
      var additionalType = ReflectionObjectMother.GetSomeOtherType();

      var participantMock1 = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock1.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock1.Expect (_ => _.GetAdditionalTypeID (additionalType)).Return (null);

      var participantMock2 = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock2.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock2.Expect (_ => _.GetAdditionalTypeID (additionalType)).Return (new object());

      var participantMock3 = MockRepository.GenerateStrictMock<IParticipant>();
      participantMock3.Stub (_ => _.PartialTypeIdentifierProvider);
      participantMock3.Expect (_ => _.GetAdditionalTypeID (additionalType)).Return (new object());

      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1, participantMock2, participantMock3 });

      Assert.That (
          () => typeAssembler.GetAdditionalTypeID (additionalType),
          Throws.InvalidOperationException
              .And.Message.EqualTo (string.Format ("More than one participant returned an ID for the additional type '{0}'", additionalType.Name)));
    }

    private TypeAssembler CreateTypeAssembler (
        IMutableTypeFactory mutableTypeFactory = null,
        IAssembledTypeIdentifierProvider assembledTypeIdentifierProvider = null,
        IComplexSerializationEnabler complexSerializationEnabler = null,
        string configurationId = "id",
        params IParticipant[] participants)
    {
      mutableTypeFactory = mutableTypeFactory ?? _mutableTypeFactoryMock;
      // Do not fix up assembledTypeIdentifierProvider.
      complexSerializationEnabler = complexSerializationEnabler ?? _complexSerializationEnablerMock;

      var typeAssembler = new TypeAssembler (configurationId, participants.AsOneTime(), mutableTypeFactory, complexSerializationEnabler);
      if (assembledTypeIdentifierProvider != null)
        PrivateInvoke.SetNonPublicField (typeAssembler, "_assembledTypeIdentifierProvider", assembledTypeIdentifierProvider);

      return typeAssembler;
    }

    private class RequestedType {}
    [AssembledType] private class AssembledType : RequestedType {}
    private class AssembledTypeSubclass : AssembledType {}
  }
}