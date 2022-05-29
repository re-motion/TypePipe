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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.TypeAssembly;
using Moq;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class TypeAssemblyContextBaseTest
  {
    private Mock<IMutableTypeFactory> _mutableTypeFactoryMock;
    private Mock<IParticipantState> _participantStateMock;
    private string _participantConfigurationID;

    private TestableTypeAssemblyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeFactoryMock = new Mock<IMutableTypeFactory> (MockBehavior.Strict);
      _participantStateMock = new Mock<IParticipantState> (MockBehavior.Strict);
      _participantConfigurationID = "participant configuration ID";

      _context = new TestableTypeAssemblyContextBase (_mutableTypeFactoryMock.Object, _participantConfigurationID, _participantStateMock.Object);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.AdditionalTypes, Is.Empty);
      Assert.That (_context.ParticipantState, Is.SameAs (_participantStateMock.Object));
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
      _mutableTypeFactoryMock.Setup (mock => mock.CreateType (name, @namespace, attributes, baseType, null)).Returns (fakeResult).Verifiable();

      var result = _context.CreateAdditionalType (id, name, @namespace, attributes, baseType);

      _mutableTypeFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_context.AdditionalTypes, Is.EqualTo (new[] { new KeyValuePair<object, MutableType> (id, result) }));
    }

    [Test]
    public void CreateProxy ()
    {
      var id = new object();
      var baseType = ReflectionObjectMother.GetSomeType();
      var fakeResult = MutableTypeObjectMother.Create();
      var typeModificationContextStub = new Mock<ITypeModificationTracker> (MockBehavior.Strict);
      typeModificationContextStub.SetupGet (stub => stub.Type).Returns (fakeResult);
      _mutableTypeFactoryMock.Setup (mock => mock.CreateProxy (baseType, ProxyKind.AdditionalType)).Returns (typeModificationContextStub.Object).Verifiable();

      var result = _context.CreateAddtionalProxyType (id, baseType);

      _mutableTypeFactoryMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_context.AdditionalTypes, Is.EqualTo (new[] { new KeyValuePair<object, MutableType> (id, result) }));
    }
  }
}