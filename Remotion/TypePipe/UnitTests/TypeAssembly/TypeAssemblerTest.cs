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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class TypeAssemblerTest
  {
    [Test]
    public void Initialization ()
    {
      var participants = new[] { MockRepository.GenerateStub<ITypeAssemblyParticipant>() };
      var codeGenerator = MockRepository.GenerateStub<ITypeModifier>();

      var typeAssembler = new TypeAssembler(participants, codeGenerator);

      Assert.That (typeAssembler.Participants, Is.EqualTo(participants));
      Assert.That (typeAssembler.TypeModifier, Is.SameAs(codeGenerator));
    }

    [Test]
    public void AssemblyType ()
    {
      var mockRepository = new MockRepository();
      var participantMock1 = mockRepository.StrictMock<ITypeAssemblyParticipant>();
      var participantMock2 = mockRepository.StrictMock<ITypeAssemblyParticipant> ();
      var participants = new[] { participantMock1, participantMock2 };

      var typeModifierMock = mockRepository.StrictMock<ITypeModifier> ();

      var typeTemplate = mockRepository.StrictMock<ITypeInfo>();
      var mutableType = new MutableType (typeTemplate);
      var requestedType = typeof (string);
      var fakeResult = typeof (DateTime);

      using (mockRepository.Ordered ())
      {
        typeModifierMock
            .Expect (mock => mock.CreateMutableType (requestedType))
            .Return (mutableType);

        participantMock1.Expect (mock => mock.ModifyType (mutableType));
        participantMock2.Expect (mock => mock.ModifyType (mutableType));

        typeModifierMock
            .Expect (mock => mock.ApplyModifications (mutableType))
            .Return (fakeResult);
      }
      mockRepository.ReplayAll();

      var typeAssembler = new TypeAssembler (participants, typeModifierMock);
      var result = typeAssembler.AssembleType (requestedType);

      mockRepository.VerifyAll();
      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}