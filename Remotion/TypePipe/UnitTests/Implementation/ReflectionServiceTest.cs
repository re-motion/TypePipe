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
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class ReflectionServiceTest
  {
    private ITypeAssembler _typeAssemblerMock;
    private ITypeCache _typeCacheMock;
    private IConstructorCallCache _constructorCallCache;
    private IConstructorForAssembledTypeCache _constructorForAssembledTypeCacheMock;

    private ReflectionService _service;

    [SetUp]
    public void SetUp ()
    {
      _typeAssemblerMock = MockRepository.GenerateStrictMock<ITypeAssembler>();
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _constructorCallCache = MockRepository.GenerateStrictMock<IConstructorCallCache>();
      _constructorForAssembledTypeCacheMock = MockRepository.GenerateStrictMock<IConstructorForAssembledTypeCache>();

      _service = new ReflectionService (_typeAssemblerMock, _typeCacheMock, _constructorCallCache, _constructorForAssembledTypeCacheMock);
    }

    [Test]
    public void IsAssembledType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _typeAssemblerMock.Expect (mock => mock.IsAssembledType (type)).Return (fakeResult);

      var result = _service.IsAssembledType (type);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetRequestedType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeRequestedType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Expect (mock => mock.GetRequestedType (assembledType)).Return (fakeRequestedType);

      var result = _service.GetRequestedType (assembledType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeRequestedType));
    }

    [Test]
    public void GetTypeIDForRequestedType ()
    {
      var fakeRequestedType = ReflectionObjectMother.GetSomeOtherType();
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (fakeRequestedType)).Return (fakeTypeID);

      var result = _service.GetTypeIDForRequestedType (fakeRequestedType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeTypeID));
    }

    [Test]
    public void GetTypeIDForAssembledType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var fakeTypeID = AssembledTypeIDObjectMother.Create();
      _typeAssemblerMock.Expect (mock => mock.ExtractTypeID (assembledType)).Return (fakeTypeID);

      var result = _service.GetTypeIDForAssembledType (assembledType);

      _typeAssemblerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeTypeID));
    }

    [Test]
    public void GetAssembledType_RequestedType ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create();
      var fakeAssembledType = ReflectionObjectMother.GetSomeOtherType();
      _typeAssemblerMock.Expect (mock => mock.ComputeTypeID (requestedType)).Return (typeID);
      _typeCacheMock.Expect (mock => mock.GetOrCreateType (Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)))).Return (fakeAssembledType);

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

    [Test]
    public void GetAdditionalType ()
    {
      var additionalTypeID = new object();
      var fakeAdditionalType = ReflectionObjectMother.GetSomeType();
      _typeCacheMock.Expect (mock => mock.GetOrCreateAdditionalType (additionalTypeID)).Return (fakeAdditionalType);

      var result = _service.GetAdditionalType (additionalTypeID);

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeAdditionalType));
    }    

    [Test]
    public void InstantiateAssembledType_WithAssembledTypeID ()
    {
      var typeID = AssembledTypeIDObjectMother.Create();
      var arguments = ParamList.Create ("abc", 7);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      _constructorCallCache
          .Expect (
              mock => mock.GetOrCreateConstructorCall (
                  // Use strongly typed Equals overload.
                  Arg<AssembledTypeID>.Matches (id => id.Equals (typeID)),
                  Arg.Is (arguments.FuncType),
                  Arg.Is (allowNonPublic)))
          .Return (new Func<string, int, object> ((s, i) => "blub"));

      var result = _service.InstantiateAssembledType (typeID, arguments, allowNonPublic);

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo ("blub"));
    }

    [Test]
    public void InstantiateAssembledType_WithExactAssembledType ()
    {
      var assembledType = ReflectionObjectMother.GetSomeType();
      var arguments = ParamList.Create ("abc", 7);
      var allowNonPublic = BooleanObjectMother.GetRandomBoolean();
      _constructorForAssembledTypeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (assembledType, arguments.FuncType, allowNonPublic))
          .Return (new Func<string, int, object> ((s, i) => "blub"));

      var result = _service.InstantiateAssembledType (assembledType, arguments, allowNonPublic);

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo ("blub"));
    }
    
    [Test]
    public void PrepareAssembledTypeInstance_Initializable ()
    {
      var initializableObjectMock = MockRepository.GenerateMock<IInitializableObject>();
      var reason = BooleanObjectMother.GetRandomBoolean() ? InitializationSemantics.Construction : InitializationSemantics.Deserialization;

      _service.PrepareExternalUninitializedObject (initializableObjectMock, reason);

      initializableObjectMock.AssertWasCalled (mock => mock.Initialize (reason));
    }

    [Test]
    public void PrepareAssembledTypeInstance_NonInitializable ()
    {
      Assert.That (() => _service.PrepareExternalUninitializedObject (new object(), 0), Throws.Nothing);
    }
  }
}