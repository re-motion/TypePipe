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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
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
      CheckAddMapping<MutableType, Type> (_provider.AddMapping, _provider.GetEmittableOperand, _mutableType);
      CheckAddMapping<MutableFieldInfo, FieldInfo> (_provider.AddMapping, _provider.GetEmittableOperand, _mutableField);
      CheckAddMapping<MutableConstructorInfo, ConstructorInfo> (_provider.AddMapping, _provider.GetEmittableOperand, _mutableConstructor);
      CheckAddMapping<MutableMethodInfo, MethodInfo> (_provider.AddMapping, _provider.GetEmittableOperand, _mutableMethod);
    }

    [Test]
    public void AddMapping_Twice ()
    {
      CheckAddMappingTwiceThrows<MutableType, Type> (
          _provider.AddMapping, _mutableType, "MutableType is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows<MutableFieldInfo, FieldInfo> (
          _provider.AddMapping, _mutableField, "MutableFieldInfo is already mapped.\r\nParameter name: mappedField");
      CheckAddMappingTwiceThrows<MutableConstructorInfo, ConstructorInfo> (
          _provider.AddMapping, _mutableConstructor, "MutableConstructorInfo is already mapped.\r\nParameter name: mappedConstructor");
      CheckAddMappingTwiceThrows<MutableMethodInfo, MethodInfo> (
          _provider.AddMapping, _mutableMethod, "MutableMethodInfo is already mapped.\r\nParameter name: mappedMethod");
    }

    [Test]
    public void GetEmittableXXX_Mutable ()
    {
      var typeBuilder = ReflectionEmitObjectMother.GetSomeTypeBuilder();
      var fieldBuilder = ReflectionEmitObjectMother.GetSomeFieldBuilder();
      var constructorBuilder = ReflectionEmitObjectMother.GetSomeConstructorBuilder();
      var methodBuilder = ReflectionEmitObjectMother.GetSomeMethodBuilder();

      _provider.AddMapping (_mutableType, typeBuilder);
      _provider.AddMapping (_mutableField, fieldBuilder);
      _provider.AddMapping (_mutableConstructor, constructorBuilder);
      _provider.AddMapping (_mutableMethod, methodBuilder);

      Assert.That (_provider.GetEmittableType (_mutableType), Is.SameAs (typeBuilder));
      Assert.That (_provider.GetEmittableField (_mutableField), Is.SameAs (fieldBuilder));
      Assert.That (_provider.GetEmittableConstructor (_mutableConstructor), Is.SameAs (constructorBuilder));
      Assert.That (_provider.GetEmittableMethod (_mutableMethod), Is.SameAs (methodBuilder));
    }

    [Test]
    public void GetEmittableXXX_RuntimeInfos ()
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
    public void GetEmittableXXX_Builders ()
    {
      var type = ReflectionEmitObjectMother.GetSomeTypeBuilder ();
      var field = ReflectionEmitObjectMother.GetSomeFieldBuilder ();
      var ctor = ReflectionEmitObjectMother.GetSomeConstructorBuilder ();
      var method = ReflectionEmitObjectMother.GetSomeMethodBuilder ();

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
      var typeBuilder = ReflectionEmitObjectMother.GetSomeTypeBuilder ();
      var fieldBuilder = ReflectionEmitObjectMother.GetSomeFieldBuilder ();
      var constructorBuilder = ReflectionEmitObjectMother.GetSomeConstructorBuilder ();
      var methodBuilder = ReflectionEmitObjectMother.GetSomeMethodBuilder ();

      _provider.AddMapping (_mutableType, typeBuilder);
      _provider.AddMapping (_mutableField, fieldBuilder);
      _provider.AddMapping (_mutableConstructor, constructorBuilder);
      _provider.AddMapping (_mutableMethod, methodBuilder);

      Assert.That (_provider.GetEmittableOperand (_mutableType), Is.SameAs (typeBuilder));
      Assert.That (_provider.GetEmittableOperand (_mutableField), Is.SameAs (fieldBuilder));
      Assert.That (_provider.GetEmittableOperand (_mutableConstructor), Is.SameAs (constructorBuilder));
      Assert.That (_provider.GetEmittableOperand (_mutableMethod), Is.SameAs (methodBuilder));
    }

    [Test]
    public void GetEmittableOperand_RuntimeInfos ()
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
    public void GetEmittableOperand_Builders ()
    {
      var type = ReflectionEmitObjectMother.GetSomeTypeBuilder();
      var field = ReflectionEmitObjectMother.GetSomeFieldBuilder();
      var ctor = ReflectionEmitObjectMother.GetSomeConstructorBuilder();
      var method = ReflectionEmitObjectMother.GetSomeMethodBuilder();

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

    [Test]
    public void GetEmittableType_GenericType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType();
      var emittableType = ReflectionObjectMother.GetSomeType();
      _provider.AddMapping (mutableType, emittableType);

      // Test recursion: List<Func<mt xxx>> as TypeBuilderInstantiation
      var constructedType = typeof (List<>).MakeGenericType (typeof (Func<>).MakeGenericType (mutableType));
      Assert.That (constructedType.IsRuntimeType(), Is.False);

      var result = _provider.GetEmittableType (constructedType);

      var mutableTypeGenericArgument = result.GetGenericArguments().Single().GetGenericArguments().Single();
      Assert.That (mutableTypeGenericArgument, Is.SameAs (emittableType));
    }

    [Test]
    public void GetEmittableField_GenericTypeDeclaringType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType();
      var constructedType = typeof (StrongBox<>).MakeGenericType (mutableType);
      var field = new FieldOnTypeInstantiation (constructedType, typeof (StrongBox<>).GetField ("Value"));

      CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (_provider, mutableType, field);
    }

    [Test]
    public void GetEmittableConstructor_GenericTypeDeclaringType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType ();
      var constructedType = typeof (List<>).MakeGenericType (mutableType);
      var ctor = new ConstructorOnTypeInstantiation (constructedType, typeof (List<>).GetConstructor (Type.EmptyTypes));

      CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (_provider, mutableType, ctor);
    }

    [Test]
    public void GetEmittableMethod_GenericTypeDeclaringType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType();
      var constructedType = typeof (List<>).MakeGenericType (mutableType);
      var method = new MethodOnTypeInstantiation (constructedType, typeof (List<>).GetMethod ("Add"));

      CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (_provider, mutableType, method);
    }

    private void CheckAddMapping<TMutable, T> (
        Action<TMutable, T> addMappingMethod, Func<object, object> getEmittableOperandMethod, TMutable operandToBeEmitted)
        where TMutable : T
        where T : class
    {
      var fakeOperand = MockRepository.GenerateStub<T>();

      addMappingMethod (operandToBeEmitted, fakeOperand);

      var result = getEmittableOperandMethod (operandToBeEmitted);
      Assert.That (result, Is.SameAs (fakeOperand));
    }

    private void CheckAddMappingTwiceThrows<TMutable, TBuilder> (
        Action<TMutable, TBuilder> addMappingMethod, TMutable operandToBeEmitted, string expectedMessage)
        where TBuilder : class
    {
      addMappingMethod (operandToBeEmitted, MockRepository.GenerateStub<TBuilder>());

      Assert.That (
          () => addMappingMethod (operandToBeEmitted, MockRepository.GenerateStub<TBuilder>()),
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

    private static void CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (EmittableOperandProvider provider, MutableType mutableTypeWithMapping, MemberInfo member)
    {
      var emittableType = ReflectionEmitObjectMother.GetSomeTypeBuilderInstantiation();
      provider.AddMapping (mutableTypeWithMapping, emittableType);

      var result = (MemberInfo) provider.GetEmittableOperand (member);

      var mutableTypeGenericArgument = result.DeclaringType.GetGenericArguments().Single();
      Assert.That (mutableTypeGenericArgument, Is.SameAs (emittableType));
    }
  }
}