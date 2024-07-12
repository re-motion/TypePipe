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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableNestedTypeCodeGeneratorTest
  {
    private Mock<ITypeBuilder> _enclosingTypeBuilderMock;
    private MutableType _mutableType;

    private MutableNestedTypeCodeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _enclosingTypeBuilderMock = new Mock<ITypeBuilder> (MockBehavior.Strict);
      _mutableType = MutableTypeObjectMother.Create();

      _generator = new MutableNestedTypeCodeGenerator
          (
          _enclosingTypeBuilderMock.Object,
          _mutableType,
          new Mock<IMutableNestedTypeCodeGeneratorFactory>().Object,
          new Mock<IReflectionEmitCodeGenerator>().Object,
          new Mock<IEmittableOperandProvider>().Object,
          new Mock<IMemberEmitter>().Object,
          new Mock<IInitializationBuilder>().Object,
          new Mock<IProxySerializationEnabler>().Object);
    }

    [Test]
    public void DeclareType ()
    {
      var fakeTypeBuilder = new Mock<ITypeBuilder>().Object;
      _enclosingTypeBuilderMock.Setup (mock => mock.DefineNestedType (_mutableType.Name, _mutableType.Attributes)).Returns (fakeTypeBuilder).Verifiable();
      var codeGeneratorStub = new Mock<IReflectionEmitCodeGenerator>();
      var emittableOperandProviderStub = new Mock<IEmittableOperandProvider>();

      var result = _generator.Invoke ("DefineType", codeGeneratorStub.Object, emittableOperandProviderStub.Object);

      _enclosingTypeBuilderMock.Verify();
      Assert.That (result, Is.SameAs (fakeTypeBuilder));
    }
  }
}