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
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.Implementation;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.Implementation
{
  [TestFixture]
  public class MutableTypeBatchCodeGeneratorTest
  {
    private MockRepository _mockRepository;
    private IDependentTypeSorter _dependentTypeSorterMock;
    private IMutableTypeCodeGeneratorFactory _mutableTypeCodeGeneratorFactoryMock;

    private MutableTypeBatchCodeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _dependentTypeSorterMock = _mockRepository.StrictMock<IDependentTypeSorter>();
      _mutableTypeCodeGeneratorFactoryMock = _mockRepository.StrictMock<IMutableTypeCodeGeneratorFactory>();

      _generator = new MutableTypeBatchCodeGenerator (_dependentTypeSorterMock, _mutableTypeCodeGeneratorFactoryMock);
    }

    [Test]
    public void GenerateTypes ()
    {
      var mutableType1 = MutableTypeObjectMother.Create();
      var mutableType2 = MutableTypeObjectMother.Create();
      var fakeMutableType1 = MutableTypeObjectMother.Create();
      var fakeMutableType2 = MutableTypeObjectMother.Create();
      var fakeType1 = ReflectionObjectMother.GetSomeType();
      var fakeType2 = ReflectionObjectMother.GetSomeOtherType();

      using (_mockRepository.Ordered())
      {
        var sortedMutableType1 = MutableTypeObjectMother.Create();
        var sortedMutableType2 = MutableTypeObjectMother.Create();
        _dependentTypeSorterMock
            .Expect (mock => mock.Sort (Arg<IEnumerable<MutableType>>.List.Equal (new[] { mutableType1, mutableType2 })))
            .Return (new[] { sortedMutableType1, sortedMutableType2 });

        var generatorMock1 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        var generatorMock2 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        _mutableTypeCodeGeneratorFactoryMock
            .Expect (mock => mock.CreateGenerators (new[] { sortedMutableType1, sortedMutableType2 }))
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

      var result = _generator.GenerateTypes (new[] { mutableType1, mutableType2 }).ForceEnumeration();

      _mockRepository.VerifyAll();
      var expectedMapping =
          new[]
          {
              new KeyValuePair<MutableType, Type> (fakeMutableType1, fakeType1),
              new KeyValuePair<MutableType, Type> (fakeMutableType2, fakeType2)
          };
      Assert.That (result, Is.EqualTo (expectedMapping));
    }
  }
}