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
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.LambdaCompilation;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.Implementation.ReflectionEmit
{
  [TestFixture]
  public class MemberEmitterFactoryTest
  {
    private MemberEmitterFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new MemberEmitterFactory();
    }

    [Test]
    public void CreateMemberEmitter ()
    {
      var emittableOperandProvider = new EmittableOperandProvider (new DelegateProvider());

      var result = _factory.CreateMemberEmitter (emittableOperandProvider);

      Assert.That (result, Is.TypeOf<MemberEmitter>());
      var memberEmitter = (MemberEmitter) result;

      Assert.That (memberEmitter.ILGeneratorFactory, Is.TypeOf<ILGeneratorDecoratorFactory>());
      var ilGeneratorDecoratorFactory = (ILGeneratorDecoratorFactory) memberEmitter.ILGeneratorFactory;
      Assert.That (ilGeneratorDecoratorFactory.InnerFactory, Is.TypeOf<OffsetTrackingILGeneratorFactory>());
      Assert.That (ilGeneratorDecoratorFactory.EmittableOperandProvider, Is.SameAs (emittableOperandProvider));

      Assert.That (memberEmitter.ExpressionPreparer, Is.TypeOf<ExpressionPreparer>());
      var expressionPreparer = (ExpressionPreparer) memberEmitter.ExpressionPreparer;
      Assert.That (expressionPreparer.MethodTrampolineProvider, Is.TypeOf<MethodTrampolineProvider>());
      var methodTrampolineProvider = (MethodTrampolineProvider) expressionPreparer.MethodTrampolineProvider;

      Assert.That (methodTrampolineProvider.MemberEmitter, Is.TypeOf<MemberEmitter>());
      var nonPreparingMemberEmitter = (MemberEmitter) methodTrampolineProvider.MemberEmitter;
      Assert.That (nonPreparingMemberEmitter.ILGeneratorFactory, Is.SameAs (memberEmitter.ILGeneratorFactory));
      Assert.That (nonPreparingMemberEmitter.ExpressionPreparer, Is.TypeOf<NullExpressionPreparer>());
    }
  }
}