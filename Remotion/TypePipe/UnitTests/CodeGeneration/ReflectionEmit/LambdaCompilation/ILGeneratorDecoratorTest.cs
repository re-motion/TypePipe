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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratorDecoratorTest
  {
    private IILGenerator _innerILGeneratorMock;
    private ILGeneratorDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerILGeneratorMock = MockRepository.GenerateStrictMock<IILGenerator>();
      _decorator = new ILGeneratorDecorator (_innerILGeneratorMock);
    }

    [Test]
    public void GetFactory ()
    {
      var fakeFactory = MockRepository.GenerateStub<IILGeneratorFactory>();
      _innerILGeneratorMock.Stub (stub => stub.GetFactory()).Return (fakeFactory);

      var result = _decorator.GetFactory();

      Assert.That (result, Is.TypeOf<ILGeneratorDecoratorFactory>().With.Property ("InnerFactory").SameAs (fakeFactory));
    }

    [Test]
    public void Emit_MethodInfo_Standard ()
    {
      var methodInfo = ReflectionObjectMother.GetSomeMethod();

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo));

      _decorator.Emit (OpCodes.Call, methodInfo);

      _innerILGeneratorMock.VerifyAllExpectations();
    }

    [Test]
    public void Emit_MethodInfo_BaseConstructorMethodInfo ()
    {
      var methodInfo = new BaseConstructorMethodInfo (ReflectionObjectMother.GetSomeDefaultConstructor());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.ConstructorInfo));

      _decorator.Emit (OpCodes.Call, methodInfo);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_Standard ()
    {
      var methodInfo = ReflectionObjectMother.GetSomeMethod ();
      var optionalParameterTypes = new[] { ReflectionObjectMother.GetSomeType() };
      _innerILGeneratorMock.Expect (mock => mock.EmitCall (OpCodes.Call, methodInfo, optionalParameterTypes));

      _decorator.EmitCall (OpCodes.Call, methodInfo, optionalParameterTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_EmptyOptionalParameters ()
    {
      var methodInfo = new BaseConstructorMethodInfo (ReflectionObjectMother.GetSomeDefaultConstructor ());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.ConstructorInfo));

      _decorator.EmitCall (OpCodes.Call, methodInfo, Type.EmptyTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_NullOptionalParameters ()
    {
      var methodInfo = new BaseConstructorMethodInfo (ReflectionObjectMother.GetSomeDefaultConstructor ());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.ConstructorInfo));

      _decorator.EmitCall (OpCodes.Call, methodInfo, null);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Constructor calls cannot have optional parameters.")]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_WithOptionalParameters ()
    {
      var methodInfo = new BaseConstructorMethodInfo (ReflectionObjectMother.GetSomeDefaultConstructor ());

      _decorator.EmitCall (OpCodes.Call, methodInfo, new[] { ReflectionObjectMother.GetSomeType() });
    }
  }
}