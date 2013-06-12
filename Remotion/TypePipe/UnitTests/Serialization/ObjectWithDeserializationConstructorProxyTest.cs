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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Serialization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class ObjectWithDeserializationConstructorProxyTest
  {
    private SerializationInfo _serializationInfo;

    private ObjectWithDeserializationConstructorProxy _proxy;

    [SetUp]
    public void SetUp ()
    {
      ReflectionObjectMother.GetSomeType();
      _serializationInfo = new SerializationInfo (ReflectionObjectMother.GetSomeOtherType(), new FormatterConverter());

      _proxy = new ObjectWithDeserializationConstructorProxy (_serializationInfo, new StreamingContext (StreamingContextStates.File));
    }

    [Test]
    public void CreateRealObject ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var context = new StreamingContext (StreamingContextStates.Persistence);
      var pipelineMock = MockRepository.GenerateStrictMock<IPipeline>();
      var fakeObject = new object();
      pipelineMock
          .Expect (mock => mock.Create (Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)), Arg<ParamList>.Is.Anything, Arg.Is (true)))
          .WhenCalled (
              mi => Assert.That (((ParamList) mi.Arguments[1]).GetParameterValues(), Is.EqualTo (new object[] { _serializationInfo, context })))
          .Return (fakeObject);

      var result = _proxy.Invoke ("CreateRealObject", pipelineMock, typeID, context);

      pipelineMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeObject));
    }

    [Test]
    [ExpectedException (typeof (SerializationException), ExpectedMessage = "The constructor to deserialize an object of type 'Int32' was not found.")]
    public void CreateRealObject_MissingDeserializationConstructor ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (typeof (int));
      var pipelineStub = MockRepository.GenerateStub<IPipeline>();
      var exception = new MissingMethodException();
      pipelineStub.Stub (_ => _.Create (typeID, null, true)).IgnoreArguments().Throw (exception);

      _proxy.Invoke ("CreateRealObject", pipelineStub, typeID, new StreamingContext());
    }
  }
}