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
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class ObjectFactoryTest
  {
    private Type _requestedType;
    
    private ITypeCache _typeCacheMock;

    private ObjectFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();

      _factory = new ObjectFactory (_typeCacheMock);

      _requestedType = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void CreateObject_NoConstructorArguments ()
    {
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (_requestedType, typeof (Func<object>), false))
          .Return (new Func<object> (() => "default .ctor"));

      var result = _factory.CreateObject (_requestedType);

      Assert.That (result, Is.EqualTo ("default .ctor"));
    }

    [Test]
    public void CreateObject_ConstructorArguments ()
    {
      var arguments = ParamList.Create ("abc", 7);
      _typeCacheMock
          .Expect (
              mock => mock.GetOrCreateConstructorCall (_requestedType, arguments.FuncType, false))
          .Return (
              new Func<string, int, object> (
                  (s, i) =>
                  {
                    Assert.That (s, Is.EqualTo ("abc"));
                    Assert.That (i, Is.EqualTo (7));
                    return "abc, 7";
                  }));

      var result = _factory.CreateObject (_requestedType, arguments);

      Assert.That (result, Is.EqualTo ("abc, 7"));
    }

    [Test]
    public void CreateObject_NonPublicConstructor ()
    {
      const bool allowNonPublic = true;
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (_requestedType, typeof (Func<object>), allowNonPublic))
          .Return (new Func<object> (() => "non-public .ctor"));

      var result = _factory.CreateObject (_requestedType, allowNonPublicConstructor: allowNonPublic);

      Assert.That (result, Is.EqualTo ("non-public .ctor"));
    }

    [Test]
    public void CreateObject_Generic ()
    {
      var assembledInstance = new AssembledType();
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (typeof (RequestedType), ParamList.Empty.FuncType, false))
          .Return (new Func<object> (() => assembledInstance));

      var result = _factory.CreateObject<RequestedType>();

      Assert.That (result, Is.SameAs (assembledInstance));
    }

    [Test]
    public void CreateObject_Initializable ()
    {
      var initializableObjectMock = MockRepository.GenerateMock<IInitializableObject>();
      _typeCacheMock
          .Expect (mock => mock.GetOrCreateConstructorCall (_requestedType, ParamList.Empty.FuncType, false))
          .Return (new Func<object> (() => initializableObjectMock));

      var result = _factory.CreateObject (_requestedType);

      initializableObjectMock.AssertWasCalled (mock => mock.Initialize(), mo => mo.Repeat.Once());
      Assert.That (result, Is.SameAs (initializableObjectMock));
    }

    [Test]
    public void GetAssembledType ()
    {
      var fakeAssembledType = ReflectionObjectMother.GetSomeDifferentType();
      _typeCacheMock.Expect (x => x.GetOrCreateType (_requestedType)).Return (fakeAssembledType);

      var result = _factory.GetAssembledType (_requestedType);

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeAssembledType));
    }

    [Test]
    public void GetAssembledType_Generic ()
    {
      var fakeAssembledType = ReflectionObjectMother.GetSomeDifferentType();
      _typeCacheMock.Expect (x => x.GetOrCreateType (typeof (RequestedType))).Return (fakeAssembledType);

      var result = _factory.GetAssembledType<RequestedType>();

      _typeCacheMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeAssembledType));
    }

    [Test]
    public void GetUninitializedObject ()
    {
      var assembledType = typeof (AssembledType);
      _typeCacheMock.Expect (mock => mock.GetOrCreateType (_requestedType)).Return (assembledType);

      var result = (AssembledType) _factory.GetUninitializedObject (_requestedType);

      Assert.That (result.GetType(), Is.SameAs (assembledType));
    }

    [Test]
    public void GetUninitializedObject_Initializable ()
    {
      var assembledType = typeof (InitializableType);
      _typeCacheMock.Expect (mock => mock.GetOrCreateType (_requestedType)).Return (assembledType);

      var result = (InitializableType) _factory.GetUninitializedObject (_requestedType);

      Assert.That (result.GetType(), Is.SameAs (assembledType));
      Assert.That (result.CtorCalled, Is.False);
      Assert.That (result.InitializeCalled, Is.True);
    }

    class RequestedType { }
    class AssembledType : RequestedType { }

    class InitializableType : IInitializableObject
    {
      public readonly bool CtorCalled;
      public bool InitializeCalled;

      public InitializableType () { CtorCalled = true; }

      public void Initialize () { InitializeCalled = true; }
    }
  }
}