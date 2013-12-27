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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableNestedTypeCodeGeneratorTest
  {
    private ITypeBuilder _enclosingTypeBuilderMock;
    private MutableType _mutableType;

    private MutableNestedTypeCodeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _enclosingTypeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();
      _mutableType = MutableTypeObjectMother.Create();

      _generator = new MutableNestedTypeCodeGenerator
          (
          _enclosingTypeBuilderMock,
          _mutableType,
          MockRepository.GenerateStub<IMutableNestedTypeCodeGeneratorFactory>(),
          MockRepository.GenerateStub<IReflectionEmitCodeGenerator>(),
          MockRepository.GenerateStub<IEmittableOperandProvider>(),
          MockRepository.GenerateStub<IMemberEmitter>(),
          MockRepository.GenerateStub<IInitializationBuilder>(),
          MockRepository.GenerateStub<IProxySerializationEnabler>());
    }

    [Test]
    public void DeclareType ()
    {
      var fakeTypeBuilder = MockRepository.GenerateStub<ITypeBuilder>();
      _enclosingTypeBuilderMock.Expect (mock => mock.DefineNestedType (_mutableType.Name, _mutableType.Attributes)).Return (fakeTypeBuilder);
      var codeGeneratorStub = MockRepository.GenerateStub<IReflectionEmitCodeGenerator>();
      var emittableOperandProviderStub = MockRepository.GenerateStub<IEmittableOperandProvider>();

      var result = _generator.Invoke ("DefineType", codeGeneratorStub, emittableOperandProviderStub);

      _enclosingTypeBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeTypeBuilder));
    }
  }
}