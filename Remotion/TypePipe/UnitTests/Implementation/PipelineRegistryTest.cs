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
using Remotion.Collections;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class PipelineRegistryTest
  {
    private PipelineRegistry _registry;

    private IPipeline _pipelineStub;

    [SetUp]
    public void SetUp ()
    {
      _registry = new PipelineRegistry();

      _pipelineStub = CreatePipelineStub ("configId");
    }

    [Test]
    public void Initialization ()
    {
      var pipelines = PrivateInvoke.GetNonPublicField (_registry, "_pipelines");

      Assert.That (pipelines, Is.TypeOf<LockingDataStoreDecorator<string, IPipeline>>());
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "No default pipeline has been specified. Use SetDefaultPipeline in your Main method or IoC configuration.")]
    public void DefaultPipeline ()
    {
      Dev.Null = _registry.DefaultPipeline;
    }

    [Test]
    public void RegisterAndGet ()
    {
      _registry.Register (_pipelineStub);

      Assert.That (_registry.Get ("configId"), Is.SameAs (_pipelineStub));
    }

    [Test]
    public void RegisterAndUnregister ()
    {
      _registry.Register (_pipelineStub);
      Assert.That (_registry.Get ("configId"), Is.Not.Null);

      _registry.Unregister ("configId");

      Assert.That (() => _registry.Get ("configId"), Throws.InvalidOperationException);
      Assert.That (() => _registry.Unregister ("configId"), Throws.Nothing);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Another pipeline is already registered for identifier 'configId'.")]
    public void Register_ExistingFactory ()
    {
      Assert.That (() => _registry.Register (_pipelineStub), Throws.Nothing);
      _registry.Register (_pipelineStub);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "No pipeline registered for identifier 'missingFactory'.")]
    public void Get_MissingFactory ()
    {
      _registry.Get ("missingFactory");
    }

    [Test]
    public void SetDefaultPipeline ()
    {
      _registry.Register (_pipelineStub);

      _registry.SetDefaultPipeline ("configId");

      Assert.That (_registry.DefaultPipeline, Is.SameAs (_pipelineStub));
      Assert.That (_registry.Get ("<default pipeline>"), Is.SameAs (_pipelineStub));
      Assert.That (() => _registry.SetDefaultPipeline ("configId"), Throws.Nothing);
    }

    private IPipeline CreatePipelineStub (string participantConfigurationID)
    {
      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      pipelineStub.Stub (stub => stub.ParticipantConfigurationID).Return (participantConfigurationID);

      return pipelineStub;
    }
  }
}