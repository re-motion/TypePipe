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
    private MutableReflectionObjectMap _mutableReflectionObjectMap;
    private ILGeneratorDecorator _decorator;

    [SetUp]
    public void SetUp ()
    {
      _innerILGeneratorMock = MockRepository.GenerateStrictMock<IILGenerator>();
      _mutableReflectionObjectMap = new MutableReflectionObjectMap();
      _decorator = new ILGeneratorDecorator (_innerILGeneratorMock, _mutableReflectionObjectMap);
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
      Assert.That (ilGeneratorDecoratorFactory.MutableReflectionObjectMap, Is.SameAs (_mutableReflectionObjectMap));
    }

    [Test]
    public void Emit_ConstructorInfo_Standard ()
    {
      var constructorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();

      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, constructorInfo));

      _decorator.Emit (OpCodes.Call, constructorInfo);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    [Ignore ("TODO 4686") ]
    public void Emit_ConstructorInfo_MutableConsructorInfo ()
    {
      Assert.Fail ("TODO 4686");
      //var mutableConstructorInfo = MutableConstructorInfoObjectMother.Create();
      //var constructorBuilder = ReflectionEmitObjectMother.GetSomeConstructorBuilder();

      //_innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, mutableConstructorInfo));

      //_decorator.Emit (OpCodes.Call, mutableConstructorInfo);

      //_innerILGeneratorMock.VerifyAllExpectations ();
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
      var methodInfo = new ConstructorAsMethodInfoAdapter (ReflectionObjectMother.GetSomeDefaultConstructor());
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
      var methodInfo = new ConstructorAsMethodInfoAdapter (ReflectionObjectMother.GetSomeDefaultConstructor ());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.ConstructorInfo));

      _decorator.EmitCall (OpCodes.Call, methodInfo, Type.EmptyTypes);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_NullOptionalParameters ()
    {
      var methodInfo = new ConstructorAsMethodInfoAdapter (ReflectionObjectMother.GetSomeDefaultConstructor ());
      _innerILGeneratorMock.Expect (mock => mock.Emit (OpCodes.Call, methodInfo.ConstructorInfo));

      _decorator.EmitCall (OpCodes.Call, methodInfo, null);

      _innerILGeneratorMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Constructor calls cannot have optional parameters.")]
    public void EmitCall_MethodInfo_BaseConstructorMethodInfo_WithOptionalParameters ()
    {
      var methodInfo = new ConstructorAsMethodInfoAdapter (ReflectionObjectMother.GetSomeDefaultConstructor ());

      _decorator.EmitCall (OpCodes.Call, methodInfo, new[] { ReflectionObjectMother.GetSomeType() });
    }
  }
}