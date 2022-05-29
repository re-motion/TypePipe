﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Moq;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class ParticipantConfigurationTest : IntegrationTestBase
  {
    public override void SetUp ()
    {
      base.SetUp();

      SkipSavingAndPeVerification();
    }

    [Test]
    public void PipelineConfiguration ()
    {
      var configurationID = "configurationID";
      var participant1 = CreateParticipant ((id, ctx) => Assert.That (ctx.ParticipantConfigurationID, Is.EqualTo (configurationID)));
      var participant2 = CreateParticipant();

      var pipeline = new DefaultPipelineFactory().Create (configurationID, participant1, participant2);

      Assert.That (pipeline.ParticipantConfigurationID, Is.EqualTo (configurationID));
      Assert.That (pipeline.Participants, Is.EqualTo (new[] { participant1, participant2 }));
    }

    [Test]
    public void ParticipantIsSuppliedWithHisTypeIDPart ()
    {
      var typeIDPart = "type ID part";
      var typeIdentifierProviderStub = new Mock<ITypeIdentifierProvider>();
      typeIdentifierProviderStub.Setup (_ => _.GetID (typeof (RequestedType))).Returns (typeIDPart);

      var participant1 = CreateParticipant ((id, ctx) => Assert.That (id, Is.Null));
      var participant2 = CreateParticipant ((id, ctx) => Assert.That (id, Is.EqualTo (typeIDPart)), typeIdentifierProviderStub.Object);
      var pipeline = CreatePipeline (participant1, participant2);

      Assert.That (() => pipeline.Create<RequestedType>(), Throws.Nothing);
    }

    public class RequestedType
    {
      public virtual object Method () { return null; }
    }
  }
}