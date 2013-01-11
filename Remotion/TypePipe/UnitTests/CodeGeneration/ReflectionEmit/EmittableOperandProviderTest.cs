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

      _mutableType = MutableTypeObjectMother.CreateForExisting();
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
    public void AddMapping_ReferenceEquality ()
    {
      var mutableType1 = MutableTypeObjectMother.CreateForExisting();
      var mutableType2 = MutableTypeObjectMother.CreateForExisting();
      Assert.That (mutableType1, Is.EqualTo (mutableType2).And.Not.SameAs (mutableType2));
      _provider.AddMapping (mutableType1, mutableType1.UnderlyingSystemType);

      Assert.That (() => _provider.AddMapping (mutableType2, mutableType2.UnderlyingSystemType), Throws.Nothing);
    }

    [Test]
    public void AddMapping_Twice ()
    {
      CheckAddMappingTwiceThrows<MutableType, Type> (
          _provider.AddMapping, _mutableType, "MutableType '{memberName}' is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows<MutableFieldInfo, FieldInfo> (
          _provider.AddMapping, _mutableField, "MutableFieldInfo '{memberName}' is already mapped.\r\nParameter name: mappedField");
      CheckAddMappingTwiceThrows<MutableConstructorInfo, ConstructorInfo> (
          _provider.AddMapping, _mutableConstructor, "MutableConstructorInfo '{memberName}' is already mapped.\r\nParameter name: mappedConstructor");
      CheckAddMappingTwiceThrows<MutableMethodInfo, MethodInfo> (
          _provider.AddMapping, _mutableMethod, "MutableMethodInfo '{memberName}' is already mapped.\r\nParameter name: mappedMethod");
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
    public void GetEmittableType ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      _provider.AddMapping (mutableType, ReflectionObjectMother.GetSomeType());

      var constructedType = typeof (List<>).MakeGenericType (mutableType);
      Assert.That (constructedType.GetGenericArguments(), Is.EqualTo (new[] { mutableType }));

      _provider.GetEmittableType (constructedType);

      Assert.That (constructedType.GetGenericArguments(), Is.EqualTo (new[] { mutableType }));
    }

    [Test]
    public void GetEmittableType_GenericType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting();
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
      var mutableType = MutableTypeObjectMother.CreateForExisting();
      var constructedType = typeof (StrongBox<>).MakeGenericType (mutableType);
      FieldInfo field = new FieldOnTypeInstantiation (constructedType, typeof (StrongBox<>).GetField ("Value"));

      CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (_provider, (p, f) => p.GetEmittableField (f), mutableType, field);
    }

    [Test]
    public void GetEmittableConstructor_GenericTypeDeclaringType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting ();
      var constructedType = typeof (List<>).MakeGenericType (mutableType);
      ConstructorInfo ctor = new ConstructorOnTypeInstantiation (constructedType, typeof (List<>).GetConstructor (Type.EmptyTypes));

      CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (_provider, (p, c) => p.GetEmittableConstructor (c), mutableType, ctor);
    }

    [Test]
    public void GetEmittableMethod_GenericTypeDeclaringType_MutableTypeGenericParameter ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting();
      var constructedType = typeof (List<>).MakeGenericType (mutableType);
      MethodInfo method = new MethodOnTypeInstantiation (constructedType, typeof (List<>).GetMethod ("Add"));

      CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument (_provider, (p, m) => p.GetEmittableMethod (m), mutableType, method);
    }

    private void CheckAddMapping<TMutable, T> (
        Action<TMutable, T> addMappingMethod, Func<T, T> getEmittableOperandMethod, TMutable operandToBeEmitted)
        where TMutable : T
        where T : class
    {
      var fakeOperand = MockRepository.GenerateStub<T>();

      addMappingMethod (operandToBeEmitted, fakeOperand);

      var result = getEmittableOperandMethod (operandToBeEmitted);
      Assert.That (result, Is.SameAs (fakeOperand));
    }

    private void CheckAddMappingTwiceThrows<TMutable, TBuilder> (
        Action<TMutable, TBuilder> addMappingMethod, TMutable operandToBeEmitted, string expectedMessageTemplate)
        where TMutable : MemberInfo
        where TBuilder : class
    {
      addMappingMethod (operandToBeEmitted, MockRepository.GenerateStub<TBuilder>());

      var expectedMessage = expectedMessageTemplate.Replace ("{memberName}", operandToBeEmitted.Name);
      Assert.That (
          () => addMappingMethod (operandToBeEmitted, MockRepository.GenerateStub<TBuilder>()),
          Throws.ArgumentException.With.Message.EqualTo (expectedMessage));
    }

    private void CheckGetEmitableOperandWithNoMappingThrows<TMutable, TBase> (
        Func<TMutable, TBase> getEmittableOperandMethod, TMutable operandToBeEmitted)
        where TMutable : TBase
    {
      var message = string.Format ("No emittable operand found for '{0}' of type '{1}'.", operandToBeEmitted, operandToBeEmitted.GetType().Name);
      Assert.That (
          () => getEmittableOperandMethod (operandToBeEmitted),
          Throws.InvalidOperationException.With.Message.EqualTo (message));
    }

    private static void CheckGetEmittableMemberOnTypeBuilderInstantiationWithOneGenericArgument<T> (
        EmittableOperandProvider provider, Func<EmittableOperandProvider, T, T> getEmittableFunc, MutableType mutableTypeWithMapping, T member)
        where T : MemberInfo
    {
      var emittableType = ReflectionEmitObjectMother.GetSomeTypeBuilderInstantiation();
      provider.AddMapping (mutableTypeWithMapping, emittableType);

      var result = getEmittableFunc (provider, member);

      var mutableTypeGenericArgument = result.DeclaringType.GetGenericArguments().Single();
      Assert.That (mutableTypeGenericArgument, Is.SameAs (emittableType));
    }
  }
}