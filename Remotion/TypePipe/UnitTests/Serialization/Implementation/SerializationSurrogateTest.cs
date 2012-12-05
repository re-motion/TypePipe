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
using System.Configuration;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.TypePipe.Serialization.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class SerializationSurrogateTest
  {
    private SerializationInfo _info;

    private SerializationSurrogate _surrogate;

    private IObjectFactoryRegistry _objectFactoryRegistryMock;
    private IObjectFactory _objectFactoryMock;

    [SetUp]
    public void SetUp ()
    {
      var serializableType = ReflectionObjectMother.GetSomeSerializableType ();
      var formatterConverter = new FormatterConverter();
      _info = new SerializationInfo (serializableType, formatterConverter);

      _objectFactoryRegistryMock = MockRepository.GenerateStrictMock<IObjectFactoryRegistry> ();
      _objectFactoryMock = MockRepository.GenerateStrictMock<IObjectFactory> ();

      using (new ServiceLocatorScope (typeof (IObjectFactoryRegistry), () => _objectFactoryRegistryMock))
        _surrogate = new SerializationSurrogate (_info, new StreamingContext());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_surrogate.SerializationInfo, Is.SameAs (_info));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "This method should not be called.")]
    public void GetObjectData ()
    {
      _surrogate.GetObjectData (null, new StreamingContext());
    }

    [Test]
    public void GetRealObject ()
    {
      var underlyingType = ReflectionObjectMother.GetSomeType();
      _info.AddValue ("<tp>underlyingType", underlyingType.AssemblyQualifiedName);
      _info.AddValue ("<tp>factoryIdentifier", "factory1");

      var fakeInstance = new object();
      _objectFactoryRegistryMock.Expect (mock => mock.Get ("factory1")).Return (_objectFactoryMock);
      _objectFactoryMock.Expect (mock => mock.CreateObject (underlyingType)).Return (fakeInstance);

      var result = _surrogate.GetRealObject (new StreamingContext());

      _objectFactoryRegistryMock.VerifyAllExpectations();
      _objectFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeInstance));
    }

    [Test]
    [ExpectedException (typeof (TypeLoadException), MatchType = MessageMatch.StartsWith,
        ExpectedMessage = "Could not load type 'UnknownType' from assembly 'Remotion.TypePipe, ")]
    public void GetRealObject_UnderlyingTypeNotFound ()
    {
      _info.AddValue ("<tp>underlyingType", "UnknownType");
      _info.AddValue ("<tp>factoryIdentifier", "factory1");

      _surrogate.GetRealObject (new StreamingContext());
    }
  }
}