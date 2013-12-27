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
  public class ObjectWithDeserializationConstructorProxyTest
  {
    private SerializationInfo _serializationInfo;
    private StreamingContext _streamingContext;
    private string _requestedTypeName;

    private ObjectWithDeserializationConstructorProxy _proxy;

    [SetUp]
    public void SetUp ()
    {
      _serializationInfo = new SerializationInfo (ReflectionObjectMother.GetSomeOtherType(), new FormatterConverter());
      _streamingContext = new StreamingContext (StreamingContextStates.File);
      _requestedTypeName = "requested type name";

      _proxy = new ObjectWithDeserializationConstructorProxy (_serializationInfo, _streamingContext);
    }

    [Test]
    public void PopulateInstance ()
    {
      var instance = new TypeWithDeserializationConstructor();

      _proxy.Invoke ("PopulateInstance", instance, _serializationInfo, _streamingContext, _requestedTypeName);

      Assert.That (instance.DeserializationCtorWasCalled, Is.True);
      Assert.That (instance.SerializationInfo, Is.SameAs (_serializationInfo));
      Assert.That (instance.StreamingContext, Is.EqualTo (_streamingContext));
    }

    [Test]
    public void PopulateInstance_NonPublicDeserializationCtor ()
    {
      var instance = new TypeWithNonPublicDeserializationConstructor();

      _proxy.Invoke("PopulateInstance", instance, _serializationInfo, _streamingContext, _requestedTypeName);

      Assert.That (instance.DeserializationCtorWasCalled, Is.True);
    }

    [Test]
    [ExpectedException (typeof (SerializationException), ExpectedMessage =
        "The constructor to deserialize an object of type 'requested type name' was not found.")]
    public void PopulateInstance_MissingDeserializationConstructor ()
    {
      _proxy.Invoke ("PopulateInstance", new int(), _serializationInfo, _streamingContext, _requestedTypeName);
    }

    [Test]
    public void PopulateInstance_ThrowingDeserializationCtor ()
    {
      var instance = new TypeWithThrowingDeserializationConstructor();

      var exception = Assert.Catch (() => _proxy.Invoke ("PopulateInstance", instance, _serializationInfo, _streamingContext, _requestedTypeName));

      Assert.That (exception.Message, Is.EqualTo ("blub"));
      Assert.That (exception.StackTrace, Is.StringContaining ("TypeWithThrowingDeserializationConstructor"));
    }

    public class TypeWithDeserializationConstructor
    {
      public readonly SerializationInfo SerializationInfo;
      public readonly StreamingContext StreamingContext;
      public readonly bool DeserializationCtorWasCalled;

      public TypeWithDeserializationConstructor () {}
      public TypeWithDeserializationConstructor (SerializationInfo serializationInfo, StreamingContext streamingContext)
      {
        SerializationInfo = serializationInfo;
        StreamingContext = streamingContext;
        DeserializationCtorWasCalled = true;
      }
    }

    public class TypeWithNonPublicDeserializationConstructor
    {
      public readonly bool DeserializationCtorWasCalled;

      public TypeWithNonPublicDeserializationConstructor () {}
      private TypeWithNonPublicDeserializationConstructor (SerializationInfo serializationInfo, StreamingContext streamingContext)
      {
        DeserializationCtorWasCalled = true;
      }
    }

    public class TypeWithThrowingDeserializationConstructor
    {
      public readonly bool DeserializationCtorWasCalled;

      public TypeWithThrowingDeserializationConstructor () { }
      public TypeWithThrowingDeserializationConstructor (SerializationInfo serializationInfo, StreamingContext streamingContext)
      {
        throw new Exception ("blub");
      }
    }
  }
}