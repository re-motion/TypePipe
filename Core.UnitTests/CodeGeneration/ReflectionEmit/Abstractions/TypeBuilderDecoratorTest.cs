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
using Remotion.Development.Moq.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class TypeBuilderDecoratorTest
  {
    private Mock<ITypeBuilder> _innerMock;
    private Mock<IEmittableOperandProvider> _operandProvider;

    private TypeBuilderDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerMock = new Mock<ITypeBuilder> (MockBehavior.Strict);
      _operandProvider = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _decorator = new TypeBuilderDecorator (_innerMock.Object, _operandProvider.Object);
    }

    [Test]
    public void SetParent ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();

      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      _operandProvider.Setup (mock => mock.GetEmittableType (baseType)).Returns (emittableType).Verifiable();
      _innerMock.Setup (mock => mock.SetParent (emittableType)).Verifiable();

      _decorator.SetParent (baseType);

      _operandProvider.Verify();
      _innerMock.Verify();
    }

    [Test]
    public void AddInterfaceImplementation ()
    {
      var interfaceType = ReflectionObjectMother.GetSomeInterfaceType();

      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      _operandProvider.Setup (mock => mock.GetEmittableType (interfaceType)).Returns (emittableType).Verifiable();
      _innerMock.Setup (mock => mock.AddInterfaceImplementation (emittableType)).Verifiable();

      _decorator.AddInterfaceImplementation (interfaceType);

      _operandProvider.Verify();
      _innerMock.Verify();
    }

    [Test]
    public void DefineNestedType ()
    {
      var name = "type";
      var attributes = (TypeAttributes) 7;

      var fakeTypeBuilder = new Mock<ITypeBuilder>().Object;
      _innerMock.Setup (mock => mock.DefineNestedType (name, attributes)).Returns (fakeTypeBuilder).Verifiable();

      var result = _decorator.DefineNestedType (name, attributes);

      _operandProvider.Verify();
      _innerMock.Verify();
      Assert.That (result, Is.TypeOf<TypeBuilderDecorator>());
      Assert.That (result.As<TypeBuilderDecorator>().DecoratedTypeBuilder, Is.SameAs (fakeTypeBuilder));
    }

    [Test]
    public void DefineField ()
    {
      var name = "field";
      var type = ReflectionObjectMother.GetSomeType();
      var attributes = (FieldAttributes) 7;

      var emittableType = ReflectionObjectMother.GetSomeOtherType();
      var fakeFieldBuilder = new Mock<IFieldBuilder>().Object;
      _operandProvider.Setup (mock => mock.GetEmittableType (type)).Returns (emittableType).Verifiable();
      _innerMock.Setup (mock => mock.DefineField (name, emittableType, attributes)).Returns (fakeFieldBuilder).Verifiable();

      var result = _decorator.DefineField (name, type, attributes);

      _operandProvider.Verify();
      _innerMock.Verify();
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
      var fakeConstructorBuilder = new Mock<IConstructorBuilder>().Object;
      _operandProvider.Setup (mock => mock.GetEmittableType (parameterType)).Returns (emittableParameterType).Verifiable();
      _innerMock.Setup (mock => mock.DefineConstructor (attributes, callingConvention, new[] { emittableParameterType })).Returns (fakeConstructorBuilder).Verifiable();

      var result = _decorator.DefineConstructor (attributes, callingConvention, new[] { parameterType });

      _operandProvider.Verify();
      _innerMock.Verify();
      Assert.That (result, Is.TypeOf<ConstructorBuilderDecorator>());
      Assert.That (result.As<ConstructorBuilderDecorator>().DecoratedConstructorBuilder, Is.SameAs (fakeConstructorBuilder));
    }

    [Test]
    public void DefineMethod ()
    {
      var name = "method";
      var attributes = (MethodAttributes) 7;

      var fakeMethodBuilder = new Mock<IMethodBuilder>().Object;
      _innerMock.Setup (mock => mock.DefineMethod (name, attributes)).Returns (fakeMethodBuilder).Verifiable();

      var result = _decorator.DefineMethod (name, attributes);

      _innerMock.Verify();
      Assert.That (result, Is.TypeOf<MethodBuilderDecorator>());
      Assert.That (result.As<MethodBuilderDecorator>().DecoratedMethodBuilder, Is.SameAs (fakeMethodBuilder));
    }

    [Test]
    public void DefineMethodOverride ()
    {
      var methods = ReflectionObjectMother.GetMultipeMethods (2);
      var type1ParseMethodInfo = typeof (int).GetMethod ("Parse", Type.EmptyTypes);
      var type2ParseMethodInfo = typeof (bool).GetMethod ("Parse", new[] { typeof (string) });

      var emittableMethods = new[] { type1ParseMethodInfo, type2ParseMethodInfo };
      _operandProvider.Setup (mock => mock.GetEmittableMethod (methods[0])).Returns (emittableMethods[0]).Verifiable();
      _operandProvider.Setup (mock => mock.GetEmittableMethod (methods[1])).Returns (emittableMethods[1]).Verifiable();
      _innerMock.Setup (mock => mock.DefineMethodOverride (emittableMethods[0], emittableMethods[1])).Verifiable();

      _decorator.DefineMethodOverride (methods[0], methods[1]);

      _operandProvider.Verify();
      _innerMock.Verify();
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
      var fakePropertyBuilder = new Mock<IPropertyBuilder>().Object;
      _operandProvider.Setup (mock => mock.GetEmittableType (returnType)).Returns (emittableReturnType).Verifiable();
      _operandProvider.Setup (mock => mock.GetEmittableType (parameterType)).Returns (emittableParameterType).Verifiable();
      _innerMock
          .Setup (mock => mock.DefineProperty (name, attributes, callingConventions, emittableReturnType, new[] { emittableParameterType }))
          .Returns (fakePropertyBuilder)
          .Verifiable();

      var result = _decorator.DefineProperty (name, attributes, callingConventions, returnType, new[] { parameterType });

      _operandProvider.Verify();
      _innerMock.Verify();
      Assert.That (result, Is.TypeOf<PropertyBuilderDecorator>());
      Assert.That (result.As<PropertyBuilderDecorator>().DecoratedPropertyBuilder, Is.SameAs (fakePropertyBuilder));
    }

    [Test]
    public void DefineEvent ()
    {
      var name = "event";
      var attributes = (EventAttributes) 7;
      var eventType = typeof (Action<string, int>);

      var emittableEventType = ReflectionObjectMother.GetSomeType();
      var fakeEventBuilder = new Mock<IEventBuilder>().Object;
      _operandProvider.Setup (mock => mock.GetEmittableType (eventType)).Returns (emittableEventType).Verifiable();
      _innerMock.Setup (mock => mock.DefineEvent (name, attributes, emittableEventType)).Returns (fakeEventBuilder).Verifiable();

      var result = _decorator.DefineEvent (name, attributes, eventType);

      _operandProvider.Verify();
      _innerMock.Verify();
      Assert.That (result, Is.TypeOf<EventBuilderDecorator>());
      Assert.That (result.As<EventBuilderDecorator>().DecoratedEventBuilder, Is.SameAs (fakeEventBuilder));
    }

    [Test]
    public void DelegatingMembers ()
    {
      var emittableOperandProvider = new Mock<IEmittableOperandProvider>();
      var proxyType = MutableTypeObjectMother.Create();

      var helper = new DecoratorTestHelper<ITypeBuilder> (_decorator, _innerMock);

      helper.CheckDelegation (d => d.RegisterWith (emittableOperandProvider.Object, proxyType));
      helper.CheckDelegation (d => d.CreateType(), ReflectionObjectMother.GetSomeType());
    }
  }
}