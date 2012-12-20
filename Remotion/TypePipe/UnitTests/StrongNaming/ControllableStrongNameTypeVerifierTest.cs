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
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class ControllableStrongNameTypeVerifierTest
  {
    private ControllableStrongNameTypeVerifier _verifier;

    private IStrongNameTypeVerifier _typeVerifierMock;

    [SetUp]
    public void SetUp ()
    {
      _typeVerifierMock = MockRepository.GenerateStrictMock<IStrongNameTypeVerifier>();
      _verifier = new ControllableStrongNameTypeVerifier (_typeVerifierMock);
    }

    [Test]
    public void IsStrongNamed ()
    {
      var type1 = ReflectionObjectMother.GetSomeType();
      var type2 = ReflectionObjectMother.GetSomeDifferentType();
      _typeVerifierMock.Expect (mock => mock.IsStrongNamed (type1)).Return (true);
      _typeVerifierMock.Expect (mock => mock.IsStrongNamed (type2)).Return (false);

      Assert.That (_verifier.IsStrongNamed (type1), Is.True);
      Assert.That (_verifier.IsStrongNamed (type2), Is.False);
      _typeVerifierMock.VerifyAllExpectations();
    }

    [Test]
    public void IsStrongNamed_Cache ()
    {
      var type = ReflectionObjectMother.GetSomeType ();
      _typeVerifierMock.Expect (mock => mock.IsStrongNamed (type)).Return (true)
          .Repeat.Once();

      _verifier.IsStrongNamed (type);
      _verifier.IsStrongNamed (type);

      _typeVerifierMock.VerifyAllExpectations();
    }

    [Test]
    public void SetIsStrongNamed ()
    {
      var mutableType = MutableTypeObjectMother.Create();

      _verifier.SetIsStrongNamed (mutableType, true);
      var result = _verifier.IsStrongNamed (mutableType);

      Assert.That (result, Is.True);
      _typeVerifierMock.AssertWasNotCalled (mock => mock.IsStrongNamed (Arg<Type>.Is.Anything));
    }

    [Test]
    public void SetIsStrongNamed_Unset ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      _typeVerifierMock.Expect (mock => mock.IsStrongNamed (mutableType)).Return (true);

      _verifier.SetIsStrongNamed (mutableType, true);
      _verifier.SetIsStrongNamed (mutableType, false);
      _verifier.IsStrongNamed (mutableType);

      _typeVerifierMock.VerifyAllExpectations();
    }
  }
}