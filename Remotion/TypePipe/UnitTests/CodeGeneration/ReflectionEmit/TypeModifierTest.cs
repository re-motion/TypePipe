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
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using System.Linq;
using Rhino.Mocks.Interfaces;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    [Test]
    public void ApplyModifications ()
    {
      var handlerFactoryMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilderFactory> ();

      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (originalType: typeof (ClassWithCtors));
      var mutableTypePartialMock = MockRepository.GeneratePartialMock<MutableType> (
          descriptor,
          new MemberSignatureEqualityComparer (),
          new BindingFlagsEvaluator ());

      var existingConstructors = mutableTypePartialMock.ExistingConstructors.ToArray ();
      Assert.That (existingConstructors, Has.Length.EqualTo (2));
      var modifiedConstructor = existingConstructors[0];
      MutableConstructorInfoTestHelper.ModifyConstructor (modifiedConstructor);

      var unmodifiedConstructor = existingConstructors[1];

      var builderMock = MockRepository.GenerateStrictMock<ISubclassProxyBuilder>();
      handlerFactoryMock.Expect (mock => mock.CreateBuilder (mutableTypePartialMock)).Return (builderMock);

      bool buildCalled = false;

      builderMock.Expect (mock => mock.AddConstructor (unmodifiedConstructor)).WhenCalled (mi => Assert.That (buildCalled, Is.False));
      mutableTypePartialMock.Expect (mock => mock.Accept (builderMock)).WhenCalled (mi => Assert.That (buildCalled, Is.False));

      var fakeType = ReflectionObjectMother.GetSomeType();
      builderMock.Expect (mock => mock.Build()).Return (fakeType).WhenCalled (mi => buildCalled = true);

      var typeModifier = new TypeModifier (handlerFactoryMock);

      var result = typeModifier.ApplyModifications (mutableTypePartialMock);

      handlerFactoryMock.VerifyAllExpectations();
      builderMock.VerifyAllExpectations();
      mutableTypePartialMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeType));
    }
  }

  public class ClassWithCtors
  {
    public ClassWithCtors () { }
    public ClassWithCtors (int i)
    {
      Dev.Null = i;
    }
  }
}

//// Copyright (c) rubicon IT GmbH, www.rubicon.eu
////
//// See the NOTICE file distributed with this work for additional information
//// regarding copyright ownership.  rubicon licenses this file to you under 
//// the Apache License, Version 2.0 (the "License"); you may not use this 
//// file except in compliance with the License.  You may obtain a copy of the 
//// License at
////
////   http://www.apache.org/licenses/LICENSE-2.0
////
//// Unless required by applicable law or agreed to in writing, software 
//// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
//// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
//// License for the specific language governing permissions and limitations
//// under the License.
//// 
//using System;
//using System.Reflection;
//using NUnit.Framework;
//using Remotion.Development.UnitTesting;
//using Remotion.Reflection;
//using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
//using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
//using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
//using Remotion.TypePipe.MutableReflection;
//using Remotion.TypePipe.UnitTests.MutableReflection;
//using Rhino.Mocks;
//using System.Linq;
//using Rhino.Mocks.Interfaces;

//namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
//{
//  [TestFixture]
//  public class TypeModifierTest
//  {
//    private MockRepository _mockRepository;

//    private IModuleBuilder _moduleBuilderMock;
//    private ISubclassProxyNameProvider _subclassProxyNameProviderStub;
//    private ISubclassProxyBuilderFactory _handlerFactoryStub;

//    private TypeModifier _typeModifier;

//    [SetUp]
//    public void SetUp ()
//    {
//      _mockRepository = new MockRepository();
//      _moduleBuilderMock = _mockRepository.StrictMock<IModuleBuilder> ();
//      _subclassProxyNameProviderStub = _mockRepository.Stub<ISubclassProxyNameProvider> ();
//      _handlerFactoryStub = _mockRepository.Stub<ISubclassProxyBuilderFactory> ();

//      _typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, _handlerFactoryStub);
//    }

//    [Test]
//    public void ApplyModifications ()
//    {
//      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (originalType: typeof (ClassWithCtors));
//      var mutableTypePartialMock = _mockRepository.StrictMock<MutableType> (
//          descriptor, 
//          new MemberSignatureEqualityComparer(), 
//          new BindingFlagsEvaluator());

//      mutableTypePartialMock.Stub (stub => stub.GetHashCode ()).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
//      mutableTypePartialMock.Stub (stub => stub.Equals (Arg<)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);

//      var existingConstructors = mutableTypePartialMock.ExistingConstructors.ToArray();
//      Assert.That (existingConstructors, Has.Length.EqualTo (2));
//      var modifiedConstructor = existingConstructors[0];
//      MutableConstructorInfoTestHelper.ModifyConstructor (modifiedConstructor);

//      var unmodifiedConstructor = existingConstructors[1];

//      var typeBuilderMock = _mockRepository.StrictMock<ITypeBuilder> ();
//      var fakeResultType = ReflectionObjectMother.GetSomeType ();

//        _subclassProxyNameProviderStub.Stub (stub => stub.GetSubclassProxyName (mutableTypePartialMock)).Return ("foofoo");
//      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
//      _moduleBuilderMock
//          .Expect (mock => mock.DefineType ("foofoo", attributes, mutableTypePartialMock.UnderlyingSystemType))
//          .Return (typeBuilderMock);

//      var builderMock = _mockRepository.StrictMock<ISubclassProxyBuilder> ();
//      _handlerFactoryStub
//          .Stub (stub =>
//            stub.CreateBuilder (
//                Arg.Is (mutableTypePartialMock),
//                Arg.Is (typeBuilderMock),
//                Arg<ReflectionToBuilderMap>.Is.Anything,
//                Arg<IILGeneratorFactory>.Is.Anything))
//          .Return (builderMock)
//          .WhenCalled (mi =>
//          {
//            var reflectionToBuilderMap = (ReflectionToBuilderMap) mi.Arguments[2];
//            Assert.That (reflectionToBuilderMap.GetBuilder (mutableTypePartialMock), Is.SameAs (typeBuilderMock));
//            var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) mi.Arguments[3];
//            Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory> ());
//          });

//      using (_mockRepository.Ordered ())
//      {
//        builderMock.Expect (mock => mock.AddConstructor (unmodifiedConstructor));
//        mutableTypePartialMock.Expect (mock => mock.Accept (builderMock));
//        builderMock.Expect (mock => mock.Build());
//        typeBuilderMock.Expect (mock => mock.CreateType()).Return (fakeResultType);
//      }

//      _mockRepository.ReplayAll();

//      var result = _typeModifier.ApplyModifications (mutableTypePartialMock);

//      _mockRepository.VerifyAll ();

//      Assert.That (result, Is.SameAs (fakeResultType));
//    }

//    public class ClassWithCtors
//    {
//      public ClassWithCtors () { }
//      public ClassWithCtors (int i)
//      {
//        Dev.Null = i;
//      }
//    }
//  }
//}