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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class MutableTypeBatchCodeGeneratorTest
  {
    private MockRepository _mockRepository;
    private Mock<IDependentTypeSorter> _dependentTypeSorterMock;
    private Mock<IMutableTypeCodeGeneratorFactory> _mutableTypeCodeGeneratorFactoryMock;

    private MutableTypeBatchCodeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _dependentTypeSorterMock = new Mock<IDependentTypeSorter> (MockBehavior.Strict);
      _mutableTypeCodeGeneratorFactoryMock = new Mock<IMutableTypeCodeGeneratorFactory> (MockBehavior.Strict);

      _generator = new MutableTypeBatchCodeGenerator (_dependentTypeSorterMock.Object, _mutableTypeCodeGeneratorFactoryMock.Object);
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


      var generatorMock1 = new Mock<IMutableTypeCodeGenerator> (MockBehavior.Strict);
      var generatorMock2 = new Mock<IMutableTypeCodeGenerator> (MockBehavior.Strict);
      var nestedGeneratorMock = new Mock<IMutableTypeCodeGenerator> (MockBehavior.Strict);

      _mutableTypeCodeGeneratorFactoryMock
          .Setup (_ => _.CreateGenerators (new[] { mutableType1, mutableType2 }))
          .Returns (new[] { generatorMock1.Object, generatorMock2.Object })
          .Verifiable();

      var sequence = new MockSequence();
      generatorMock1
          .InSequence (sequence)
          .Setup (_ => _.DeclareType());
      generatorMock1
          .InSequence (sequence)
          .Setup (_ => _.CreateNestedTypeGenerators()).Returns (new[] { nestedGeneratorMock.Object });
      nestedGeneratorMock
          .InSequence (sequence)
          .Setup (_ => _.DeclareType());
      nestedGeneratorMock
          .InSequence (sequence)
          .Setup (_ => _.CreateNestedTypeGenerators()).Returns (new IMutableTypeCodeGenerator[0]);
      generatorMock2
          .InSequence (sequence)
          .Setup (_ => _.DeclareType());
      generatorMock2
          .InSequence (sequence)
          .Setup (_ => _.CreateNestedTypeGenerators()).Returns (new IMutableTypeCodeGenerator[0]);

      generatorMock1
          .InSequence (sequence)
          .SetupGet (_ => _.MutableType).Returns (mutableType1);
      nestedGeneratorMock
          .InSequence (sequence)
          .SetupGet (_ => _.MutableType).Returns (nestedMutableType);
      generatorMock2
          .InSequence (sequence)
          .SetupGet (_ => _.MutableType).Returns (mutableType2);

      generatorMock1
          .InSequence (sequence)
          .SetupGet (_ => _.MutableType).Returns (mutableType1);
      nestedGeneratorMock
          .InSequence (sequence)
          .SetupGet (_ => _.MutableType).Returns (nestedMutableType);
      generatorMock2
          .InSequence (sequence)
          .SetupGet (_ => _.MutableType).Returns (mutableType2);

      _dependentTypeSorterMock
          .Setup (
              _ => _.Sort (It.Is<IEnumerable<MutableType>> (types => types.SequenceEqual (new[] { mutableType1, nestedMutableType, mutableType2 }))))
          .Returns (new[] { mutableType2, mutableType1, nestedMutableType })
          .Verifiable();

      generatorMock2.Setup (_ => _.DefineTypeFacets()).Verifiable();
      generatorMock1.Setup (_ => _.DefineTypeFacets()).Verifiable();
      nestedGeneratorMock.Setup (_ => _.DefineTypeFacets()).Verifiable();
      generatorMock2.SetupGet (_ => _.MutableType).Returns (mutableType2).Verifiable();
      generatorMock2.Setup (_ => _.CreateType()).Returns (fakeType2).Verifiable();
      generatorMock1.SetupGet (_ => _.MutableType).Returns (mutableType1).Verifiable();
      generatorMock1.Setup (_ => _.CreateType()).Returns (fakeType1).Verifiable();
      nestedGeneratorMock.SetupGet (_ => _.MutableType).Returns (nestedMutableType).Verifiable();
      nestedGeneratorMock.Setup (_ => _.CreateType()).Returns (fakeNestedType).Verifiable();

      var result = _generator.GenerateTypes (new[] { mutableType1, mutableType2 }).ForceEnumeration();

      generatorMock1.Verify();
      generatorMock2.Verify();
      nestedGeneratorMock.Verify();

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