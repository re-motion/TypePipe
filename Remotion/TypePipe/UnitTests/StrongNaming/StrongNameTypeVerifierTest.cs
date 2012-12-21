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
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class StrongNameTypeVerifierTest
  {
    private IStrongNameAssemblyVerifier _assemblyVerifierMock;

    private StrongNameTypeVerifier _verifier;

    [SetUp]
    public void SetUp ()
    {
      _assemblyVerifierMock = MockRepository.GenerateStrictMock<IStrongNameAssemblyVerifier>();

      _verifier = new StrongNameTypeVerifier (_assemblyVerifierMock);
    }

    [Test]
    public void IsStrongNamed ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _assemblyVerifierMock.Expect (x => x.IsStrongNamed (type.Assembly)).Return (fakeResult);

      var result1 = _verifier.IsStrongNamed (type);
      var result2 = _verifier.IsStrongNamed (type);

      _assemblyVerifierMock.VerifyAllExpectations();
      Assert.That (result1, Is.EqualTo (fakeResult));
      Assert.That (result2, Is.EqualTo (fakeResult));
    }

    [Test]
    public void IsStrongNamed_Generic ()
    {
      var type = typeof (IList<Set<IParticipant>>);
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _assemblyVerifierMock.Expect (x => x.IsStrongNamed (typeof (IList<>).Assembly)).Return (true);
      _assemblyVerifierMock.Expect (x => x.IsStrongNamed (typeof (Set<>).Assembly)).Return (true);
      _assemblyVerifierMock.Expect (x => x.IsStrongNamed (typeof (IParticipant).Assembly)).Return (fakeResult);

      var result = _verifier.IsStrongNamed (type);

      _assemblyVerifierMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void IsStrongNamed_CacheUsesReferenceEquality ()
    {
      var type = ReflectionObjectMother.GetSomeSubclassableType();
      var mutableType = MutableTypeObjectMother.CreateForExisting (type);
      _verifier.SetStrongNamed (mutableType, true);
      _assemblyVerifierMock.Expect (x => x.IsStrongNamed (type.Assembly)).Return (true);

      _verifier.IsStrongNamed (type);

      _assemblyVerifierMock.VerifyAllExpectations();
    }

    [Test]
    public void SetStrongNamed ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      _assemblyVerifierMock.Stub (x => x.IsStrongNamed (type.Assembly)).Return (false);
      Assert.That (_verifier.IsStrongNamed (type), Is.False);

      _verifier.SetStrongNamed (type, true);

      Assert.That (_verifier.IsStrongNamed (type), Is.True);
    }
  }
}