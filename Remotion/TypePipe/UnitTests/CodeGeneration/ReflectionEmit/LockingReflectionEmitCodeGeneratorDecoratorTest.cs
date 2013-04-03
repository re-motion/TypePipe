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
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting.Threading;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class LockingReflectionEmitCodeGeneratorDecoratorTest
  {
    private LockingDecoratorTestHelper<IReflectionEmitCodeGenerator> _helper;

    [SetUp]
    public void SetUp ()
    {
      var innerCodeGeneratorMock = MockRepository.GenerateStrictMock<IReflectionEmitCodeGenerator>();

      var decorator = new LockingReflectionEmitCodeGeneratorDecorator (innerCodeGeneratorMock);

      var lockObject = PrivateInvoke.GetNonPublicField (decorator, "_lock");
      _helper = new LockingDecoratorTestHelper<IReflectionEmitCodeGenerator> (decorator, lockObject, innerCodeGeneratorMock);
    }

    [Test]
    public void DelegatingMembers_GuardedByLock ()
    {
      _helper.ExpectSynchronizedDelegation (g => g.AssemblyDirectory, "get dir");
      _helper.ExpectSynchronizedDelegation (g => g.AssemblyName, "get name");
      _helper.ExpectSynchronizedDelegation (g => g.DebugInfoGenerator, DebugInfoGenerator.CreatePdbGenerator());
      _helper.ExpectSynchronizedDelegation (g => g.SetAssemblyDirectory ("set dir"));
      _helper.ExpectSynchronizedDelegation (g => g.SetAssemblyName ("set name"));
      _helper.ExpectSynchronizedDelegation (g => g.FlushCodeToDisk ("config id"), "assembly path");
      _helper.ExpectSynchronizedDelegation (g => g.CreateEmittableOperandProvider(), MockRepository.GenerateStub<IEmittableOperandProvider>());
      _helper.ExpectSynchronizedDelegation (g => g.DefineType ("type name", (TypeAttributes) 7, null), MockRepository.GenerateStub<ITypeBuilder>());
    }
  }
}