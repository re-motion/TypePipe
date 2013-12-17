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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.CodeGeneration;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.TypeAssembly;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class TypeAssemblyContextBaseTest
  {
    private IMutableTypeFactory _mutableTypeFactoryMock;
    private IParticipantState _participantStateMock;
    private string _participantConfigurationID;

    private TestableTypeAssemblyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeFactoryMock = MockRepository.GenerateStrictMock<IMutableTypeFactory>();
      _participantStateMock = MockRepository.GenerateStrictMock<IParticipantState>();
      _participantConfigurationID = "participant configuration ID";

      _context = new TestableTypeAssemblyContextBase (_mutableTypeFactoryMock, _participantConfigurationID, _participantStateMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.AdditionalTypes, Is.Empty);
      Assert.That (_context.ParticipantState, Is.SameAs (_participantStateMock));
      Assert.That (_context.ParticipantConfigurationID, Is.EqualTo (_participantConfigurationID));
    }

    [Test]
    public void CreateType ()
    {
      var id = new object();
      var name = "name";
      var @namespace = "namespace";
      var attributes = (TypeAttributes) 7;
      var baseType = ReflectionObjectMother.GetSomeType();
      var fakeResult = MutableTypeObjectMother.Create();
      _mutableTypeFactoryMock.Expect (mock => mock.CreateType (name, @namespace, attributes, baseType, null)).Return (fakeResult);

      var result = _context.CreateAdditionalType (id, name, @namespace, attributes, baseType);

      _mutableTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_context.AdditionalTypes, Is.EqualTo (new[] { new KeyValuePair<object, MutableType> (id, result) }));
    }

    [Test]
    public void CreateProxy ()
    {
      var id = new object();
      var baseType = ReflectionObjectMother.GetSomeType();
      var fakeResult = MutableTypeObjectMother.Create();
      var typeModificationContextStub = MockRepository.GenerateStrictMock<ITypeModificationTracker>();
      typeModificationContextStub.Stub (stub => stub.Type).Return (fakeResult);
      _mutableTypeFactoryMock.Expect (mock => mock.CreateProxy (baseType, ProxyKind.AdditionalType)).Return (typeModificationContextStub);

      var result = _context.CreateAddtionalProxyType (id, baseType);

      _mutableTypeFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_context.AdditionalTypes, Is.EqualTo (new[] { new KeyValuePair<object, MutableType> (id, result) }));
    }
  }
}