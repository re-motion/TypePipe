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
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class TypeModificationHandlerFactoryTest
  {
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private IExpressionPreparer _expressionPreparer;

    private TypeModificationHandlerFactory _handlerFactory;

    [SetUp]
    public void SetUp ()
    {
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator> ();
      _expressionPreparer = MockRepository.GenerateStub<IExpressionPreparer>();

      _handlerFactory = new TypeModificationHandlerFactory (_expressionPreparer, _debugInfoGeneratorStub);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_handlerFactory.ExpressionPreparer, Is.SameAs (_expressionPreparer));
      Assert.That (_handlerFactory.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var handlerFactory = new TypeModificationHandlerFactory (_expressionPreparer, null);
      Assert.That (handlerFactory.DebugInfoGenerator, Is.Null);
    }

    [Test]
    [Ignore ("4745")]
    public void CreateHandler ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var typeBuilderStub = MockRepository.GenerateStub<ITypeBuilder>();
      var reflectionToBuilderMap = new ReflectionToBuilderMap();
      var ilGeneratorFactory = MockRepository.GenerateStub<IILGeneratorFactory>();

      var result = _handlerFactory.CreateHandler (mutableType, typeBuilderStub, reflectionToBuilderMap, ilGeneratorFactory);

      Assert.That (result, Is.TypeOf<TypeModificationHandler>());
      var handler = (TypeModificationHandler) result;

      Assert.That (handler.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorStub));
      Assert.That (handler.ExpressionPreparer, Is.SameAs (_expressionPreparer));

      Assert.That (handler.SubclassProxyBuilder, Is.SameAs (typeBuilderStub));
      Assert.That (handler.ReflectionToBuilderMap, Is.SameAs (reflectionToBuilderMap));
      Assert.That (handler.ILGeneratorFactory, Is.SameAs (ilGeneratorFactory));
    }

    [Test]
    [Ignore("4745")]
    public void CreateHandler_UnmodifiedAndModifiedExistingCtors ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExistingType (typeof (ClassWithCtors));
      var ctor = mutableType.GetConstructor (Type.EmptyTypes);
      MutableConstructorInfoTestHelper.ModifyConstructor (mutableType.GetMutableConstructor (ctor));

      var typeBuilderStub = MockRepository.GenerateStub<ITypeBuilder> ();
      var reflectionToBuilderMap = new ReflectionToBuilderMap ();
      var ilGeneratorFactory = MockRepository.GenerateStub<IILGeneratorFactory> ();

      var result = _handlerFactory.CreateHandler (mutableType, typeBuilderStub, reflectionToBuilderMap, ilGeneratorFactory);

      // Test that //  modificationHandler.HandleUnmodifiedConstructor (clonedCtor);
      // was called for public ClassWithCtors (int i) { }
    }

    class ClassWithCtors
    {
      public ClassWithCtors () { }
      public ClassWithCtors (int i) { }
    }
  }
}