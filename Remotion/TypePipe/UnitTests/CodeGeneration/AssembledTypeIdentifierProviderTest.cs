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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Caching;
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
    public void GetTypeID ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      _identifierProviderStub.Stub (_ => _.GetID (requestedType)).Return ("abc");

      var result = _provider.GetTypeID (requestedType);

      var expectedTypeID = new AssembledTypeID (requestedType, new object[] { "abc" });
      Assert.That (result, Is.EqualTo (expectedTypeID));
    }

    [Test]
    public void GetExpression ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType, new object[] { "abc" });
      var idPart = ExpressionTreeObjectMother.GetSomeExpression();
      _identifierProviderStub.Stub (_ => _.GetExpression ("abc")).Return (idPart);

      var result = _provider.GetExpression (typeID);

      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeID (null, null));
      var expected = Expression.New (
          ctor,
          Expression.Constant (requestedType),
          Expression.NewArrayInit (typeof (object), new[] { idPart }));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void GetExpression_TypeIdentifierProviderReturnsNull ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (parts: new object[] { "abc" });
      _identifierProviderStub.Stub (_ => _.GetExpression ("abc")).Return (null);

      var result = _provider.GetExpression (typeID);

      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeID (null, null));
      var expected = Expression.New (
          ctor,
          Expression.Constant (typeID.RequestedType),
          Expression.NewArrayInit (typeof (object), new Expression[] { Expression.Constant (null) }));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void GetPart_ParticipantDidNotContributeToTypeID ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (parts: new object[0]);

      var result = _provider.GetPart (typeID, _participantWithoutIdentifierProvider);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetPart_ParticipantContributedToTypeID ()
    {
      var typeID = AssembledTypeIDObjectMother.Create (parts: new object[] { "abc", "def" });

      var result = _provider.GetPart (typeID, _participantWithIdentifierProvider);

      Assert.That (result, Is.EqualTo ("abc"));
    }
  }
}