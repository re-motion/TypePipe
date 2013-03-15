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
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableTypeCodeGeneratorFactoryTest
  {
    private IProxySerializationEnabler _proxySerializationEnablerMock;
    private IInitializationBuilder _initializationBuilderMock;
    private IMemberEmitterFactory _memberEmitterFactoryMock;

    private MutableTypeCodeGeneratorFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterFactoryMock = MockRepository.GenerateStrictMock<IMemberEmitterFactory> ();
      _initializationBuilderMock = MockRepository.GenerateStrictMock<IInitializationBuilder> ();
      _proxySerializationEnablerMock = MockRepository.GenerateStrictMock<IProxySerializationEnabler> ();

      _factory = new MutableTypeCodeGeneratorFactory (_memberEmitterFactoryMock, _initializationBuilderMock, _proxySerializationEnablerMock);
    }

    [Test]
    public void Create ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var codeGeneratorMock = MockRepository.GenerateStrictMock<IReflectionEmitCodeGenerator>();

      var fakeEmittableOperandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
      var fakeMemberEmitter = MockRepository.GenerateStrictMock<IMemberEmitter>();
      codeGeneratorMock.Expect (mock => mock.EmittableOperandProvider).Return (fakeEmittableOperandProvider);
      _memberEmitterFactoryMock.Expect (mock => mock.CreateMemberEmitter (fakeEmittableOperandProvider)).Return (fakeMemberEmitter);

      var result = _factory.Create (mutableType, codeGeneratorMock);

      Assert.That (result, Is.TypeOf<MutableTypeCodeGenerator>());
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_mutableType"), Is.SameAs (mutableType));
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_codeGenerator"), Is.SameAs (codeGeneratorMock));
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_memberEmitter"), Is.SameAs (fakeMemberEmitter));
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_initializationBuilder"), Is.SameAs (_initializationBuilderMock));
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_proxySerializationEnabler"), Is.SameAs (_proxySerializationEnablerMock));
    }
  }
}