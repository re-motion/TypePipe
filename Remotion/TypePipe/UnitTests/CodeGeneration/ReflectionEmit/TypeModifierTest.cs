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
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderStub;
    private DebugInfoGenerator _debugInfoGeneratorStub;

    private TypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder> ();
      _subclassProxyNameProviderStub = MockRepository.GenerateStub<ISubclassProxyNameProvider>();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator>();

      _typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, _debugInfoGeneratorStub);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, null);
      Assert.That (typeModifier.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void ApplyModifications ()
    {
      var fakeUnderlyingSystemType = ReflectionObjectMother.GetSomeType ();
      var underlyingStrategyStub = CreateUnderlyingTypeStrategyStub (fakeUnderlyingSystemType);
      
      var mutableTypeMock = MutableTypeObjectMother.CreatePartialMock (underlyingTypeStrategy: underlyingStrategyStub);
      
      var typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      var fakeResultType = ReflectionObjectMother.GetSomeType ();
      bool acceptCalled = false;

      _subclassProxyNameProviderStub.Stub (stub => stub.GetSubclassProxyName (mutableTypeMock)).Return ("foofoo");
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, fakeUnderlyingSystemType))
          .Return (typeBuilderMock);
      mutableTypeMock
          .Expect (mock => mock.Accept (Arg<ITypeModificationHandler>.Is.Anything))
          .WhenCalled (mi =>
          {
            acceptCalled = true;
            Assert.That (mi.Arguments[0], Is.TypeOf<TypeModificationHandler>());
            var handler = (TypeModificationHandler) mi.Arguments[0];
            Assert.That (handler.SubclassProxyBuilder, Is.SameAs (typeBuilderMock));
            Assert.That (handler.ExpressionPreparer, Is.TypeOf<ExpandingExpressionPreparer> ());
            Assert.That (handler.ReflectionToBuilderMap.GetBuilder (mutableTypeMock), Is.SameAs (typeBuilderMock));
            Assert.That (handler.ILGeneratorFactory, Is.TypeOf <ILGeneratorDecoratorFactory>());
            var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) handler.ILGeneratorFactory;
            Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory>());
            Assert.That (handler.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
          });
      typeBuilderMock
          .Expect (mock => mock.CreateType ()).Return (fakeResultType)
          .WhenCalled (mi => Assert.That (acceptCalled, Is.True));

      var result = _typeModifier.ApplyModifications (mutableTypeMock);

      _moduleBuilderMock.VerifyAllExpectations ();
      typeBuilderMock.VerifyAllExpectations ();
      mutableTypeMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    [Test]
    public void ApplyModifications_ClonesConstructorsReturnedByMutableType ()
    {
      var constructor = ReflectionObjectMother.GetConstructor (() => new ClassWithConstructors ("s"));
      var mutableType = CreateMutableTypeWithConstructors (constructor);

      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder>();
      _moduleBuilderMock
          .Stub (mock => mock.DefineType (Arg<string>.Is.Anything, Arg<TypeAttributes>.Is.Anything, Arg<Type>.Is.Anything))
          .Return (typeBuilderMock);

      var constructorBuilderStub = MockRepository.GenerateStub<IConstructorBuilder>();
      typeBuilderMock
          .Expect (mock => mock.DefineConstructor (
              Arg<MethodAttributes>.Is.Anything,
              Arg<CallingConventions>.Is.Anything,
              Arg<Type[]>.List.Equal (new[] { typeof (string) })))
          .Return (constructorBuilderStub);

      _typeModifier.ApplyModifications (mutableType);

      typeBuilderMock.VerifyAllExpectations ();
    }

    private MutableType CreateMutableTypeWithConstructors (params ConstructorInfo[] constructors)
    {
      IUnderlyingTypeStrategy underlyingTypeStrategy = CreateUnderlyingTypeStrategyStub(typeof (ClassWithConstructors), constructors: constructors);

      return MutableTypeObjectMother.Create (underlyingTypeStrategy: underlyingTypeStrategy);
    }

    private IUnderlyingTypeStrategy CreateUnderlyingTypeStrategyStub (
        Type underlyingSystemType,
        FieldInfo[] fields = null,
        ConstructorInfo[] constructors = null)
    {
      var underlyingTypeStrategy = MockRepository.GenerateStub<IUnderlyingTypeStrategy> ();

      underlyingTypeStrategy.Stub (stub => stub.GetUnderlyingSystemType ()).Return (underlyingSystemType);
      underlyingTypeStrategy.Stub (stub => stub.GetInterfaces()).Return (Type.EmptyTypes);
      underlyingTypeStrategy.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (fields ?? new FieldInfo[0]);
      underlyingTypeStrategy.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (constructors ?? new ConstructorInfo[0]);

      return underlyingTypeStrategy;
    }

    public class ClassWithConstructors
    {
      public ClassWithConstructors (string s)
      {
        Dev.Null = s;
      }
    }
  }
}