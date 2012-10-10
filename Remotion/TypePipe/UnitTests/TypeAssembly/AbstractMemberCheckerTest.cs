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
using Remotion.TypePipe.TypeAssembly;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class AbstractMemberCheckerTest
  {
    private AbstractMemberChecker _checker;

    [SetUp]
    public void SetUp ()
    {
      _checker = new AbstractMemberChecker ();
    }

    [Test]
    public void IsFullyImplemented_ConcretType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (ConcreteType));

      var result = _checker.IsFullyImplemented (mutableType);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsFullyImplemented_ImplicitConcretType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (AbstractTypeWithoutMethods));

      var result = _checker.IsFullyImplemented (mutableType);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsFullyImplemented_AbstractType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (AbstractTypeWithOneMethod));

      var result = _checker.IsFullyImplemented (mutableType);

      Assert.That (result, Is.False);
    }

    class ConcreteType { }

    abstract class AbstractTypeWithoutMethods { }

    abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method ();
    }
  }
}