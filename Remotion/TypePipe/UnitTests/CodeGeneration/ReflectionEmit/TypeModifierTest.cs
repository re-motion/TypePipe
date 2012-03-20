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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    private IModuleBuilder _moduleBuilderMock;
    private ISubclassProxyNameProvider _subclassProxyNameProviderStub;
    private DebugInfoGenerator _debugInfoGenerator;

    private TypeModifier _typeModifier;

    [SetUp]
    public void SetUp ()
    {
      _moduleBuilderMock = MockRepository.GenerateStrictMock<IModuleBuilder> ();
      _subclassProxyNameProviderStub = MockRepository.GenerateStub<ISubclassProxyNameProvider>();
      _debugInfoGenerator = MockRepository.GenerateStub<DebugInfoGenerator>();

      _typeModifier = new TypeModifier (_moduleBuilderMock, _subclassProxyNameProviderStub, _debugInfoGenerator);
    }

    [Test]
    public void ApplyModifications ()
    {
      var mutableTypeMock = MutableTypeObjectMother.CreateStrictMock ();

      var fakeUnderlyingSystemType = ReflectionObjectMother.GetSomeType ();
      var typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      var fakeResultType = ReflectionObjectMother.GetSomeType ();
      bool acceptCalled = false;

      _subclassProxyNameProviderStub.Stub (stub => stub.GetSubclassProxyName (mutableTypeMock)).Return ("foofoo");
      mutableTypeMock
          .Stub (mock => mock.GetConstructors())
          .Return (new ConstructorInfo[0]);
      mutableTypeMock
          .Stub (stub => stub.UnderlyingSystemType)
          .Return (fakeUnderlyingSystemType);
      _moduleBuilderMock
          .Expect (mock => mock.DefineType ("foofoo", TypeAttributes.Public | TypeAttributes.BeforeFieldInit, fakeUnderlyingSystemType))
          .Return (typeBuilderMock);
      mutableTypeMock
          .Expect (mock => mock.Accept (Arg<TypeModificationHandler>.Matches (handler => handler.SubclassProxyBuilder == typeBuilderMock)))
          .WhenCalled (mi => acceptCalled = true);
      typeBuilderMock
          .Expect (mock => mock.CreateType ()).Return (fakeResultType)
          .WhenCalled (mi => Assert.That (acceptCalled, Is.True));

      var result = _typeModifier.ApplyModifications (mutableTypeMock);

      _moduleBuilderMock.VerifyAllExpectations ();
      typeBuilderMock.VerifyAllExpectations ();
      mutableTypeMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResultType));
    }

    [Test]
    public void ApplyModifications_ClonesPublicConstructors ()
    {
      var typeBuilderMock = MockRepository.GenerateMock<ITypeBuilder> ();
      var nonDefaultConstructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();

      var constructor = ReflectionObjectMother.GetConstructor (() => new ClassWithConstructors ("string"));
      var mutableTypeStub = MutableTypeObjectMother.CreateStub ();
      mutableTypeStub
          .Stub (mock => mock.GetConstructors ())
          .Return (new[] { constructor });
      mutableTypeStub
          .Stub (stub => stub.UnderlyingSystemType)
          .Return (typeof (ClassWithConstructors));

      _moduleBuilderMock
          .Stub (mock => mock.DefineType (Arg<string>.Is.Anything, Arg<TypeAttributes>.Is.Anything, Arg<Type>.Is.Anything))
          .Return (typeBuilderMock);
      typeBuilderMock
          .Expect (mock => mock.DefineConstructor (
                  MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.Public,
                  CallingConventions.HasThis,
                  new[] { typeof (string) }))
          .Return (nonDefaultConstructorBuilderMock);

      nonDefaultConstructorBuilderMock
          .Expect (mock => mock.SetBody (Arg<LambdaExpression>.Is.Anything, Arg.Is (_debugInfoGenerator)))
          .WhenCalled (mi => CheckBaseCtorCallExpression ((LambdaExpression) mi.Arguments[0], constructor, mutableTypeStub));

      _typeModifier.ApplyModifications (mutableTypeStub);

      typeBuilderMock.VerifyAllExpectations ();
      nonDefaultConstructorBuilderMock.VerifyAllExpectations();
    }

    private void CheckBaseCtorCallExpression (LambdaExpression lambdaExpression, ConstructorInfo baseConstructor, Type expectedThisType)
    {
      var expectedParameterData = baseConstructor.GetParameters().Select (pi => new { pi.ParameterType, pi.Name });
      var actualParameterData = lambdaExpression.Parameters.Select (pe => new { ParameterType = pe.Type, pe.Name });
      Assert.That (expectedParameterData, Is.EqualTo (actualParameterData));

      Assert.That (lambdaExpression.Body, Is.AssignableTo<MethodCallExpression> ());
      Assert.That (lambdaExpression.Body.Type, Is.SameAs (typeof (void)));
      var methodCallExpression = (MethodCallExpression) lambdaExpression.Body;
      Assert.That (methodCallExpression.Method, Is.TypeOf<BaseConstructorMethodInfo>().With.Property ("ConstructorInfo").EqualTo (baseConstructor));
      Assert.That (methodCallExpression.Object, Is.TypeOf<TypeAsUnderlyingSystemTypeExpression>());
      var typeAsUnderlyingSystemTypeExpression = (TypeAsUnderlyingSystemTypeExpression) methodCallExpression.Object;
      Assert.That (typeAsUnderlyingSystemTypeExpression.InnerExpression, Is.TypeOf<ThisExpression>().With.Property ("Type").SameAs (expectedThisType));
      Assert.That (methodCallExpression.Arguments, Is.EqualTo (lambdaExpression.Parameters));
    }

    public class ClassWithConstructors
    {
     public ClassWithConstructors (string s)
      {
      }

      protected ClassWithConstructors (int i)
      {
      }

      protected internal ClassWithConstructors (double d)
      {
      }
    }
  }
}