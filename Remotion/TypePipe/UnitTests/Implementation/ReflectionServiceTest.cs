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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Implementation.Synchronization;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class ReflectionServiceTest
  {
    private IReflectionServiceSynchronizationPoint _reflectionServiceSynchronizationPointMock;
    private ITypeCache _typeCacheMock;

    private ReflectionService _service;

    [SetUp]
    public void SetUp ()
    {
      _reflectionServiceSynchronizationPointMock = MockRepository.GenerateStrictMock<IReflectionServiceSynchronizationPoint> ();
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();

      _service = new ReflectionService (_reflectionServiceSynchronizationPointMock, _typeCacheMock);
    }

    [Test]
    public void IsAssembledType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _reflectionServiceSynchronizationPointMock.Expect (mock => mock.IsAssembledType (type)).Return (fakeResult);

      var result = _service.IsAssembledType (type);

      _reflectionServiceSynchronizationPointMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetRequestedType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeRequestedType = ReflectionObjectMother.GetSomeOtherType();
      _reflectionServiceSynchronizationPointMock.Expect (mock => mock.GetRequestedType (assembledType)).Return (fakeRequestedType);

      var result = _service.GetRequestedType (assembledType);

      _reflectionServiceSynchronizationPointMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeRequestedType));
    }

    [Test]
    public void GetTypeID ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      _reflectionServiceSynchronizationPointMock.Expect (mock => mock.GetTypeID (assembledType)).Return (fakeTypeID);

      var result = _service.GetTypeID (assembledType);

      _reflectionServiceSynchronizationPointMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeTypeID));
    }

    [Test]
    public void GetAssembledType_RequestedType ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var fakeAssembledType = ReflectionObjectMother.GetSomeOtherType();
      _typeCacheMock.Expect (mock => mock.GetOrCreateType (requestedType)).Return (fakeAssembledType);

      var result = _service.GetAssembledType (requestedType);

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeAssembledType));
    }

    [Test]
    public void GetAssembledType_AssembledTypeID ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var fakeAssembledType = ReflectionObjectMother.GetSomeOtherType();
      _typeCacheMock.Expect (mock => mock.GetOrCreateType (Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)))).Return (fakeAssembledType);

      var result = _service.GetAssembledType (typeID);

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeAssembledType));
    }
  }
}