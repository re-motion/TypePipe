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
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class ILGeneratorDecoratorFactoryTest
  {
    private IILGeneratorFactory _innerFactoryStub;
    private ILGeneratorDecoratorFactory _decoratorFactory;

    [SetUp]
    public void SetUp ()
    {
      _innerFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory>();
      _decoratorFactory = new ILGeneratorDecoratorFactory (_innerFactoryStub);
    }

    [Test]
    public void CreateAdaptedILGenerator ()
    {
      var realILGenerator = (ILGenerator) PrivateInvoke.CreateInstanceNonPublicCtor (typeof (ILGenerator), 12);

      var fakeInnerResult = MockRepository.GenerateStub<IILGenerator> ();
      _innerFactoryStub.Stub (stub => stub.CreateAdaptedILGenerator (realILGenerator)).Return (fakeInnerResult);

      var result = _decoratorFactory.CreateAdaptedILGenerator (realILGenerator);

      Assert.That (result, Is.TypeOf<ILGeneratorDecorator>().With.Property ("InnerILGenerator").SameAs (fakeInnerResult));
    }
  }
}