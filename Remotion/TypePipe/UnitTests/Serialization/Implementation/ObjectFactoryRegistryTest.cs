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
using Remotion.TypePipe.Serialization.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class ObjectFactoryRegistryTest
  {
    private ObjectFactoryRegistry _registry;

    private IObjectFactory _objectFactoryStub;

    [SetUp]
    public void SetUp ()
    {
      _registry = new ObjectFactoryRegistry();

      _objectFactoryStub = MockRepository.GenerateStub<IObjectFactory>();
      _objectFactoryStub.Stub (stub => stub.ParticipantConfigurationID).Return ("configId");
    }

    [Test]
    public void Initialization ()
    {
      var objectFactories = PrivateInvoke.GetNonPublicField (_registry, "_objectFactories");

      Assert.That (objectFactories, Is.TypeOf<LockingDataStoreDecorator<string, IObjectFactory>>());
    }

    [Test]
    public void RegisterAndGet ()
    {
      _registry.Register (_objectFactoryStub);

      Assert.That (_registry.Get ("configId"), Is.SameAs (_objectFactoryStub));
    }

    [Test]
    public void RegisterAndUnregister ()
    {
      _registry.Register (_objectFactoryStub);
      Assert.That (_registry.Get ("configId"), Is.Not.Null);

      _registry.Unregister ("configId");

      Assert.That (() => _registry.Get ("configId"), Throws.InvalidOperationException);
      Assert.That (() => _registry.Unregister ("configId"), Throws.Nothing);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Another factory is already registered for identifier 'configId'.")]
    public void Register_ExistingFactory ()
    {
      Assert.That (() => _registry.Register (_objectFactoryStub), Throws.Nothing);
      _registry.Register (_objectFactoryStub);
    }

    [Test]
    [ExpectedException (typeof(InvalidOperationException), ExpectedMessage = "No factory registered for identifier 'missingFactory'.")]
    public void Get_MissingFactory ()
    {
      _registry.Get ("missingFactory");
    }
  }
}