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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class StrongNameAnalyzerTest
  {
    private IStrongNameTypeVerifier _strongNameTypeVerifierMock;
    private IStrongNameExpressionVerifier _strongNameExpressionVerifierMock;

    private StrongNameAnalyzer _analyzer;

    private MutableType _mutableType;
    private IMethodOptions<bool> _underlyingTypeCheck;
    private Type _someType;
    private bool _randomBool;

    [SetUp]
    public void SetUp ()
    {
      _strongNameTypeVerifierMock = MockRepository.GenerateStrictMock<IStrongNameTypeVerifier>();
      _strongNameExpressionVerifierMock = MockRepository.GenerateStrictMock<IStrongNameExpressionVerifier>();

      _analyzer = new StrongNameAnalyzer (_strongNameTypeVerifierMock, _strongNameExpressionVerifierMock);

      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));
      _underlyingTypeCheck = _strongNameTypeVerifierMock.Expect (mock => mock.IsStrongNamed (_mutableType.UnderlyingSystemType)).Return (true);
      _someType = ReflectionObjectMother.GetSomeType();
      _randomBool = BooleanObjectMother.GetRandomBoolean();
    }

    [Test]
    public void IsStrongNameCompatible ()
    {
      Assert.That (_analyzer.IsStrongNameCompatible (_mutableType), Is.True);
    }

    [Test]
    public void IsStrongNameCompatible_UnderlyingType ()
    {
      _underlyingTypeCheck.Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Interfaces ()
    {
      Assert.That (_mutableType.ExistingInterfaces, Is.Not.Empty);
      _mutableType.AddInterface (typeof (IAddedInterface));
      _strongNameTypeVerifierMock.Expect (mock => mock.IsStrongNamed (typeof (IAddedInterface))).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Fields_Type ()
    {
      Assert.That (_mutableType.ExistingMutableFields, Is.Not.Empty);
      _mutableType.AddField ("field", _someType);
      _strongNameTypeVerifierMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Constructors_Parameters ()
    {
      Assert.That (_mutableType.ExistingMutableConstructors, Is.Not.Empty);
      _mutableType.AddConstructor (0, new[] { new ParameterDeclaration (_someType, "p") }, ctx => Expression.Empty());
      _strongNameTypeVerifierMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Methods_ReturnParameter ()
    {
      Assert.That (_mutableType.ExistingMutableMethods, Is.Not.Empty);
      _mutableType.AddMethod ("method", 0, _someType, ParameterDeclaration.EmptyParameters, ctx => Expression.Default (_someType));
      _strongNameTypeVerifierMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);

      CheckIsStrongNameCompatible();
    }

    [Test]
    public void IsStrongNameCompatible_Methods_Parameters ()
    {
      Assert.That (_mutableType.ExistingMutableMethods, Is.Not.Empty);
      _mutableType.AddMethod ("method", 0, typeof (void), new[] { new ParameterDeclaration (_someType, "p") }, ctx => Expression.Empty());
      _strongNameTypeVerifierMock.Expect (mock => mock.IsStrongNamed (_someType)).Return (_randomBool);
      _strongNameTypeVerifierMock.Stub (stub => stub.IsStrongNamed (typeof (void))).Return (true);

      CheckIsStrongNameCompatible();
    }

    //[Test]
    //public void MethodBody ()
    //{
    //  var expression = Expression.Block (typeof (void), ExpressionTreeObjectMother.GetSomeExpression());
    //  _mutableType.AddMethod ("Method", MethodAttributes.Public, typeof (void), ParameterDeclaration.EmptyParameters, ctx => expression);

    //  Check (_mutableType, signable: true, expression: expression);
    //  Check (_mutableType, signable: false, expression: expression);
    //}

    //[Test]
    //public void ConstructorBody ()
    //{
    //  var expression = Expression.Block (typeof (void), ExpressionTreeObjectMother.GetSomeExpression());
    //  _mutableType.AddConstructor (MethodAttributes.Public, new[] { new ParameterDeclaration (typeof (int), "param") }, ctx => expression);

    //  Check (_mutableType, signable: true, expression: expression);
    //  Check (_mutableType, signable: false, expression: expression);
    //}

    private void CheckIsStrongNameCompatible ()
    {
      var result = _analyzer.IsStrongNameCompatible (_mutableType);

      _strongNameTypeVerifierMock.VerifyAllExpectations();
      _strongNameExpressionVerifierMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (_randomBool));
    }

    class DomainType : IExistingInterface
    {
      public int ExistingField = 0;

      public void ExistingMethod() {}
    }

    interface IExistingInterface {}
    interface IAddedInterface { }
  }
}