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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableTypeCodeGeneratorFactoryTest
  {
    private Mock<IReflectionEmitCodeGenerator> _codeGeneratorMock;
    private Mock<IInitializationBuilder> _initializationBuilderMock;
    private Mock<IMemberEmitterFactory> _memberEmitterFactoryMock;

    private MutableTypeCodeGeneratorFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterFactoryMock = new Mock<IMemberEmitterFactory> (MockBehavior.Strict);
      _codeGeneratorMock = new Mock<IReflectionEmitCodeGenerator> (MockBehavior.Strict);
      _initializationBuilderMock = new Mock<IInitializationBuilder> (MockBehavior.Strict);

      _factory = new MutableTypeCodeGeneratorFactory (
          _memberEmitterFactoryMock.Object, _codeGeneratorMock.Object, _initializationBuilderMock.Object);
    }

    [Test]
    public void Create ()
    {
      var fakeEmittableOperandProvider = new Mock<IEmittableOperandProvider> (MockBehavior.Strict).Object;
      var fakeMemberEmitter = new Mock<IMemberEmitter> (MockBehavior.Strict).Object;
      _codeGeneratorMock.Setup (mock => mock.CreateEmittableOperandProvider()).Returns (fakeEmittableOperandProvider).Verifiable();
      _memberEmitterFactoryMock.Setup (mock => mock.CreateMemberEmitter (fakeEmittableOperandProvider)).Returns (fakeMemberEmitter).Verifiable();
      var mutableType1 = MutableTypeObjectMother.Create();
      var mutableType2 = MutableTypeObjectMother.Create();

      var result = _factory.CreateGenerators (new[] { mutableType1, mutableType2 }.AsOneTime()).ToList();

      Assert.That (result, Has.Count.EqualTo (2));
      Assert.That (result[0], Is.TypeOf<MutableTypeCodeGenerator>());
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_mutableType"), Is.SameAs (mutableType1));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_codeGenerator"), Is.SameAs (_codeGeneratorMock.Object));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_memberEmitter"), Is.SameAs (fakeMemberEmitter));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_initializationBuilder"), Is.SameAs (_initializationBuilderMock.Object));

      Assert.That (PrivateInvoke.GetNonPublicField (result[1], "_mutableType"), Is.SameAs (mutableType2));
      Assert.That (
          PrivateInvoke.GetNonPublicField (result[1], "_memberEmitter"),
          Is.SameAs (fakeMemberEmitter),
          "Generators share the MemberEmitter (and therefore also the EmittableOperandProvider).");
    }
  }
}