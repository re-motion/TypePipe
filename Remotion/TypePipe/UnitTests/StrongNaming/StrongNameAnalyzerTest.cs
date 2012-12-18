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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class StrongNameAnalyzerTest
  {
    private MutableType _mutableType;
    private Type _type;

    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));
      _type = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void Cache ()
    {
      var expressionVerifierMock = MockRepository.GenerateStrictMock<IStrongNamedExpressionVerifier> ();
      var typeVerifierMock = MockRepository.GenerateStrictMock<IStrongNamedTypeVerifier>();
      var analyzer = new StrongNameAnalyzer (typeVerifierMock, expressionVerifierMock);

      typeVerifierMock.Expect (mock => mock.IsStrongNamed (_mutableType.UnderlyingSystemType)).Return (true).Repeat.Once();

      analyzer.IsSignable (_mutableType);
      analyzer.IsSignable (_mutableType);
    }

    [Test]
    public void UnderlyingType ()
    {
      Check (_mutableType, signable: true, type: _mutableType.UnderlyingSystemType);
      Check (_mutableType, signable: false, type: _mutableType.UnderlyingSystemType);
    }

    [Test]
    public void Interface ()
    {
      var interfaceType = typeof (IDomainInterface);
      _mutableType.AddInterface (interfaceType);

      Check (_mutableType, signable: true, type: interfaceType);
      Check (_mutableType, signable: false, type: interfaceType);
    }

    [Test]
    public void GenericInterface ()
    {
      var interfaceType = typeof (IGenericInterface<>).MakeGenericType (_type);
      _mutableType.AddInterface (interfaceType);

      Check (_mutableType, signable: true, type: _type);
      Check (_mutableType, signable: false, type: _type);
    }

    [Test]
    public void Fields ()
    {
      _mutableType.AddField ("Field", _type, FieldAttributes.Public);

      Check (_mutableType, signable: true, type: _type);
      Check (_mutableType, signable: false, type: _type);
    }

    [Test]
    public void MethodReturnType ()
    {
      _mutableType.AddMethod ("Method", MethodAttributes.Public, _type, ParameterDeclaration.EmptyParameters, ctx => Expression.Default(_type));

      Check (_mutableType, signable: true, type: _type);
      Check (_mutableType, signable: false, type: _type);
    }

    [Test]
    public void MethodParameter ()
    {
      var parameters = new[] { new ParameterDeclaration (_type, "param") };
      _mutableType.AddMethod ("Method", MethodAttributes.Public, typeof (void), parameters, ctx => Expression.Empty());

      Check (_mutableType, signable: true, type: _type);
      Check (_mutableType, signable: false, type: _type);
    }

    [Test]
    public void MethodBody ()
    {
      var expression = Expression.Block (typeof (void), ExpressionTreeObjectMother.GetSomeExpression());
      _mutableType.AddMethod ("Method", MethodAttributes.Public, typeof (void), ParameterDeclaration.EmptyParameters, ctx => expression);

      Check (_mutableType, signable: true, expression: expression);
      Check (_mutableType, signable: false, expression: expression);
    }

    [Test]
    public void ConstructorBody ()
    {
      var expression = Expression.Block (typeof (void), ExpressionTreeObjectMother.GetSomeExpression());
      _mutableType.AddConstructor (MethodAttributes.Public, new[] { new ParameterDeclaration (typeof (int), "param") }, ctx => expression);

      Check (_mutableType, signable: true, expression: expression);
      Check (_mutableType, signable: false, expression: expression);
    }

    private void Check (MutableType mutableType, bool signable, Type type = null, Expression expression = null)
    {
      var expressionVerifierMock = MockRepository.GenerateStrictMock<IStrongNamedExpressionVerifier> ();
      var typeVerifierMock = MockRepository.GenerateStrictMock<IStrongNamedTypeVerifier> ();
      var analyzer = new StrongNameAnalyzer (typeVerifierMock, expressionVerifierMock);

      if (type != null)
        typeVerifierMock.Expect (mock => mock.IsStrongNamed (type)).Return (signable);
      if (expression != null)
        expressionVerifierMock.Expect (mock => mock.IsStrongNamed (expression)).Return (signable);
      typeVerifierMock.Stub (stub => stub.IsStrongNamed (null)).IgnoreArguments ().Return (true);
      expressionVerifierMock.Stub (stub => stub.IsStrongNamed (null)).IgnoreArguments ().Return (true);

      var result = analyzer.IsSignable (mutableType);
      typeVerifierMock.VerifyAllExpectations ();
      expressionVerifierMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (signable));
    }

    public class DomainType {}

    public interface IDomainInterface {}

    public interface IGenericInterface<T> {}
  }
}