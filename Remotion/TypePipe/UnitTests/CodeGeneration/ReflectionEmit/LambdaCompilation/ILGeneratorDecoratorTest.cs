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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratorDecoratorTest
  {
    private IILGenerator _innerILGeneratorMock;
    private IEmittableOperandProvider _emittableOperandProviderStub;

    private ILGeneratorDecorator _decorator;
    
    [SetUp]
    public void SetUp ()
    {
      _innerILGeneratorMock = MockRepository.GenerateStrictMock<IILGenerator>();
      _emittableOperandProviderStub = MockRepository.GenerateStub<IEmittableOperandProvider>();

      _decorator = new ILGeneratorDecorator (_innerILGeneratorMock, _emittableOperandProviderStub);
    }

    [Test]
    public void GetFactory ()
    {
      var fakeFactory = MockRepository.GenerateStub<IILGeneratorFactory>();
      _innerILGeneratorMock.Stub (stub => stub.GetFactory()).Return (fakeFactory);

      var ilGeneratorFactory = _decorator.GetFactory();

      Assert.That (ilGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());
      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) ilGeneratorFactory;
      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.SameAs (fakeFactory));
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (_emittableOperandProviderStub));
    }

    [Test]
    public void BeginCatchBlock ()
    {
      var exceptionType = ReflectionObjectMother.GetSomeType();
      var fakeEmittableOperand = ReflectionObjectMother.GetSomeOtherType();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableType (exceptionType)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.BeginCatchBlock (fakeEmittableOperand));

      _decorator.BeginCatchBlock (exceptionType);

      _innerILGeneratorMock.VerifyAllExpectations();
    }

    [Test]
    public void DeclareLocal ()
    {
      var localVariableType = ReflectionObjectMother.GetSomeType();
      var fakeEmittableOperand = MockRepository.GenerateStub<Type>();
      var fakeLocalBuilder = ReflectionEmitObjectMother.GetSomeLocalBuilder();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableType (localVariableType)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.DeclareLocal (fakeEmittableOperand)).Return (fakeLocalBuilder);

      var result = _decorator.DeclareLocal (localVariableType);

      _innerILGeneratorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeLocalBuilder));
    }

    [Test]
    public void Emit_ConstructorInfo ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor();
      var fakeEmittableOperand = MockRepository.GenerateStub<ConstructorInfo>();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableConstructor (constructor)).Return (fakeEmittableOperand);
      
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));

      _decorator.Emit (OpCodes.Call, constructor);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_Type ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeEmittableOperand = MockRepository.GenerateStub<Type> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableType (type)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldtoken, fakeEmittableOperand));

      _decorator.Emit (OpCodes.Ldtoken, type);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_FieldInfo ()
    {
      var field = ReflectionObjectMother.GetSomeField();
      var fakeEmittableOperand = MockRepository.GenerateStub<FieldInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableField (field)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Ldfld, fakeEmittableOperand));

      _decorator.Emit (OpCodes.Ldfld, field);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod();
      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));
      
      _decorator.Emit (OpCodes.Call, method);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo_BaseConstructorMethodInfo ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor ();
      var fakeEmittableOperand = MockRepository.GenerateStub<ConstructorInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableConstructor (constructor)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));

      var constructorAsMethodInfoAdapter = new ConstructorAsMethodInfoAdapter (constructor);
      _decorator.Emit (OpCodes.Call, constructorAsMethodInfoAdapter);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo_BaseCallMethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));
      
      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.Emit (OpCodes.Call, baseAdaptedMethod);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo_BaseCallMethodInfo_TurnsCallvirt_IntoCall ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.Emit (OpCodes.Callvirt, baseAdaptedMethod);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType() };
      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, optionalParameterTypes));

      _decorator.EmitCall (OpCodes.Call, method, optionalParameterTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_NullOptionalParameters ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor ();
      var fakeEmittableOperand = MockRepository.GenerateStub<ConstructorInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableConstructor (constructor)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));

      var constructorAsMethodInfoAdapter = new ConstructorAsMethodInfoAdapter (constructor);
      _decorator.EmitCall (OpCodes.Call, constructorAsMethodInfoAdapter, null);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_EmptyOptionalParameters ()
    {
      var constructor = ReflectionObjectMother.GetSomeConstructor ();
      var fakeEmittableOperand = MockRepository.GenerateStub<ConstructorInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableConstructor (constructor)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, fakeEmittableOperand));

      var constructorAsMethodInfoAdapter = new ConstructorAsMethodInfoAdapter (constructor);
      _decorator.EmitCall (OpCodes.Call, constructorAsMethodInfoAdapter, Type.EmptyTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Constructor calls cannot have optional parameters.")]
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
      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, optionalParameterTypes));

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.EmitCall (OpCodes.Call, baseAdaptedMethod, optionalParameterTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseCallMethodInfo_TurnsCallvirt_IntoCall ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType () };

      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, optionalParameterTypes));

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.EmitCall (OpCodes.Callvirt, baseAdaptedMethod, optionalParameterTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_MappedMethodInfo_NullOptionalParameters ()
    {
      var method = ReflectionObjectMother.GetSomeMethod ();

      var fakeEmittableOperand = MockRepository.GenerateStub<MethodInfo> ();
      _emittableOperandProviderStub.Stub (stub => stub.GetEmittableMethod (method)).Return (fakeEmittableOperand);

      _innerILGeneratorMock.Expect (mock => mock.EmitCall (OpCodes.Call, fakeEmittableOperand, null));

      var baseAdaptedMethod = new NonVirtualCallMethodInfoAdapter (method);
      _decorator.EmitCall (OpCodes.Callvirt, baseAdaptedMethod, null);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }
  }
}