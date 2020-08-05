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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Moq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class NestedFactoryTest
  {
    private NestedTypeFactory _factory;

    private Mock<IMutableTypeFactory> _mutableTypeFactoryMock;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeFactoryMock = new Mock<IMutableTypeFactory> (MockBehavior.Strict);

      _factory = new NestedTypeFactory (_mutableTypeFactoryMock.Object);
    }

    [Test]
    public void CreateNestedType ()
    {
      var typeName = "NestedType";
      var @namespace = "namespace";
      var declaringType = MutableTypeObjectMother.Create (@namespace: @namespace);
      var typeAttributes = TypeAttributes.NestedPublic;
      var baseType = ReflectionObjectMother.GetSomeType();
      var mutableTypeFake = MutableTypeObjectMother.Create();

      _mutableTypeFactoryMock
          .Setup (mock => mock.CreateType (typeName, @namespace, typeAttributes, baseType, declaringType))
          .Returns (mutableTypeFake)
          .Verifiable();

      var result = _factory.CreateNestedType (declaringType, typeName, typeAttributes, baseType);

      _mutableTypeFactoryMock.Verify();
      Assert.That (result, Is.SameAs (mutableTypeFake));
    }
  }
}