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

    public override void TestFixtureSetUp ()
    {
      base.TestFixtureSetUp ();

      PreGenerateAssembly();
    }

    [Test]
    public void GlobalState ()
    {
      var stateWasRead = false;

      var participant1 = CreateParticipant (ctx =>
      {
        if (ctx.RequestedType == typeof (RequestedType1))
        {
          Assert.That (ctx.ParticipantState.GetState ("key"), Is.Null);
          ctx.ParticipantState.AddState ("key", 7);
        }
        else
        {
          Assert.That (ctx.ParticipantState.GetState("key"), Is.EqualTo (7), "Participant sees state even when requsted types differ.");
          stateWasRead = true;
        }
      });
      var participant2 = CreateParticipant (
          ctx => Assert.That (ctx.ParticipantState.GetState ("key"), Is.EqualTo (7), "Participant 2 sees state of participant 1."));

      var pipeline = CreatePipeline (participant1, participant2);

      Assert.That (() => pipeline.Create<RequestedType1>(), Throws.Nothing);
      Assert.That (() => pipeline.Create<RequestedType2>(), Throws.Nothing);

      Assert.That (stateWasRead, Is.True);
    }

    [Test]
    public void StateIsResetAfterFlush ()
    {
      var stateWasRead = false;
      var hasFlushed = false;
      var participant = CreateParticipant (ctx =>
      {
        if (ctx.RequestedType == typeof (RequestedType1))
        {
          Assert.That (hasFlushed, Is.False);
          Assert.That (ctx.ParticipantState.GetState ("key"), Is.Null);
          ctx.ParticipantState.AddState ("key", 7);
        }
        else
        {
          Assert.That (hasFlushed, Is.True);
          Assert.That (ctx.ParticipantState.GetState ("key"), Is.Null, "Participant does not see state after flush");
          stateWasRead = true;
        }
      });

      var pipeline = CreatePipeline (participant);

      Assert.That (() => pipeline.Create<RequestedType1>(), Throws.Nothing);
      Flush();
      hasFlushed = true;
      Assert.That (() => pipeline.Create<RequestedType2>(), Throws.Nothing);

      Assert.That (stateWasRead, Is.True);
    }

    private void PreGenerateAssembly ()
    {
      var participant = CreateParticipant (ctx => ctx.CreateAdditionalType (new object(), "AdditionalType", "MyNs", TypeAttributes.Class, typeof (object)));
      var pipeline = CreatePipeline (c_participantConfigurationID, participant);
      pipeline.Create<RequestedType1>(); // Trigger generation of types.
      var assemblyPath = Flush().Single();

      AssemblyLoader.LoadWithoutLocking (assemblyPath);
    }

    public class RequestedType1 {}
    public class RequestedType2 {}
  }
}