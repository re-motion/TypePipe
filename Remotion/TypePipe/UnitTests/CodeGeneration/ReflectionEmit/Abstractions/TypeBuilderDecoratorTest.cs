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
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class TypeBuilderDecoratorTest
  {
    private ITypeBuilder _innerMock;
    private IEmittableOperandProvider _operandProvider;

    private TypeBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      _operandProvider = MockRepository.GenerateStrictMock<IEmittableOperandProvider> ();

      _decorator = new TypeBuilderDecorator (_innerMock, _operandProvider);
    }

    [Test]
    public void AddInterfaceImplementation ()
    {
      var interfaceType = ReflectionObjectMother.GetSomeInterfaceType();

      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      _operandProvider.Expect (mock => mock.GetEmittableType (interfaceType)).Return (emittableType);
      _innerMock.Expect (mock => mock.AddInterfaceImplementation (emittableType));

      _decorator.AddInterfaceImplementation (interfaceType);

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void DefineField ()
    {
      var name = "field";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;

      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      var fakeFieldBuilder = MockRepository.GenerateStub<IFieldBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (type)).Return (emittableType);
      _innerMock.Expect (mock => mock.DefineField (name, emittableType, attributes)).Return (fakeFieldBuilder);

      var result = _decorator.DefineField (name, type, attributes);

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<FieldBuilderDecorator>());
      Assert.That (result.As<FieldBuilderDecorator>().DecoratedFieldBuilder, Is.SameAs (fakeFieldBuilder));
    }

    [Test]
    public void DefineConstructor ()
    {
      var parameterType = ReflectionObjectMother.GetSomeType();
      var attributes = (MethodAttributes) 7;
      var callingConvention = (CallingConventions) 7;

      var emittableParameterType = ReflectionObjectMother.GetSomeOtherType();
      var fakeConstructorBuilder = MockRepository.GenerateStub<IConstructorBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (parameterType)).Return (emittableParameterType);
      _innerMock.Expect (mock => mock.DefineConstructor (attributes, callingConvention, new[] { emittableParameterType })).Return (fakeConstructorBuilder);

      var result = _decorator.DefineConstructor (attributes, callingConvention, new[] { parameterType });

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<ConstructorBuilderDecorator>());
      Assert.That (result.As<ConstructorBuilderDecorator>().DecoratedConstructorBuilder, Is.SameAs (fakeConstructorBuilder));
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
      _innerMock.Expect (mock => mock.DefineMethod (name, attributes, emittableReturnType, new[] { emittableParameterType })).Return (fakeMethodBuilder);

      var result = _decorator.DefineMethod (name, attributes, returnType, new[] { parameterType });

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<MethodBuilderDecorator>());
      Assert.That (result.As<MethodBuilderDecorator>().DecoratedMethodBuilder, Is.SameAs (fakeMethodBuilder));
    }

    [Test]
    public void DefineMethodOverride ()
    {
      var methods = ReflectionObjectMother.GetMultipeMethods (2);

      var emittableMethods = new[] { typeof (int).GetMethod ("Parse", Type.EmptyTypes), typeof (bool).GetMethod ("Parse") };
      _operandProvider.Expect (mock => mock.GetEmittableMethod (methods[0])).Return (emittableMethods[0]);
      _operandProvider.Expect (mock => mock.GetEmittableMethod (methods[1])).Return (emittableMethods[1]);
      _innerMock.Expect (mock => mock.DefineMethodOverride (emittableMethods[0], emittableMethods[1]));

      _decorator.DefineMethodOverride (methods[0], methods[1]);

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
    }

    [Test]
    public void DefineProperty ()
    {
      var name = "property";
      var attributes = (PropertyAttributes) 7;
      var callingConventions = (CallingConventions) 8;
      var returnType = typeof (int);
      var parameterType = typeof (string);

      var emittableReturnType = typeof (bool);
      var emittableParameterType = typeof (double);
      var fakePropertyBuilder = MockRepository.GenerateStub<IPropertyBuilder>();
      _operandProvider.Expect (mock => mock.GetEmittableType (returnType)).Return (emittableReturnType);
      _operandProvider.Expect (mock => mock.GetEmittableType (parameterType)).Return (emittableParameterType);
      _innerMock.Expect (mock => mock.DefineProperty (name, attributes, callingConventions, emittableReturnType, new[] { emittableParameterType }))
                .Return (fakePropertyBuilder);

      var result = _decorator.DefineProperty (name, attributes, callingConventions, returnType, new[] { parameterType });

      _operandProvider.VerifyAllExpectations();
      _innerMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf<PropertyBuilderDecorator>());
      Assert.That (result.As<PropertyBuilderDecorator>().DecoratedPropertyBuilder, Is.SameAs (fakePropertyBuilder));
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = MockRepository.GenerateStub<IEmittableOperandProvider>();
      var proxyType = ProxyTypeObjectMother.Create();

      var helper = new DecoratorTestHelper<ITypeBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider, proxyType));
      helper.CheckDelegation (d => d.CreateType(), ReflectionObjectMother.GetSomeType());
    }
  }
}