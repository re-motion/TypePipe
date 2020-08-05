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
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratorDecoratorTest
  {
    private Mock<IILGenerator> _innerILGeneratorMock;
    private Mock<IEmittableOperandProvider> _emittableOperandProviderStub;

    private ILGeneratorDecorator _decorator;
    
    [SetUp]
    public void SetUp ()
    {
      _innerILGeneratorMock = new Mock<IILGenerator> (MockBehavior.Strict);
      _emittableOperandProviderStub = new Mock<IEmittableOperandProvider>();

      _decorator = new ILGeneratorDecorator (_innerILGeneratorMock.Object, _emittableOperandProviderStub.Object);
    }

    [Test]
    public void GetFactory ()
    {
      var fakeFactory = new Mock<IILGeneratorFactory>().Object;
      _innerILGeneratorMock.Setup (stub => stub.GetFactory()).Returns (fakeFactory);

      var ilGeneratorFactory = _decorator.GetFactory();

      Assert.That (ilGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());
      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) ilGeneratorFactory;
      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.SameAs (fakeFactory));
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (_emittableOperandProviderStub.Object));
    }

    [Test]
    public void BeginCatchBlock ()
    {
      var exceptionType = ReflectionObjectMother.GetSomeType();
      var fakeEmittableOperand = ReflectionObjectMother.GetSomeOtherType();
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableType (exceptionType)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.BeginCatchBlock (fakeEmittableOperand)).Verifiable();

      _decorator.BeginCatchBlock (exceptionType);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void DeclareLocal ()
    {
      var localVariableType = ReflectionObjectMother.GetSomeType();
      var fakeEmittableOperand = new Mock<Type>().Object;
      var fakeLocalBuilder = ReflectionEmitObjectMother.GetSomeLocalBuilder();
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableType (localVariableType)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.DeclareLocal (fakeEmittableOperand)).Returns (fakeLocalBuilder).Verifiable();

      var result = _decorator.DeclareLocal (localVariableType);

      _innerILGeneratorMock.Verify();
      Assert.That (result, Is.SameAs (fakeLocalBuilder));
    }

    [Test]
    public void Emit_ConstructorInfo ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var fakeEmittableOperand = new Mock<ConstructorInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableConstructor (constructor)).Returns (fakeEmittableOperand);
      
      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();

      _decorator.Emit (OpCodes.Call, constructor);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void Emit_Type ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeEmittableOperand = new Mock<Type>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableType (type)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldtoken, fakeEmittableOperand)).Verifiable();

      _decorator.Emit (OpCodes.Ldtoken, type);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void Emit_FieldInfo ()
    {
      var field = ReflectionObjectMother.GetSomeField();
      var fakeEmittableOperand = new Mock<FieldInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableField (field)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Ldfld, fakeEmittableOperand)).Verifiable();

      _decorator.Emit (OpCodes.Ldfld, field);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void Emit_MethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();
      
      _decorator.Emit (OpCodes.Call, method);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void Emit_MethodInfo_BaseConstructorMethodInfo ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor ();
      var fakeEmittableOperand = new Mock<ConstructorInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableConstructor (constructor)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();

      var constructorAsMethodInfoAdapter = new ConstructorAsMethodInfoAdapter (constructor);
      _decorator.Emit (OpCodes.Call, constructorAsMethodInfoAdapter);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void Emit_MethodInfo_BaseCallMethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();
      
      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.Emit (OpCodes.Call, baseAdaptedMethod);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void Emit_MethodInfo_BaseCallMethodInfo_TurnsCallvirt_IntoCall ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.Emit (OpCodes.Callvirt, baseAdaptedMethod);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void EmitCall_MethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType() };
      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, optionalParameterTypes)).Verifiable();

      _decorator.EmitCall (OpCodes.Call, method, optionalParameterTypes);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_NullOptionalParameters ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor ();
      var fakeEmittableOperand = new Mock<ConstructorInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableConstructor (constructor)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();

      var constructorAsMethodInfoAdapter = new ConstructorAsMethodInfoAdapter (constructor);
      _decorator.EmitCall (OpCodes.Call, constructorAsMethodInfoAdapter, null);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_EmptyOptionalParameters ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor ();
      var fakeEmittableOperand = new Mock<ConstructorInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableConstructor (constructor)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand)).Verifiable();

      var constructorAsMethodInfoAdapter = new ConstructorAsMethodInfoAdapter (constructor);
      _decorator.EmitCall (OpCodes.Call, constructorAsMethodInfoAdapter, Type.EmptyTypes);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = 
        "Constructor calls cannot have optional parameters.\r\nParameter name: optionalParameterTypes")]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_WithOptionalParameters ()
    {
      var methodInfo = new ConstructorAsMethodInfoAdapter (ReflectionObjectMother.GetSomeDefaultConstructor ());

      _decorator.EmitCall (OpCodes.Call, methodInfo, new[] { ReflectionObjectMother.GetSomeType() });
    }

    [Test]
    public void EmitCall_MethodInfo_BaseCallMethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType () };
      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, optionalParameterTypes)).Verifiable();

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.EmitCall (OpCodes.Call, baseAdaptedMethod, optionalParameterTypes);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseCallMethodInfo_TurnsCallvirt_IntoCall ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType () };

      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, optionalParameterTypes)).Verifiable();

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.EmitCall (OpCodes.Callvirt, baseAdaptedMethod, optionalParameterTypes);

      _innerILGeneratorMock.Verify();
    }

    [Test]
    public void EmitCall_MethodInfo_MappedMethodInfo_NullOptionalParameters ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();

      var fakeEmittableOperand = new Mock<MethodInfo>().Object;
      _emittableOperandProviderStub.Setup (stub => stub.GetEmittableMethod (method)).Returns (fakeEmittableOperand);

      _innerILGeneratorMock.Setup (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, null)).Verifiable();

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.EmitCall (OpCodes.Callvirt, baseAdaptedMethod, null);

      _innerILGeneratorMock.Verify();
    }
  }
}