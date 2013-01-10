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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class StrongNamingEmittableOperandProviderDecoratorTest
  {
    private ITypeAnalyzer _typeAnalyzerMock;
    private IEmittableOperandProvider _innerMock;

    private StrongNamingEmittableOperandProviderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _typeAnalyzerMock = MockRepository.GenerateStrictMock<ITypeAnalyzer>();
      _innerMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      using (new ServiceLocatorScope (typeof (ITypeAnalyzer), () => _typeAnalyzerMock))
        _decorator = new StrongNamingEmittableOperandProviderDecorator (_innerMock);
    }

    [Test]
    public void GetEmittableType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var emittableType = ReflectionObjectMother.GetSomeType();

      CheckGetEmittable ((p, t) => p.GetEmittableType (t), type, emittableType, emittableType);
    }

    [Test]
    public void GetEmittableField ()
    {
      var field = ReflectionObjectMother.GetSomeField();
      var emittableField = ReflectionObjectMother.GetSomeField();

      CheckGetEmittable ((p, f) => p.GetEmittableField (f), field, emittableField, emittableField.DeclaringType);
    }

    [Test]
    public void GetEmittableConstructor ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableConstructor = ReflectionObjectMother.GetSomeConstructor();

      CheckGetEmittable ((p, f) => p.GetEmittableConstructor (f), constructor, emittableConstructor, emittableConstructor.DeclaringType);
    }

    [Test]
    public void GetEmittableMethod ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionObjectMother.GetSomeNonGenericMethod();

      CheckGetEmittable ((p, f) => p.GetEmittableMethod (f), method, emittableMethod, emittableMethod.DeclaringType);
    }

    [Test]
    public void GetEmittableMethod_GenericArguments ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => GenericMethod<int>());
      _innerMock.Stub (stub => stub.GetEmittableMethod (method)).Return (emittableMethod);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (emittableMethod.DeclaringType)).Return (true);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (typeof (int))).Return (true);

      var result = _decorator.GetEmittableMethod (method);

      _typeAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (emittableMethod));
    }

    [Test]
    public void GetEmittableMethod_MethodBuilder ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionEmitObjectMother.CreateMethodBuilder();
      Assert.That (emittableMethod.GetGenericArguments(), Is.Null);
      _innerMock.Stub (stub => stub.GetEmittableMethod (method)).Return (emittableMethod);
      _typeAnalyzerMock.Stub (stub => stub.IsStrongNamed (emittableMethod.DeclaringType)).Return (true);

      Assert.That (() => _decorator.GetEmittableMethod (method), Throws.Nothing);
    }

    [Test]
    public void GetEmittableOperand ()
    {
      var operand = new object();
      var emittableOperand = new object();
      var type = ReflectionObjectMother.GetSomeType();
      var emittableType = ReflectionObjectMother.GetSomeType();
      var field = ReflectionObjectMother.GetSomeField();
      var emittableField = ReflectionObjectMother.GetSomeField();
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableConstructor = ReflectionObjectMother.GetSomeConstructor();
      var method = ReflectionObjectMother.GetSomeMethod();
      var emittableMethod = ReflectionObjectMother.GetSomeNonGenericMethod();

      _innerMock.Expect (mock => mock.GetEmittableOperand (operand)).Return (emittableOperand);
      _innerMock.Expect (mock => mock.GetEmittableType (type)).Return (emittableType);
      _innerMock.Expect (mock => mock.GetEmittableField (field)).Return (emittableField);
      _innerMock.Expect (mock => mock.GetEmittableConstructor (constructor)).Return (emittableConstructor);
      _innerMock.Expect (mock => mock.GetEmittableMethod (method)).Return (emittableMethod);

      _typeAnalyzerMock.Stub (mock => mock.IsStrongNamed (emittableType)).Return (true);
      _typeAnalyzerMock.Stub (mock => mock.IsStrongNamed (emittableField.DeclaringType)).Return (true);
      _typeAnalyzerMock.Stub (mock => mock.IsStrongNamed (emittableConstructor.DeclaringType)).Return (true);
      _typeAnalyzerMock.Stub (mock => mock.IsStrongNamed (emittableMethod.DeclaringType)).Return (true);

      Assert.That (_decorator.GetEmittableOperand (operand), Is.SameAs (emittableOperand));
      Assert.That (_decorator.GetEmittableOperand (type), Is.SameAs (emittableType));
      Assert.That (_decorator.GetEmittableOperand (field), Is.SameAs (emittableField));
      Assert.That (_decorator.GetEmittableOperand (constructor), Is.SameAs (emittableConstructor));
      Assert.That (_decorator.GetEmittableOperand (method), Is.SameAs (emittableMethod));

      _typeAnalyzerMock.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void DelegatingMembers ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var mutableField = MutableFieldInfoObjectMother.Create();
      var mutableConstructor = MutableConstructorInfoObjectMother.Create();
      var mutableMethod = MutableMethodInfoObjectMother.Create();

      var helper = new DecoratorTestHelper<IEmittableOperandProvider> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.AddMapping (mutableType, mutableType.UnderlyingSystemType));
      helper.CheckDelegation (d => d.AddMapping (mutableField, mutableField.UnderlyingSystemFieldInfo));
      helper.CheckDelegation (d => d.AddMapping (mutableConstructor, mutableConstructor.UnderlyingSystemConstructorInfo));
      helper.CheckDelegation (d => d.AddMapping (mutableMethod, mutableMethod.UnderlyingSystemMethodInfo));
    }

    private void CheckGetEmittable<T> (Func<IEmittableOperandProvider, T, T> getEmittableOperandFunc, T operand, T emittableOperand, Type checkedType)
    {
      _innerMock.Expect (mock => getEmittableOperandFunc (mock, operand)).Return (emittableOperand);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (checkedType)).Return (true);

      var result = getEmittableOperandFunc (_decorator, operand);

      _innerMock.VerifyAllExpectations();
      _typeAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (emittableOperand));

      _innerMock.BackToRecord();
      _typeAnalyzerMock.BackToRecord();
      _innerMock.Expect (mock => getEmittableOperandFunc (mock, operand)).Return (emittableOperand);
      _typeAnalyzerMock.Expect (mock => mock.IsStrongNamed (checkedType)).Return (false);
      _innerMock.Replay();
      _typeAnalyzerMock.Replay();

      var message = "Strong-naming is enabled but a participant used the type '" + checkedType.FullName + "' which comes from the unsigned assembly '"
                    + checkedType.Assembly.GetName().Name + "'.";
      Assert.That (() => getEmittableOperandFunc (_decorator, operand), Throws.InvalidOperationException.With.Message.EqualTo (message));
      _innerMock.VerifyAllExpectations();
      _typeAnalyzerMock.VerifyAllExpectations();
    }

    void GenericMethod<[UsedImplicitly] T> () { }
  }
}