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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
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

    private MutableType _mutableType;
    private MemberEmitterContext _context;

    [SetUp]
    public void SetUp ()
    {
      _memberEmitterMock = MockRepository.GenerateStrictMock<IMemberEmitter>();

      _provider = new MethodTrampolineProvider (_memberEmitterMock);

      _mutableType = MutableTypeObjectMother.CreateForExisting (typeof (DomainType));
      _context = new MemberEmitterContext (
          _mutableType,
          MockRepository.GenerateStub<ITypeBuilder>(),
          null,
          MockRepository.GenerateStub<IEmittableOperandProvider>(),
          MockRepository.GenerateStub<IMethodTrampolineProvider>(),
          new DeferredActionManager());
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

      Assert.That (mutableMethod.Body, Is.InstanceOf<MethodCallExpression>());
      var methodCallExpression = ((MethodCallExpression) mutableMethod.Body);
      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression>().And.Property ("Type").SameAs (_mutableType));
      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>().And.Property ("AdaptedMethodInfo").SameAs (method));
      Assert.That (methodCallExpression.Arguments, Is.EqualTo (mutableMethod.ParameterExpressions));
    }

    [Test]
    public void GetNonVirtualCallTrampoline_ReuseGeneratedMethod ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var method2 = typeof (DomainType).GetMethod ("ToString");
      Assert.That (method1.ReflectedType, Is.Not.SameAs (method2.ReflectedType));
      _memberEmitterMock.Expect (mock => mock.AddMethod (null, null, 0)).IgnoreArguments().Repeat.Once();

      var result1 = _provider.GetNonVirtualCallTrampoline (_context, method1);
      var result2 = _provider.GetNonVirtualCallTrampoline (_context, method2);

      Assert.That (result1, Is.SameAs (result2));
    }

    class DomainType
    {
      public virtual string Abc (out int i, double d) { i = 7; return ""; }
    }
  }
}