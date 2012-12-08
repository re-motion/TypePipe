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
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.Serialization.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class DeserializationSurrogateBaseTest
  {
    private SerializationInfo _info;
    private StreamingContext _context;

    private DeserializationSurrogateBase _deserializationSurrogateBase;

    private IObjectFactoryRegistry _objectFactoryRegistryMock;
    private Func<IObjectFactory, Type, StreamingContext, object> _createRealObjectAssertions;

    [SetUp]
    public void SetUp ()
    {
      var serializableType = ReflectionObjectMother.GetSomeSerializableType();
      var formatterConverter = new FormatterConverter();
      _info = new SerializationInfo (serializableType, formatterConverter);
      _context = new StreamingContext ((StreamingContextStates) 7);

      _objectFactoryRegistryMock = MockRepository.GenerateStrictMock<IObjectFactoryRegistry>();
      _createRealObjectAssertions = (f, t, c) => { throw new Exception ("Setup assertions and return real object."); };

      using (new ServiceLocatorScope (typeof (IObjectFactoryRegistry), () => _objectFactoryRegistryMock))
      {
        // Use testable class instead of partial mock, because RhinoMocks chokes on non-virtual ISerializable.GetObjectData.
        _deserializationSurrogateBase = new TestableDeserializationSurrogateBase (_info, _context, (f, t, c) => _createRealObjectAssertions (f, t, c));
      }
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_deserializationSurrogateBase.SerializationInfo, Is.SameAs (_info));
    }

    [Test]
    public void GetRealObject ()
    {
      var underlyingType = ReflectionObjectMother.GetSomeType();
      var context = new StreamingContext ((StreamingContextStates) 8);

      _info.AddValue ("<tp>underlyingType", underlyingType.AssemblyQualifiedName);
      _info.AddValue ("<tp>factoryIdentifier", "factory1");

      var fakeObjectFactory = MockRepository.GenerateStub<IObjectFactory>();
      var fakeObject = new object();
      _objectFactoryRegistryMock.Expect (mock => mock.Get ("factory1")).Return (fakeObjectFactory);
      _createRealObjectAssertions = (factory, type, ctx) =>
      {
        Assert.That (factory, Is.SameAs (fakeObjectFactory));
        Assert.That (type, Is.SameAs (underlyingType));
        Assert.That (ctx, Is.EqualTo (context).And.Not.EqualTo (_context));

        return fakeObject;
      };

      var result = _deserializationSurrogateBase.GetRealObject (context);

      _objectFactoryRegistryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeObject));
    }

    [Test]
    public void GetRealObject_DeserializationCallback ()
    {
      _info.AddValue ("<tp>underlyingType", typeof (object).AssemblyQualifiedName);
      _info.AddValue ("<tp>factoryIdentifier", "factory1");

      var objectMock = MockRepository.GenerateStrictMock<IDeserializationCallback> ();
      _objectFactoryRegistryMock.Stub (mock => mock.Get ("factory1"));
      _createRealObjectAssertions = (factory, type, ctx) => objectMock;
      objectMock.Expect (mock => mock.OnDeserialization (_deserializationSurrogateBase));

      var result = _deserializationSurrogateBase.GetRealObject (new StreamingContext());

      _objectFactoryRegistryMock.VerifyAllExpectations();
      objectMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (objectMock));
    }

    [Test]
    [ExpectedException (typeof (TypeLoadException), MatchType = MessageMatch.StartsWith,
        ExpectedMessage = "Could not load type 'UnknownType' from assembly 'Remotion.TypePipe, ")]
    public void GetRealObject_UnderlyingTypeNotFound ()
    {
      _info.AddValue ("<tp>underlyingType", "UnknownType");
      _info.AddValue ("<tp>factoryIdentifier", "factory1");

      _deserializationSurrogateBase.GetRealObject (new StreamingContext());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "This method should not be called.")]
    public void GetObjectData ()
    {
      _deserializationSurrogateBase.GetObjectData (null, new StreamingContext());
    }
  }
}