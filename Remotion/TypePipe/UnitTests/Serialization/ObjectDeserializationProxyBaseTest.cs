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
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Serialization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class ObjectDeserializationProxyBaseTest
  {
    private SerializationInfo _info;
    private StreamingContext _context;

    private ObjectDeserializationProxyBase _objectDeserializationProxyBase;

    private IPipelineRegistry _pipelineRegistryStub;
    private IDeserializationMethodInvoker _deserializationMethodInvokerMock;
    private Action<object, SerializationInfo, StreamingContext, string> _createRealObjectAssertions;

    [SetUp]
    public void SetUp ()
    {
      var serializableType = ReflectionObjectMother.GetSomeSerializableType();
      var formatterConverter = new FormatterConverter();
      _info = new SerializationInfo (serializableType, formatterConverter);
      _context = new StreamingContext ((StreamingContextStates) 7);

      _pipelineRegistryStub = MockRepository.GenerateStub<IPipelineRegistry>();
      _deserializationMethodInvokerMock = MockRepository.GenerateMock<IDeserializationMethodInvoker>();
      _createRealObjectAssertions = (instance, info, ctx, typeName) => { throw new Exception ("Setup assertions and return real object."); };

      using (new ServiceLocatorScope (typeof (IPipelineRegistry), () => _pipelineRegistryStub))
      {
        // Use testable class instead of partial mock, because RhinoMocks chokes on non-virtual ISerializable.GetObjectData.
        _objectDeserializationProxyBase = new TestableObjectDeserializationProxyBase (
            _info, _context, (instance, info, ctx, typeName) => _createRealObjectAssertions (instance, info, ctx, typeName));
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
      var requestedType = typeof (DomainType);
      var data = new AssembledTypeIDData (requestedType.AssemblyQualifiedName, new IFlatValue[0]);

      _info.AddValue ("<tp>participantConfigurationID", "config1");
      _info.AddValue ("<tp>assembledTypeIDData", data);

      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      var reflectionServiceStub = MockRepository.GenerateStub<IReflectionService>();
      _pipelineRegistryStub.Stub (_ => _.Get ("config1")).Return (pipelineStub);
      pipelineStub.Stub (_ => _.ReflectionService).Return (reflectionServiceStub);
      reflectionServiceStub.Stub (_ => _.GetAssembledType (Arg<AssembledTypeID>.Matches (id => id.Equals (data.CreateTypeID())))).Return (requestedType);

      object instance = null;
      _createRealObjectAssertions = (inst, info, ctx, typeName) =>
      {
        Assert.That (inst, Is.TypeOf (requestedType));
        Assert.That (info, Is.SameAs (_info));
        Assert.That (ctx, Is.EqualTo (_context));
        Assert.That (typeName, Is.EqualTo (requestedType.Name));
        instance = inst;
      };

      var result = _objectDeserializationProxyBase.GetRealObject (_context);

      _pipelineRegistryStub.VerifyAllExpectations();
      _deserializationMethodInvokerMock.AssertWasCalled (_ => _.InvokeOnDeserializing (Arg<DomainType>.Is.Anything, Arg.Is (_context)));
      Assert.That (result, Is.SameAs (instance).And.Not.Null);
      Assert.That (PrivateInvoke.GetNonPublicField (_objectDeserializationProxyBase, "_instance"), Is.SameAs (instance), "Should be cached.");
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

    private class DomainType {}
  }
}