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
using Remotion.Reflection.TypeDiscovery;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Implementation.Remotion;
using Remotion.TypePipe.TypeAssembly;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation.Remotion
{
  [TestFixture]
  public class RemotionPipelineRegistryTest
  {
    private IParticipant[] _participants;

    private IPipeline _defaultPipeline;

    [SetUp]
    public void SetUp ()
    {
      var action = (Action<object, IProxyTypeAssemblyContext>) ((id, ctx) => ctx.ProxyType.AddField ("field", 0, typeof (int)));
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      // Modify proxy type to avoid no-modification optimization.
      participantStub.Stub (_ => _.Participate (Arg<object>.Is.Anything, Arg<IProxyTypeAssemblyContext>.Is.Anything)).Do (action);
      _participants = new[] { participantStub };

      var registry = new RemotionPipelineRegistry (_participants);
      _defaultPipeline = registry.DefaultPipeline;
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_defaultPipeline.ParticipantConfigurationID, Is.EqualTo ("remotion-default-pipeline"));
      Assert.That (_defaultPipeline.Participants, Is.EqualTo (_participants));
    }

    [Test]
    public void Initialization_IntegrationTest_AddsNonApplicationAssemblyAttribute_OnModuleCreation ()
    {
      // Creates new in-memory assembly.
      var type = _defaultPipeline.ReflectionService.GetAssembledType (typeof (RequestedType));

      Assert.That (type, Is.Not.SameAs (typeof (RequestedType)));
      Assert.That (type.Assembly.IsDefined (typeof (NonApplicationAssemblyAttribute), false), Is.True);
    }

    public class RequestedType {}
  }
}