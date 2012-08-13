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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class EmittableOperandProviderTest
  {
    private EmittableOperandProvider _provider;

    private MutableType _mutableType;
    private MutableFieldInfo _mutableField;
    private MutableConstructorInfo _mutableConstructor;
    private MutableMethodInfo _mutableMethod;

    [SetUp]
    public void SetUp ()
    {
      _provider = new EmittableOperandProvider();

      _mutableType = MutableTypeObjectMother.CreateForExistingType();
      _mutableField = MutableFieldInfoObjectMother.CreateForExisting();
      _mutableConstructor = MutableConstructorInfoObjectMother.CreateForExisting();
      _mutableMethod = MutableMethodInfoObjectMother.CreateForExisting();
    }

    [Test]
    public void AddMapping ()
    {
      CheckAddMapping<MutableType, Type> (_provider.AddMapping, _provider.GetEmittableType, _mutableType);
      CheckAddMapping<MutableFieldInfo, FieldInfo> (_provider.AddMapping, _provider.GetEmittableField, _mutableField);
      CheckAddMapping<MutableConstructorInfo, ConstructorInfo> (_provider.AddMapping, _provider.GetEmittableConstructor, _mutableConstructor);
      CheckAddMapping<MutableMethodInfo, MethodInfo> (_provider.AddMapping, _provider.GetEmittableMethod, _mutableMethod);
    }

    [Test]
    public void AddMapping_Twice ()
    {
      CheckAddMappingTwiceThrows<MutableType, Type> (_provider.AddMapping, _mutableType, "Type is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows<MutableFieldInfo, FieldInfo> (_provider.AddMapping, _mutableField, "FieldInfo is already mapped.\r\nParameter name: mappedField");
      CheckAddMappingTwiceThrows<MutableConstructorInfo, ConstructorInfo> (_provider.AddMapping, _mutableConstructor, "ConstructorInfo is already mapped.\r\nParameter name: mappedConstructor");
      CheckAddMappingTwiceThrows<MutableMethodInfo, MethodInfo> (_provider.AddMapping, _mutableMethod, "MethodInfo is already mapped.\r\nParameter name: mappedMethod");
    }

    [Test]
    public void GetEmittableXXX_Mutable ()
    {
      var type = _mutableType.UnderlyingSystemType;
      var field = _mutableField.UnderlyingSystemFieldInfo;
      var ctor = _mutableConstructor.UnderlyingSystemConstructorInfo;
      var method = _mutableMethod.UnderlyingSystemMethodInfo;

      _provider.AddMapping (_mutableType, type);
      _provider.AddMapping (_mutableField, field);
      _provider.AddMapping (_mutableConstructor, ctor);
      _provider.AddMapping (_mutableMethod, method);

      Assert.That (_provider.GetEmittableType (_mutableType), Is.SameAs (type));
      Assert.That (_provider.GetEmittableField (_mutableField), Is.SameAs (field));
      Assert.That (_provider.GetEmittableConstructor (_mutableConstructor), Is.SameAs (ctor));
      Assert.That (_provider.GetEmittableMethod (_mutableMethod), Is.SameAs (method));
    }

    [Test]
    public void GetEmittableXXX_NonMutable_NoMapping ()
    {
      var type = _mutableType.UnderlyingSystemType;
      var field = _mutableField.UnderlyingSystemFieldInfo;
      var ctor = _mutableConstructor.UnderlyingSystemConstructorInfo;
      var method = _mutableMethod.UnderlyingSystemMethodInfo;

      Assert.That (_provider.GetEmittableType (type), Is.SameAs (type));
      Assert.That (_provider.GetEmittableField (field), Is.SameAs (field));
      Assert.That (_provider.GetEmittableConstructor (ctor), Is.SameAs (ctor));
      Assert.That (_provider.GetEmittableMethod (method), Is.SameAs (method));
    }
    
    [Test]
    public void GetEmittableXXX_NoMapping ()
    {
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableType, _mutableType);
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableField, _mutableField);
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableConstructor, _mutableConstructor);
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableMethod, _mutableMethod);
    }

    [Test]
    public void GetEmittableOperand_Mutable ()
    {
      var type = _mutableType.UnderlyingSystemType;
      var field = _mutableField.UnderlyingSystemFieldInfo;
      var ctor = _mutableConstructor.UnderlyingSystemConstructorInfo;
      var method = _mutableMethod.UnderlyingSystemMethodInfo;

      _provider.AddMapping (_mutableType, type);
      _provider.AddMapping (_mutableField, field);
      _provider.AddMapping (_mutableConstructor, ctor);
      _provider.AddMapping (_mutableMethod, method);

      Assert.That (_provider.GetEmittableOperand (_mutableType), Is.SameAs (type));
      Assert.That (_provider.GetEmittableOperand (_mutableField), Is.SameAs (field));
      Assert.That (_provider.GetEmittableOperand (_mutableConstructor), Is.SameAs (ctor));
      Assert.That (_provider.GetEmittableOperand (_mutableMethod), Is.SameAs (method));
    }

    [Test]
    public void GetEmittableOperand_NonMutable_NoMapping ()
    {
      var type = _mutableType.UnderlyingSystemType;
      var field = _mutableField.UnderlyingSystemFieldInfo;
      var ctor = _mutableConstructor.UnderlyingSystemConstructorInfo;
      var method = _mutableMethod.UnderlyingSystemMethodInfo;

      Assert.That (_provider.GetEmittableOperand (type), Is.SameAs (type));
      Assert.That (_provider.GetEmittableOperand (field), Is.SameAs (field));
      Assert.That (_provider.GetEmittableOperand (ctor), Is.SameAs (ctor)); 
      Assert.That (_provider.GetEmittableOperand (method), Is.SameAs (method));
    }

    [Test]
    public void GetEmittableOperand_Mutable_NoMapping ()
    {
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableOperand, _mutableType);
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableOperand, _mutableField);
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableOperand, _mutableConstructor);
      CheckGetEmitableOperandWithNoMappingThrows (_provider.GetEmittableOperand, _mutableMethod);
    }

    private void CheckAddMapping<TMutable, TBase> (
        Action<TMutable, TBase> addMappingMethod, Func<TBase, TBase> getEmittableOperandMethod, TMutable operandToBeEmitted)
        where TBase: MemberInfo
        where TMutable: TBase
    {
      var fakeOperand = MockRepository.GenerateStub<TBase>();

      addMappingMethod (operandToBeEmitted, fakeOperand);

      var result = getEmittableOperandMethod (operandToBeEmitted);
      Assert.That (result, Is.SameAs (fakeOperand));
    }

    private void CheckAddMappingTwiceThrows<TMutable, TBase> (
        Action<TMutable, TBase> addMappingMethod, TMutable operandToBeEmitted, string expectedMessage)
        where TBase: class
        where TMutable: TBase
    {
      addMappingMethod (operandToBeEmitted, MockRepository.GenerateStub<TBase>());

      Assert.That (
          () => addMappingMethod (operandToBeEmitted, MockRepository.GenerateStub<TBase>()),
          Throws.ArgumentException.With.Message.EqualTo (expectedMessage));
    }

    private void CheckGetEmitableOperandWithNoMappingThrows<TMutable, TBase> (
        Func<TMutable, TBase> getEmittableOperandMethod, TMutable operandToBeEmitted)
        where TMutable: TBase
    {
      var message = string.Format ("No emittable operand found for '{0}' of type '{1}'.", operandToBeEmitted, operandToBeEmitted.GetType().Name);
      Assert.That (
          () => getEmittableOperandMethod (operandToBeEmitted),
          Throws.InvalidOperationException.With.Message.EqualTo (message));
    }
  }
}