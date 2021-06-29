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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.TypeAssembly;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    private Mock<IMutableTypeFactory> _mutableTypeFactoryMock;
    private Mock<IComplexSerializationEnabler> _complexSerializationEnablerMock;
    private Mock<IParticipantState> _participantStateMock;

    private AssembledTypeID _typeID;
    private Type _requestedType;
    private Type _assembledType;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeFactoryMock = new Mock<IMutableTypeFactory> (MockBehavior.Strict);
      _complexSerializationEnablerMock = new Mock<IComplexSerializationEnabler> (MockBehavior.Strict);
      _participantStateMock = new Mock<IParticipantState> (MockBehavior.Strict);

      _requestedType = typeof (RequestedType);
      _typeID = AssembledTypeIDObjectMother.Create (_requestedType);
      _assembledType = typeof (AssembledType);
    }

    [Test]
    public void Initialization ()
    {
      var participantStub = new Mock<IParticipant>();
      var participantWithCacheProviderStub = new Mock<IParticipant>();
      var identifierProviderStub = new Mock<ITypeIdentifierProvider>();
      participantWithCacheProviderStub.SetupGet (stub => stub.PartialTypeIdentifierProvider).Returns (identifierProviderStub.Object);
      var participants = new[] { participantStub.Object, participantWithCacheProviderStub.Object };

      var typeAssembler = new TypeAssembler ("configId", participants.AsOneTime(), _mutableTypeFactoryMock.Object, _complexSerializationEnablerMock.Object);

      Assert.That (typeAssembler.ParticipantConfigurationID, Is.EqualTo ("configId"));
      Assert.That (typeAssembler.Participants, Is.EqualTo (participants));

      var assembledTypeIdentifierProvider = PrivateInvoke.GetNonPublicField (typeAssembler, "_assembledTypeIdentifierProvider");
      Assert.That (assembledTypeIdentifierProvider, Is.TypeOf<AssembledTypeIdentifierProvider>());
      var identifierProviders = PrivateInvoke.GetNonPublicField (assembledTypeIdentifierProvider, "_identifierProviders");
      Assert.That (identifierProviders, Is.EqualTo (new[] { identifierProviderStub.Object }));
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
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo ("The argument type 'Int32' is not an assembled type.", "assembledType"));
    }

    [Test]
    public void ComputeTypeID ()
    {
      var participantMock1 = new Mock<IParticipant> (MockBehavior.Strict);
      var participantMock2 = new Mock<IParticipant> (MockBehavior.Strict);
      var idProviderMock1 = new Mock<ITypeIdentifierProvider> (MockBehavior.Strict);
      var idProviderMock2 = new Mock<ITypeIdentifierProvider> (MockBehavior.Strict);
      participantMock1.SetupGet (mock => mock.PartialTypeIdentifierProvider).Returns (idProviderMock1.Object).Verifiable();
      participantMock2.SetupGet (mock => mock.PartialTypeIdentifierProvider).Returns (idProviderMock2.Object).Verifiable();
      idProviderMock1.Setup (mock => mock.GetID (_requestedType)).Returns (1).Verifiable();
      idProviderMock2.Setup (mock => mock.GetID (_requestedType)).Returns ("2").Verifiable();
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1.Object, participantMock2.Object });

      var result = typeAssembler.ComputeTypeID (_requestedType);

      Assert.That (result, Is.EqualTo (new AssembledTypeID (_requestedType, new object[] { 1, "2" })));
    }

    [Test]
    public void ExtractTypeID ()
    {
      var otherType = typeof (int);
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      var assembledTypeIdentifierProviderStub = new Mock<IAssembledTypeIdentifierProvider>();
      assembledTypeIdentifierProviderStub.Setup (_ => _.ExtractTypeID (_assembledType)).Returns (fakeTypeID);

      var typeAssembler = CreateTypeAssembler (assembledTypeIdentifierProvider: assembledTypeIdentifierProviderStub.Object);

      Assert.That (typeAssembler.ExtractTypeID (_assembledType), Is.EqualTo (fakeTypeID));
      Assert.That (
          () => typeAssembler.ExtractTypeID (otherType),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo ("The argument type 'Int32' is not an assembled type.", "assembledType"));
    }

    [Test]
    public void AssembleType ()
    {
      var participantMock1 = new Mock<IParticipant> (MockBehavior.Strict);
      var participantMock2 = new Mock<IParticipant> (MockBehavior.Strict);
      var mutableTypeFactoryMock = new Mock<IMutableTypeFactory> (MockBehavior.Strict);
      var assembledTypeIdentifierProviderMock = new Mock<IAssembledTypeIdentifierProvider> (MockBehavior.Strict);
      var complexSerializationEnablerMock = new Mock<IComplexSerializationEnabler> (MockBehavior.Strict);
      var codeGeneratorMock = new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict);

      var participantConfigurationID = "participant configuration id";
      var typeID = AssembledTypeIDObjectMother.Create (_requestedType, new object[] { "type id part" });
      var additionalTypeID = new object();
      var generationCompletedEventRaised = false;
      var fakeGeneratedType = ReflectionObjectMother.GetSomeType();
      var fakeGeneratedAdditionalType = ReflectionObjectMother.GetSomeType();

      var sequence = new MockSequence();
      var typeIdentifierProviderMock = new Mock<ITypeIdentifierProvider> (MockBehavior.Strict);
      participantMock1
          .InSequence (sequence)
          .SetupGet (mock => mock.PartialTypeIdentifierProvider)
          .Returns (typeIdentifierProviderMock.Object);
      participantMock2
          .InSequence (sequence)
          .SetupGet (mock => mock.PartialTypeIdentifierProvider)
          .Returns ((ITypeIdentifierProvider) null);

      var proxyType = MutableTypeObjectMother.Create();
      var typeModificationTrackerMock = new Mock<ITypeModificationTracker> (MockBehavior.Strict);
      mutableTypeFactoryMock
          .InSequence (sequence)
          .Setup (mock => mock.CreateProxy (_requestedType, ProxyKind.AssembledType))
          .Returns (typeModificationTrackerMock.Object);
      typeModificationTrackerMock
          .InSequence (sequence)
          .SetupGet (stub => stub.Type)
          .Returns (proxyType);

      var idPart = new object();
      assembledTypeIdentifierProviderMock
          .InSequence (sequence)
          .Setup (mock => mock.GetPart (It.Is<AssembledTypeID> (id => id.Equals (typeID)), participantMock1.Object))
          .Returns (idPart);

      IProxyTypeAssemblyContext proxyTypeAssemblyContextPassedToParticipant1 = null;
      participantMock1
          .InSequence (sequence)
          .Setup (mock => mock.Participate (idPart, It.IsAny<IProxyTypeAssemblyContext>()))
          .Callback (
              (object id, IProxyTypeAssemblyContext proxyTypeAssemblyContextArg) =>
              {
                proxyTypeAssemblyContextPassedToParticipant1 = proxyTypeAssemblyContextArg;
                Assert.That (proxyTypeAssemblyContextPassedToParticipant1.ParticipantConfigurationID, Is.EqualTo (participantConfigurationID));
                Assert.That (proxyTypeAssemblyContextPassedToParticipant1.ParticipantState, Is.SameAs (_participantStateMock.Object));
                Assert.That (proxyTypeAssemblyContextPassedToParticipant1.RequestedType, Is.SameAs (_requestedType));
                Assert.That (proxyTypeAssemblyContextPassedToParticipant1.ProxyType, Is.SameAs (proxyType));

                proxyTypeAssemblyContextPassedToParticipant1.CreateAdditionalType (additionalTypeID, "AdditionalType", null, 0, typeof (int));

                proxyTypeAssemblyContextPassedToParticipant1.GenerationCompleted += ctx =>
                {
                  Assert.That (ctx.GetGeneratedType (proxyType), Is.SameAs (fakeGeneratedType));
                  generationCompletedEventRaised = true;
                };
              });
      var additionalType = MutableTypeObjectMother.Create();
      mutableTypeFactoryMock
          .InSequence (sequence)
          .Setup (mock => mock.CreateType ("AdditionalType", null, 0, typeof (int), null)).Returns (additionalType);

      assembledTypeIdentifierProviderMock
          .InSequence (sequence)
          .Setup (mock => mock.GetPart (It.Is<AssembledTypeID> (id => id.Equals (typeID)), participantMock2.Object))
          .Returns ((object) null);
      participantMock2
          .InSequence (sequence)
          .Setup (mock => mock.Participate (null, It.IsAny<IProxyTypeAssemblyContext>()))
          .Callback (
              (object _, IProxyTypeAssemblyContext proxyTypeAssemblyContext) =>
              {
                Assert.That (proxyTypeAssemblyContext, Is.EqualTo (proxyTypeAssemblyContextPassedToParticipant1));
              });

      typeModificationTrackerMock
          .InSequence (sequence)
          .Setup (mock => mock.IsModified()).Returns (true);

      assembledTypeIdentifierProviderMock
          .InSequence (sequence)
          .Setup (mock => mock.AddTypeID (proxyType, It.Is<AssembledTypeID> (id => id.Equals (typeID))));
      complexSerializationEnablerMock
          .InSequence (sequence)
          .Setup (
              mock => mock.MakeSerializable (
                  proxyType,
                  participantConfigurationID,
                  assembledTypeIdentifierProviderMock.Object,
                  It.Is<AssembledTypeID> (id => id.Equals (typeID))));

      codeGeneratorMock
          .InSequence (sequence)
          .Setup (mock => mock.GenerateTypes (It.Is<IEnumerable<MutableType>> (mutableTypes => mutableTypes.SequenceEqual (new[] { additionalType, proxyType }))))
          .Returns (
              new[]
              {
                  new KeyValuePair<MutableType, Type> (proxyType, fakeGeneratedType),
                  new KeyValuePair<MutableType, Type> (additionalType, fakeGeneratedAdditionalType)
              })
          .Callback (
              (IEnumerable<MutableType> _) =>
              {
                Assert.That (generationCompletedEventRaised, Is.False);

                var proxyAttribute = proxyType.AddedCustomAttributes.Single();
                Assert.That (proxyAttribute.Type, Is.SameAs (typeof (AssembledTypeAttribute)));
                Assert.That (proxyAttribute.ConstructorArguments, Is.Empty);
                Assert.That (proxyAttribute.NamedArguments, Is.Empty);
              });

      var typeAssembler = CreateTypeAssembler (
          mutableTypeFactoryMock.Object,
          assembledTypeIdentifierProviderMock.Object,
          complexSerializationEnablerMock.Object,
          participantConfigurationID,
          new[] { participantMock1.Object, participantMock2.Object });

      var result = typeAssembler.AssembleType (typeID, _participantStateMock.Object, codeGeneratorMock.Object);

      participantMock1.Verify();
      participantMock2.Verify();
      mutableTypeFactoryMock.Verify();
      typeModificationTrackerMock.Verify();
      assembledTypeIdentifierProviderMock.Verify();
      codeGeneratorMock.Verify();

      Assert.That (generationCompletedEventRaised, Is.True);
      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (fakeGeneratedType));
      Assert.That (result.AdditionalTypes.Count, Is.EqualTo (1));
      Assert.That (result.AdditionalTypes[additionalTypeID], Is.SameAs (fakeGeneratedAdditionalType));
    }

    [Test]
    public void AssembleType_RequestAssembledType ()
    {
      var typeAssembler = CreateTypeAssembler();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType: _assembledType);
      var codeGeneratorStub = new Mock<IMutableTypeBatchCodeGenerator>();
      Assert.That (
          () => typeAssembler.AssembleType (typeID, _participantStateMock.Object, codeGeneratorStub.Object),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The provided requested type 'AssembledType' is already an assembled type."));
    }

    [Test]
    public void AssembleType_NonSubclassableType_LetParticipantReportErrors_AndReturnsRequestedType ()
    {
      var participantMock = new Mock<IParticipant>();
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock.Object });
      var nonSubclassableType = ReflectionObjectMother.GetSomeNonSubclassableType();
      var typeID = AssembledTypeIDObjectMother.Create (nonSubclassableType);
      var codeGeneratorStub = new Mock<IMutableTypeBatchCodeGenerator>();

      var result = typeAssembler.AssembleType (typeID, _participantStateMock.Object, codeGeneratorStub.Object);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (nonSubclassableType));
      participantMock.Verify (mock => mock.HandleNonSubclassableType (nonSubclassableType), Times.Once());
      participantMock.Verify (mock => mock.Participate (null, It.IsAny<IProxyTypeAssemblyContext>()), Times.Never());
    }

    [Test]
    public void AssembleType_NoModifications_ReturnsRequestedType ()
    {
      var mutableTypeFactoryStub = new Mock<IMutableTypeFactory>();
      var typeModificationContextStub = new Mock<ITypeModificationTracker>();
      mutableTypeFactoryStub.Setup (_ => _.CreateProxy (_requestedType, ProxyKind.AssembledType)).Returns (typeModificationContextStub.Object);
      typeModificationContextStub.SetupGet (_ => _.Type).Returns (MutableTypeObjectMother.Create());
      var typeAssembler = CreateTypeAssembler (mutableTypeFactoryStub.Object);
      var codeGeneratorMock = new Mock<IMutableTypeBatchCodeGenerator>();

      var result = typeAssembler.AssembleType (_typeID, _participantStateMock.Object, codeGeneratorMock.Object);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (_requestedType));
      codeGeneratorMock.Verify (mock => mock.GenerateTypes (It.IsAny<IEnumerable<MutableType>>()), Times.Never());
    }

    [Test]
    public void AssembleType_ExceptionInCodeGeneraton ()
    {
      var typeModificationContextStub = new Mock<ITypeModificationTracker>();
      typeModificationContextStub.Setup (_ => _.Type).Returns (() => MutableTypeObjectMother.Create());
      typeModificationContextStub.Setup (_ => _.IsModified()).Returns (true);
      _mutableTypeFactoryMock.Setup (_ => _.CreateProxy (_requestedType, ProxyKind.AssembledType)).Returns (typeModificationContextStub.Object);
      _complexSerializationEnablerMock.Setup (_ => _.MakeSerializable (It.IsAny<MutableType>(), It.IsAny<string>(), It.IsAny<IAssembledTypeIdentifierProvider>(), It.IsAny<AssembledTypeID>()));
      var typeAssemblyContextCodeGeneratorMock = new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict);
      var exception1 = new InvalidOperationException ("blub");
      var exception2 = new NotSupportedException ("blub");
      var exception3 = new Exception();
      var sequence = new MockSequence();
      typeAssemblyContextCodeGeneratorMock.InSequence (sequence).Setup (mock => mock.GenerateTypes (It.IsAny<IEnumerable<MutableType>>())).Throws (exception1);
      typeAssemblyContextCodeGeneratorMock.InSequence (sequence).Setup (mock => mock.GenerateTypes (It.IsAny<IEnumerable<MutableType>>())).Throws (exception2);
      typeAssemblyContextCodeGeneratorMock.InSequence (sequence).Setup (mock => mock.GenerateTypes (It.IsAny<IEnumerable<MutableType>>())).Throws (exception3);
      var typeAssembler = CreateTypeAssembler (participants: new Mock<IParticipant>().Object);

      var expectedMessageRegex = "An error occurred during code generation for '" + _requestedType.Name + "':\r\nblub\r\n"
                                 + @"The following participants are currently configured and may have caused the error: 'IParticipantProxy.*'\.";
      Assert.That (
          () => typeAssembler.AssembleType (_typeID, _participantStateMock.Object, typeAssemblyContextCodeGeneratorMock.Object),
          Throws.InvalidOperationException.With.InnerException.SameAs (exception1).And.With.Message.Matches (expectedMessageRegex));
      Assert.That (
          () => typeAssembler.AssembleType (_typeID, _participantStateMock.Object, typeAssemblyContextCodeGeneratorMock.Object),
          Throws.TypeOf<NotSupportedException>().With.InnerException.SameAs (exception2).And.With.Message.Matches (expectedMessageRegex));

      Assert.That (
          () => typeAssembler.AssembleType (_typeID, _participantStateMock.Object, typeAssemblyContextCodeGeneratorMock.Object),
          Throws.Exception.SameAs (exception3));
    }

    [Test]
    public void AssembleAdditionalType_ParticipantReturnsMutableType ()
    {
      var participantMock1 = new Mock<IParticipant> (MockBehavior.Strict);
      var participantMock2 = new Mock<IParticipant> (MockBehavior.Strict);
      var participantMock3 = new Mock<IParticipant> (MockBehavior.Strict);
      var mutableTypeFactoryMock = new Mock<IMutableTypeFactory> (MockBehavior.Strict);
      var codeGeneratorMock = new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict);
      participantMock1.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock2.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock3.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);

      var participantConfigurationID = "participant configuration ID";
      var additionalTypeID = new object();
      var generationCompletedEventRaised = false;
      var fakeAdditionalType = ReflectionObjectMother.GetSomeType();
      var otherAdditionalTypeID = new object();
      var otherFakeAdditionalType = ReflectionObjectMother.GetSomeType();

      IAdditionalTypeAssemblyContext additionalTypeAssemblyContextPassedToParticipant1 = null;
      var additionalMutableType = MutableTypeObjectMother.Create();
      var otherAdditionalType = MutableTypeObjectMother.Create();
      var sequence = new MockSequence();
      participantMock1
          .InSequence (sequence)
          .Setup (mock => mock.GetOrCreateAdditionalType (additionalTypeID, It.IsAny<IAdditionalTypeAssemblyContext>()))
          .Returns ((Type) null)
          .Callback (
              (object _, IAdditionalTypeAssemblyContext additionalTypeAssemblyContext) =>
              {
                additionalTypeAssemblyContextPassedToParticipant1 = additionalTypeAssemblyContext;
                Assert.That (additionalTypeAssemblyContext.ParticipantConfigurationID, Is.EqualTo (participantConfigurationID));
                Assert.That (additionalTypeAssemblyContext.ParticipantState, Is.SameAs (_participantStateMock.Object));

                additionalTypeAssemblyContext.CreateAdditionalType (additionalTypeID, "AdditionalType", null, 0, typeof (int));
                additionalTypeAssemblyContext.CreateAdditionalType (otherAdditionalTypeID, "OtherAdditionalType", null, 0, typeof (int));

                additionalTypeAssemblyContext.GenerationCompleted += ctx =>
                {
                  Assert.That (ctx.GetGeneratedType (additionalMutableType), Is.SameAs (fakeAdditionalType));
                  Assert.That (ctx.GetGeneratedType (otherAdditionalType), Is.SameAs (otherFakeAdditionalType));
                  generationCompletedEventRaised = true;
                };
              });
      mutableTypeFactoryMock
          .InSequence (sequence)
          .Setup (mock => mock.CreateType ("AdditionalType", null, 0, typeof (int), null)).Returns (additionalMutableType);
      mutableTypeFactoryMock
          .InSequence (sequence)
          .Setup (mock => mock.CreateType ("OtherAdditionalType", null, 0, typeof (int), null)).Returns (otherAdditionalType);

      participantMock2
          .InSequence (sequence)
          .Setup (mock => mock.GetOrCreateAdditionalType (additionalTypeID, It.IsAny<IAdditionalTypeAssemblyContext>()))
          .Returns (additionalMutableType)
          .Callback (
              (object _, IAdditionalTypeAssemblyContext additionalTypeAssemblyContextArg) =>
              {
                Assert.That (additionalTypeAssemblyContextArg, Is.SameAs (additionalTypeAssemblyContextPassedToParticipant1));
              });
      // Participant 3 is not invoked.

      codeGeneratorMock
          .InSequence (sequence)
          .Setup (mock => mock.GenerateTypes (new[] { additionalMutableType, otherAdditionalType }))
          .Returns (
              new[]
              {
                  new KeyValuePair<MutableType, Type> (additionalMutableType, fakeAdditionalType),
                  new KeyValuePair<MutableType, Type> (otherAdditionalType, otherFakeAdditionalType)
              })
          .Callback ((IEnumerable<MutableType> mutableTypes) => Assert.That (generationCompletedEventRaised, Is.False));
      var typeAssembler = CreateTypeAssembler (
          mutableTypeFactoryMock.Object,
          configurationId: participantConfigurationID,
          participants: new[] { participantMock1.Object, participantMock2.Object, participantMock3.Object });

      var result = typeAssembler.AssembleAdditionalType (additionalTypeID, _participantStateMock.Object, codeGeneratorMock.Object);

      participantMock1.Verify();
      participantMock2.Verify();
      participantMock3.Verify();
      mutableTypeFactoryMock.Verify();
      codeGeneratorMock.Verify();
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
      var participantStub = new Mock<IParticipant>();
      var codeGeneratorStub = new Mock<IMutableTypeBatchCodeGenerator>();
      participantStub.Setup (_ => _.GetOrCreateAdditionalType (It.IsAny<object>(), It.IsAny<IAdditionalTypeAssemblyContext>())).Returns (fakeType);
      codeGeneratorStub.Setup (_ => _.GenerateTypes (new MutableType[0])).Returns (new KeyValuePair<MutableType, Type>[0]);
      var typeAssembler = CreateTypeAssembler (participants: new[] { participantStub.Object });

      var result = typeAssembler.AssembleAdditionalType (new object(), _participantStateMock.Object, codeGeneratorStub.Object);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Type, Is.SameAs (fakeType));
    }

    [Test]
    public void AssembleAdditionalType_NoParticipantReturnsType ()
    {
      var codeGeneratorStub = new Mock<IMutableTypeBatchCodeGenerator>();
      var typeAssembler = CreateTypeAssembler();
      Assert.That (
          () => typeAssembler.AssembleAdditionalType (new object(), _participantStateMock.Object, codeGeneratorStub.Object),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "No participant provided an additional type for the given identifier."));
    }

    [Test]
    public void GetAdditionalTypeID_OneParticpantProvidesID_ReturnsResultFromParticipant ()
    {
      var additionalType = ReflectionObjectMother.GetSomeOtherType();
      var expectedAdditionalTypeID = new object();

      var participantMock1 = new Mock<IParticipant> (MockBehavior.Strict);
      participantMock1.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock1.Setup (_ => _.GetAdditionalTypeID (additionalType)).Returns (null).Verifiable();

      var participantMock2 = new Mock<IParticipant> (MockBehavior.Strict);
      participantMock2.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock2.Setup (_ => _.GetAdditionalTypeID (additionalType)).Returns (expectedAdditionalTypeID).Verifiable();

      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1.Object, participantMock2.Object });
      var additionalTypeID = typeAssembler.GetAdditionalTypeID (additionalType);

      Assert.That (additionalTypeID, Is.SameAs (expectedAdditionalTypeID));
      participantMock1.Verify();
      participantMock2.Verify();
    }

    [Test]
    public void GetAdditionalTypeID_NoParticpantProvidesID_ReturnsNull ()
    {
      var additionalType = ReflectionObjectMother.GetSomeOtherType();

      var participantMock = new Mock<IParticipant> (MockBehavior.Strict);
      participantMock.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock.Setup (_ => _.GetAdditionalTypeID (additionalType)).Returns (null).Verifiable();

      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock.Object });
      var additionalTypeID = typeAssembler.GetAdditionalTypeID (additionalType);

      Assert.That (additionalTypeID, Is.Null);
    }

    [Test]
    public void GetAdditionalTypeID_MultipleParticpantsProvidesID_ThrowsInvalidOperationException ()
    {
      var additionalType = ReflectionObjectMother.GetSomeOtherType();

      var participantMock1 = new Mock<IParticipant> (MockBehavior.Strict);
      participantMock1.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock1.Setup (_ => _.GetAdditionalTypeID (additionalType)).Returns (null).Verifiable();

      var participantMock2 = new Mock<IParticipant> (MockBehavior.Strict);
      participantMock2.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock2.Setup (_ => _.GetAdditionalTypeID (additionalType)).Returns (new object()).Verifiable();

      var participantMock3 = new Mock<IParticipant> (MockBehavior.Strict);
      participantMock3.SetupGet (_ => _.PartialTypeIdentifierProvider).Returns (new Mock<ITypeIdentifierProvider>().Object);
      participantMock3.Setup (_ => _.GetAdditionalTypeID (additionalType)).Returns (new object()).Verifiable();

      var typeAssembler = CreateTypeAssembler (participants: new[] { participantMock1.Object, participantMock2.Object, participantMock3.Object });

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
      mutableTypeFactory = mutableTypeFactory ?? _mutableTypeFactoryMock.Object;
      // Do not fix up assembledTypeIdentifierProvider.
      complexSerializationEnabler = complexSerializationEnabler ?? _complexSerializationEnablerMock.Object;

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