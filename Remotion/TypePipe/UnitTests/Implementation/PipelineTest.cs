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
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class PipelineTest
  {
    private ITypeCache _typeCacheMock;
    private ICodeManager _codeManagerMock;
    private IReflectionService _reflectionServiceMock;

    private Pipeline _pipeline;

    private Type _requestedType;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _codeManagerMock = MockRepository.GenerateStrictMock<ICodeManager>();
      _reflectionServiceMock = MockRepository.GenerateStrictMock<IReflectionService>();

      _pipeline = new Pipeline (_typeCacheMock, _codeManagerMock, _reflectionServiceMock);

      _requestedType = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void Initialization ()
    {
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
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (_requestedType, typeof (Func<object>), false))
          .Return (new Func<object> (() => "default .ctor"));

      var result = _pipeline.Create (_requestedType);

      Assert.That (result, Is.EqualTo ("default .ctor"));
    }

    [Test]
    public void CreateObject_ConstructorArguments ()
    {
      var arguments = ParamList.Create ("abc", 7);
      _typeCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (_requestedType, arguments.FuncType, false))
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
      const bool allowNonPublic = true;
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (_requestedType, typeof (Func<object>), allowNonPublic))
          .Return (new Func<object> (() => "non-public .ctor"));

      var result = _pipeline.Create (_requestedType, allowNonPublicConstructor: allowNonPublic);

      Assert.That (result, Is.EqualTo ("non-public .ctor"));
    }

    [Test]
    public void CreateObject_Generic ()
    {
      var assembledInstance = new AssembledType();
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (typeof (RequestedType), ParamList.Empty.FuncType, false))
          .Return (new Func<object> (() => assembledInstance));

      var result = _pipeline.Create<RequestedType>();

      Assert.That (result, Is.SameAs (assembledInstance));
    }

    [Test]
    public void PrepareAssembledTypeInstance_Initializable ()
    {
      var initializableObjectMock = MockRepository.GenerateMock<IInitializableObject>();

      _pipeline.PrepareExternalUninitializedObject (initializableObjectMock);

      initializableObjectMock.AssertWasCalled (mock => mock.Initialize());
    }

    [Test]
    public void PrepareAssembledTypeInstance_NonInitializable ()
    {
      Assert.That (() => _pipeline.PrepareExternalUninitializedObject (new object()), Throws.Nothing);
    }

    class RequestedType { }
    class AssembledType : RequestedType { }
  }
}