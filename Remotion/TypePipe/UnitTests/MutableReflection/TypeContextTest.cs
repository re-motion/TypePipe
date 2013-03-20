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
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypeContextTest
  {
    private Type _requestedType;
    private MutableType _proxyType;
    private IMutableTypeFactory _mutableTypeFactoryMock;
    private IDictionary<string, object> _state;

    private TypeContext _typeContext;

    [SetUp]
    public void SetUp ()
    {
      _requestedType = ReflectionObjectMother.GetSomeType();
      _proxyType = MutableTypeObjectMother.Create();
      _mutableTypeFactoryMock = MockRepository.GenerateStrictMock<IMutableTypeFactory>();
      _mutableTypeFactoryMock.Expect (mock => mock.CreateProxy (_requestedType)).Return (_proxyType);

      _typeContext = new TypeContext (_mutableTypeFactoryMock, _requestedType, _state);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_typeContext.RequestedType, Is.SameAs (_requestedType));
      Assert.That (_typeContext.ProxyType, Is.SameAs (_proxyType));
      Assert.That (_typeContext.AdditionalTypes, Is.Empty);
      Assert.That (_typeContext.State, Is.SameAs (_state));
    }

    [Test]
    public void CreateType ()
    {
      var name = "name";
      var @namespace = "namespace";
      var attributes = (TypeAttributes) 7;
      var baseType = ReflectionObjectMother.GetSomeType();
      var fakeResult = MutableTypeObjectMother.Create();
      _mutableTypeFactoryMock.Expect (mock => mock.CreateType (name, @namespace, attributes, baseType)).Return (fakeResult);

      var result = _typeContext.CreateType (name, @namespace, attributes, baseType);

      _mutableTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_typeContext.AdditionalTypes, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void CreateInterface ()
    {
      var name = "name";
      var @namespace = "namespace";
      var fakeResult = MutableTypeObjectMother.Create();
      _mutableTypeFactoryMock.Expect (mock => mock.CreateInterface (name, @namespace)).Return (fakeResult);

      var result = _typeContext.CreateInterface (name, @namespace);

      _mutableTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_typeContext.AdditionalTypes, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void CreateProxy ()
    {
      var baseType = ReflectionObjectMother.GetSomeType();
      var fakeResult = MutableTypeObjectMother.Create();
      _mutableTypeFactoryMock.Expect (mock => mock.CreateProxy (baseType)).Return (fakeResult);

      var result = _typeContext.CreateProxy (baseType);

      _mutableTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_typeContext.AdditionalTypes, Is.EqualTo (new[] { result }));
    }
  }
}