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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Development.UnitTesting.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Caching;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class AssembledTypeIdentifierProviderTest
  {
    private IParticipant _participantWithoutIdentifierProvider;
    private IParticipant _participantWithIdentifierProvider;
    private ITypeIdentifierProvider _identifierProviderMock;

    private AssembledTypeIdentifierProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _participantWithoutIdentifierProvider = MockRepository.GenerateStub<IParticipant>();
      _participantWithIdentifierProvider = MockRepository.GenerateStub<IParticipant>();
      _identifierProviderMock = MockRepository.GenerateMock<ITypeIdentifierProvider>();
      _participantWithIdentifierProvider.Stub (_ => _.PartialTypeIdentifierProvider).Return (_identifierProviderMock);

      _provider = new AssembledTypeIdentifierProvider (new[] { _participantWithoutIdentifierProvider, _participantWithIdentifierProvider }.AsOneTime());
    }

    [Test]
    public void ComputeTypeID ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      _identifierProviderMock.Stub (_ => _.GetID (requestedType)).Return ("abc");

      var result = _provider.ComputeTypeID (requestedType);

      var expectedTypeID = new AssembledTypeID (requestedType, new object[] { "abc" });
      Assert.That (result, Is.EqualTo (expectedTypeID));
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

    [Test]
    public void AddTypeID ()
    {
      var proxyType = MutableTypeObjectMother.Create();
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType, new object[] { "abc" });
      var idPartExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _identifierProviderMock.Stub (_ => _.GetExpression ("abc")).Return (idPartExpression);

      _provider.AddTypeID (proxyType, typeID);

      Assert.That (proxyType.AddedFields, Has.Count.EqualTo (1));
      var typeIDField = proxyType.AddedFields.Single();
      Assert.That (typeIDField.Name, Is.EqualTo ("__typeID"));
      Assert.That (typeIDField.Attributes, Is.EqualTo (FieldAttributes.Private | FieldAttributes.Static));
      Assert.That (typeIDField.FieldType, Is.SameAs (typeof (AssembledTypeID)));

      CheckTypeIDInitialization (proxyType, typeID.RequestedType, idPartExpression);
    }

    [Test]
    public void AddTypeID_ProviderNotCalledForNull ()
    {
      var proxyType = MutableTypeObjectMother.Create();
      var typeID = AssembledTypeIDObjectMother.Create (parts: new object[] { null });

      _provider.AddTypeID (proxyType, typeID);

      _identifierProviderMock.AssertWasNotCalled (mock => mock.GetExpression (Arg<object>.Is.Anything));
      var expectedIdPartExpression = Expression.Constant (null);
      CheckTypeIDInitialization (proxyType, typeID.RequestedType, expectedIdPartExpression);
    }

    [Test]
    public void AddTypeID_ProviderReturnsNull_IsSubstitutedForConstantNullExpression ()
    {
      var proxyType = MutableTypeObjectMother.Create();
      var typeID = AssembledTypeIDObjectMother.Create (parts: new object[] { "abc" });
      _identifierProviderMock.Stub (_ => _.GetExpression ("abc")).Return (null);

      _provider.AddTypeID (proxyType, typeID);

      var expectedIdPartExpression = Expression.Constant (null);
      CheckTypeIDInitialization (proxyType, typeID.RequestedType, expectedIdPartExpression);
    }
    
    [Test]
    public void ExtractTypeID ()
    {
      var result = _provider.ExtractTypeID (typeof (AssembledType));

      Assert.That (result.RequestedType, Is.SameAs (typeof (int)));
      Assert.That (result.Parts, Is.EqualTo (new object[] { 1, "2" }));
    }

    [Test]
    public void GetFlattenedExpressionForSerialization ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType, new object[] { "abc" });
      var idPartExpression = ExpressionTreeObjectMother.GetSomeExpression (typeof (IFlatValue));
      _identifierProviderMock.Stub (_ => _.GetFlatValueExpressionForSerialization ("abc")).Return (idPartExpression);

      var result = _provider.GetAssembledTypeIDDataExpression (typeID);

      CheckTypeIDDataExpression (result, requestedType, idPartExpression);
    }

    [Test]
    public void GetFlattenedExpressionForSerialization_ProviderNotCalledForNull ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType, new object[] { null });

      var result = _provider.GetAssembledTypeIDDataExpression (typeID);

      _identifierProviderMock.AssertWasNotCalled (mock => mock.GetFlatValueExpressionForSerialization (Arg<object>.Is.Anything));
      var expectedIdPartExpression = Expression.Constant (null, typeof (IFlatValue));
      CheckTypeIDDataExpression (result, requestedType, expectedIdPartExpression);
    }

    [Test]
    public void GetFlattenedExpressionForSerialization_ProviderReturnsNull_IsSubstitutedForConstantNullExpression ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType ();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType, new object[] { "abc" });
      _identifierProviderMock.Stub (_ => _.GetFlatValueExpressionForSerialization ("abc")).Return (null);

      var result = _provider.GetAssembledTypeIDDataExpression (typeID);

      var expectedIdPartExpression = Expression.Constant (null, typeof (IFlatValue));
      CheckTypeIDDataExpression (result, requestedType, expectedIdPartExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The expression returned from 'GetFlatValueExpressionForSerialization' must build an serializable instance of 'IFlatValue'.")]
    public void GetFlattenedExpressionForSerialization_ProviderReturnsNonFlatValue ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();
      var typeID = AssembledTypeIDObjectMother.Create (requestedType, new object[] { "abc" });
      var nonFlatValueExpression = ExpressionTreeObjectMother.GetSomeExpression();
      _identifierProviderMock.Stub (_ => _.GetFlatValueExpressionForSerialization ("abc")).Return (nonFlatValueExpression);

      _provider.GetAssembledTypeIDDataExpression (typeID);
    }

    private static void CheckTypeIDDataExpression (Expression result, Type requestedType, Expression idPartExpression)
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeIDData ("name", null));
      var expected = Expression.New (
          constructor, Expression.Constant (requestedType.AssemblyQualifiedName), Expression.NewArrayInit (typeof (IFlatValue), idPartExpression));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    private static void CheckTypeIDInitialization (MutableType proxyType, Type requestedType, Expression idPartExpression)
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AssembledTypeID (null, null));
      var typeIDField = proxyType.AddedFields.Single ();

      Assert.That (proxyType.MutableTypeInitializer, Is.Not.Null);
      var expected = Expression.Block (
          typeof (void),
          Expression.Assign (
              Expression.Field (null, typeIDField),
              Expression.New (constructor, Expression.Constant (requestedType), Expression.NewArrayInit (typeof (object), idPartExpression))));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, proxyType.MutableTypeInitializer.Body);
    }

    private class AssembledType
    {
      // ReSharper disable InconsistentNaming
      // ReSharper disable UnusedMember.Local
      private static AssembledTypeID __typeID = new AssembledTypeID (typeof (int), new object[] { 1, "2" });
      // ReSharper restore UnusedMember.Local
      // ReSharper restore InconsistentNaming
    }
  }
}