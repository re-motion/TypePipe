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
  public class TypeModificationHandlerTest
  {
    private IExpressionPreparer _expressionPreparerMock;
    private ITypeBuilder _subclassProxyBuilderMock;
    private IILGeneratorFactory _ilGeneratorFactoryStub;
    private DebugInfoGenerator _debugInfoGeneratorStub;
    private ReflectionToBuilderMap _reflectionToBuilderMap;

    private TypeModificationHandler _handler;

    [SetUp]
    public void SetUp ()
    {
      _expressionPreparerMock = MockRepository.GenerateStrictMock<IExpressionPreparer>();
      _subclassProxyBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();
      _ilGeneratorFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory>();
      _debugInfoGeneratorStub = MockRepository.GenerateStub<DebugInfoGenerator>();
      _reflectionToBuilderMap = new ReflectionToBuilderMap();

      _handler = new TypeModificationHandler (
          _subclassProxyBuilderMock, _expressionPreparerMock, _reflectionToBuilderMap, _ilGeneratorFactoryStub, _debugInfoGeneratorStub);
    }

    [Test]
    public void Initialization_NullDebugInfoGenerator ()
    {
      var handler = new TypeModificationHandler (
          _subclassProxyBuilderMock, _expressionPreparerMock, _reflectionToBuilderMap, _ilGeneratorFactoryStub, null);
      Assert.That (handler.DebugInfoGenerator, Is.Null);
    }

    [Test]
    public void HandleAddedInterface ()
    {
      var addedInterface = ReflectionObjectMother.GetSomeInterfaceType();
      _subclassProxyBuilderMock.Expect (mock => mock.AddInterfaceImplementation (addedInterface));

      _handler.HandleAddedInterface (addedInterface);

      _subclassProxyBuilderMock.VerifyAllExpectations();
    }

    [Test]
    public void HandleAddedField ()
    {
      var addedField = MutableFieldInfoObjectMother.Create();
      var fieldBuilderStub = MockRepository.GenerateStub<IFieldBuilder>();

      _subclassProxyBuilderMock
          .Expect(mock => mock.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes))
          .Return (fieldBuilderStub);

      _handler.HandleAddedField (addedField);

      _subclassProxyBuilderMock.VerifyAllExpectations ();
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
      _subclassProxyBuilderMock
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
      
      _handler.HandleAddedField (addedField);

      fieldBuilderMock.VerifyAllExpectations();
    }

    [Test]
    public void HandleAddedConstructor_CallsAddConstructorToSubclassProxy ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForNew();
      CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (_handler.HandleAddedConstructor, ctor);
    }

    [Test]
    public void HandleAddedConstructor_Throws ()
    {
      var message = "The supplied constructor must be a new constructor.\r\nParameter name: addedConstructor";
      CheckThrowsForInvalidArguments (_handler.HandleAddedConstructor, message, isNewConstructor: false, isModified: true);
      CheckThrowsForInvalidArguments (_handler.HandleAddedConstructor, message, isNewConstructor: false, isModified: false);
    }

    [Test]
    public void HandleModifiedConstructor_CallsAddConstructorToSubclassProxy ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForExisting ();
      MutableConstructorInfoTestHelper.ModifyConstructor (ctor);
      CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (_handler.HandleModifiedConstructor, ctor);
    }

    [Test]
    public void HandleModifiedConstructor_Throws ()
    {
      var message = "The supplied constructor must be a modified existing constructor.\r\nParameter name: modifiedConstructor";
      CheckThrowsForInvalidArguments (_handler.HandleModifiedConstructor, message, isNewConstructor: true, isModified: true);
      CheckThrowsForInvalidArguments (_handler.HandleModifiedConstructor, message, isNewConstructor: true, isModified: false);
      CheckThrowsForInvalidArguments (_handler.HandleModifiedConstructor, message, isNewConstructor: false, isModified: false);
    }

    [Test]
    public void HandleUnmodifiedConstructor_CallsAddConstructorToSubclassProxy ()
    {
      var ctor = MutableConstructorInfoObjectMother.CreateForExisting ();
      CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (_handler.HandleUnmodifiedConstructor, ctor);
    }

    [Test]
    public void HandleUnmodifiedConstructor_Throws ()
    {
      var message = "The supplied constructor must be an unmodified existing constructor.\r\nParameter name: existingConstructor";
      CheckThrowsForInvalidArguments (_handler.HandleUnmodifiedConstructor, message, isNewConstructor: true, isModified: true);
      CheckThrowsForInvalidArguments (_handler.HandleUnmodifiedConstructor, message, isNewConstructor: true, isModified: false);
      CheckThrowsForInvalidArguments (_handler.HandleUnmodifiedConstructor, message, isNewConstructor: false, isModified: true);
    }

    [Test]
    public void AddConstructorToSubclassProxy_DefinesConstructor ()
    {
      var parameterDeclarations =
          new[]
          {
              ParameterDeclarationObjectMother.Create (typeof (string), "p1", ParameterAttributes.In),
              ParameterDeclarationObjectMother.Create (typeof (int).MakeByRefType(), "p2", ParameterAttributes.Out)
          };
      var descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: parameterDeclarations);
      var mutableConstructor = MutableConstructorInfoObjectMother.Create (underlyingConstructorInfoDescriptor: descriptor);

      var expectedAttributes = mutableConstructor.Attributes;
      var expectedCallingConvention = mutableConstructor.CallingConvention;
      var expectedParameterTypes = new[] { typeof (string), typeof (int).MakeByRefType() };
      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      _subclassProxyBuilderMock
          .Expect (mock => mock.DefineConstructor (expectedAttributes, expectedCallingConvention, expectedParameterTypes))
          .Return (constructorBuilderMock);

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression();
      _expressionPreparerMock.Expect (mock => mock.PrepareConstructorBody (mutableConstructor)).Return (fakeBody);

      constructorBuilderMock.Expect (mock => mock.DefineParameter (1, ParameterAttributes.In, "p1"));
      constructorBuilderMock.Expect (mock => mock.DefineParameter (2, ParameterAttributes.Out, "p2"));

      CallAddConstructorToSubclassProxy (_handler, mutableConstructor);

      _subclassProxyBuilderMock.VerifyAllExpectations();
      _expressionPreparerMock.VerifyAllExpectations();
      constructorBuilderMock.VerifyAllExpectations();

      Assert.That (_reflectionToBuilderMap.GetBuilder (mutableConstructor), Is.SameAs (constructorBuilderMock));
    }

    [Test]
    public void AddConstructorToSubclassProxy_RegistersDisposeAction ()
    {
      var descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: Enumerable.Empty<ParameterDeclaration>());
      var mutableConstructor = MutableConstructorInfoObjectMother.Create (underlyingConstructorInfoDescriptor: descriptor);

      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder> ();
      _subclassProxyBuilderMock
          .Stub (mock => mock.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything))
          .Return (constructorBuilderMock);

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression ();
      _expressionPreparerMock.Expect (mock => mock.PrepareConstructorBody (mutableConstructor)).Return (fakeBody);

      Assert.That (GetDisposeActions (_handler), Has.Count.EqualTo (0));

      CallAddConstructorToSubclassProxy (_handler, mutableConstructor);
      
      var disposeActions = GetDisposeActions (_handler);
      Assert.That (disposeActions, Has.Count.EqualTo (1));
      var action = disposeActions.Single ();

      constructorBuilderMock
          .Expect (mock => mock.SetBody (Arg<LambdaExpression>.Is.Anything, Arg.Is (_ilGeneratorFactoryStub), Arg.Is (_debugInfoGeneratorStub)))
          .WhenCalled (
              mi =>
              {
                var lambdaExpression = (LambdaExpression) mi.Arguments[0];
                Assert.That (lambdaExpression.Body, Is.SameAs (fakeBody));
                Assert.That (lambdaExpression.Parameters, Is.EqualTo (mutableConstructor.ParameterExpressions));
              });

      action ();

      constructorBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddConstructorToSubclassProxy_WithByRefParameters ()
    {
      var byRefType = typeof (object).MakeByRefType();
      
      var parameterDeclarations = new[] { ParameterDeclarationObjectMother.Create (byRefType) };
      var descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: parameterDeclarations);
      var mutableConstructor = MutableConstructorInfoObjectMother.Create (underlyingConstructorInfoDescriptor: descriptor);

      var constructorBuilderStub = MockRepository.GenerateStub<IConstructorBuilder> ();
      _subclassProxyBuilderMock
          .Expect (
              mock => mock.DefineConstructor (
                  Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.List.Equal (new[] { byRefType })))
          .Return (constructorBuilderStub);

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression ();
      _expressionPreparerMock.Stub (stub => stub.PrepareConstructorBody (mutableConstructor)).Return (fakeBody);

      CallAddConstructorToSubclassProxy (_handler, mutableConstructor);

      _subclassProxyBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void Dispose ()
    {
      int disposeActionCallCount = 0;
      AddDisposeAction (_handler, () => ++disposeActionCallCount);

      // First call
      _handler.Dispose ();

      Assert.That (disposeActionCallCount, Is.EqualTo (1));

      // Second call
      _handler.Dispose ();

      Assert.That (disposeActionCallCount, Is.EqualTo (1));
    }

    private void AddDisposeAction (TypeModificationHandler handler, Action action)
    {
      var disposeActions = GetDisposeActions(handler);
      disposeActions.Add (action);
    }

    private List<Action> GetDisposeActions (TypeModificationHandler handler)
    {
      return (List<Action>) PrivateInvoke.GetNonPublicField (handler, "_disposeActions");
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

    private void CallAddConstructorToSubclassProxy (TypeModificationHandler handler, MutableConstructorInfo mutableConstructor)
    {
      PrivateInvoke.InvokeNonPublicMethod (handler, "AddConstructorToSubclassProxy", mutableConstructor);
    }

    private void CheckThatMethodIsDelegatedToAddConstructorToSubclassProxy (
        Action<MutableConstructorInfo> methodInvocation, MutableConstructorInfo mutableConstructor)
    {
      var constructorBuilderStub = MockRepository.GenerateStub<IConstructorBuilder> ();
      _subclassProxyBuilderMock
          .Expect (mock => mock.DefineConstructor (Arg<MethodAttributes>.Is.Anything, Arg<CallingConventions>.Is.Anything, Arg<Type[]>.Is.Anything))
          .Return (constructorBuilderStub);
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      _expressionPreparerMock
          .Stub (mock => mock.PrepareConstructorBody (mutableConstructor))
          .Return (fakeBody);

      methodInvocation (mutableConstructor);

      _subclassProxyBuilderMock.VerifyAllExpectations();
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