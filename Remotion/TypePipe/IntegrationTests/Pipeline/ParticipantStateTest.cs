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
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Remotion.Development.UnitTesting.IO;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class ParticipantStateTest : IntegrationTestBase
  {
    private const string c_participantConfigurationID = "ParticipantStateTest";

    private Assembly _assembly;

    public override void TestFixtureSetUp ()
    {
      base.TestFixtureSetUp ();

      _assembly = PreGenerateAssembly();
    }

    [Test]
    public void GlobalState ()
    {
      var stateWasRead = false;

      var participant1 = CreateParticipant (ctx =>
      {
        if (ctx.RequestedType == typeof (RequestedType1))
        {
          Assert.That (ctx.State, Is.Empty);
          ctx.State.Add ("key", 7);
        }
        else
        {
          Assert.That (ctx.State["key"], Is.EqualTo (7), "Participant sees state even when requsted types differ.");
          stateWasRead = true;
        }
      });
      var participant2 = CreateParticipant (ctx => Assert.That (ctx.State["key"], Is.EqualTo (7), "Participant 2 sees state of participant 1."));

      var pipeline = CreatePipeline (participant1, participant2);

      Assert.That (() => pipeline.Create<RequestedType1>(), Throws.Nothing);
      Assert.That (() => pipeline.Create<RequestedType2>(), Throws.Nothing);

      Assert.That (stateWasRead, Is.True);
    }

    [Test]
    [Ignore ("TODO 5840: RebuildParticipantState is currently enabled")]
    public void RebuildState_FromLoadedTypes ()
    {
      Type loadedType = null;
      var stateWasRead = false;
      var participant = CreateParticipant (
          rebuildStateAction: ctx =>
          {
            var loadedProxy = ctx.ProxyTypes.Single();
            var additionalType = ctx.AdditionalTypes.Single();
            Assert.That (loadedProxy.RequestedType, Is.SameAs (typeof (RequestedType1)));
            Assert.That (additionalType.FullName, Is.EqualTo ("MyNs.AdditionalType"));

            loadedType = loadedProxy.AssembledType;
            ctx.State["key"] = "reconstructed state";
          },
          participateAction: (id, ctx) =>
          {
            Assert.That (ctx.State["key"], Is.EqualTo ("reconstructed state"));
            stateWasRead = true;
          });
      var pipeline = CreatePipeline (c_participantConfigurationID, participant);

      pipeline.CodeManager.LoadFlushedCode (_assembly);

      Assert.That (loadedType, Is.Not.Null);
      Assert.That (pipeline.ReflectionService.GetAssembledType (typeof (RequestedType1)), Is.SameAs (loadedType));
      Assert.That (stateWasRead, Is.False);

      pipeline.ReflectionService.GetAssembledType (typeof (RequestedType2));
      Assert.That (stateWasRead, Is.True);
    }

    [Test]
    [Ignore ("TODO 5840: RebuildParticipantState is currently enabled")]
    public void RebuildState_ContextDoesNotContainProxyTypesAlreadyInCacheWhenLoadingAssembly ()
    {
      var rebuildStateWasCalled = false;
      var participant = CreateParticipant (
          rebuildStateAction: ctx =>
          {
            Assert.That (ctx.ProxyTypes, Is.Empty);
            Assert.That (ctx.AdditionalTypes, Has.Count.EqualTo (1));
            rebuildStateWasCalled = true;
          });
      var pipeline = CreatePipeline (c_participantConfigurationID, participant);
      pipeline.Create<RequestedType1>(); // Put type 1 into cache.

      pipeline.CodeManager.LoadFlushedCode (_assembly);

      Assert.That (rebuildStateWasCalled, Is.True);
    }

    private Assembly PreGenerateAssembly ()
    {
      var participant = CreateParticipant (ctx => ctx.CreateType ("AdditionalType", "MyNs", TypeAttributes.Class, typeof (object)));
      var pipeline = CreatePipeline (c_participantConfigurationID, participant);
      pipeline.Create<RequestedType1>(); // Trigger generation of types.
      var assemblyPath = Flush().Single();

      return AssemblyLoader.LoadWithoutLocking (assemblyPath);
    }

    public class RequestedType1 {}
    public class RequestedType2 {}
  }
}