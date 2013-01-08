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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class TypeBuilderDecoratorTest
  {
    private ITypeBuilder _inner;
    private IEmittableOperandProvider _operandProvider;

    private TypeBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _inner = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider> ();

      _decorator = new TypeBuilderDecorator (_inner, _operandProvider);
    }

    [Test]
    public void AddInterfaceImplementation ()
    {
      var interfaceType = ReflectionObjectMother.GetSomeInterfaceType();

      var emittableType = ReflectionObjectMother.GetSomeDifferentType();
      _operandProvider.Expect (mock => mock.GetEmittableType (interfaceType)).Return (emittableType);
      _inner.Expect (mock => mock.AddInterfaceImplementation (emittableType));

      _decorator.AddInterfaceImplementation (interfaceType);

      _operandProvider.VerifyAllExpectations();
      _inner.VerifyAllExpectations();
    }

    [Test]
    public void DefineField ()
    {
      var name = "field";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;

      var emittableType = ReflectionObjectMother.GetSomeDifferentType();
      var fakeFieldBuilder = MockRepository.GenerateStub<IFieldBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (type)).Return (emittableType);
      _inner.Expect (mock => mock.DefineField (name, emittableType, attributes)).Return (fakeFieldBuilder);

      var result = _decorator.DefineField (name, type, attributes);

      _operandProvider.VerifyAllExpectations();
      _inner.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<FieldBuilderDecorator>());
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_fieldBuilder"), Is.SameAs (fakeFieldBuilder));
    }

    [Test]
    public void DefineConstructor ()
    {
      var parameterType = ReflectionObjectMother.GetSomeType();
      var attributes = (MethodAttributes) 7;
      var callingConvention = (CallingConventions) 7;

      var emittableParameterType = ReflectionObjectMother.GetSomeDifferentType();
      var fakeConstructorBuilder = MockRepository.GenerateStub<IConstructorBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (parameterType)).Return (emittableParameterType);
      _inner.Expect (mock => mock.DefineConstructor (attributes, callingConvention, new[] { emittableParameterType })).Return (fakeConstructorBuilder);

      var result = _decorator.DefineConstructor (attributes, callingConvention, new[] { parameterType });

      _operandProvider.VerifyAllExpectations();
      _inner.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<ConstructorBuilderDecorator>());
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_constructorBuilder"), Is.SameAs (fakeConstructorBuilder));
    }

    [Test]
    public void DefineMethod ()
    {
      var name = "method";
      var returnType = typeof (int);
      var parameterType = typeof (string);
      var attributes = (MethodAttributes) 7;

      var emittableReturnType = typeof (bool);
      var emittableParameterType = typeof (double);
      var fakeMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (returnType)).Return (emittableReturnType);
      _operandProvider.Expect (mock => mock.GetEmittableType (parameterType)).Return (emittableParameterType);
      _inner.Expect (mock => mock.DefineMethod (name, attributes, emittableReturnType, new[] { emittableParameterType })).Return (fakeMethodBuilder);

      var result = _decorator.DefineMethod (name, attributes, returnType, new[] { parameterType });

      _operandProvider.VerifyAllExpectations ();
      _inner.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf<MethodBuilderDecorator> ());
      Assert.That (PrivateInvoke.GetNonPublicField (result, "_methodBuilder"), Is.SameAs (fakeMethodBuilder));
    }
  }
}