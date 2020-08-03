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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Moq;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MemberEmitterTest
  {
    private Mock<IExpressionPreparer> _expressionPreparerMock;
    private Mock<IILGeneratorFactory> _ilGeneratorFactoryStub;

    private MemberEmitter _emitter;

    private Mock<ITypeBuilder> _typeBuilderMock;
    private Mock<IEmittableOperandProvider> _emittableOperandProviderMock;

    private CodeGenerationContext _context;

    private Expression _fakeBody;

    [SetUp]
    public void SetUp ()
    {
      _expressionPreparerMock = new Mock<IExpressionPreparer> (MockBehavior.Strict);
      _ilGeneratorFactoryStub = new Mock<IILGeneratorFactory>();

      _emitter = new MemberEmitter (_expressionPreparerMock.Object, _ilGeneratorFactoryStub.Object);

      _typeBuilderMock = new Mock<ITypeBuilder> (MockBehavior.Strict);
      _emittableOperandProviderMock = new Mock<IEmittableOperandProvider> (MockBehavior.Strict);

      _context = CodeGenerationContextObjectMother.GetSomeContext (
          typeBuilder: _typeBuilderMock.Object,
          emittableOperandProvider: _emittableOperandProviderMock.Object);

      _fakeBody = ExpressionTreeObjectMother.GetSomeExpression();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_emitter.ExpressionPreparer, Is.SameAs (_expressionPreparerMock.Object));
      Assert.That (_emitter.ILGeneratorFactory, Is.SameAs (_ilGeneratorFactoryStub.Object));
    }

    [Test]
    public void AddField ()
    {
      var field = MutableFieldInfoObjectMother.Create();
      var fieldBuilderMock = new Mock<IFieldBuilder> (MockBehavior.Strict);

      _typeBuilderMock
          .Setup (mock => mock.DefineField (field.Name, field.FieldType, field.Attributes))
          .Returns (fieldBuilderMock.Object)
          .Verifiable();
      fieldBuilderMock.Setup (mock => mock.RegisterWith (_emittableOperandProviderMock.Object, field)).Verifiable();
      SetupDefineCustomAttribute (fieldBuilderMock, field);

      _emitter.AddField (_context, field);

      _typeBuilderMock.Verify();
      fieldBuilderMock.Verify();
    }

    [Test]
    public void AddConstructor ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create (
          parameters: new[]
                      {
                          new ParameterDeclaration (typeof (string), "p1", ParameterAttributes.In),
                          new ParameterDeclaration (typeof (int).MakeByRefType(), "p2", ParameterAttributes.Out)
                      });
      var expectedAttributes = constructor.Attributes;
      var expectedParameterTypes = new[] { typeof (string), typeof (int).MakeByRefType() };
      var expectedCallingConventions = CallingConventions.Standard | CallingConventions.HasThis;

      var constructorBuilderMock = new Mock<IConstructorBuilder>();
      _typeBuilderMock
          .Setup (mock => mock.DefineConstructor (expectedAttributes, expectedCallingConventions, expectedParameterTypes))
          .Returns (constructorBuilderMock.Object)
          .Verifiable();
      constructorBuilderMock.Setup (mock => mock.RegisterWith (_emittableOperandProviderMock.Object, constructor)).Verifiable();

      SetupDefineCustomAttribute (constructorBuilderMock, constructor);
      var parameterBuilderMock = SetupDefineParameter (constructorBuilderMock, 1, "p1", ParameterAttributes.In);
      SetupDefineCustomAttribute (parameterBuilderMock, constructor.MutableParameters[0]);
      SetupDefineParameter (constructorBuilderMock, 2, "p2", ParameterAttributes.Out);

      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);

      _emitter.AddConstructor (_context, constructor);

      _typeBuilderMock.Verify();
      constructorBuilderMock.Verify();
      parameterBuilderMock.Verify();

      Assert.That (_context.PostDeclarationsActionManager.Actions.Count(), Is.EqualTo (1));
      CheckBodyBuildAction (_context.PostDeclarationsActionManager.Actions.Single(), constructorBuilderMock, constructor);
    }

    [Test]
    public void AddConstructor_Static ()
    {
      var ctor = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var constructorBuilderStub = new Mock<IConstructorBuilder>();
      var attributes = MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      _typeBuilderMock
          .Setup (mock => mock.DefineConstructor (attributes, CallingConventions.Standard, Type.EmptyTypes))
          .Returns (constructorBuilderStub.Object)
          .Verifiable();

      _emitter.AddConstructor (_context, ctor);

      _typeBuilderMock.Verify();
    }

    [Test]
    public void AddMethod ()
    {
      var method = MutableMethodInfoObjectMother.Create (
          MutableTypeObjectMother.Create (baseType: typeof (DomainType)),
          "Method",
          MethodAttributes.Virtual,
          typeof (string),
          new[]
          {
              new ParameterDeclaration (typeof (int), "i", ParameterAttributes.Reserved3),
              new ParameterDeclaration (typeof (double).MakeByRefType(), "d", ParameterAttributes.Out)
          });

      var overriddenMethod =
          NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ExplicitBaseDefinition (7, out Dev<double>.Dummy));
      method.AddExplicitBaseDefinition (overriddenMethod);

      var methodBuilderMock = new Mock<IMethodBuilder> (MockBehavior.Strict);
      _typeBuilderMock.Setup (mock => mock.DefineMethod ("Method", MethodAttributes.Virtual)).Returns (methodBuilderMock.Object).Verifiable();
      methodBuilderMock.Setup (mock => mock.RegisterWith (_emittableOperandProviderMock.Object, method)).Verifiable();

      methodBuilderMock.Setup (mock => mock.SetReturnType (typeof (string))).Verifiable();
      methodBuilderMock.Setup (mock => mock.SetParameters (new[] { typeof (int), typeof (double).MakeByRefType() })).Verifiable();

      var returnParameterBuilderMock = SetupDefineParameter (methodBuilderMock, 0, null, ParameterAttributes.None);
      SetupDefineCustomAttribute (returnParameterBuilderMock, method.MutableReturnParameter);
      var parameterBuilderMock = SetupDefineParameter (methodBuilderMock, 1, "i", ParameterAttributes.Reserved3);
      SetupDefineCustomAttribute (parameterBuilderMock, method.MutableParameters[0]);
      SetupDefineParameter (methodBuilderMock, 2, "d", ParameterAttributes.Out);
      SetupDefineCustomAttribute (methodBuilderMock, method);

      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);

      _emitter.AddMethod (_context, method);

      _typeBuilderMock.Verify();
      methodBuilderMock.Verify();
      parameterBuilderMock.Verify();

      Assert.That (_context.MethodBuilders, Has.Count.EqualTo(1));
      Assert.That (_context.MethodBuilders[method], Is.SameAs(methodBuilderMock.Object));

      var actions = _context.PostDeclarationsActionManager.Actions.ToArray();
      Assert.That (actions, Has.Length.EqualTo (2));

      CheckBodyBuildAction (actions[0], methodBuilderMock, method);
      CheckExplicitOverrideAction (actions[1], overriddenMethod, method);
    }

    [Test]
    public void AddMethod_GenericMethodDefinition ()
    {
      var baseTypeConstraint = typeof (DomainType);
      var interfaceConstraint = typeof (IDisposable);
      Assert.That (baseTypeConstraint.GetInterfaces(), Is.Not.Empty);
      Assert.That (baseTypeConstraint.GetInterfaces(), Has.No.Member (interfaceConstraint));

      var genericParameter = MutableGenericParameterObjectMother.Create (
          name: "TParam",
          genericParameterAttributes: (GenericParameterAttributes) 7,
          constraints: new[] { baseTypeConstraint, interfaceConstraint });
      var method = MutableMethodInfoObjectMother.Create (
          genericParameters: new[] { genericParameter },
          returnType: genericParameter,
          parameters: new[] { ParameterDeclarationObjectMother.Create (genericParameter, "genericParam") });

      var methodBuilderMock = new Mock<IMethodBuilder> (MockBehavior.Strict);
      _typeBuilderMock.Setup (mock => mock.DefineMethod (method.Name, method.Attributes)).Returns (methodBuilderMock.Object);
      methodBuilderMock.Setup (mock => mock.RegisterWith (_emittableOperandProviderMock.Object, method));

      var genericParameterBuilderMock = new Mock<IGenericTypeParameterBuilder> (MockBehavior.Strict);
      methodBuilderMock.Setup (mock => mock.DefineGenericParameters (new[] { "TParam" })).Returns (new[] { genericParameterBuilderMock.Object }).Verifiable();
      genericParameterBuilderMock.Setup (mock => mock.RegisterWith (_emittableOperandProviderMock.Object, genericParameter)).Verifiable();
      genericParameterBuilderMock.Setup (mock => mock.SetGenericParameterAttributes ((GenericParameterAttributes) 7)).Verifiable();
      genericParameterBuilderMock.Setup (mock => mock.SetBaseTypeConstraint (baseTypeConstraint)).Verifiable();
      genericParameterBuilderMock.Setup (mock => mock.SetInterfaceConstraints (new[] { interfaceConstraint })).Verifiable();
      SetupDefineCustomAttribute (genericParameterBuilderMock, genericParameter);

      methodBuilderMock.Setup (mock => mock.SetReturnType (genericParameter)).Verifiable();
      methodBuilderMock.Setup (mock => mock.SetParameters (new Type[] { genericParameter })).Verifiable();
      SetupDefineParameter (methodBuilderMock, 0, null, ParameterAttributes.None);
      SetupDefineParameter (methodBuilderMock, 1, "genericParam", ParameterAttributes.None);

      _emitter.AddMethod (_context, method);

      methodBuilderMock.Verify();
      genericParameterBuilderMock.Verify();
    }

    [Test]
    public void AddMethod_Abstract ()
    {
      var method = MutableMethodInfoObjectMother.Create (
          null,
          "AbstractMethod",
          MethodAttributes.Abstract,
          typeof (int),
          ParameterDeclaration.None);

      var methodBuilderMock = new Mock<IMethodBuilder> (MockBehavior.Strict);
      _typeBuilderMock.Setup (stub => stub.DefineMethod ("AbstractMethod", MethodAttributes.Abstract)).Returns (methodBuilderMock.Object);
      methodBuilderMock.Setup (stub => stub.RegisterWith (_emittableOperandProviderMock.Object, method));

      methodBuilderMock.Setup (stub => stub.SetReturnType (typeof (int)));
      methodBuilderMock.Setup (stub => stub.SetParameters (Type.EmptyTypes));
      SetupDefineParameter (methodBuilderMock, 0, parameterName: null, parameterAttributes: ParameterAttributes.None);

      _emitter.AddMethod (_context, method);

      _typeBuilderMock.Verify();
      methodBuilderMock.Verify();
      var actions = _context.PostDeclarationsActionManager.Actions.ToArray();
      Assert.That (actions, Has.Length.EqualTo (1));

      // Executing the action has no side effect (strict mocks; empty override build action).
      _emittableOperandProviderMock.Setup (mock => mock.GetEmittableMethod (method)).Returns (new Mock<MethodInfo>().Object);
      actions[0].Invoke();
    }

    [Test]
    public void AddProperty ()
    {
      var name = "Property";
      var attributes = (PropertyAttributes) 7;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var setMethodParameters = parameters.Concat (new[] { ParameterDeclarationObjectMother.Create (returnType) });
      var indexParameterTypes = parameters.Select (p => p.Type).ToArray();
      var getMethod = MutableMethodInfoObjectMother.Create (returnType: returnType, parameters: parameters);
      var setMethod = MutableMethodInfoObjectMother.Create (parameters: setMethodParameters);
      var property = MutablePropertyInfoObjectMother.Create (name: name, attributes: attributes, getMethod: getMethod, setMethod: setMethod);

      var getMethodBuilder = new Mock<IMethodBuilder>();
      var setMethodBuilder = new Mock<IMethodBuilder>();
      _context.MethodBuilders.Add (getMethod, getMethodBuilder.Object);
      _context.MethodBuilders.Add (setMethod, setMethodBuilder.Object);

      var callingConventions = CallingConventions.Standard | CallingConventions.HasThis;
      var propertyBuilderMock = new Mock<IPropertyBuilder> (MockBehavior.Strict);
      _typeBuilderMock
          .Setup (mock => mock.DefineProperty (name, attributes, callingConventions, returnType, indexParameterTypes))
          .Returns (propertyBuilderMock.Object)
          .Verifiable();
      SetupDefineCustomAttribute (propertyBuilderMock, property);
      propertyBuilderMock.Setup (mock => mock.SetGetMethod (getMethodBuilder.Object)).Verifiable();
      propertyBuilderMock.Setup (mock => mock.SetSetMethod (setMethodBuilder.Object)).Verifiable();

      _emitter.AddProperty (_context, property);

      _typeBuilderMock.Verify();
      propertyBuilderMock.Verify();
    }

    [Test]
    public void AddProperty_ReadOnly_WriteOnly ()
    {
      var staticGetMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static, returnType: typeof (int));
      var setMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (long)) });
      var readOnlyProperty = MutablePropertyInfoObjectMother.Create (getMethod: staticGetMethod);
      var writeOnlyProperty = MutablePropertyInfoObjectMother.Create (setMethod: setMethod);
      Assert.That (readOnlyProperty.MutableSetMethod, Is.Null);
      Assert.That (writeOnlyProperty.MutableGetMethod, Is.Null);

      var methodBuilder = new Mock<IMethodBuilder>();
      _context.MethodBuilders.Add (readOnlyProperty.MutableGetMethod, methodBuilder.Object);
      _context.MethodBuilders.Add (writeOnlyProperty.MutableSetMethod, methodBuilder.Object);

      var propertyBuilderMock1 = new Mock<IPropertyBuilder> (MockBehavior.Strict);
      var propertyBuilderMock2 = new Mock<IPropertyBuilder> (MockBehavior.Strict);
      _typeBuilderMock
          .Setup (
              mock => mock.DefineProperty (
                  readOnlyProperty.Name,
                  readOnlyProperty.Attributes,
                  staticGetMethod.CallingConvention,
                  typeof (int),
                  Type.EmptyTypes))
          .Returns (propertyBuilderMock1.Object)
          .Verifiable();
      _typeBuilderMock
          .Setup (
              mock => mock.DefineProperty (
                  writeOnlyProperty.Name,
                  writeOnlyProperty.Attributes,
                  setMethod.CallingConvention,
                  typeof (long),
                  Type.EmptyTypes))
          .Returns (propertyBuilderMock2.Object)
          .Verifiable();
      propertyBuilderMock1.Setup (mock => mock.SetGetMethod (methodBuilder.Object)).Verifiable();
      propertyBuilderMock2.Setup (mock => mock.SetSetMethod (methodBuilder.Object)).Verifiable();

      _emitter.AddProperty (_context, readOnlyProperty);
      _emitter.AddProperty (_context, writeOnlyProperty);

      _typeBuilderMock.Verify();
      propertyBuilderMock1.Verify();
      propertyBuilderMock2.Verify();
    }

    [Test]
    public void AddEvent ()
    {
      var name = "Event";
      var attributes = (EventAttributes) 7;
      var handlerType = typeof (Func<int, string>);
      var addMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType) });
      var removeMethod = MutableMethodInfoObjectMother.Create (parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType) });
      var raiseMethod = MutableMethodInfoObjectMother.Create (
          returnType: typeof (string), parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (int)) });
      var event_ = MutableEventInfoObjectMother.Create (
          name: name, attributes: attributes, addMethod: addMethod, removeMethod: removeMethod, raiseMethod: raiseMethod);

      var addMethodBuilder = new Mock<IMethodBuilder>();
      var removeMethodBuilder = new Mock<IMethodBuilder>();
      var raiseMethodBuilder = new Mock<IMethodBuilder>();
      _context.MethodBuilders.Add (addMethod, addMethodBuilder.Object);
      _context.MethodBuilders.Add (removeMethod, removeMethodBuilder.Object);
      _context.MethodBuilders.Add (raiseMethod, raiseMethodBuilder.Object);

      var eventBuilderMock = new Mock<IEventBuilder> (MockBehavior.Strict);
      _typeBuilderMock.Setup (mock => mock.DefineEvent (name, attributes, handlerType)).Returns (eventBuilderMock.Object).Verifiable();
      SetupDefineCustomAttribute (eventBuilderMock, event_);
      eventBuilderMock.Setup (mock => mock.SetAddOnMethod (addMethodBuilder.Object)).Verifiable();
      eventBuilderMock.Setup (mock => mock.SetRemoveOnMethod (removeMethodBuilder.Object)).Verifiable();
      eventBuilderMock.Setup (mock => mock.SetRaiseMethod (raiseMethodBuilder.Object)).Verifiable();

      _emitter.AddEvent (_context, event_);

      _typeBuilderMock.Verify();
      eventBuilderMock.Verify();
    }

    [Test]
    public void AddEvent_NoRaiseMethod ()
    {
      var event_ = MutableEventInfoObjectMother.CreateWithAccessors();
      Assert.That (event_.MutableRaiseMethod, Is.Null);

      var addMethodBuilder = new Mock<IMethodBuilder>();
      var removeMethodBuilder = new Mock<IMethodBuilder>();
      _context.MethodBuilders.Add (event_.MutableAddMethod, addMethodBuilder.Object);
      _context.MethodBuilders.Add (event_.MutableRemoveMethod, removeMethodBuilder.Object);

      var eventBuilderMock = new Mock<IEventBuilder> (MockBehavior.Strict);
      _typeBuilderMock.Setup (stub => stub.DefineEvent (event_.Name, event_.Attributes, event_.EventHandlerType)).Returns (eventBuilderMock.Object);
      eventBuilderMock.Setup (mock => mock.SetAddOnMethod (addMethodBuilder.Object)).Verifiable();
      eventBuilderMock.Setup (mock => mock.SetRemoveOnMethod (removeMethodBuilder.Object)).Verifiable();

      _emitter.AddEvent (_context, event_);

      eventBuilderMock.Verify (mock => mock.SetRaiseMethod (It.IsAny<IMethodBuilder>()), Times.Never());
      eventBuilderMock.Verify();
    }

    private void SetupDefineCustomAttribute<T> (Mock<T> customAttributeTargetBuilderMock, IMutableInfo mutableInfo) where T : class, ICustomAttributeTargetBuilder
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create();
      mutableInfo.AddCustomAttribute (declaration);
      customAttributeTargetBuilderMock.Setup (mock => mock.SetCustomAttribute (declaration)).Verifiable();
    }

    private Mock<IParameterBuilder> SetupDefineParameter<T> (
        Mock<T> methodBaseBuilderMock,
        int position,
        string parameterName,
        ParameterAttributes parameterAttributes) where T : class, IMethodBaseBuilder
    {
      var parameterBuilderMock = new Mock<IParameterBuilder> (MockBehavior.Strict);
      methodBaseBuilderMock.Setup (mock => mock.DefineParameter (position, parameterAttributes, parameterName)).Returns (parameterBuilderMock.Object).Verifiable();
      return parameterBuilderMock;
    }

    private void CheckBodyBuildAction<T> (Action testedAction, Mock<T> methodBuilderMock, IMutableMethodBase mutableMethodBase) where T : class, IMethodBaseBuilder
    {
      _expressionPreparerMock.Setup (mock => mock.PrepareBody (_context, mutableMethodBase.Body)).Returns (_fakeBody).Verifiable();
      methodBuilderMock
          .Setup (
              mock => mock.SetBody (
                  It.IsAny<LambdaExpression>(),
                  _ilGeneratorFactoryStub.Object,
                  _context.DebugInfoGenerator))
          .Callback (
              (LambdaExpression lambdaExpression, IILGeneratorFactory ilGeneratorFactory, DebugInfoGenerator debugInfoGenerator) =>
              {
                Assert.That (lambdaExpression.Body, Is.SameAs (_fakeBody));
                Assert.That (lambdaExpression.Parameters, Is.EqualTo (mutableMethodBase.ParameterExpressions));
              })
          .Verifiable();

      testedAction();

      _emittableOperandProviderMock.Verify();
      methodBuilderMock.Verify();
    }

    private void CheckExplicitOverrideAction (Action testedAction, MethodInfo overriddenMethod, MutableMethodInfo overridingMethod)
    {
      var fakeOverriddenMethod = ReflectionObjectMother.GetSomeMethod();
      var fakeOverridingMethod = ReflectionObjectMother.GetSomeMethod();
      _emittableOperandProviderMock.Setup (mock => mock.GetEmittableMethod (overriddenMethod)).Returns (fakeOverriddenMethod).Verifiable();
      _emittableOperandProviderMock.Setup (mock => mock.GetEmittableMethod (overridingMethod)).Returns (fakeOverridingMethod).Verifiable();

      _typeBuilderMock.Setup (mock => mock.DefineMethodOverride (fakeOverridingMethod, fakeOverriddenMethod)).Verifiable();

      testedAction ();

      _emittableOperandProviderMock.Verify();
      _typeBuilderMock.Verify();
    }

    interface IDomainInterface { }
    public class DomainType : IDomainInterface
    {
      public virtual string ExplicitBaseDefinition (int i, out double d) { Dev.Null = i; d = Dev<double>.Null; return ""; }
    }
  }
}