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
using NUnit.Framework;
using Remotion.TypePipe.StrongNaming;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class StrongNamedTypeVerifierTest
  {
    [Test]
    public void IsStrongNamed ()
    {
      var type = ReflectionObjectMother.GetSomeType();

      Check (type, type, strongNamed: true);
      Check (type, type, strongNamed: false);
    }

    [Test]
    public void IsStrongNamed_Generic ()
    {
      var type = typeof (IEquatable<StrongNamedTypeVerifierTest>);

      Check (type, typeof (StrongNamedTypeVerifierTest), strongNamed: true);
    }

    [Test]
    public void Cache ()
    {
      var assemblyVerifierMock = MockRepository.GenerateStrictMock<IStrongNameAssemblyVerifier> ();
      var verifier = new StrongNameTypeVerifier (assemblyVerifierMock);

      var type = ReflectionObjectMother.GetSomeType();
      assemblyVerifierMock.Expect (x => x.IsStrongNamed (type.Assembly)).Return (false).Repeat.Once();

      verifier.IsStrongNamed (type);
      verifier.IsStrongNamed (type);
    }
 
    private void Check (Type type, Type expectedTypeToVerify, bool strongNamed)
    {
      var assemblyVerifierMock = MockRepository.GenerateStrictMock<IStrongNameAssemblyVerifier>();
      var verifier = new StrongNameTypeVerifier (assemblyVerifierMock);
      assemblyVerifierMock.Expect (x => x.IsStrongNamed (expectedTypeToVerify.Assembly)).Return (strongNamed);
      assemblyVerifierMock.Stub (x => x.IsStrongNamed (Arg<Assembly>.Is.Anything)).Return (true);

      var result = verifier.IsStrongNamed (type);

      assemblyVerifierMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (strongNamed));
    }

    public interface IGenericInterface<T> { }
  }
}