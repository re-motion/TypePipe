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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Serialization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class ObjectWithoutDeserializationConstructorProxyTest
  {
    private SerializationInfo _serializationInfo;

    private ObjectWithoutDeserializationConstructorProxy _proxy;

    private IPipeline _pipelineMock;

    [SetUp]
    public void SetUp ()
    {
      _serializationInfo = new SerializationInfo (ReflectionObjectMother.GetSomeOtherType(), new FormatterConverter());

      _proxy = new ObjectWithoutDeserializationConstructorProxy (_serializationInfo, new StreamingContext (StreamingContextStates.File));

      _pipelineMock = MockRepository.GenerateStrictMock<IPipeline>();
    }

    [Test]
    public void CreateRealObject ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      _serializationInfo.AddValue ("<tp>IntField", 7);
      _pipelineMock
          .Expect (mock => mock.ReflectionService.GetAssembledType (Arg<AssembledTypeID>.Matches (id => id.Equals (typeID))))
          .Return (typeof (DomainType));

      var result = _proxy.Invoke ("CreateRealObject", _pipelineMock, typeID);

      _pipelineMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<DomainType>());
      Assert.That (((DomainType) result).IntField, Is.EqualTo (7));
    }

    [Serializable]
    class DomainType
    {
      public int IntField = 0;
    }
  }
}