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
using Remotion.TypePipe.Development.UnitTesting;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  public class PipelineRegistryTest
  {
    private IPipelineRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      Assert.That (PipelineRegistry.HasInstanceProvider, Is.False);

      _registry = MockRepository.GenerateStub<IPipelineRegistry>();
    }

    [TearDown]
    public void TearDown ()
    {
      PipelineRegistryTestHelper.ResetPipelineRegistry();
    }

    [Test]
    public void SetInstanceProvider_GetInstance ()
    {
      PipelineRegistry.SetInstanceProvider (() => _registry);
      Assert.That (PipelineRegistry.Instance, Is.SameAs (_registry));
    }

    [Test]
    public void GetInstance_NoInstanceProviderSet_ThrowsInvalidOperationException ()
    {
      Assert.That (PipelineRegistry.HasInstanceProvider, Is.False);
      Assert.That (
          () => PipelineRegistry.Instance,
          Throws.InvalidOperationException.With.Message.EqualTo (
              "No instance provider was set for the PipelineRegistry's Instance property. "
              + "Use PipelineRegistry.SetInstanceProvider (() => thePipelineRegistry) during application startup to initialize the TypePipe infrastructure."));
    }

    [Test]
    public void GetInstance_InstanceProviderReturnedNull_ThrowsInvalidOperationException ()
    {
      PipelineRegistry.SetInstanceProvider (() => null);
      Assert.That (
          () => PipelineRegistry.Instance,
          Throws.InvalidOperationException.With.Message.EqualTo (
              "The registered instance provider returned null. "
              + "Use PipelineRegistry.SetInstanceProvider (() => thePipelineRegistry) during application startup to initialize the TypePipe infrastructure."));
    }

    [Test]
    public void GetHasInstanceProvider ()
    {
      Assert.That (PipelineRegistry.HasInstanceProvider, Is.False);

      PipelineRegistry.SetInstanceProvider (() => _registry);
      Assert.That (PipelineRegistry.HasInstanceProvider, Is.True);
    }
  }
}