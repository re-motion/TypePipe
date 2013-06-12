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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Serialization;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class ObjectDeserializationProxyBaseTest
  {
    private SerializationInfo _info;
    private StreamingContext _context;

    private ObjectDeserializationProxyBase _objectDeserializationProxyBase;

    private IPipelineRegistry _pipelineRegistryMock;
    private IDeserializationMethodInvoker _deserializationMethodInvokerMock;
    private Func<IPipeline, AssembledTypeID, object> _createRealObjectAssertions;

    [SetUp]
    public void SetUp ()
    {
      var serializableType = ReflectionObjectMother.GetSomeSerializableType();
      var formatterConverter = new FormatterConverter();
      _info = new SerializationInfo (serializableType, formatterConverter);
      _context = new StreamingContext ((StreamingContextStates) 7);

      _pipelineRegistryMock = MockRepository.GenerateStrictMock<IPipelineRegistry>();
      _deserializationMethodInvokerMock = MockRepository.GenerateMock<IDeserializationMethodInvoker>();
      _createRealObjectAssertions = (f, t) => { throw new Exception ("Setup assertions and return real object."); };

      using (new ServiceLocatorScope (typeof (IPipelineRegistry), () => _pipelineRegistryMock))
      {
        // Use testable class instead of partial mock, because RhinoMocks chokes on non-virtual ISerializable.GetObjectData.
        _objectDeserializationProxyBase = new TestableObjectDeserializationProxyBase (_info, _context, (f, t) => _createRealObjectAssertions (f, t));
      }
      PrivateInvoke.SetNonPublicField (_objectDeserializationProxyBase, "_deserializationMethodInvoker", _deserializationMethodInvokerMock);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "This method should not be called.")]
    public void GetObjectData ()
    {
      _objectDeserializationProxyBase.GetObjectData (null, new StreamingContext ());
    }

    [Test]
    public void GetRealObject ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var data = new AssembledTypeIDData (requestedType.AssemblyQualifiedName, new IFlatValue[0]);

      _info.AddValue ("<tp>participantConfigurationID", "config1");
      _info.AddValue ("<tp>assembledTypeIDData", data);

      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      pipelineStub.Stub (_ => _.Participants).Return (new IParticipant[0].ToList().AsReadOnly());
      var fakeInstance = new object();
      _pipelineRegistryMock.Expect (mock => mock.Get ("config1")).Return (pipelineStub);
      _createRealObjectAssertions = (factory, typeID) =>
      {
        Assert.That (factory, Is.SameAs (pipelineStub));
        Assert.That (typeID.RequestedType, Is.EqualTo (requestedType));

        return fakeInstance;
      };

      var result = _objectDeserializationProxyBase.GetRealObject (_context);

      _pipelineRegistryMock.VerifyAllExpectations();
      _deserializationMethodInvokerMock.AssertWasCalled (_ => _.InvokeOnDeserializing (fakeInstance, _context));
      Assert.That (result, Is.SameAs (fakeInstance));
      Assert.That (PrivateInvoke.GetNonPublicField (_objectDeserializationProxyBase, "_instance"), Is.SameAs (fakeInstance));
    }

    [Test]
    public void GetRealObject_Caches ()
    {
      var instance = new object ();
      PrivateInvoke.SetNonPublicField (_objectDeserializationProxyBase, "_instance", instance);

      var result = _objectDeserializationProxyBase.GetRealObject (_context);

      Assert.That (result, Is.SameAs (instance));
    }

    [Test]
    public void OnDeserialization ()
    {
      var instance = new object();
      PrivateInvoke.SetNonPublicField (_objectDeserializationProxyBase, "_instance", instance);
      var sender = new object();

      _objectDeserializationProxyBase.OnDeserialization (sender);

      _deserializationMethodInvokerMock.AssertWasCalled (_ => _.InvokeOnDeserialized (instance, _context));
      _deserializationMethodInvokerMock.AssertWasCalled (_ => _.InvokeOnDeserialization (instance, sender));
    }
  }
}