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
using Remotion.Development.UnitTesting.ObjectMothers;
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
    private ITypeCache _typeCacheMock;
    private ICodeManager _codeManagerMock;
    private IReflectionService _reflectionServiceMock;
    private ITypeAssembler _typeAssemblerMock;

    private Pipeline _pipeline;

    private Type _requestedType;
    private AssembledTypeID _typeID;
    private IConstructorCallCache _constructorCallCacheMock;

    [SetUp]
    public void SetUp ()
    {
      _settings = PipelineSettings.New().Build();
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _codeManagerMock = MockRepository.GenerateStrictMock<ICodeManager>();
      _reflectionServiceMock = MockRepository.GenerateStrictMock<IReflectionService>();
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _constructorCallCacheMock = MockRepository.GenerateStrictMock<IConstructorCallCache>();

      _pipeline = new Pipeline (_settings, _typeCacheMock, _codeManagerMock, _reflectionServiceMock, _typeAssemblerMock, _constructorCallCacheMock);

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
      _typeCacheMock.Expect (mock => mock.ParticipantConfigurationID).Return ("configId");

      Assert.That (_pipeline.ParticipantConfigurationID, Is.EqualTo ("configId"));
    }

    [Test]
    public void Participants ()
    {
      var participants = new ReadOnlyCollection<IParticipant> (new IParticipant[0]);
      _typeCacheMock.Expect (mock => mock.Participants).Return (participants);

      Assert.That (_pipeline.Participants, Is.SameAs (participants));
    }

    [Test]
    public void CreateObject_NoConstructorArguments ()
    {
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (_typeID);
      _constructorCallCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (_typeID)),
                  Arg.Is (typeof (Func<object>)),
                  Arg.Is (false)))
          .Return (new Func<object> (() => "default .ctor"));

      var result = _pipeline.Create (_requestedType);

      Assert.That (result, Is.EqualTo ("default .ctor"));
    }

    [Test]
    public void CreateObject_ConstructorArguments ()
    {
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (_typeID);
      var arguments = ParamList.Create ("abc", 7);
      _constructorCallCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (_typeID)),
                  Arg.Is (arguments.FuncType),
                  Arg.Is (false)))
          .Return (
              new Func<string, int, object> (
                  (s, i) =>
                  {
                    Assert.That (s, Is.EqualTo ("abc"));
                    Assert.That (i, Is.EqualTo (7));
                    return "abc, 7";
                  }));

      var result = _pipeline.Create (_requestedType, arguments);

      Assert.That (result, Is.EqualTo ("abc, 7"));
    }

    [Test]
    public void CreateObject_NonPublicConstructor ()
    {
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (_requestedType)).Return (_typeID);
      const bool allowNonPublic = true;
      _constructorCallCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (_typeID)),
                  Arg.Is (typeof (Func<object>)),
                  Arg.Is (allowNonPublic)))
          .Return (new Func<object> (() => "non-public .ctor"));

      var result = _pipeline.Create (_requestedType, allowNonPublicConstructor: allowNonPublic);

      Assert.That (result, Is.EqualTo ("non-public .ctor"));
    }

    [Test]
    public void CreateObject_Generic ()
    {
      var typeID = new AssembledTypeID (typeof (RequestedType), new object[0]);
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (typeof (RequestedType))).Return (typeID);
      var assembledInstance = new AssembledType();
      _constructorCallCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)),
                  Arg.Is (ParamList.Empty.FuncType),
                  Arg.Is (false)))
          .Return (new Func<object> (() => assembledInstance));

      var result = _pipeline.Create<RequestedType>();

      Assert.That (result, Is.SameAs (assembledInstance));
    }

    [Test]
    public void CreateObject_AssembledTypeID ()
    {
      var assembledInstance = new AssembledType();
      var assembledTypeID = AssembledTypeIDObjectMother.Create();
      _constructorCallCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (assembledTypeID)),
                  Arg.Is (ParamList.Empty.FuncType),
                  Arg.Is (false)))
          .Return (new Func<object> (() => assembledInstance));

      var result = _pipeline.Create (assembledTypeID);

      Assert.That (result, Is.SameAs (assembledInstance));
    }

    [Test]
    public void PrepareAssembledTypeInstance_Initializable ()
    {
      var initializableObjectMock = MockRepository.GenerateMock<IInitializableObject>();
      var reason = BooleanObjectMother.GetRandomBoolean() ? InitializationSemantics.Construction : InitializationSemantics.Deserialization;

      _pipeline.PrepareExternalUninitializedObject (initializableObjectMock, reason);

      initializableObjectMock.AssertWasCalled (mock => mock.Initialize (reason));
    }

    [Test]
    public void PrepareAssembledTypeInstance_NonInitializable ()
    {
      Assert.That (() => _pipeline.PrepareExternalUninitializedObject (new object(), 0), Throws.Nothing);
    }

    class RequestedType { }
    class AssembledType : RequestedType { }
  }
}