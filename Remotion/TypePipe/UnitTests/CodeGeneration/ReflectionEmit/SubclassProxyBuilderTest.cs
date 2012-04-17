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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using System.Collections.Generic;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class SubclassProxyBuilderTest
  {
    private IExpressionPreparer _expressionPreparerMock;
    private ITypeBuilder _typeBuilderMock;
    private IILGeneratorFactory _ilGeneratorFactoryStub;
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private ReflectionToBuilderMap _reflectionToBuilderMap;

    private SubclassProxyBuilder _builder;

    private Expression _fakeBody;

    [SetUp]
    public void SetUp ()
    {
      _expressionPreparerMock = MockRepository.GenerateStrictMock<IExpressionPreparer>();
      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();
      _ilGeneratorFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory>();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator>();
      _reflectionToBuilderMap = new ReflectionToBuilderMap();

      _builder = new SubclassProxyBuilder (
          _typeBuilderMock, _expressionPreparerMock, _reflectionToBuilderMap, _ilGeneratorFactoryStub, _debugInfoGeneratorStub);

      _fakeBody = ExpressionTreeObjectMother.GetSomeExpression();
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var handler = new SubclassProxyBuilder (
          _typeBuilderMock, _expressionPreparerMock, _reflectionToBuilderMap, _ilGeneratorFactoryStub, null);
      Assert.That (handler.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void HandleAddedInterface ()
    {
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();
      _typeBuilderMock.Expect (mock => mock.AddInterfaceImplementation (addedInterface));

      _builder.HandleAddedInterface (addedInterface);

      _typeBuilderMock.VerifyAllExpectations();
    }

    [Test]
    public void HandleAddedField ()
    {
      var addedField = MutableFieldInfoObjectMother.Create();
      var fieldBuilderStub = MockRepository.GenerateStub<IFieldBuilder>();

      _typeBuilderMock
          .Expect(mock => mock.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes))
          .Return (fieldBuilderStub);

      _builder.HandleAddedField (addedField);

      _typeBuilderMock.VerifyAllExpectations ();
      Assert.That (_reflectionToBuilderMap.GetBuilder (addedField), Is.SameAs (fieldBuilderStub));
    }

    [Test]
    public void HandleAddedField_WithCustomAttribute ()
    {
      var constructor = ReflectionObjectMother.GetConstructor (() => new CustomAttribute(""));
      var property = ReflectionObjectMother.GetProperty ((CustomAttribute attr) => attr.Property);
      var field = ReflectionObjectMother.GetField ((CustomAttribute attr) => attr.Field);
      var constructorArguments = new object[] { "ctorArgs" };
      var declaration = new CustomAttributeDeclaration (
          constructor,
          constructorArguments,
          new NamedAttributeArgumentDeclaration (property, 7),
          new NamedAttributeArgumentDeclaration (field, "test"));
      var addedField = MutableFieldInfoObjectMother.Create ();
      addedField.AddCustomAttribute (declaration);

      var fieldBuilderMock = MockRepository.GenerateMock<IFieldBuilder>();
      _typeBuilderMock
          .Stub (stub => stub.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes))
          .Return (fieldBuilderMock);
      fieldBuilderMock
          .Expect (mock => mock.SetCustomAttribute (Arg<CustomAttributeBuilder>.Is.Anything))
          .WhenCalled (mi => CheckCustomAttributeBuilder (
              (CustomAttributeBuilder) mi.Arguments[0], 
              constructor,
              constructorArguments,
              new[] { property },
              new object[]  { 7 },
              new[] { field },
              new[] { "test" }));
      
      _builder.HandleAddedField (addedField);

      fieldBuilderMock.VerifyAllExpectations();
    }

    [Test]
    public void HandleAddedConstructor_CallsAddConstructor ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForNew();
      CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (_builder.HandleAddedConstructor, ctor);
    }

    [Test]
    public void HandleAddedConstructor_Throws ()
    {
      var message = "The supplied constructor must be a new constructor.\r\nParameter name: addedConstructor";
      CheckThrowsForInvalidArguments (_builder.HandleAddedConstructor, message, isNewConstructor: false, isModified: true);
      CheckThrowsForInvalidArguments (_builder.HandleAddedConstructor, message, isNewConstructor: false, isModified: false);
    }

    [Test]
    public void HandleModifiedConstructor_CallsAddConstructor ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForExistingAndModify();
      CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (_builder.HandleModifiedConstructor, ctor);
    }

    [Test]
    public void HandleModifiedConstructor_Throws ()
    {
      var message = "The supplied constructor must be a modified existing constructor.\r\nParameter name: modifiedConstructor";
      CheckThrowsForInvalidArguments (_builder.HandleModifiedConstructor, message, isNewConstructor: true, isModified: true);
      CheckThrowsForInvalidArguments (_builder.HandleModifiedConstructor, message, isNewConstructor: true, isModified: false);
      CheckThrowsForInvalidArguments (_builder.HandleModifiedConstructor, message, isNewConstructor: false, isModified: false);
    }

    [Test]
    public void AddConstructor_DefinesConstructor ()
    {
      var mutableConstructor = MutableConstructorInfoObjectMother.CreateForNewWithParameters (
          ParameterDeclarationObjectMother.Create (typeof (string), "p1", ParameterAttributes.In),
          ParameterDeclarationObjectMother.Create (typeof (int).MakeByRefType (), "p2", ParameterAttributes.Out));

      var expectedAttributes = mutableConstructor.Attributes;
      var expectedParameterTypes = new[] { typeof (string), typeof (int).MakeByRefType() };
      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineConstructor (expectedAttributes, CallingConventions.HasThis, expectedParameterTypes))
          .Return (constructorBuilderMock);

      _expressionPreparerMock
          .Expect (mock => mock.PrepareConstructorBody (mutableConstructor))
          .Return (_fakeBody)
          .WhenCalled (mi => Assert.That (_reflectionToBuilderMap.GetBuilder (mutableConstructor), Is.SameAs (constructorBuilderMock)));

      constructorBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.In, "p1"));
      constructorBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "p2"));

      _builder.AddConstructor (mutableConstructor);

      _typeBuilderMock.VerifyAllExpectations();
      _expressionPreparerMock.VerifyAllExpectations();
      constructorBuilderMock.VerifyAllExpectations();
    }

    [Test]
    public void AddConstructor_RegistersBuildAction ()
    {
      var mutableConstructor = MutableConstructorInfoObjectMother.CreateForNewWithParameters();
      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      _typeBuilderMock
          .Stub (mock => mock.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything))
          .Return (constructorBuilderMock);
      _expressionPreparerMock.Stub (mock => mock.PrepareConstructorBody (mutableConstructor)).Return (_fakeBody);

      Assert.That (GetBuildActions (_builder), Has.Count.EqualTo (0));

      _builder.AddConstructor (mutableConstructor);

      CheckSingleSetBodyBuildAction (constructorBuilderMock, mutableConstructor.ParameterExpressions);
    }

    [Test]
    public void HandleAddedMethod_DefinesMethod ()
    {
      var mutableMethod = MutableMethodInfoObjectMother.CreateForNew (
          parameterDeclarations: new[]
                                 {
                                     ParameterDeclarationObjectMother.Create (typeof (string), "p1", ParameterAttributes.In),
                                     ParameterDeclarationObjectMother.Create (typeof (int).MakeByRefType(), "p2", ParameterAttributes.Out)
                                 });

      var expectedName = mutableMethod.Name;
      var expectedAttributes = mutableMethod.Attributes;
      var expectedReturnType = mutableMethod.ReturnType;
      var expectedParameterTypes = new[] { typeof (string), typeof (int).MakeByRefType () };
      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineMethod (expectedName, expectedAttributes, expectedReturnType, expectedParameterTypes))
          .Return (methodBuilderMock);

      _expressionPreparerMock
          .Expect (mock => mock.PrepareMethodBody (mutableMethod))
          .Return (_fakeBody)
          .WhenCalled (mi => Assert.That (_reflectionToBuilderMap.GetBuilder (mutableMethod), Is.SameAs (methodBuilderMock)));

      methodBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.In, "p1"));
      methodBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "p2"));

      _builder.HandleAddedMethod (mutableMethod);

      _typeBuilderMock.VerifyAllExpectations ();
      _expressionPreparerMock.VerifyAllExpectations ();
      methodBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddMethod_RegistersBuildAction ()
    {
      var mutableMethod = MutableMethodInfoObjectMother.Create (parameterDeclarations: ParameterDeclaration.EmptyParameters);
      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder> ();
      _typeBuilderMock
          .Stub (mock => mock.DefineMethod (Arg<string>.Is.Anything, Arg<MethodAttributes>.Is.Anything, Arg<Type>.Is.Anything, Arg<Type[]>.Is.Anything))
          .Return (methodBuilderMock);
      _expressionPreparerMock.Stub (mock => mock.PrepareMethodBody (mutableMethod)).Return (_fakeBody);

      Assert.That (GetBuildActions (_builder), Has.Count.EqualTo (0));

      _builder.HandleAddedMethod (mutableMethod);

      CheckSingleSetBodyBuildAction (methodBuilderMock, mutableMethod.ParameterExpressions);
    }

    [Test]
    public void Build ()
    {
      bool buildActionCalled = false;
      AddBuildAction (_builder, () => buildActionCalled = true);

      var fakeType = ReflectionObjectMother.GetSomeType();
      _typeBuilderMock
          .Expect (mock => mock.CreateType())
          .Return (fakeType)
          .WhenCalled (mi => Assert.That (buildActionCalled, Is.True));

      var result = _builder.Build ();

      _typeBuilderMock.VerifyAllExpectations();
      Assert.That (buildActionCalled, Is.True);
      Assert.That (result, Is.SameAs (fakeType));
    }

    [Test]
    public void Build_Twice ()
    {
      _typeBuilderMock.Stub (mock => mock.CreateType ());

      _builder.Build ();
      Assert.That (() => _builder.Build (), Throws.InvalidOperationException.With.Message.EqualTo ("Build can only be called once."));
    }

    [Test]
    public void Build_DisablesOperations ()
    {
      _typeBuilderMock.Stub (mock => mock.CreateType ());
      _builder.Build ();

      CheckThrowsForOperationAfterBuild (() => _builder.HandleAddedInterface (ReflectionObjectMother.GetSomeInterfaceType ()));
      CheckThrowsForOperationAfterBuild (() => _builder.HandleAddedField (MutableFieldInfoObjectMother.Create ()));
      CheckThrowsForOperationAfterBuild (() => _builder.HandleAddedConstructor (MutableConstructorInfoObjectMother.CreateForNew()));
      CheckThrowsForOperationAfterBuild (() => _builder.HandleAddedMethod (MutableMethodInfoObjectMother.CreateForNew()));

      CheckThrowsForOperationAfterBuild (() => _builder.HandleModifiedConstructor (MutableConstructorInfoObjectMother.CreateForExistingAndModify ()));
      CheckThrowsForOperationAfterBuild (() => _builder.AddConstructor (MutableConstructorInfoObjectMother.Create()));
    }

    private void CheckThrowsForOperationAfterBuild (Action action)
    {
      Assert.That (() => action(), Throws.InvalidOperationException.With.Message.EqualTo ("Subclass proxy has already been built."));
    }

    private void AddBuildAction (SubclassProxyBuilder handler, Action action)
    {
      var buildActions = GetBuildActions(handler);
      buildActions.Add (action);
    }

    private List<Action> GetBuildActions (SubclassProxyBuilder handler)
    {
      return (List<Action>) PrivateInvoke.GetNonPublicField (handler, "_buildActions");
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

    private void CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (
        Action<MutableConstructorInfo> methodInvocation, MutableConstructorInfo mutableConstructor)
    {
      var constructorBuilderStub = MockRepository.GenerateStub<IConstructorBuilder> ();
      _typeBuilderMock
          .Expect (mock => mock.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything))
          .Return (constructorBuilderStub);
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      _expressionPreparerMock
          .Stub (mock => mock.PrepareConstructorBody (mutableConstructor))
          .Return (fakeBody);

      methodInvocation (mutableConstructor);

      _typeBuilderMock.VerifyAllExpectations();
    }

    private void CheckThrowsForInvalidArguments (
        Action<MutableConstructorInfo> methodInvocation, string exceptionMessage, bool isNewConstructor, bool isModified)
    {
      var constructor = isNewConstructor ? MutableConstructorInfoObjectMother.CreateForNew() : MutableConstructorInfoObjectMother.CreateForExisting();
      if (isModified)
        MutableConstructorInfoTestHelper.ModifyConstructor (constructor);
      Assert.That (constructor.IsNewConstructor, Is.EqualTo (isNewConstructor));
      Assert.That (constructor.IsModified, Is.EqualTo (isModified));

      Assert.That (() => methodInvocation (constructor), Throws.ArgumentException.With.Message.EqualTo (exceptionMessage));
    }

    private void CheckSingleSetBodyBuildAction (IMethodBaseBuilder methodBuilderMock, IEnumerable<ParameterExpression> parameterExpressions)
    {
      // To check the build action registers by AddConstructor, we need to invoke it and observe its effects.
      methodBuilderMock
          .Expect (mock => mock.SetBody (Arg<LambdaExpression>.Is.Anything, Arg.Is (_ilGeneratorFactoryStub), Arg.Is (_debugInfoGeneratorStub)))
          .WhenCalled (
              mi =>
              {
                var lambdaExpression = (LambdaExpression) mi.Arguments[0];
                Assert.That (lambdaExpression.Body, Is.SameAs (_fakeBody));
                Assert.That (lambdaExpression.Parameters, Is.EqualTo (parameterExpressions));
              });

      var buildActions = GetBuildActions (_builder);
      Assert.That (buildActions, Has.Count.EqualTo (1));
      var action = buildActions.Single ();
      action ();

      methodBuilderMock.VerifyAllExpectations ();
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
  }
}