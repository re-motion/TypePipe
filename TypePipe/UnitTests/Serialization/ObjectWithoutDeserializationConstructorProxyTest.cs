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

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class ObjectWithoutDeserializationConstructorProxyTest
  {
    private SerializationInfo _serializationInfo;
    private StreamingContext _streamingContext;

    private ObjectWithoutDeserializationConstructorProxy _proxy;

    [SetUp]
    public void SetUp ()
    {
      _serializationInfo = new SerializationInfo (ReflectionObjectMother.GetSomeOtherType(), new FormatterConverter());
      _streamingContext = new StreamingContext (StreamingContextStates.File);

      _proxy = new ObjectWithoutDeserializationConstructorProxy (_serializationInfo, _streamingContext);
    }

    [Test]
    public void PopulateInstance ()
    {
      var instance = new DomainType();
      _serializationInfo.AddValue ("<tp>IntField", 7);

      _proxy.Invoke ("PopulateInstance", instance, _serializationInfo, _streamingContext, "requested type name");

      Assert.That (instance.IntField, Is.EqualTo (7));
    }

    [Serializable]
    class DomainType
    {
      public int IntField = 0;
    }
  }
}