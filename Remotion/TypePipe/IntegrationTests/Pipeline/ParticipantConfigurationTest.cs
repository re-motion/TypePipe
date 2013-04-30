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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Dlr.Ast;
using Rhino.Mocks;

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
      var participant1 = CreateParticipant();
      var participant2 = CreateParticipant();

      var pipeline = PipelineFactory.Create (configurationID, participant1, participant2);

      Assert.That (pipeline.ParticipantConfigurationID, Is.EqualTo (configurationID));
      Assert.That (pipeline.Participants, Is.EqualTo (new[] { participant1, participant2 }));
    }

    [Test]
    public void ParticipantHasAccessToParticipantConfigurationID ()
    {
      var configurationID = "configurationID";
      var participant = CreateParticipant (ctx => Assert.That (ctx.ParticipantConfigurationID, Is.EqualTo (configurationID)));

      var pipeline = PipelineFactory.Create (configurationID, participant);

      Assert.That (() => pipeline.Create<RequestedType>(), Throws.Nothing);
    }

    [Test]
    public void ParticipantHasAccessToTypeIDExpression ()
    {
      var typeIDPart = "type id part";
      var typeIDPartExpression = Expression.Constant (typeIDPart);
      var typeIdentifierProviderStub = MockRepository.GenerateStub<ITypeIdentifierProvider>();
      typeIdentifierProviderStub.Stub (_ => _.GetID (typeof (RequestedType))).Return (typeIDPart);
      typeIdentifierProviderStub.Stub (_ => _.GetExpressionForID (typeIDPart)).Return (typeIDPartExpression);
      var participant = CreateParticipant (
          context =>
          {
            var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((RequestedType o) => o.Method());
            context.ProxyType.GetOrAddOverride (method).SetBody (ctx => context.TypeID);
          },
          typeIdentifierProviderStub);

      var instance = CreatePipeline (participant).Create<RequestedType>();

      var expectedTypeID = new AssembledTypeID (typeof (RequestedType), new object[] { typeIDPart });
      Assert.That (instance.Method(), Is.EqualTo (expectedTypeID));
    }

    public class RequestedType
    {
      public virtual object Method () { return null; }
    }
  }
}