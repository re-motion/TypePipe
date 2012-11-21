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
using System.Threading;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class LockingCodeGeneratorDecoratorTest
  {
    private ICodeGenerator _innerCodeGeneratorMock;
    private object _lockObject;

    private LockingCodeGeneratorDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerCodeGeneratorMock = MockRepository.GenerateStrictMock<ICodeGenerator>();
      _lockObject = new object();

      _decorator = new LockingCodeGeneratorDecorator (_innerCodeGeneratorMock, _lockObject);
    }

    [Test]
    public void AssemblyName ()
    {
      ExpectSynchronizedDelegation (cg => cg.AssemblyName, "abc");
    }

    [Test]
    public void SetAssemblyName ()
    {
      ExpectSynchronizedDelegation (cg => cg.SetAssemblyName ("def"));
    }

    [Test]
    public void FlushCodeToDisk ()
    {
      ExpectSynchronizedDelegation (cg => cg.FlushCodeToDisk(), "ghi");
    }

    private void ExpectSynchronizedDelegation<TResult> (Func<ICodeGenerator, TResult> action, TResult fakeResult)
    {
      _innerCodeGeneratorMock
          .Expect (mock => action (mock))
          .Return (fakeResult)
          .WhenCalled (mi => CheckLockIsHeld (_lockObject));

      var actualResult = action (_decorator);

      _innerCodeGeneratorMock.VerifyAllExpectations();
      Assert.That (actualResult, Is.EqualTo (fakeResult));
    }

    private void ExpectSynchronizedDelegation (Action<ICodeGenerator> action)
    {
      _innerCodeGeneratorMock
          .Expect (action)
          .WhenCalled (mi => CheckLockIsHeld (_lockObject));

      action (_decorator);

      _innerCodeGeneratorMock.VerifyAllExpectations();
    }

    private void CheckLockIsHeld (object lockObject)
    {
      var lockAcquired = true;
      ThreadRunner.Run (() => lockAcquired = Monitor.TryEnter (lockObject));

      Assert.That (lockAcquired, Is.False, "Parallel thread should have been blocked.");
    }
  }
}