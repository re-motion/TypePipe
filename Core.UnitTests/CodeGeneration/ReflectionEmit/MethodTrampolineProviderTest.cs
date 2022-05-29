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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MethodTrampolineProviderTest
  {
    private Mock<IMemberEmitter> _memberEmitterMock;

    private MethodTrampolineProvider _provider;

    private MutableType _mutableType;
    private CodeGenerationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterMock = new Mock<IMemberEmitter> (MockBehavior.Strict);

      _provider = new MethodTrampolineProvider (_memberEmitterMock.Object);

      _mutableType = MutableTypeObjectMother.Create (baseType: typeof (DomainType));
      _context = CodeGenerationContextObjectMother.GetSomeContext (_mutableType);
    }

    [Test]
    public void GetNonVirtualCallTrampoline ()
    {
      int i;
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Abc (out i, 0.7));
      MutableMethodInfo mutableMethod = null;
      _memberEmitterMock
          .Setup (mock => mock.AddMethod (_context, It.IsAny<MutableMethodInfo>()))
          .Callback ((CodeGenerationContext context, MutableMethodInfo methodArg) => mutableMethod = methodArg)
          .Verifiable();

      var result = _provider.GetNonVirtualCallTrampoline (_context, method);

      _memberEmitterMock.Verify();
      Assert.That (result, Is.SameAs (mutableMethod));
      Assert.That (_mutableType.AddedMethods, Has.Member (result));

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
      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression>().And.Property ("Type").SameAs (_mutableType));
      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>().And.Property ("AdaptedMethod").SameAs (method));
      Assert.That (methodCallExpression.Arguments, Is.EqualTo (mutableMethod.ParameterExpressions));
    }

    [Test]
    public void GetNonVirtualCallTrampoline_ReuseGeneratedMethod ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var method2 = typeof (DomainType).GetMethod ("ToString");
      Assert.That (method1, Is.Not.SameAs (method2));

      _memberEmitterMock.Setup (mock => mock.AddMethod (It.IsAny<CodeGenerationContext>(), It.IsAny<MutableMethodInfo>())).Verifiable();

      var result1 = _provider.GetNonVirtualCallTrampoline (_context, method1);
      var result2 = _provider.GetNonVirtualCallTrampoline (_context, method2);
      _memberEmitterMock.Verify (mock => mock.AddMethod (It.IsAny<CodeGenerationContext>(), It.IsAny<MutableMethodInfo>()), Times.Once());
      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetNonVirtualCallTrampoline_CreatesDifferentTrampolineForOverloads ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Def());
      var method2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Def (7));

      _memberEmitterMock.Setup (mock => mock.AddMethod (It.IsAny<CodeGenerationContext>(), It.IsAny<MutableMethodInfo>())).Verifiable();

      var result1 = _provider.GetNonVirtualCallTrampoline (_context, method1);
      var result2 = _provider.GetNonVirtualCallTrampoline (_context, method2);

      _memberEmitterMock.Verify (mock => mock.AddMethod (It.IsAny<CodeGenerationContext>(), It.IsAny<MutableMethodInfo>()), Times.Exactly (2));
      Assert.That (result1, Is.Not.SameAs (result2));
      Assert.That (result1.GetParameters(), Is.Empty);
      Assert.That (result2.GetParameters(), Has.Length.EqualTo (1));
    }

    public class DomainType
    {
      public string Abc (out int i, double d) { i = 7; Dev.Null = d; return ""; }

      public void Def () { }
      public void Def (int i) { Dev.Null = i; }
    }
  }
}