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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class SubclassProxyBuilderFactoryTest
  {
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private IExpressionPreparer _expressionPreparer;

    private SubclassProxyBuilderFactory _builderFactory;

    private ReflectionToBuilderMap _reflectionToBuilderMap;
    private IILGeneratorFactory _ilGeneratorFactory;

    [SetUp]
    public void SetUp ()
    {
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator> ();
      _expressionPreparer = MockRepository.GenerateStub<IExpressionPreparer>();

      _builderFactory = new SubclassProxyBuilderFactory (_expressionPreparer, _debugInfoGeneratorStub);

      _reflectionToBuilderMap = new ReflectionToBuilderMap ();
      _ilGeneratorFactory = MockRepository.GenerateStub<IILGeneratorFactory> ();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_builderFactory.ExpressionPreparer, Is.SameAs (_expressionPreparer));
      Assert.That (_builderFactory.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var handlerFactory = new SubclassProxyBuilderFactory (_expressionPreparer, null);
      Assert.That (handlerFactory.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void CreateBuilder ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var typeBuilderStub = MockRepository.GenerateStub<ITypeBuilder>();

      typeBuilderStub.Stub (
          stub => stub.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything));
      // TODO 4745: Remove when body generation has been moved from HandleUnmodifiedConstructor to handler.Build
      _expressionPreparer
          .Stub (stub => stub.PrepareConstructorBody (Arg<MutableConstructorInfo>.Is.Anything))
          .Return (ExpressionTreeObjectMother.GetSomeExpression (typeof (void)));

      var result = _builderFactory.CreateBuilder (mutableType, typeBuilderStub, _reflectionToBuilderMap, _ilGeneratorFactory);

      Assert.That (result, Is.TypeOf<SubclassProxyBuilder>());
      var handler = (SubclassProxyBuilder) result;

      Assert.That (handler.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
      Assert.That (handler.ExpressionPreparer, Is.SameAs (_expressionPreparer));

      Assert.That (handler.TypeBuilder, Is.SameAs (typeBuilderStub));
      Assert.That (handler.ReflectionToBuilderMap, Is.SameAs (_reflectionToBuilderMap));
      Assert.That (handler.ILGeneratorFactory, Is.SameAs (_ilGeneratorFactory));
    }

    [Test]
    public void CreateBuilder_UnmodifiedAndModifiedExistingCtors ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (ClassWithCtors));
      var modifiedCtor = ReflectionObjectMother.GetConstructor (() => new ClassWithCtors());
      MutableConstructorInfoTestHelper.ModifyConstructor (mutableType.GetMutableConstructor (modifiedCtor));
      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder> ();

      typeBuilderMock.Stub (
          stub => stub.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything));
      // TODO 4745: Remove when body generation has been moved from HandleUnmodifiedConstructor to handler.Build
      _expressionPreparer
          .Stub (stub => stub.PrepareConstructorBody (Arg<MutableConstructorInfo>.Is.Anything))
          .Return (ExpressionTreeObjectMother.GetSomeExpression (typeof (void)));
      
      _builderFactory.CreateBuilder (mutableType, typeBuilderMock, _reflectionToBuilderMap, _ilGeneratorFactory);

      typeBuilderMock.AssertWasCalled (
          mock => mock.DefineConstructor (
              Arg<MethodAttributes>.Is.Anything,
              Arg<CallingConventions>.Is.Anything,
              Arg<Type[]>.List.Equal (new []{typeof(int)})));

      typeBuilderMock.AssertWasNotCalled (
          mock => mock.DefineConstructor (
              Arg<MethodAttributes>.Is.Anything,
              Arg<CallingConventions>.Is.Anything,
              Arg<Type[]>.List.Equal (Type.EmptyTypes)));
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
}