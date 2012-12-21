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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class MutableTypeAnalyzerTest
  {
    private ITypeAnalyzer _typeAnalyzerMock;
    private IExpressionAnalyzer _expressionAnalyzerMock;

    private MutableTypeAnalyzer _analyzer;

    private MutableType _mutableType;
    private Type _someType;
    private bool _randomBool;

    private IMethodOptions<bool> _underlyingTypeCheck;

    [SetUp]
    public void SetUp ()
    {
      _typeAnalyzerMock = MockRepository.GenerateStrictMock<ITypeAnalyzer>();
      _expressionAnalyzerMock = MockRepository.GenerateStrictMock<IExpressionAnalyzer>();

      _analyzer = new MutableTypeAnalyzer (_typeAnalyzerMock, _expressionAnalyzerMock);

      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));
      _someType = ReflectionObjectMother.GetSomeType();
      _randomBool = BooleanObjectMother.GetRandomBoolean();

      _underlyingTypeCheck = _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (_mutableType.UnderlyingSystemType)).Return (true);
    }

    [Test]
    public void IsStrongNameCompatible ()
    {
      Assert.That (_mutableType.ExistingInterfaces, Is.Not.Empty);
      Assert.That (_mutableType.ExistingMutableFields, Is.Not.Empty);
      Assert.That (_mutableType.ExistingMutableConstructors, Is.Not.Empty);
      Assert.That (_mutableType.ExistingMutableMethods, Is.Not.Empty);

      Assert.That (_analyzer.IsStrongNameCompatible (_mutableType), Is.True);
    }

    [Test]
    public void IsStrongNameCompatible_UnderlyingType ()
    {
      _underlyingTypeCheck.Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_TypeInitializations ()
    {
      var initialization = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableType.AddTypeInitialization (ctx => initialization);
      _expressionAnalyzerMock.Expect (mock => mock.IsStrongNameCompatible (initialization)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_InstanceInitializations ()
    {
      var initialization = ExpressionTreeObjectMother.GetSomeExpression();
      _mutableType.AddInstanceInitialization (ctx => initialization);
      _expressionAnalyzerMock.Expect (mock => mock.IsStrongNameCompatible (initialization)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Interfaces ()
    {
      _mutableType.AddInterface (typeof (IAddedInterface));
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (IAddedInterface))).Return (_randomBool);

      CheckIsStrongNameCompatible ();
    }

    [Test]
    public void IsStrongNameCompatible_Fields_Type ()
    {
      _mutableType.AddField ("field", _someType);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Constructors_Parameters ()
    {
      _mutableType.AddConstructor (0, new[] { new ParameterDeclaration (_someType, "p") }, ctx => Expression.Empty());
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);
      _expressionAnalyzerMock.Stub (stub => stub.IsStrongNameCompatible (Arg<Expression>.Is.Anything)).Return (true);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Constructors_Body ()
    {
      var ctor = _mutableType.AddConstructor (0, ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());
      _expressionAnalyzerMock.Expect (mock => mock.IsStrongNameCompatible (ctor.Body)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Methods_Parameters ()
    {
      _mutableType.AddMethod ("method", 0, typeof (void), new[] { new ParameterDeclaration (_someType, "p") }, ctx => Expression.Empty());
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);
      _expressionAnalyzerMock.Stub (stub => stub.IsStrongNameCompatible (Arg<Expression>.Is.Anything)).Return (true);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (void))).Return (true);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Methods_Body ()
    {
      var method = _mutableType.AddMethod ("method", 0, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());
      _expressionAnalyzerMock.Expect (mock => mock.IsStrongNameCompatible (method.Body)).Return (_randomBool);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (typeof (void))).Return (true);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Methods_ReturnParameter ()
    {
      _mutableType.AddMethod ("method", 0, _someType, ParameterDeclaration.EmptyParameters, ctx => Expression.Default (_someType));
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);
      _expressionAnalyzerMock.Stub (stub => stub.IsStrongNameCompatible (Arg<Expression>.Is.Anything)).Return (true);

      CheckIsStrongNameCompatible();
    }

    private void CheckIsStrongNameCompatible ()
    {
      var result = _analyzer.IsStrongNameCompatible (_mutableType);

      _typeAnalyzerMock.VerifyAllExpectations();
      _expressionAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (_randomBool));
    }

    class DomainType : IExistingInterface
    {
      public int ExistingField = 0;
      public DomainType (int i) { Dev.Null = i; }
      public void ExistingMethod() {}
    }

    interface IExistingInterface {}
    interface IAddedInterface { }
  }
}