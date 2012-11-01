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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MethodTrampolineProviderTest
  {
    private ITypeBuilder _typeBuilderMock;

    private MethodTrampolineProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();

      _provider = new MethodTrampolineProvider (_typeBuilderMock);
    }

    [Test]
    public void GetNonVirtualCallTrampoline ()
    {
      int i;
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Abc (out i, 0.7));
      var trampolineName = "Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.MethodTrampolineProviderTest+DomainType.Abc_NonVirtualCallTrampoline";
      var parameterTypes = new[] { typeof (int).MakeByRefType(), typeof (double) }; 
      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder>();
      _typeBuilderMock
          .Expect (mock => mock.DefineMethod (trampolineName, MethodAttributes.Private, typeof (string), parameterTypes))
          .Return (methodBuilderMock);

      methodBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.Out, "i"));
      methodBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.None, "d"));
      var fakeMethodBuilder = ReflectionEmitObjectMother.GetSomeMethodBuilder();
      methodBuilderMock.Expect (mock => mock.GetInternalMethodBuilder()).Return (fakeMethodBuilder);

      var result = _provider.GetNonVirtualCallTrampoline (method);

      _typeBuilderMock.VerifyAllExpectations();
      methodBuilderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeMethodBuilder));
    }

    [Test]
    public void GetNonVirtualCallTrampoline_ReuseGeneratedMethod ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var method2 = typeof (DomainType).GetMethod ("ToString");
      Assert.That (method1.ReflectedType, Is.Not.SameAs (method2.ReflectedType));

      var methodBuilderStub = MockRepository.GenerateStub<IMethodBuilder>();
      _typeBuilderMock.Expect (mock => mock.DefineMethod ("", 0, null, null)).IgnoreArguments().Return (methodBuilderStub);

      var result1 = _provider.GetNonVirtualCallTrampoline (method1);
      var result2 = _provider.GetNonVirtualCallTrampoline (method2);

      _typeBuilderMock.VerifyAllExpectations();
      Assert.That (result1, Is.SameAs (result2));
    }

    class DomainType
    {
      public virtual string Abc (out int i, double d) { i = 7; return ""; }
    }
  }
}