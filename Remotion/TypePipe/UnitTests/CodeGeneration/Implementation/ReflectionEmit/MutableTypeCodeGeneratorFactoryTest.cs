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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.Implementation.ReflectionEmit
{
  [TestFixture]
  public class MutableTypeCodeGeneratorFactoryTest
  {
    private IProxySerializationEnabler _proxySerializationEnablerMock;
    private IReflectionEmitCodeGenerator _codeGeneratorMock;
    private IInitializationBuilder _initializationBuilderMock;
    private IMemberEmitterFactory _memberEmitterFactoryMock;

    private MutableTypeCodeGeneratorFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterFactoryMock = MockRepository.GenerateStrictMock<IMemberEmitterFactory> ();
      _codeGeneratorMock = MockRepository.GenerateStrictMock<IReflectionEmitCodeGenerator>();
      _initializationBuilderMock = MockRepository.GenerateStrictMock<IInitializationBuilder> ();
      _proxySerializationEnablerMock = MockRepository.GenerateStrictMock<IProxySerializationEnabler> ();

      _factory = new MutableTypeCodeGeneratorFactory (
          _memberEmitterFactoryMock, _codeGeneratorMock, _initializationBuilderMock, _proxySerializationEnablerMock);
    }

    [Test]
    public void Create ()
    {
      var fakeEmittableOperandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
      var fakeMemberEmitter = MockRepository.GenerateStrictMock<IMemberEmitter>();
      _codeGeneratorMock.Expect (mock => mock.CreateEmittableOperandProvider()).Return (fakeEmittableOperandProvider);
      _memberEmitterFactoryMock.Expect (mock => mock.CreateMemberEmitter (fakeEmittableOperandProvider)).Return (fakeMemberEmitter);
      var mutableType1 = MutableTypeObjectMother.Create();
      var mutableType2 = MutableTypeObjectMother.Create();

      var result = _factory.CreateGenerators (new[] { mutableType1, mutableType2 }.AsOneTime()).ToList();

      Assert.That (result, Has.Count.EqualTo (2));
      Assert.That (result[0], Is.TypeOf<MutableTypeCodeGenerator>());
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_mutableType"), Is.SameAs (mutableType1));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_codeGenerator"), Is.SameAs (_codeGeneratorMock));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_memberEmitter"), Is.SameAs (fakeMemberEmitter));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_initializationBuilder"), Is.SameAs (_initializationBuilderMock));
      Assert.That (PrivateInvoke.GetNonPublicField (result[0], "_proxySerializationEnabler"), Is.SameAs (_proxySerializationEnablerMock));

      Assert.That (PrivateInvoke.GetNonPublicField (result[1], "_mutableType"), Is.SameAs (mutableType2));
      Assert.That (
          PrivateInvoke.GetNonPublicField (result[1], "_memberEmitter"),
          Is.SameAs (fakeMemberEmitter),
          "Generators share the MemberEmitter (and therefore also the EmittableOperandProvider).");
    }
  }
}