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
using System.Collections.ObjectModel;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class PipelineTest
  {
    private PipelineSettings _settings;
    private Mock<ICodeManager> _codeManagerMock;
    private Mock<IReflectionService> _reflectionServiceMock;
    private Mock<ITypeAssembler> _typeAssemblerMock;

    private Pipeline _pipeline;

    private Type _requestedType;
    private AssembledTypeID _typeID;

    [SetUp]
    public void SetUp ()
    {
      _settings = PipelineSettings.New().Build();
      _codeManagerMock = new Mock<ICodeManager> (MockBehavior.Strict);
      _reflectionServiceMock = new Mock<IReflectionService> (MockBehavior.Strict);
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);

      _pipeline = new Pipeline (_settings, _codeManagerMock.Object, _reflectionServiceMock.Object, _typeAssemblerMock.Object);

      _requestedType = ReflectionObjectMother.GetSomeType();
      _typeID = AssembledTypeIDObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_pipeline.Settings, Is.SameAs (_settings));
      Assert.That (_pipeline.CodeManager, Is.SameAs (_codeManagerMock.Object));
      Assert.That (_pipeline.ReflectionService, Is.SameAs (_reflectionServiceMock.Object));
    }

    [Test]
    public void ParticipantConfigurationID ()
    {
      _typeAssemblerMock.SetupGet (mock => mock.ParticipantConfigurationID).Returns ("configId").Verifiable();

      Assert.That (_pipeline.ParticipantConfigurationID, Is.EqualTo ("configId"));
    }

    [Test]
    public void Participants ()
    {
      var participants = new ReadOnlyCollection<IParticipant> (new IParticipant[0]);
      _typeAssemblerMock.SetupGet (mock => mock.Participants).Returns (participants).Verifiable();

      Assert.That (_pipeline.Participants, Is.SameAs (participants));
    }

    [Test]
    public void CreateObject_NoConstructorArguments ()
    {
      _reflectionServiceMock.Setup (mock => mock.GetTypeIDForRequestedType (_requestedType)).Returns (_typeID).Verifiable();
      _reflectionServiceMock
          .Setup (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (_typeID)),
                  ParamList.Empty,
                  false))
          .Returns ("default .ctor")
          .Verifiable();

      var result = _pipeline.Create (_requestedType);

      Assert.That (result, Is.EqualTo ("default .ctor"));
    }

    [Test]
    public void CreateObject_ConstructorArguments ()
    {
      _reflectionServiceMock.Setup (mock => mock.GetTypeIDForRequestedType (_requestedType)).Returns (_typeID).Verifiable();
      var arguments = ParamList.Create ("abc", 7);
      _reflectionServiceMock
          .Setup (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (_typeID)),
                  arguments,
                  false))
          .Returns ("abc, 7")
          .Verifiable();

      var result = _pipeline.Create (_requestedType, arguments);

      Assert.That (result, Is.EqualTo ("abc, 7"));
    }

    [Test]
    public void CreateObject_NonPublicConstructor ()
    {
      _reflectionServiceMock.Setup (mock => mock.GetTypeIDForRequestedType (_requestedType)).Returns (_typeID).Verifiable();
      const bool allowNonPublic = true;
      _reflectionServiceMock
          .Setup (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (_typeID)),
                  ParamList.Empty,
                  allowNonPublic))
          .Returns ("non-public .ctor")
          .Verifiable();

      var result = _pipeline.Create (_requestedType, allowNonPublicConstructor: allowNonPublic);

      Assert.That (result, Is.EqualTo ("non-public .ctor"));
    }

    [Test]
    public void CreateObject_Generic ()
    {
      var typeID = new AssembledTypeID (typeof (RequestedType), new object[0]);
      _reflectionServiceMock.Setup (mock => mock.GetTypeIDForRequestedType (typeof (RequestedType))).Returns (typeID).Verifiable();
      var assembledInstance = new AssembledType();
      _reflectionServiceMock
          .Setup (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (typeID)),
                  ParamList.Empty,
                  false))
          .Returns (assembledInstance)
          .Verifiable();

      var result = _pipeline.Create<RequestedType>();

      Assert.That (result, Is.SameAs (assembledInstance));
    }

    class RequestedType { }
    class AssembledType : RequestedType { }
  }
}