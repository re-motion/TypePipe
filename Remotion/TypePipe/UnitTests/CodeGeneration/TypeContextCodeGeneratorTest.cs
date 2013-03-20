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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class TypeContextCodeGeneratorTest
  {
    private MockRepository _mockRepository;
    private IDependentTypeSorter _dependentTypeSorterMock;
    private IMutableTypeCodeGeneratorFactory _mutableTypeCodeGeneratorFactoryMock;

    private TypeContextCodeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _dependentTypeSorterMock = _mockRepository.StrictMock<IDependentTypeSorter>();
      _mutableTypeCodeGeneratorFactoryMock = _mockRepository.StrictMock<IMutableTypeCodeGeneratorFactory>();

      _generator = new TypeContextCodeGenerator (_dependentTypeSorterMock, _mutableTypeCodeGeneratorFactoryMock);
    }

    [Test]
    public void CodeGenerator ()
    {
      var fakeCodeGenerator = MockRepository.GenerateStrictMock<ICodeGenerator>();
      _mutableTypeCodeGeneratorFactoryMock.Expect (mock => mock.CodeGenerator).Return (fakeCodeGenerator);
      _mockRepository.ReplayAll();

      Assert.That (_generator.CodeGenerator, Is.SameAs (fakeCodeGenerator));
    }

    [Test]
    public void GenerateProxy ()
    {
      var requestedType = ReflectionObjectMother.GetSomeSubclassableType();
      var typeContext = TypeContextObjectMother.Create (requestedType);
      var proxyType = typeContext.ProxyType;
      var additionalType = typeContext.CreateType ("AdditionalType", null, TypeAttributes.Class, typeof (object));

      var fakeProxyType = ReflectionObjectMother.GetSomeType ();
      using (_mockRepository.Ordered())
      {
        var sortedType1 = MutableTypeObjectMother.Create ();
        var sortedType2 = MutableTypeObjectMother.Create ();
        _dependentTypeSorterMock.Expect (mock => mock.Sort (new[] { additionalType, proxyType })).Return (new[] { sortedType1, sortedType2 });

        var generatorMock1 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        var generatorMock2 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        _mutableTypeCodeGeneratorFactoryMock.Expect (mock => mock.Create (sortedType1)).Return (generatorMock1);
        _mutableTypeCodeGeneratorFactoryMock.Expect (mock => mock.Create (sortedType2)).Return (generatorMock2);

        generatorMock1.Expect (mock => mock.DeclareType());
        generatorMock2.Expect (mock => mock.DeclareType());
        generatorMock1.Expect (mock => mock.DefineTypeFacet());
        generatorMock2.Expect (mock => mock.DefineTypeFacet());
        generatorMock1.Expect (mock => mock.CreateType()).Return (fakeProxyType);
        generatorMock2.Expect (mock => mock.CreateType()).Return (MutableTypeObjectMother.Create());
      }
      _mockRepository.ReplayAll();

      var result = _generator.GenerateProxy (typeContext);

      _mockRepository.VerifyAll();
      Assert.That (result, Is.SameAs (fakeProxyType));
    }
  }
}