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
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Expressions;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Dlr.Ast;
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

      var result = _provider.GetTypeID (requestedType);

      var expectedTypeID = new AssembledTypeID (requestedType, new object[] { "abc" });
      Assert.That (result, Is.EqualTo (expectedTypeID));
    }

    [Test]
    public void GetPartialIdentifier_ParticipantDoesNotContributeToIdentifier ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = new AssembledTypeID (requestedType, new object[] { "abc" });
      var idPart = ExpressionTreeObjectMother.GetSomeExpression();
      _identifierProviderStub.Stub (_ => _.GetExpressionForID ("abc")).Return (idPart);

      var result = _provider.GetTypeIDExpression (typeID);

      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeID (null, null));
      var expected = Expression.New (
          ctor,
          Expression.Constant (requestedType),
          Expression.NewArrayInit (typeof (object), new[] { idPart }));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }
  }
}