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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderStub;
    private ISubclassProxyBuilderFactory _handlerFactoryStub;

    private TypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder> ();
      _subclassProxyNameProviderStub = MockRepository.GenerateStub<ISubclassProxyNameProvider>();
      _handlerFactoryStub = MockRepository.GenerateStub<ISubclassProxyBuilderFactory>();

      _typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, _handlerFactoryStub);
    }

    // TODO 4745: Change to use MockRepository.Ordered()
    [Test]
    public void ApplyModifications ()
    {
      var mutableTypePartialMock = MutableTypeObjectMother.CreatePartialMock ();
      
      var typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      var fakeResultType = ReflectionObjectMother.GetSomeType ();
      bool acceptCalled = false;
      bool disposeCalled = false;

      _subclassProxyNameProviderStub.Stub (stub => stub.GetSubclassProxyName (mutableTypePartialMock)).Return ("foofoo");
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", attributes, mutableTypePartialMock.UnderlyingSystemType))
          .Return (typeBuilderMock);

      var builderMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilder> ();
      _handlerFactoryStub
          .Stub (stub =>
            stub.CreateBuilder (
                Arg.Is (mutableTypePartialMock),
                Arg.Is (typeBuilderMock),
                Arg<ReflectionToBuilderMap>.Is.Anything,
                Arg<IILGeneratorFactory>.Is.Anything))
          .Return (builderMock)
          .WhenCalled (mi =>
          {
            var reflectionToBuilderMap = (ReflectionToBuilderMap) mi.Arguments[2];
            Assert.That (reflectionToBuilderMap.GetBuilder (mutableTypePartialMock), Is.SameAs (typeBuilderMock));
            var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) mi.Arguments[3];
            Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory> ());
          });
      mutableTypePartialMock
          .Expect (mock => mock.Accept (builderMock))
          .WhenCalled (mi => acceptCalled = true);
      builderMock
          .Expect (mock => mock.Build())
          .WhenCalled (mi =>
          {
            Assert.That (acceptCalled, Is.True);
            disposeCalled = true;
          });
      typeBuilderMock
          .Expect (mock => mock.CreateType ())
          .Return (fakeResultType)
          .WhenCalled (mi => Assert.True (acceptCalled && disposeCalled));

      var result = _typeModifier.ApplyModifications (mutableTypePartialMock);

      _moduleBuilderMock.VerifyAllExpectations ();
      typeBuilderMock.VerifyAllExpectations ();
      mutableTypePartialMock.VerifyAllExpectations();
      builderMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResultType));
    }
  }
}