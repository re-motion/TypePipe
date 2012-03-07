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
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.FutureReflection;
using Remotion.TypePipe.TypeAssembly;
using Rhino.Mocks;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AddMarkerInterfaceTest
  {
    private ReflectionEmitTypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("Test"), AssemblyBuilderAccess.RunAndSave);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule ("Test.dll");
      _typeModifier = new ReflectionEmitTypeModifier (new ModuleBuilderAdapter (moduleBuilder), new GuidBasedSubclassProxyNameProvider());
    }

    [Test]
    public void AddMarkerInterface ()
    {
      Assert.That (typeof (OriginalType).GetInterfaces (), Is.EquivalentTo (new[] { typeof (IOriginalInterface) }));
      var participant = CreateTypeAssemblyParticipant (mutableType => mutableType.AddInterface (typeof (IMarkerInterface)));

      Type type = AssembleType (typeof (OriginalType), participant);

      Assert.That (type.GetInterfaces (), Is.EquivalentTo (new[] { typeof (IOriginalInterface), typeof (IMarkerInterface) }));
    }

    private Type AssembleType (Type requestedType, ITypeAssemblyParticipant participantStub)
    {
      var typeAssembler = new TypeAssembler (new[] { participantStub }, _typeModifier);
      return typeAssembler.AssembleType (requestedType);
    }

    private ITypeAssemblyParticipant CreateTypeAssemblyParticipant (Action<MutableType> typeModification)
    {
      var participantStub = MockRepository.GenerateStub<ITypeAssemblyParticipant>();
      participantStub
          .Stub (stub => stub.ModifyType (Arg<MutableType>.Is.Anything))
          .Do (typeModification);
      return participantStub;
    }

    public class OriginalType : IOriginalInterface
    {
    }

    public interface IOriginalInterface
    {
    }

    public interface IMarkerInterface
    {
    }


  }
}