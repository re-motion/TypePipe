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
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratorDecoratorTest
  {
    private IILGenerator _innerILGeneratorMock;
    private EmittableOperandProvider _emittableOperandProvider;

    private ILGeneratorDecorator _decorator;

    private IEmittableOperand _emittableOperandMock;
    private IEmittableMethodOperand _emittableMethodOperandMock;


    [SetUp]
    public void SetUp ()
    {
      _innerILGeneratorMock = MockRepository.GenerateStrictMock<IILGenerator>();
      _emittableOperandProvider = new EmittableOperandProvider();

      _decorator = new ILGeneratorDecorator (_innerILGeneratorMock, _emittableOperandProvider);

      _emittableOperandMock = MockRepository.GenerateStrictMock<IEmittableOperand>();
      _emittableMethodOperandMock = MockRepository.GenerateStrictMock<IEmittableMethodOperand>();
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
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (_emittableOperandProvider));
    }

    [Test]
    public void Emit_ConstructorInfo ()
    {
      var constructorInfo = ReflectionObjectMother.GetSomeConstructor();
      _emittableOperandProvider.AddMapping (constructorInfo, _emittableOperandMock);
      _emittableOperandMock.Expect (mock => mock.Emit (_innerILGeneratorMock, OpCodes.Call));

      _decorator.Emit (OpCodes.Call, constructorInfo);

      _emittableOperandMock.VerifyAllExpectations();
    }

    [Test]
    public void Emit_FieldInfo ()
    {
      var fieldInfo = ReflectionObjectMother.GetSomeField();
      _emittableOperandProvider.AddMapping (fieldInfo, _emittableOperandMock);
      _emittableOperandMock.Expect (mock => mock.Emit (_innerILGeneratorMock, OpCodes.Ldfld));

      _decorator.Emit (OpCodes.Ldfld, fieldInfo);

      _emittableOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo ()
    {
      var methodInfo = ReflectionObjectMother.GetSomeMethod();
      _emittableOperandProvider.AddMapping (methodInfo, _emittableMethodOperandMock);
      _emittableMethodOperandMock.Expect (mock => mock.Emit (_innerILGeneratorMock, OpCodes.Call));

      _decorator.Emit (OpCodes.Call, methodInfo);

      _emittableMethodOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo_BaseConstructorMethodInfo ()
    {
      var mappedConstructorInfo = ReflectionObjectMother.GetSomeConstructor ();
      _emittableMethodOperandMock.Expect (mock => mock.Emit (_innerILGeneratorMock, OpCodes.Call));
      _emittableOperandProvider.AddMapping (mappedConstructorInfo, _emittableMethodOperandMock);

      var methodInfo = new ConstructorAsMethodInfoAdapter (mappedConstructorInfo);

      _decorator.Emit (OpCodes.Call, methodInfo);

      _emittableMethodOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo_BaseCallMethodInfo ()
    {
      var mappedMethodInfo = ReflectionObjectMother.GetSomeMethod ();
      _emittableMethodOperandMock.Expect (mock => mock.Emit (_innerILGeneratorMock, OpCodes.Call));
      _emittableOperandProvider.AddMapping (mappedMethodInfo, _emittableMethodOperandMock);

      var methodInfo = new BaseCallMethodInfoAdapter (mappedMethodInfo);

      _decorator.Emit (OpCodes.Call, methodInfo);

      _emittableMethodOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void Emit_MethodInfo_BaseCallMethodInfo_TurnsCallvirt_IntoCall ()
    {
      var methodInfo = new BaseCallMethodInfoAdapter (ReflectionObjectMother.GetSomeMethod ());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.AdaptedMethodInfo));

      _decorator.Emit (OpCodes.Callvirt, methodInfo);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo ()
    {
      var methodInfo = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType() };
      _emittableMethodOperandMock.Expect (mock => mock.EmitCall (_innerILGeneratorMock, OpCodes.Call, optionalParameterTypes));
      _emittableOperandProvider.AddMapping (methodInfo, _emittableMethodOperandMock);

      _decorator.EmitCall (OpCodes.Call, methodInfo, optionalParameterTypes);

      _emittableMethodOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo ()
    {
      var mappedConstructorInfo = ReflectionObjectMother.GetSomeConstructor ();
      _emittableOperandMock.Expect (mock => mock.Emit (_innerILGeneratorMock, OpCodes.Call));
      _emittableOperandProvider.AddMapping (mappedConstructorInfo, _emittableOperandMock);

      var methodInfo = new ConstructorAsMethodInfoAdapter (mappedConstructorInfo);

      _decorator.EmitCall (OpCodes.Call, methodInfo, null);

      _emittableOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_EmptyOptionalParameters ()
    {
      var methodInfo = new ConstructorAsMethodInfoAdapter (ReflectionObjectMother.GetSomeDefaultConstructor ());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.ConstructorInfo));

      _decorator.EmitCall (OpCodes.Call, methodInfo, Type.EmptyTypes);

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
      var mappedMethodInfo = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType () };
      _emittableMethodOperandMock.Expect (mock => mock.EmitCall (_innerILGeneratorMock, OpCodes.Call, optionalParameterTypes));
      _emittableOperandProvider.AddMapping (mappedMethodInfo, _emittableMethodOperandMock);

      var methodInfo = new BaseCallMethodInfoAdapter (mappedMethodInfo);

      _decorator.EmitCall (OpCodes.Call, methodInfo, optionalParameterTypes);

      _emittableMethodOperandMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseCallMethodInfo_TurnsCallvirt_IntoCall ()
    {
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType () };

      var adaptedMethod = ReflectionObjectMother.GetSomeMethod ();
      var methodInfo = new BaseCallMethodInfoAdapter (adaptedMethod);

      _innerILGeneratorMock.Expect (mock => mock.EmitCall (OpCodes.Call, adaptedMethod, optionalParameterTypes));
      
      _decorator.EmitCall (OpCodes.Callvirt, methodInfo, optionalParameterTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_MappedMethodInfo_NullOptionalParameters ()
    {
      var mappedMethodInfo = ReflectionObjectMother.GetSomeMethod ();
      _emittableMethodOperandMock.Expect (mock => mock.EmitCall (_innerILGeneratorMock, OpCodes.Call, null));
      _emittableOperandProvider.AddMapping (mappedMethodInfo, _emittableMethodOperandMock);

      _decorator.EmitCall (OpCodes.Call, mappedMethodInfo, null);

      _emittableMethodOperandMock.VerifyAllExpectations ();
    }
  }
}