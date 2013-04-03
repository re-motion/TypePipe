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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class TypeAssemblyContextCodeGeneratorTest
  {
    private MockRepository _mockRepository;
    private IDependentTypeSorter _dependentTypeSorterMock;
    private IMutableTypeCodeGeneratorFactory _mutableTypeCodeGeneratorFactoryMock;

    private TypeAssemblyContextCodeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _dependentTypeSorterMock = _mockRepository.StrictMock<IDependentTypeSorter>();
      _mutableTypeCodeGeneratorFactoryMock = _mockRepository.StrictMock<IMutableTypeCodeGeneratorFactory>();

      _generator = new TypeAssemblyContextCodeGenerator (_dependentTypeSorterMock, _mutableTypeCodeGeneratorFactoryMock);
    }

    [Test]
    public void GenerateTypes ()
    {
      var requestedType = ReflectionObjectMother.GetSomeSubclassableType();
      var typeContext = TypeAssemblyContextObjectMother.Create (requestedType: requestedType);
      var proxyType = typeContext.ProxyType;
      var additionalType = typeContext.CreateType ("AdditionalType", null, TypeAttributes.Class, typeof (object));

      var fakeMutableType1 = MutableTypeObjectMother.Create();
      var fakeMutableType2 = MutableTypeObjectMother.Create();
      var fakeType1 = ReflectionObjectMother.GetSomeType();
      var fakeType2 = ReflectionObjectMother.GetSomeOtherType();
      using (_mockRepository.Ordered())
      {
        var fakeSortedType1 = MutableTypeObjectMother.Create();
        var fakeSortedType2 = MutableTypeObjectMother.Create();
        _dependentTypeSorterMock
            .Expect (mock => mock.Sort (Arg<IEnumerable<MutableType>>.List.Equal (new[] { additionalType, proxyType })))
            .Return (new[] { fakeSortedType1, fakeSortedType2 });

        var generatorMock1 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        var generatorMock2 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        _mutableTypeCodeGeneratorFactoryMock
            .Expect (mock => mock.CreateGenerators (new[] { fakeSortedType1, fakeSortedType2 }))
            .Return (new[] { generatorMock1, generatorMock2 });

        generatorMock1.Expect (mock => mock.DeclareType());
        generatorMock2.Expect (mock => mock.DeclareType());
        generatorMock1.Expect (mock => mock.DefineTypeFacets());
        generatorMock2.Expect (mock => mock.DefineTypeFacets());
        generatorMock1.Expect (mock => mock.MutableType).Return (fakeMutableType1);
        generatorMock1.Expect (mock => mock.CreateType()).Return (fakeType1);
        generatorMock2.Expect (mock => mock.MutableType).Return (fakeMutableType2);
        generatorMock2.Expect (mock => mock.CreateType()).Return (fakeType2);
      }
      _mockRepository.ReplayAll();

      var result = _generator.GenerateTypes (typeContext);

      _mockRepository.VerifyAll();
      Assert.That (result.GetGeneratedType (fakeMutableType1), Is.SameAs (fakeType1));
      Assert.That (result.GetGeneratedType (fakeMutableType2), Is.SameAs (fakeType2));
    }
  }
}