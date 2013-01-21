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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MethodTrampolineProviderTest
  {
    private IMemberEmitter _memberEmitterMock;

    private MethodTrampolineProvider _provider;

    private ProxyType _proxyType;
    private CodeGenerationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterMock = MockRepository.GenerateStrictMock<IMemberEmitter>();

      _provider = new MethodTrampolineProvider (_memberEmitterMock);

      _proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));
      _context = CodeGenerationContextObjectMother.GetSomeContext (_proxyType);
    }

    [Test]
    public void GetNonVirtualCallTrampoline ()
    {
      int i;
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Abc (out i, 0.7));
      MutableMethodInfo mutableMethod = null;
      _memberEmitterMock
          .Expect (mock => mock.AddMethod (Arg.Is (_context), Arg<MutableMethodInfo>.Is.Anything, Arg.Is (MethodAttributes.Private)))
          .WhenCalled (mi => mutableMethod = (MutableMethodInfo) mi.Arguments[1]);

      var result = _provider.GetNonVirtualCallTrampoline (_context, method);

      _memberEmitterMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (mutableMethod));
      Assert.That (_proxyType.AddedMethods, Has.Member (result));

      var name = "Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.MethodTrampolineProviderTest+DomainType.Abc_NonVirtualCallTrampoline";
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (MethodAttributes.Private));
      Assert.That (result.ReturnType, Is.SameAs (typeof (string)));

      var parameters = result.GetParameters().Select (p => new { p.ParameterType, p.Name, p.Attributes });
      var expectedParameters = new[]
                               {
                                   new { ParameterType = typeof (int).MakeByRefType(), Name = "i", Attributes = ParameterAttributes.Out },
                                   new { ParameterType = typeof (double), Name = "d", Attributes = ParameterAttributes.None }
                               };
      Assert.That (parameters, Is.EqualTo (expectedParameters));

      Assert.That (result.GetBaseDefinition(), Is.SameAs (result));
      Assert.That (result.IsGenericMethod, Is.False);
      Assert.That (result.IsGenericMethodDefinition, Is.False);
      Assert.That (result.ContainsGenericParameters, Is.False);

      Assert.That (mutableMethod.Body, Is.InstanceOf<MethodCallExpression>());
      var methodCallExpression = ((MethodCallExpression) mutableMethod.Body);
      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression>().And.Property ("Type").SameAs (_proxyType));
      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>().And.Property ("AdaptedMethod").SameAs (method));
      Assert.That (methodCallExpression.Arguments, Is.EqualTo (mutableMethod.ParameterExpressions));
    }

    [Test]
    public void GetNonVirtualCallTrampoline_ReuseGeneratedMethod ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var method2 = typeof (DomainType).GetMethod ("ToString");
      Assert.That (method1, Is.Not.SameAs (method2));
      _memberEmitterMock.Expect (mock => mock.AddMethod (null, null, 0)).IgnoreArguments().Repeat.Once();

      var result1 = _provider.GetNonVirtualCallTrampoline (_context, method1);
      var result2 = _provider.GetNonVirtualCallTrampoline (_context, method2);
      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetNonVirtualCallTrampoline_CreatesDifferentTrampolineForOverloads ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Def());
      var method2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Def (7));

      _memberEmitterMock.Expect (mock => mock.AddMethod (null, null, 0)).IgnoreArguments().Repeat.Twice();

      var result1 = _provider.GetNonVirtualCallTrampoline (_context, method1);
      var result2 = _provider.GetNonVirtualCallTrampoline (_context, method2);

      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1.GetParameters(), Is.Empty);
      Assert.That (result2.GetParameters(), Has.Length.EqualTo (1));
    }

    // ReSharper disable UnusedParameter.Local
    class DomainType
    {
      public string Abc (out int i, double d) { i = 7; return ""; }

      public void Def () { }
      public void Def (int i) { }
    }
  }
}