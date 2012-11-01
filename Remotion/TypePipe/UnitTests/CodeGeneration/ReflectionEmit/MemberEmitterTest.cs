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
using System.Reflection.Emit;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MemberEmitterTest
  {
    private IExpressionPreparer _expressionPreparerMock;
    private IILGeneratorFactory _ilGeneratorFactoryStub;

    private MemberEmitter _emitter;

    private MutableType _mutableType; 
    private ITypeBuilder _typeBuilderMock;
    private IEmittableOperandProvider _emittableOperandProviderMock;
    private DeferredActionManager _postDeclarationsManager;

    private MemberEmitterContext _context;

    private Expression _fakeBody;

    [SetUp]
    public void SetUp ()
    {
      _expressionPreparerMock = MockRepository.GenerateStrictMock<IExpressionPreparer>();
      _ilGeneratorFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory>();

      _emitter = new MemberEmitter (_expressionPreparerMock, _ilGeneratorFactoryStub);

      _mutableType = MutableTypeObjectMother.CreateForExistingType();
      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();
      _postDeclarationsManager = new DeferredActionManager();

      _context = MemberEmitterContextObjectMother.GetSomeContext (
          _mutableType,
          _typeBuilderMock,
          emittableOperandProvider: _emittableOperandProviderMock,
          postDeclarationsActionManager: _postDeclarationsManager);

      _fakeBody = ExpressionTreeObjectMother.GetSomeExpression();
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

      _emitter.AddField (_context, addedField);

      _typeBuilderMock.VerifyAllExpectations ();
      fieldBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddField_WithCustomAttribute ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new CustomAttribute (""));
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((CustomAttribute attr) => attr.Property);
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((CustomAttribute attr) => attr.Field);
      var constructorArguments = new object[] { "ctorArgs" };
      var declaration = new CustomAttributeDeclaration (
          constructor,
          constructorArguments,
          new NamedArgumentDeclaration (property, 7),
          new NamedArgumentDeclaration (field, "test"));
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

      _emitter.AddField (_context, addedField);

      fieldBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddConstructor ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForNewWithParameters (
          new ParameterDeclaration (typeof (string), "p1", ParameterAttributes.In),
          new ParameterDeclaration (typeof (int).MakeByRefType(), "p2", ParameterAttributes.Out));
      var expectedAttributes = ctor.Attributes;
      var expectedParameterTypes = new[] { typeof (string), typeof (int).MakeByRefType () };

      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineConstructor (expectedAttributes, CallingConventions.HasThis, expectedParameterTypes))
          .Return (constructorBuilderMock);
      constructorBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, ctor));

      constructorBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.In, "p1"));
      constructorBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "p2"));

      Assert.That (_postDeclarationsManager.Actions, Is.Empty);

      _emitter.AddConstructor (_context, ctor);

      _typeBuilderMock.VerifyAllExpectations ();
      constructorBuilderMock.VerifyAllExpectations ();

      Assert.That (_postDeclarationsManager.Actions.Count(), Is.EqualTo (1));
      CheckBodyBuildAction (_postDeclarationsManager.Actions.Single(), constructorBuilderMock, ctor);
    }

    [Test]
    public void AddMethod ()
    {
      var addedMethod = MutableMethodInfoObjectMother.CreateForNew (
          MutableTypeObjectMother.CreateForExistingType (typeof (DomainType)),
          "AddedMethod",
          MethodAttributes.Virtual,
          typeof (string),
          new[]
          {
              new ParameterDeclaration (typeof (int), "i", ParameterAttributes.None),
              new ParameterDeclaration (typeof (double).MakeByRefType(), "d", ParameterAttributes.Out)
          });

      var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType dt) => dt.OverridableMethod (7, out Dev<double>.Dummy));
      addedMethod.AddExplicitBaseDefinition (overriddenMethod);

      var expectedAttributes = MethodAttributes.HideBySig;
      var expectedParameterTypes = new[] { typeof (int), typeof (double).MakeByRefType() };

      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder>();
      _typeBuilderMock
          .Expect (mock => mock.DefineMethod ("AddedMethod", expectedAttributes, typeof (string), expectedParameterTypes))
          .Return (methodBuilderMock);
      methodBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, addedMethod));

      methodBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.None, "i"));
      methodBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "d"));

      Assert.That (_postDeclarationsManager.Actions, Is.Empty);

      _emitter.AddMethod (_context, addedMethod, expectedAttributes);

      _typeBuilderMock.VerifyAllExpectations ();
      methodBuilderMock.VerifyAllExpectations ();
      var actions = _postDeclarationsManager.Actions.ToArray();
      Assert.That (actions, Has.Length.EqualTo (2));

      CheckBodyBuildAction (actions[0], methodBuilderMock, addedMethod);
      CheckExplicitOverrideAction (actions[1], addedMethod, overriddenMethod);
    }

    [Test]
    public void AddMethod_Abstract ()
    {
      var addedMethod = MutableMethodInfoObjectMother.CreateForNew (
          MutableTypeObjectMother.CreateForExistingType (typeof (DomainType)),
          "AddedAbstractMethod",
          MethodAttributes.Abstract,
          typeof (int),
          ParameterDeclaration.EmptyParameters);

      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineMethod ("AddedAbstractMethod", MethodAttributes.Abstract, typeof (int), Type.EmptyTypes))
          .Return (methodBuilderMock);
      methodBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, addedMethod));

      _emitter.AddMethod (_context, addedMethod, MethodAttributes.Abstract);

      _typeBuilderMock.VerifyAllExpectations();
      methodBuilderMock.VerifyAllExpectations();
      var actions = _postDeclarationsManager.Actions.ToArray();
      Assert.That (actions, Has.Length.EqualTo (1));

      // Executing the action has no side effect (strict mocks; empty override build action).
      _emittableOperandProviderMock.Stub (mock => mock.GetEmittableMethod (addedMethod));
      actions[0].Invoke();
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

    private void CheckBodyBuildAction (Action testedAction, IMethodBaseBuilder methodBuilderMock, IMutableMethodBase mutableMethodBase)
    {
      methodBuilderMock.BackToRecord();
      _expressionPreparerMock.Expect (mock => mock.PrepareBody (_context, mutableMethodBase.Body)).Return (_fakeBody);
      methodBuilderMock
          .Expect (mock => mock.SetBody (Arg<LambdaExpression>.Is.Anything, Arg.Is (_ilGeneratorFactoryStub), Arg.Is (_context.DebugInfoGenerator)))
          .WhenCalled (
              mi =>
              {
                var lambdaExpression = (LambdaExpression) mi.Arguments[0];
                Assert.That (lambdaExpression.Body, Is.SameAs (_fakeBody));
                Assert.That (lambdaExpression.Parameters, Is.EqualTo (mutableMethodBase.ParameterExpressions));
              });
      methodBuilderMock.Replay();

      testedAction();

      _emittableOperandProviderMock.VerifyAllExpectations();
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