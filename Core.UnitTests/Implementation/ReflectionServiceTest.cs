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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Moq;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class ReflectionServiceTest
  {
    private Mock<ITypeAssembler> _typeAssemblerMock;
    private Mock<ITypeCache> _typeCacheMock;
    private Mock<IConstructorCallCache> _constructorCallCache;
    private Mock<IConstructorForAssembledTypeCache> _constructorForAssembledTypeCacheMock;

    private ReflectionService _service;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = new Mock<ITypeAssembler> (MockBehavior.Strict);
      _typeCacheMock = new Mock<ITypeCache> (MockBehavior.Strict);
      _constructorCallCache = new Mock<IConstructorCallCache> (MockBehavior.Strict);
      _constructorForAssembledTypeCacheMock = new Mock<IConstructorForAssembledTypeCache> (MockBehavior.Strict);

      _service = new ReflectionService (_typeAssemblerMock.Object, _typeCacheMock.Object, _constructorCallCache.Object, _constructorForAssembledTypeCacheMock.Object);
    }

    [Test]
    public void IsAssembledType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _typeAssemblerMock.Setup (mock => mock.IsAssembledType (type)).Returns (fakeResult).Verifiable();

      var result = _service.IsAssembledType (type);

      _typeAssemblerMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetRequestedType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeRequestedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Setup (mock => mock.GetRequestedType (assembledType)).Returns (fakeRequestedType).Verifiable();

      var result = _service.GetRequestedType (assembledType);

      _typeAssemblerMock.Verify();
      Assert.That (result, Is.SameAs (fakeRequestedType));
    }

    [Test]
    public void GetTypeIDForRequestedType ()
    {
      var fakeRequestedType = ReflectionObjectMother.GetSomeOtherType();
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Setup (mock => mock.ComputeTypeID (fakeRequestedType)).Returns (fakeTypeID).Verifiable();

      var result = _service.GetTypeIDForRequestedType (fakeRequestedType);

      _typeAssemblerMock.Verify();
      Assert.That (result, Is.EqualTo (fakeTypeID));
    }

    [Test]
    public void GetTypeIDForAssembledType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Setup (mock => mock.ExtractTypeID (assembledType)).Returns (fakeTypeID).Verifiable();

      var result = _service.GetTypeIDForAssembledType (assembledType);

      _typeAssemblerMock.Verify();
      Assert.That (result, Is.EqualTo (fakeTypeID));
    }

    [Test]
    public void GetAssembledType_RequestedType ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create();
      var fakeAssembledType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Setup (mock => mock.ComputeTypeID (requestedType)).Returns (typeID).Verifiable();
      _typeCacheMock.Setup (mock => mock.GetOrCreateType (It.Is<AssembledTypeID> (id => id.Equals (typeID)))).Returns (fakeAssembledType).Verifiable();

      var result = _service.GetAssembledType (requestedType);

      _typeCacheMock.Verify();
      Assert.That (result, Is.SameAs (fakeAssembledType));
    }

    [Test]
    public void GetAssembledType_AssembledTypeID ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var fakeAssembledType = ReflectionObjectMother.GetSomeOtherType();
      _typeCacheMock.Setup (mock => mock.GetOrCreateType (It.Is<AssembledTypeID> (id => id.Equals (typeID)))).Returns (fakeAssembledType).Verifiable();

      var result = _service.GetAssembledType (typeID);

      _typeCacheMock.Verify();
      Assert.That (result, Is.SameAs (fakeAssembledType));
    }

    [Test]
    public void GetAdditionalType ()
    {
      var additionalTypeID = new object();
      var fakeAdditionalType = ReflectionObjectMother.GetSomeType();
      _typeCacheMock.Setup (mock => mock.GetOrCreateAdditionalType (additionalTypeID)).Returns (fakeAdditionalType).Verifiable();

      var result = _service.GetAdditionalType (additionalTypeID);

      _typeCacheMock.Verify();
      Assert.That (result, Is.SameAs (fakeAdditionalType));
    }    

    [Test]
    public void InstantiateAssembledType_WithAssembledTypeID ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var arguments = ParamList.Create ("abc", 7);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      _constructorCallCache
          .Setup (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  It.Is<AssembledTypeID> (id => id.Equals (typeID)),
                  arguments.FuncType,
                  allowNonPublic))
          .Returns (new Func<string, int, object> ((s, i) => "blub"));

      var result = _service.InstantiateAssembledType (typeID, arguments, allowNonPublic);

      _typeCacheMock.Verify();
      Assert.That (result, Is.EqualTo ("blub"));
    }

    [Test]
    public void InstantiateAssembledType_WithExactAssembledType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var arguments = ParamList.Create ("abc", 7);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      _constructorForAssembledTypeCacheMock
          .Setup (mock => mock.GetOrCreateConstructorCall (assembledType, arguments.FuncType, allowNonPublic))
          .Returns (new Func<string, int, object> ((s, i) => "blub"))
          .Verifiable();

      var result = _service.InstantiateAssembledType (assembledType, arguments, allowNonPublic);

      _typeCacheMock.Verify();
      Assert.That (result, Is.EqualTo ("blub"));
    }
    
    [Test]
    public void PrepareAssembledTypeInstance_Initializable ()
    {
      var initializableObjectMock = new Mock<IInitializableObject>();
      var reason = BooleanObjectMother.GetRandomBoolean() ? InitializationSemantics.Construction : InitializationSemantics.Deserialization;

      _service.PrepareExternalUninitializedObject (initializableObjectMock.Object, reason);

      initializableObjectMock.Verify (mock => mock.Initialize (reason), Times.Once());
    }

    [Test]
    public void PrepareAssembledTypeInstance_NonInitializable ()
    {
      Assert.That (() => _service.PrepareExternalUninitializedObject (new object(), 0), Throws.Nothing);
    }
  }
}