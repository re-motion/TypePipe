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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MemberEmitterTest
  {
    private IExpressionPreparer _expressionPreparerMock;
    private ITypeBuilder _typeBuilderMock;
    private IILGeneratorFactory _ilGeneratorFactoryStub;
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private IEmittableOperandProvider _emittableOperandProviderMock;
    private DeferredActionManager _postDeclarationsManager;

    private MemberEmitter _emitter;

    private Expression _fakeBody;

    [SetUp]
    public void SetUp ()
    {
      _expressionPreparerMock = MockRepository.GenerateStrictMock<IExpressionPreparer> ();
      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder> ();
      _ilGeneratorFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory> ();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator> ();
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider> ();
      _postDeclarationsManager = new DeferredActionManager();

      _emitter = new MemberEmitter (_expressionPreparerMock, _ilGeneratorFactoryStub);

      _fakeBody = ExpressionTreeObjectMother.GetSomeExpression ();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_emitter.ExpressionPreparer, Is.SameAs (_expressionPreparerMock));
      Assert.That (_emitter.ILGeneratorFactory, Is.SameAs (_ilGeneratorFactoryStub));
    }

    [Test]
    public void AddField ()
    {
      var addedField = MutableFieldInfoObjectMother.Create ();
      var fieldBuilderMock = MockRepository.GenerateStrictMock<IFieldBuilder> ();

      _typeBuilderMock
          .Expect (mock => mock.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes))
          .Return (fieldBuilderMock);
      fieldBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, addedField));

      _emitter.AddField (_typeBuilderMock, _emittableOperandProviderMock, addedField);

      _typeBuilderMock.VerifyAllExpectations ();
      fieldBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddField_WithCustomAttribute ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute (""));
      var property = MemberInfoFromExpressionUtility.GetProperty ((CustomAttribute attr) => attr.Property);
      var field = MemberInfoFromExpressionUtility.GetField ((CustomAttribute attr) => attr.Field);
      var constructorArguments = new object[] { "ctorArgs" };
      var declaration = new CustomAttributeDeclaration (
          constructor,
          constructorArguments,
          new NamedAttributeArgumentDeclaration (property, 7),
          new NamedAttributeArgumentDeclaration (field, "test"));
      var addedField = MutableFieldInfoObjectMother.Create ();
      addedField.AddCustomAttribute (declaration);

      var fieldBuilderMock = MockRepository.GenerateMock<IFieldBuilder> ();
      _typeBuilderMock
          .Stub (stub => stub.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes))
          .Return (fieldBuilderMock);
      fieldBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, addedField));
      fieldBuilderMock
          .Expect (mock => mock.SetCustomAttribute (Arg<CustomAttributeBuilder>.Is.Anything))
          .WhenCalled (mi => CheckCustomAttributeBuilder (
              (CustomAttributeBuilder) mi.Arguments[0],
              constructor,
              constructorArguments,
              new[] { property },
              new object[] { 7 },
              new[] { field },
              new[] { "test" }));

      _emitter.AddField (_typeBuilderMock, _emittableOperandProviderMock, addedField);

      fieldBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddConstructor ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForNewWithParameters (
          ParameterDeclarationObjectMother.Create (typeof (string), "p1", ParameterAttributes.In),
          ParameterDeclarationObjectMother.Create (typeof (int).MakeByRefType (), "p2", ParameterAttributes.Out));
      var expectedAttributes = ctor.Attributes;
      var expectedParameterTypes = new[] { typeof (string), typeof (int).MakeByRefType () };

      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineConstructor (expectedAttributes, CallingConventions.HasThis, expectedParameterTypes))
          .Return (constructorBuilderMock);
      constructorBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, ctor));
      _expressionPreparerMock
          .Expect (mock => mock.PrepareConstructorBody (ctor))
          .Return (_fakeBody);

      constructorBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.In, "p1"));
      constructorBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "p2"));

      Assert.That (_postDeclarationsManager.Actions, Is.Empty);

      _emitter.AddConstructor (_typeBuilderMock, _debugInfoGeneratorStub, _emittableOperandProviderMock, _postDeclarationsManager, ctor);

      _typeBuilderMock.VerifyAllExpectations ();
      _expressionPreparerMock.VerifyAllExpectations ();
      constructorBuilderMock.VerifyAllExpectations ();

      Assert.That (_postDeclarationsManager.Actions.Count(), Is.EqualTo (1));
      CheckBodyBuildAction (_postDeclarationsManager.Actions.Single(), constructorBuilderMock, ctor.ParameterExpressions);
    }

    [Test]
    public void AddMethod ()
    {
      var addedMethod = MutableMethodInfoObjectMother.CreateForNew (
          declaringType: MutableTypeObjectMother.CreateForExistingType (typeof (DomainType)),
          name: "AddedMethod",
          attributes: MethodAttributes.Virtual,
          returnType: typeof (string),
          parameterDeclarations: new[]
                                 {
                                     ParameterDeclarationObjectMother.Create (typeof (int), "i", ParameterAttributes.None),
                                     ParameterDeclarationObjectMother.Create (typeof (double).MakeByRefType(), "d", ParameterAttributes.Out)
                                 });

      var overriddenMethod = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((DomainType dt) => dt.OverridableMethod (7, out Dev<double>.Dummy));
      addedMethod.AddExplicitBaseDefinition (overriddenMethod);

      var expectedName = "ExplicitlySpecifiedName";
      var expectedAttributes = MethodAttributes.Virtual;
      var expectedReturnType = typeof (string);
      var expectedParameterTypes = new[] { typeof (int), typeof (double).MakeByRefType () };

      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineMethod (expectedName, expectedAttributes, expectedReturnType, expectedParameterTypes))
          .Return (methodBuilderMock);
      methodBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, addedMethod));

      _expressionPreparerMock.Expect (mock => mock.PrepareMethodBody (addedMethod)).Return (_fakeBody);

      methodBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.None, "i"));
      methodBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "d"));

      Assert.That (_postDeclarationsManager.Actions, Is.Empty);

      _emitter.AddMethod (
          _typeBuilderMock,
          _debugInfoGeneratorStub,
          _emittableOperandProviderMock,
          _postDeclarationsManager,
          addedMethod,
          "ExplicitlySpecifiedName",
          expectedAttributes);

      _typeBuilderMock.VerifyAllExpectations ();
      _expressionPreparerMock.VerifyAllExpectations ();
      methodBuilderMock.VerifyAllExpectations ();

      var actions = _postDeclarationsManager.Actions.ToArray();
      Assert.That (actions, Has.Length.EqualTo (2));

      CheckBodyBuildAction (actions[0], methodBuilderMock, addedMethod.ParameterExpressions);
      CheckExplicitOverrideAction (actions[1], addedMethod, overriddenMethod);
    }

    private void CheckCustomAttributeBuilder (
        CustomAttributeBuilder builder,
        ConstructorInfo expectedCtor,
        object[] expectedCtorArgs,
        PropertyInfo[] expectedPropertyInfos,
        object[] expectedPropertyValues,
        FieldInfo[] expectedFieldInfos,
        object[] expectedFieldValues)
    {
      var actualConstructor = (ConstructorInfo) PrivateInvoke.GetNonPublicField (builder, "m_con");
      var actualConstructorArgs = (object[]) PrivateInvoke.GetNonPublicField (builder, "m_constructorArgs");
      var actualBlob = (byte[]) PrivateInvoke.GetNonPublicField (builder, "m_blob");

      Assert.That (actualConstructor, Is.SameAs (expectedCtor));
      Assert.That (actualConstructorArgs, Is.EqualTo (expectedCtorArgs));

      var testBuilder = new CustomAttributeBuilder (
          expectedCtor, expectedCtorArgs, expectedPropertyInfos, expectedPropertyValues, expectedFieldInfos, expectedFieldValues);
      var expectedBlob = (byte[]) PrivateInvoke.GetNonPublicField (testBuilder, "m_blob");
      Assert.That (actualBlob, Is.EqualTo (expectedBlob));
    }

    private void CheckBodyBuildAction (Action testedAction, IMethodBaseBuilder methodBuilderMock, IEnumerable<ParameterExpression> parameterExpressions)
    {
      methodBuilderMock.BackToRecord ();
      methodBuilderMock
          .Expect (mock => mock.SetBody (Arg<LambdaExpression>.Is.Anything, Arg.Is (_ilGeneratorFactoryStub), Arg.Is (_debugInfoGeneratorStub)))
          .WhenCalled (
              mi =>
              {
                var lambdaExpression = (LambdaExpression) mi.Arguments[0];
                Assert.That (lambdaExpression.Body, Is.SameAs (_fakeBody));
                Assert.That (lambdaExpression.Parameters, Is.EqualTo (parameterExpressions));
              });
      methodBuilderMock.Replay ();

      testedAction ();

      methodBuilderMock.VerifyAllExpectations ();
    }

    private void CheckExplicitOverrideAction (Action testedAction, MutableMethodInfo overridingMethod, MethodInfo overriddenMethod)
    {
      _emittableOperandProviderMock.BackToRecord ();
      _typeBuilderMock.BackToRecord ();

      var emittableFakeMethod1 = ReflectionObjectMother.GetSomeMethod ();
      var emittableFakeMethod2 = ReflectionObjectMother.GetSomeMethod ();
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableMethod (overridingMethod)).Return (emittableFakeMethod1);
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableMethod (overriddenMethod)).Return (emittableFakeMethod2);

      _typeBuilderMock.Expect (mock => mock.DefineMethodOverride (emittableFakeMethod1, emittableFakeMethod2));

      _emittableOperandProviderMock.Replay ();
      _typeBuilderMock.Replay ();

      testedAction ();

      _emittableOperandProviderMock.VerifyAllExpectations ();
      _typeBuilderMock.VerifyAllExpectations ();
    }

    public class CustomAttribute : Attribute
    {
      // ReSharper disable UnassignedField.Global
      public string Field;
      // ReSharper restore UnassignedField.Global

      public CustomAttribute (string ctorArgument)
      {
        CtorArgument = ctorArgument;

        Dev.Null = CtorArgument;
        Dev.Null = Property;
        Property = 0;
      }

      public string CtorArgument { get; private set; }
      public int Property { get; set; }
    }

    public class DomainType
    {
      public virtual string Method (int i, out double d)
      {
        Dev.Null = i;
        d = Dev<double>.Null;
        return "";
      }

      public virtual string OverridableMethod (int i, out double d)
      {
        Dev.Null = i;
        d = Dev<double>.Null;
        return "";
      }
    }
  }
}