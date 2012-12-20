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
using Remotion.Development.RhinoMocks.UnitTesting.Threading;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.CodeGeneration;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class LockingCodeGeneratorDecoratorTest
  {
    private LockingDecoratorTestHelper<ICodeGenerator> _helper;

    [SetUp]
    public void SetUp ()
    {
      var innerCodeGeneratorMock = MockRepository.GenerateStrictMock<ICodeGenerator>();
      var lockObject = new object();

      var decorator = new LockingCodeGeneratorDecorator (innerCodeGeneratorMock, lockObject);

      _helper = new LockingDecoratorTestHelper<ICodeGenerator> (decorator, lockObject, innerCodeGeneratorMock);
    }

    [Test]
    public void AssemblyDirectory ()
    {
      _helper.ExpectSynchronizedDelegation (cg => cg.AssemblyDirectory, "xyz");
    }

    [Test]
    public void AssemblyName ()
    {
      _helper.ExpectSynchronizedDelegation (cg => cg.AssemblyName, "abc");
    }

    [Test]
    public void IsStrongNamingEnabled ()
    {
      var randomBool = BooleanObjectMother.GetRandomBoolean();
      _helper.ExpectSynchronizedDelegation (cg => cg.IsStrongNamingEnabled, randomBool);
    }

    [Test]
    public void SetAssemblyDirectory ()
    {
      _helper.ExpectSynchronizedDelegation (cg => cg.SetAssemblyDirectory ("klm"));
    }

    [Test]
    public void SetAssemblyName ()
    {
      _helper.ExpectSynchronizedDelegation (cg => cg.SetAssemblyName ("def"));
    }

    [Test]
    public void FlushCodeToDisk ()
    {
      _helper.ExpectSynchronizedDelegation (cg => cg.FlushCodeToDisk(), "ghi");
    }
  }
}