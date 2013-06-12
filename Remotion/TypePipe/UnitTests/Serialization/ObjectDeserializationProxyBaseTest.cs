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
using System.Runtime.Serialization;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Serialization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class ObjectDeserializationProxyBaseTest
  {
    private SerializationInfo _info;
    private StreamingContext _context;

    [SetUp]
    public void SetUp ()
    {
      var serializableType = ReflectionObjectMother.GetSomeSerializableType();
      var formatterConverter = new FormatterConverter();
      _info = new SerializationInfo (serializableType, formatterConverter);
      _context = new StreamingContext ((StreamingContextStates) 7);
    }

    [Test]
    public void Initialization ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var data = new AssembledTypeIDData (requestedType.AssemblyQualifiedName, new IFlatValue[0]);
      _info.AddValue ("<tp>participantConfigurationID", "config1");
      _info.AddValue ("<tp>assembledTypeIDData", data);

      var pipelineRegistryMock = MockRepository.GenerateStrictMock<IPipelineRegistry>();
      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      var fakeInstance = new object();

      pipelineRegistryMock.Expect (mock => mock.Get ("config1")).Return (pipelineStub);

      TestableObjectDeserializationProxyBase.CreateRealObjectAssertions = (factory, typeID, info, ctx) =>
      {
        Assert.That (factory, Is.SameAs (pipelineStub));
        Assert.That (typeID.RequestedType, Is.EqualTo (requestedType));
        Assert.That (ctx, Is.EqualTo (_context));

        return fakeInstance;
      };

      ObjectDeserializationProxyBase proxyBase;
      using (new ServiceLocatorScope (typeof (IPipelineRegistry), () => pipelineRegistryMock))
      {
        // Use testable class instead of partial mock, because RhinoMocks chokes on non-virtual ISerializable.GetObjectData.
        proxyBase = new TestableObjectDeserializationProxyBase (_info, _context);
      }

      pipelineRegistryMock.VerifyAllExpectations();
      Assert.That (proxyBase.GetRealObject (new StreamingContext()), Is.SameAs (fakeInstance));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "This method should not be called.")]
    public void GetObjectData ()
    {
      var proxyBase = CreateObjectDeserializationProxyBase (fakeInstance: null);

      proxyBase.GetObjectData (null, new StreamingContext());
    }

    [Test]
    public void GetRealObject ()
    {
      var fakeInstance = new object();
      var proxyBase = CreateObjectDeserializationProxyBase (fakeInstance);

      var result = proxyBase.GetRealObject (new StreamingContext());

      Assert.That (result, Is.SameAs (fakeInstance));
    }

    [Test]
    public void OnDeserialization_SenderNull ()
    {
      var proxyBase = CreateObjectDeserializationProxyBase (fakeInstance: new object());

      Assert.That (() => proxyBase.OnDeserialization (sender: null), Throws.Nothing);
    }

    [Test]
    public void OnDeserialization_DeserializationCallback ()
    {
      var deserializationCallbackMock = MockRepository.GenerateStrictMock<IDeserializationCallback> ();
      var sender = new object ();
      deserializationCallbackMock.Expect (x => x.OnDeserialization (sender));
      var proxyBase = CreateObjectDeserializationProxyBase(fakeInstance: deserializationCallbackMock);
      
      proxyBase.OnDeserialization (sender);

      deserializationCallbackMock.VerifyAllExpectations ();
    }

    private ObjectDeserializationProxyBase CreateObjectDeserializationProxyBase (object fakeInstance)
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var data = new AssembledTypeIDData(requestedType.AssemblyQualifiedName, new IFlatValue[0]);
      _info.AddValue("<tp>participantConfigurationID", "config1");
      _info.AddValue("<tp>assembledTypeIDData", data);

      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      var pipelineRegistryStub = MockRepository.GenerateStub<IPipelineRegistry>();
      pipelineRegistryStub.Stub(mock => mock.Get("config1")).Return(pipelineStub);

      TestableObjectDeserializationProxyBase.CreateRealObjectAssertions = (factory, typeID, info, ctx) => fakeInstance;

      using (new ServiceLocatorScope(typeof(IPipelineRegistry), () => pipelineRegistryStub))
      {
        // Use testable class instead of partial mock, because RhinoMocks chokes on non-virtual ISerializable.GetObjectData.
        return new TestableObjectDeserializationProxyBase(_info, _context);
      }
    }
  }
}