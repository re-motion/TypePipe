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
using Remotion.TypePipe.TypeAssembly;
using Rhino.Mocks;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AddMarkerInterfaceTest
  {
    private ReflectionEmitCodeGenerator _codeGenerator;

    [SetUp]
    public void SetUp ()
    {
      _codeGenerator = new ReflectionEmitCodeGenerator ();
    }

    [Test]
    [Ignore ("TODO Type Pipe")]
    public void AddMarkerInterface ()
    {
      Assert.That (typeof (ModifiedType).GetInterfaces (), Has.No.Member (typeof (IMarkerInterface)));
      var participant = CreateTypeAssemblyParticipant (mutableType => mutableType.AddInterface (typeof (IMarkerInterface)));

      Type type = AssembleType (participant);

      Assert.That (type.GetInterfaces (), Has.Member (typeof (IMarkerInterface)));
    }

    private Type AssembleType (ITypeAssemblyParticipant participantStub)
    {
      var typeAssembler = new TypeAssembler (new[] { participantStub }, _codeGenerator);
      return typeAssembler.AssembleType (typeof (ModifiedType));
    }

    private ITypeAssemblyParticipant CreateTypeAssemblyParticipant (Action<IMutableType> typeModification)
    {
      var participantStub = MockRepository.GenerateStub<ITypeAssemblyParticipant>();
      participantStub
          .Stub (stub => stub.ModifyType (Arg<IMutableType>.Is.Anything))
          .Do (typeModification);
      return participantStub;
    }

    public class ModifiedType
    {
    }

    public interface IMarkerInterface
    {
    }


  }
}