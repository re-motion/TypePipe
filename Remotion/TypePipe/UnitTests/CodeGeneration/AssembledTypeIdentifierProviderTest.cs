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

using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class AssembledTypeIdentifierProviderTest
  {
    private IParticipant _participantWithoutIdentifierProvider;
    private IParticipant _participantWithIdentifierProvider;
    private ITypeIdentifierProvider _identifierProviderStub;

    private AssembledTypeIdentifierProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _participantWithoutIdentifierProvider = MockRepository.GenerateStub<IParticipant>();
      _participantWithIdentifierProvider = MockRepository.GenerateStub<IParticipant>();
      _identifierProviderStub = MockRepository.GenerateStub<ITypeIdentifierProvider>();
      _participantWithIdentifierProvider.Stub (_ => _.PartialTypeIdentifierProvider).Return (_identifierProviderStub);

      _provider = new AssembledTypeIdentifierProvider (new[] { _participantWithoutIdentifierProvider, _participantWithIdentifierProvider }.AsOneTime());
    }

    [Test]
    public void GetIdentifier ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      _identifierProviderStub.Stub (_ => _.GetID (requestedType)).Return ("abc");

      var result = _provider.GetIdentifier (requestedType);

      Assert.That (result, Is.EqualTo (new object[] { requestedType, "abc" }));
    }

    [Test]
    public void GetPartialIdentifier_ParticipantDoesNotContributeToIdentifier ()
    {
      var identifier = new object[0];

      var result = _provider.GetPartialIdentifier (identifier, _participantWithoutIdentifierProvider);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetPartialIdentifier_ParticipantContributesToIdentifier ()
    {
      var identifier = new object[] { "requestedType", "abc", "def" };

      var result = _provider.GetPartialIdentifier (identifier, _participantWithIdentifierProvider);

      Assert.That (result, Is.EqualTo ("abc"));
    }
  }
}