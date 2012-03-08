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
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.FutureReflection;
using Remotion.TypePipe.TypeAssembly;
using Rhino.Mocks;

namespace TypePipe.IntegrationTests
{
  public abstract class TypeAssemblerIntegrationTestBase
  {
    private AssemblyBuilder _assemblyBuilder;

    [TearDown]
    public void TearDown ()
    {
      var assemblyFileName = GetAssemblyNameForThisTest() + ".dll";
      _assemblyBuilder.Save (assemblyFileName);
      PEVerifier.CreateDefault ().VerifyPEFile (assemblyFileName);
    }

    protected Type AssembleType<T> (params Action<MutableType>[] participantActions)
    {
      var participants = participantActions.Select (CreateTypeAssemblyParticipant).ToArray();
      var originalType = typeof (T);

      return AssembleType (originalType, participants);
    }

    protected Type AssembleType (Type originalType, params ITypeAssemblyParticipant[] participants)
    {
      var typeAssembler = new TypeAssembler (participants, CreateReflectionEmitTypeModifier());
      var assembledType = typeAssembler.AssembleType (originalType);

      return assembledType;
    }

    protected ITypeAssemblyParticipant CreateTypeAssemblyParticipant (Action<MutableType> typeModification)
    {
      var participantStub = MockRepository.GenerateStub<ITypeAssemblyParticipant>();
      participantStub
          .Stub (stub => stub.ModifyType (Arg<MutableType>.Is.Anything))
          .Do (typeModification);

      return participantStub;
    }

    private ITypeModifier CreateReflectionEmitTypeModifier ()
    {
      var assemblyName = GetAssemblyNameForThisTest();
      _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.RunAndSave);
      var moduleBuilder = _assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeModifier = new ReflectionEmitTypeModifier (new ModuleBuilderAdapter (moduleBuilder), new GuidBasedSubclassProxyNameProvider());

      return typeModifier;
    }

    private string GetAssemblyNameForThisTest ()
    {
      return "TypeAssemblerIntegrationTest_" + GetType().Name;
    }
  }
}