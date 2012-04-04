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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
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
      var originalType = typeof (DomainClassWithoutMembers);
      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (originalType);
      
      var mutableTypePartialMock = MutableTypeObjectMother.CreatePartialMock (underlyingTypeDescriptor: descriptor);
      
      var typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      var fakeResultType = ReflectionObjectMother.GetSomeType ();
      bool acceptCalled = false;

      _subclassProxyNameProviderStub.Stub (stub => stub.GetSubclassProxyName (mutableTypePartialMock)).Return ("foofoo");
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, originalType))
          .Return (typeBuilderMock);
      mutableTypePartialMock
          .Expect (mock => mock.Accept (Arg<ITypeModificationHandler>.Is.Anything))
          .WhenCalled (mi =>
          {
            acceptCalled = true;
            Assert.That (mi.Arguments[0], Is.TypeOf<TypeModificationHandler>());
            var handler = (TypeModificationHandler) mi.Arguments[0];
            Assert.That (handler.SubclassProxyBuilder, Is.SameAs (typeBuilderMock));
            Assert.That (handler.ExpressionPreparer, Is.TypeOf<ExpandingExpressionPreparer> ());
            Assert.That (handler.ReflectionToBuilderMap.GetBuilder (mutableTypePartialMock), Is.SameAs (typeBuilderMock));
            Assert.That (handler.ILGeneratorFactory, Is.TypeOf <ILGeneratorDecoratorFactory>());
            var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) handler.ILGeneratorFactory;
            Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory>());
            Assert.That (handler.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
          });
      typeBuilderMock.Stub (
          stub => stub.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything));
      typeBuilderMock
          .Expect (mock => mock.CreateType ()).Return (fakeResultType)
          .WhenCalled (mi => Assert.That (acceptCalled, Is.True));

      var result = _typeModifier.ApplyModifications (mutableTypePartialMock);

      _moduleBuilderMock.VerifyAllExpectations ();
      typeBuilderMock.VerifyAllExpectations ();
      mutableTypePartialMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    [Test]
    public void ApplyModifications_ClonesConstructorsReturnedByMutableType ()
    {
      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (typeof (DomainClassWithConstructor));
      var mutableType = MutableTypeObjectMother.Create (underlyingTypeDescriptor: descriptor);

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

    [Test]
    public void ApplyModifications_DoesNotCloneModifiedConstructors ()
    {
      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (typeof (DomainClassWithConstructor));
      var mutableTypePartialMock = MutableTypeObjectMother.CreatePartialMock (underlyingTypeDescriptor: descriptor);
      MutableConstructorInfoTestHelper.ModifyConstructor (mutableTypePartialMock.ExistingConstructors.Single ());

      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder> ();
      _moduleBuilderMock
          .Stub (mock => mock.DefineType (Arg<string>.Is.Anything, Arg<TypeAttributes>.Is.Anything, Arg<Type>.Is.Anything))
          .Return (typeBuilderMock);
      mutableTypePartialMock.Stub (mock => mock.Accept (Arg<ITypeModificationHandler>.Is.Anything));

      _typeModifier.ApplyModifications (mutableTypePartialMock);

      typeBuilderMock.AssertWasNotCalled (
          mock => mock.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything));
    }

    public class DomainClassWithoutMembers
    { 
    }

    public class DomainClassWithConstructor
    {
      public DomainClassWithConstructor (string s)
      {
        Dev.Null = s;
      }
    }
  }
}