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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class EmittableOperandProviderTest
  {
    private IDelegateProvider _delegateProviderMock;

    private EmittableOperandProvider _provider;

    private MutableType _mutableType;
    private MutableGenericParameter _mutableGenericParameter;
    private MutableFieldInfo _mutableField;
    private MutableConstructorInfo _mutableConstructor;
    private MutableMethodInfo _mutableMethod;

    private Type _emittableType;

    [SetUp]
    public void SetUp ()
    {
      _delegateProviderMock = MockRepository.GenerateStrictMock<IDelegateProvider>();

      _provider = new EmittableOperandProvider (_delegateProviderMock);

      _mutableType = MutableTypeObjectMother.Create();
      _mutableGenericParameter = MutableGenericParameterObjectMother.Create();
      _mutableField = MutableFieldInfoObjectMother.Create();
      _mutableConstructor = MutableConstructorInfoObjectMother.Create();
      _mutableMethod = MutableMethodInfoObjectMother.Create();

      _emittableType = ReflectionObjectMother.GetSomeType();
    }

    [Test]
    public void AddMapping ()
    {
      CheckAddMapping<MutableType, Type> (_provider.AddMapping, _provider.GetEmittableType, _mutableType);
      CheckAddMapping<MutableGenericParameter, Type> (_provider.AddMapping, _provider.GetEmittableType, _mutableGenericParameter);
      CheckAddMapping<MutableFieldInfo, FieldInfo> (_provider.AddMapping, _provider.GetEmittableField, _mutableField);
      CheckAddMapping<MutableConstructorInfo, ConstructorInfo> (_provider.AddMapping, _provider.GetEmittableConstructor, _mutableConstructor);
      CheckAddMapping<MutableMethodInfo, MethodInfo> (_provider.AddMapping, _provider.GetEmittableMethod, _mutableMethod);
    }

    [Test]
    public void AddMapping_Twice ()
    {
      CheckAddMappingTwiceThrows<MutableType, Type> (
          _provider.AddMapping, _mutableType, "Type '{memberName}' is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows<MutableGenericParameter, Type> (
          _provider.AddMapping, _mutableGenericParameter, "Type '{memberName}' is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows<MutableFieldInfo, FieldInfo> (
          _provider.AddMapping, _mutableField, "FieldInfo '{memberName}' is already mapped.\r\nParameter name: mappedField");
      CheckAddMappingTwiceThrows<MutableConstructorInfo, ConstructorInfo> (
          _provider.AddMapping, _mutableConstructor, "ConstructorInfo '{memberName}' is already mapped.\r\nParameter name: mappedConstructor");
      CheckAddMappingTwiceThrows<MutableMethodInfo, MethodInfo> (
          _provider.AddMapping, _mutableMethod, "MethodInfo '{memberName}' is already mapped.\r\nParameter name: mappedMethod");
    }

    [Test]
    public void GetEmittableXXX_Mutable ()
    {
      var emittableType = ReflectionObjectMother.GetSomeType();
      var emittableGenericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var emittableField = ReflectionObjectMother.GetSomeField();
      var emittableConstructor = ReflectionObjectMother.GetSomeConstructor();
      var emittableMethod = ReflectionObjectMother.GetSomeMethod();

      _provider.AddMapping (_mutableType, emittableType);
      _provider.AddMapping (_mutableGenericParameter, emittableGenericParameter);
      _provider.AddMapping (_mutableField, emittableField);
      _provider.AddMapping (_mutableConstructor, emittableConstructor);
      _provider.AddMapping (_mutableMethod, emittableMethod);

      Assert.That (_provider.GetEmittableType (_mutableType), Is.SameAs (emittableType));
      Assert.That (_provider.GetEmittableType (_mutableGenericParameter), Is.SameAs (emittableGenericParameter));
      Assert.That (_provider.GetEmittableField (_mutableField), Is.SameAs (emittableField));
      Assert.That (_provider.GetEmittableConstructor (_mutableConstructor), Is.SameAs (emittableConstructor));
      Assert.That (_provider.GetEmittableMethod (_mutableMethod), Is.SameAs (emittableMethod));
    }

    [Test]
    public void GetEmittableXXX_RuntimeInfos ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var genericParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var field = ReflectionObjectMother.GetSomeField();
      var ctor = ReflectionObjectMother.GetSomeConstructor();
      var method = ReflectionObjectMother.GetSomeMethod();

      Assert.That (_provider.GetEmittableType (type), Is.SameAs (type));
      Assert.That (_provider.GetEmittableType (genericParameter), Is.SameAs (genericParameter));
      Assert.That (_provider.GetEmittableField (field), Is.SameAs (field));
      Assert.That (_provider.GetEmittableConstructor (ctor), Is.SameAs (ctor));
      Assert.That (_provider.GetEmittableMethod (method), Is.SameAs (method));
    }
    
    [Test]
    public void GetEmittableXXX_NoMapping ()
    {
      Assert.That (() => _provider.GetEmittableType (_mutableType), Throws.Exception);
      Assert.That (() => _provider.GetEmittableType (_mutableGenericParameter), Throws.Exception);
      Assert.That (() => _provider.GetEmittableField (_mutableField), Throws.Exception);
      Assert.That (() => _provider.GetEmittableConstructor (_mutableConstructor), Throws.Exception);
      Assert.That (() => _provider.GetEmittableMethod (_mutableMethod), Throws.Exception);
    }

    [Test]
    public void GetEmittableType_TypeInstantiation ()
    {
      _provider.AddMapping (_mutableType, _emittableType);
      var instantiation = typeof (List<>).MakeTypePipeGenericType (_mutableType);
      Assert.That (instantiation, Is.TypeOf<TypeInstantiation>());

      var result = _provider.GetEmittableType (instantiation);

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (result.GetGenericTypeDefinition(), Is.SameAs (typeof (List<>)));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (new[] { _emittableType }));
    }

    [Test]
    public void GetEmittableType_TypeInstantiation_Recursive ()
    {
      _provider.AddMapping (_mutableType, _emittableType);
      var instantiation = typeof (List<>).MakeTypePipeGenericType (typeof (Func<>).MakeTypePipeGenericType (_mutableType));

      var result = _provider.GetEmittableType (instantiation);

      var emittableGenericArgument = result.GetGenericArguments().Single().GetGenericArguments().Single();
      Assert.That (emittableGenericArgument, Is.SameAs (_emittableType));
    }
    
    [Test]
    public void GetEmittableType_DelegateTypePlaceholder ()
    {
      var mutableReturnType = MutableTypeObjectMother.Create();
      var emittableReturnType = ReflectionObjectMother.GetSomeOtherType();
      _provider.AddMapping (mutableReturnType, emittableReturnType);
      _provider.AddMapping (_mutableType, _emittableType);
      var delegateTypePlaceholder = new DelegateTypePlaceholder (mutableReturnType, new[] { _mutableType });

      var fakeResult = ReflectionObjectMother.GetSomeDelegateType();
      _delegateProviderMock.Expect (mock => mock.GetDelegateType (mutableReturnType, new[] { _mutableType })).Return (fakeResult);

      var result = _provider.GetEmittableType (delegateTypePlaceholder);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetEmittableType_ByRefType ()
    {
      _provider.AddMapping (_mutableType, _emittableType);
      var byRefType = ByRefTypeObjectMother.Create (_mutableType);

      var result = _provider.GetEmittableType (byRefType);

      Assert.That (result, Is.SameAs (_emittableType.MakeByRefType()));
    }

    [Test]
    public void GetEmittableType_VectorType ()
    {
      _provider.AddMapping (_mutableType, _emittableType);
      var vectorType = VectorTypeObjectMother.Create (_mutableType);

      var result = _provider.GetEmittableType (vectorType);

      Assert.That (result, Is.SameAs (_emittableType.MakeArrayType()));
    }

    [Test]
    public void GetEmittableType_MultiDimensionalArrayType ()
    {
      _provider.AddMapping (_mutableType, _emittableType);
      var rank = 7;
      var multiDimensionalArrayType = MultiDimensionalArrayTypeObjectMother.Create (_mutableType, rank);

      var result = _provider.GetEmittableType (multiDimensionalArrayType);

      Assert.That (result, Is.SameAs (_emittableType.MakeArrayType (rank)));
    }

    [Test]
    public void GetEmittableMethod_MethodInstantiation_OnRuntimeType ()
    {
      var genericMethodDefinition = typeof (Enumerable).GetMethod ("Empty");
      var instantiation = genericMethodDefinition.MakeTypePipeGenericMethod (_mutableType);
      Assert.That (instantiation, Is.TypeOf<MethodInstantiation>());
      _provider.AddMapping (_mutableType, _emittableType);

      var result = _provider.GetEmittableMethod (instantiation);

      Assert.That (result, Is.Not.InstanceOf<CustomMethodInfo>());
      var emittableGenericArgument = result.GetGenericArguments().Single();
      Assert.That (emittableGenericArgument, Is.SameAs (_emittableType));
    }

    [Test]
    public void GetEmittableMethod_MethodInstantiation_OnCustomType ()
    {
      var genericMethodDefinition = MutableMethodInfoObjectMother.Create (genericParameters: new[] { MutableGenericParameterObjectMother.Create() });
      var emittableGenericMethodDefinition = typeof (Enumerable).GetMethod ("Empty");
      _provider.AddMapping (genericMethodDefinition, emittableGenericMethodDefinition);

      var instantiation = genericMethodDefinition.MakeTypePipeGenericMethod (_emittableType);
      Assert.That (instantiation, Is.TypeOf<MethodInstantiation>());

      var result = _provider.GetEmittableMethod (instantiation);

      Assert.That (result, Is.Not.InstanceOf<CustomMethodInfo>());
      var emittableGenericArgument = result.GetGenericArguments().Single();
      Assert.That (emittableGenericArgument, Is.SameAs (_emittableType));
    }

    [Test]
    public void GetEmittableXXX_MembersFromTypeInstantiation ()
    {
      var instantiation = typeof (List<>).MakeTypePipeGenericType (_mutableType);
      var field = instantiation.GetField ("_size", BindingFlags.NonPublic | BindingFlags.Instance);
      var ctor = instantiation.GetConstructor (Type.EmptyTypes);
      var method = instantiation.GetMethod ("Add");

      var emittableType = ReflectionEmitObjectMother.GetSomeTypeBuilder();
      _provider.AddMapping (_mutableType, emittableType);

      var emittableField = _provider.GetEmittableField (field);
      var emittableCtor = _provider.GetEmittableConstructor (ctor);
      var emittableMethod = _provider.GetEmittableMethod (method);

      CheckEmittableMemberInstantiation (emittableField, emittableType);
      CheckEmittableMemberInstantiation (emittableCtor, emittableType);
      CheckEmittableMemberInstantiation (emittableMethod, emittableType);
    }

    private static void CheckEmittableMemberInstantiation (MemberInfo emittableMember, Type emittableTypeArgument)
    {
      Assertion.IsNotNull (emittableMember.DeclaringType);
      var proxyTypeGenericArgument = emittableMember.DeclaringType.GetGenericArguments().Single();
      Assert.That (proxyTypeGenericArgument, Is.SameAs (emittableTypeArgument));
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
  }
}