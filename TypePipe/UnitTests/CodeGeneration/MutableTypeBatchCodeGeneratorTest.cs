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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
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
      var nestedMutableType = MutableTypeObjectMother.Create();
      var fakeType1 = ReflectionObjectMother.GetSomeType();
      var fakeType2 = ReflectionObjectMother.GetSomeOtherType();
      var fakeNestedType = ReflectionObjectMother.GetSomeNestedType();

      using (_mockRepository.Ordered())
      {
        var generatorMock1 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        var generatorMock2 = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();
        var nestedGeneratorMock = _mockRepository.StrictMock<IMutableTypeCodeGenerator>();

        _mutableTypeCodeGeneratorFactoryMock
            .Expect (_ => _.CreateGenerators (new[] { mutableType1, mutableType2 }))
            .Return (new[] { generatorMock1, generatorMock2 });

        generatorMock1.Expect (_ => _.DeclareType());
        generatorMock1.Expect (_ => _.CreateNestedTypeGenerators()).Return (new[] { nestedGeneratorMock });
        nestedGeneratorMock.Expect (_ => _.DeclareType());
        nestedGeneratorMock.Expect (_ => _.CreateNestedTypeGenerators()).Return (new IMutableTypeCodeGenerator[0]);
        generatorMock2.Expect (_ => _.DeclareType());
        generatorMock2.Expect (_ => _.CreateNestedTypeGenerators()).Return (new IMutableTypeCodeGenerator[0]);

        generatorMock1.Expect (_ => _.MutableType).Return (mutableType1);
        nestedGeneratorMock.Expect (_ => _.MutableType).Return (nestedMutableType);
        generatorMock2.Expect (_ => _.MutableType).Return (mutableType2);

        generatorMock1.Expect (_ => _.MutableType).Return (mutableType1);
        nestedGeneratorMock.Expect (_ => _.MutableType).Return (nestedMutableType);
        generatorMock2.Expect (_ => _.MutableType).Return (mutableType2);

        _dependentTypeSorterMock
            .Expect (_ => _.Sort (Arg<IEnumerable<MutableType>>.List.Equal (new[] { mutableType1, nestedMutableType, mutableType2 })))
            .Return (new[] { mutableType2, mutableType1, nestedMutableType });

        generatorMock2.Expect (_ => _.DefineTypeFacets());
        generatorMock1.Expect (_ => _.DefineTypeFacets());
        nestedGeneratorMock.Expect (_ => _.DefineTypeFacets());
        generatorMock2.Expect (_ => _.MutableType).Return (mutableType2);
        generatorMock2.Expect (_ => _.CreateType()).Return (fakeType2);
        generatorMock1.Expect (_ => _.MutableType).Return (mutableType1);
        generatorMock1.Expect (_ => _.CreateType()).Return (fakeType1);
        nestedGeneratorMock.Expect (_ => _.MutableType).Return (nestedMutableType);
        nestedGeneratorMock.Expect (_ => _.CreateType()).Return (fakeNestedType);
      }
      _mockRepository.ReplayAll();

      var result = _generator.GenerateTypes (new[] { mutableType1, mutableType2 }).ForceEnumeration();

      _mockRepository.VerifyAll();
      var expectedMapping =
          new[]
          {
              new KeyValuePair<MutableType, Type> (mutableType2, fakeType2),
              new KeyValuePair<MutableType, Type> (mutableType1, fakeType1)
              // Nested types are not included in the result mapping.
          };
      Assert.That (result, Is.EqualTo (expectedMapping));
    }
  }
}