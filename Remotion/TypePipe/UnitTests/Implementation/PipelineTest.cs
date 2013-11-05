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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class PipelineTest
  {
    private PipelineSettings _settings;
    private ICodeManager _codeManagerMock;
    private IReflectionService _reflectionServiceMock;
    private ITypeAssembler _typeAssemblerMock;

    private Pipeline _pipeline;

    private Type _requestedType;
    private AssembledTypeID _typeID;

    [SetUp]
    public void SetUp ()
    {
      _settings = PipelineSettings.New().Build();
      _codeManagerMock = MockRepository.GenerateStrictMock<ICodeManager>();
      _reflectionServiceMock = MockRepository.GenerateStrictMock<IReflectionService>();
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();

      _pipeline = new Pipeline (_settings, _codeManagerMock, _reflectionServiceMock, _typeAssemblerMock);

      _requestedType = ReflectionObjectMother.GetSomeType();
      _typeID = AssembledTypeIDObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_pipeline.Settings, Is.SameAs (_settings));
      Assert.That (_pipeline.CodeManager, Is.SameAs (_codeManagerMock));
      Assert.That (_pipeline.ReflectionService, Is.SameAs (_reflectionServiceMock));
    }

    [Test]
    public void ParticipantConfigurationID ()
    {
      _typeAssemblerMock.Expect (mock => mock.ParticipantConfigurationID).Return ("configId");

      Assert.That (_pipeline.ParticipantConfigurationID, Is.EqualTo ("configId"));
    }

    [Test]
    public void Participants ()
    {
      var participants = new ReadOnlyCollection<IParticipant> (new IParticipant[0]);
      _typeAssemblerMock.Expect (mock => mock.Participants).Return (participants);

      Assert.That (_pipeline.Participants, Is.SameAs (participants));
    }

    [Test]
    public void CreateObject_NoConstructorArguments ()
    {
      _reflectionServiceMock.Expect (mock => mock.GetTypeIDForRequestedType (_requestedType)).Return (_typeID);
      _reflectionServiceMock
          .Expect (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (_typeID)),
                  Arg.Is (ParamList.Empty),
                  Arg.Is (false)))
          .Return ("default .ctor");

      var result = _pipeline.Create (_requestedType);

      Assert.That (result, Is.EqualTo ("default .ctor"));
    }

    [Test]
    public void CreateObject_ConstructorArguments ()
    {
      _reflectionServiceMock.Expect (mock => mock.GetTypeIDForRequestedType (_requestedType)).Return (_typeID);
      var arguments = ParamList.Create ("abc", 7);
      _reflectionServiceMock
          .Expect (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (_typeID)),
                  Arg.Is (arguments),
                  Arg.Is (false)))
          .Return ("abc, 7");

      var result = _pipeline.Create (_requestedType, arguments);

      Assert.That (result, Is.EqualTo ("abc, 7"));
    }

    [Test]
    public void CreateObject_NonPublicConstructor ()
    {
      _reflectionServiceMock.Expect (mock => mock.GetTypeIDForRequestedType (_requestedType)).Return (_typeID);
      const bool allowNonPublic = true;
      _reflectionServiceMock
          .Expect (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (_typeID)),
                  Arg.Is (ParamList.Empty),
                  Arg.Is (allowNonPublic)))
          .Return ("non-public .ctor");

      var result = _pipeline.Create (_requestedType, allowNonPublicConstructor: allowNonPublic);

      Assert.That (result, Is.EqualTo ("non-public .ctor"));
    }

    [Test]
    public void CreateObject_Generic ()
    {
      var typeID = new AssembledTypeID (typeof (RequestedType), new object[0]);
      _reflectionServiceMock.Expect (mock => mock.GetTypeIDForRequestedType (typeof (RequestedType))).Return (typeID);
      var assembledInstance = new AssembledType();
      _reflectionServiceMock
          .Expect (
              mock => mock.InstantiateAssembledType (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)),
                  Arg.Is (ParamList.Empty),
                  Arg.Is (false)))
          .Return (assembledInstance);

      var result = _pipeline.Create<RequestedType>();

      Assert.That (result, Is.SameAs (assembledInstance));
    }

    class RequestedType { }
    class AssembledType : RequestedType { }
  }
}