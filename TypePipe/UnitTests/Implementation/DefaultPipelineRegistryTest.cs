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
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class DefaultPipelineRegistryTest
  {
    private IPipeline _defaultPipeline;

    private DefaultPipelineRegistry _registry;

    private IPipeline _somePipeline;

    [SetUp]
    public void SetUp ()
    {
      _defaultPipeline = CreatePipelineStub ("default id");

      _registry = new DefaultPipelineRegistry (_defaultPipeline);

      _somePipeline = CreatePipelineStub ("some id");
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_registry.DefaultPipeline, Is.SameAs (_defaultPipeline));
    }

    [Test]
    public void SetDefaultPipeline ()
    {
      _registry.SetDefaultPipeline (_somePipeline);

      Assert.That (_registry.DefaultPipeline, Is.SameAs (_somePipeline));
      Assert.That (_registry.Get ("some id"), Is.SameAs (_somePipeline));
      Assert.That (() => _registry.SetDefaultPipeline (_somePipeline), Throws.Nothing);
    }

    [Test]
    public void RegisterAndGet ()
    {
      _registry.Register (_somePipeline);

      Assert.That (_registry.Get ("some id"), Is.SameAs (_somePipeline));
    }

    [Test]
    public void RegisterAndUnregister ()
    {
      _registry.Register (_somePipeline);
      Assert.That (_registry.Get ("some id"), Is.Not.Null);

      _registry.Unregister ("some id");

      Assert.That (() => _registry.Get ("some id"), Throws.InvalidOperationException);
      Assert.That (() => _registry.Unregister ("some id"), Throws.Nothing);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Another pipeline is already registered for identifier 'some id'.")]
    public void Register_ExistingPipeline ()
    {
      Assert.That (() => _registry.Register (_somePipeline), Throws.Nothing);
      _registry.Register (_somePipeline);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The default pipeline cannot be unregistered.")]
    public void Unregister_DefaultPipeline ()
    {
      _registry.Unregister (_defaultPipeline.ParticipantConfigurationID);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "No pipeline registered for identifier 'missingPipeline'.")]
    public void Get_MissingPipeline ()
    {
      _registry.Get ("missingPipeline");
    }

    private IPipeline CreatePipelineStub (string participantConfigurationID)
    {
      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      pipelineStub.Stub (stub => stub.ParticipantConfigurationID).Return (participantConfigurationID);

      return pipelineStub;
    }
  }
}