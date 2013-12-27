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

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class DeferredActionManagerTest
  {
    private DeferredActionManager _manager;

    [SetUp]
    public void SetUp ()
    {
      _manager = new DeferredActionManager();
    }

    [Test]
    public void AddAction ()
    {
      Assert.That (_manager.Actions, Is.Empty);

      Action action1 = () => { };
      Action action2 = () => { };

      _manager.AddAction (action1);
      _manager.AddAction (action2);

      Assert.That (_manager.Actions, Is.EqualTo (new[] { action1, action2 }));
    }

    [Test]
    public void ExecuteAllActions ()
    {
      var actionExecuted = false;
      Action action = () => actionExecuted = true;
      _manager.AddAction (action);
      Assert.That (actionExecuted, Is.False);

      _manager.ExecuteAllActions();

      Assert.That (actionExecuted, Is.True);
    }

    [Test]
    public void ExecuteAllActions_AddsAnotherActionWhichIsAlsoExecuted ()
    {
      var actionExecuted = false;
      Action action1 = () => actionExecuted = true;
      Action action2 = () => _manager.AddAction (action1);
      _manager.AddAction (action2);

      _manager.ExecuteAllActions ();

      Assert.That (actionExecuted, Is.True);
    }
  }
}