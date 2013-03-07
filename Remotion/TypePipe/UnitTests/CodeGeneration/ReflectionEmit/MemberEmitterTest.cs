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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Generics;
using Rhino.Mocks;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MemberEmitterTest
  {
    private IExpressionPreparer _expressionPreparerMock;
    private IILGeneratorFactory _ilGeneratorFactoryStub;

    private MemberEmitter _emitter;

    private ITypeBuilder _typeBuilderMock;
    private IEmittableOperandProvider _emittableOperandProviderMock;

    private CodeGenerationContext _context;

    private Expression _fakeBody;

    [SetUp]
    public void SetUp ()
    {
      _expressionPreparerMock = MockRepository.GenerateStrictMock<IExpressionPreparer>();
      _ilGeneratorFactoryStub = MockRepository.GenerateStub<IILGeneratorFactory>();

      _emitter = new MemberEmitter (_expressionPreparerMock, _ilGeneratorFactoryStub);

      _typeBuilderMock = MockRepository.GenerateStrictMock<ITypeBuilder>();
      _emittableOperandProviderMock = MockRepository.GenerateStrictMock<IEmittableOperandProvider>();

      _context = CodeGenerationContextObjectMother.GetSomeContext (
          typeBuilder: _typeBuilderMock, emittableOperandProvider: _emittableOperandProviderMock);

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
      var field = MutableFieldInfoObjectMother.Create();
      var fieldBuilderMock = MockRepository.GenerateStrictMock<IFieldBuilder>();

      _typeBuilderMock
          .Expect (mock => mock.DefineField (field.Name, field.FieldType, field.Attributes))
          .Return (fieldBuilderMock);
      fieldBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, field));
      SetupDefineCustomAttribute (fieldBuilderMock, field);

      _emitter.AddField (_context, field);

      _typeBuilderMock.VerifyAllExpectations();
      fieldBuilderMock.VerifyAllExpectations();
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

      var constructorBuilderMock = MockRepository.GenerateStrictMock<IConstructorBuilder>();
      _typeBuilderMock
          .Expect (mock => mock.DefineConstructor (expectedAttributes, expectedCallingConventions, expectedParameterTypes))
          .Return (constructorBuilderMock);
      constructorBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, constructor));

      SetupDefineCustomAttribute (constructorBuilderMock, constructor);
      var parameterBuilderMock = SetupDefineParameter (constructorBuilderMock, 1, "p1", ParameterAttributes.In);
      SetupDefineCustomAttribute (parameterBuilderMock, constructor.MutableParameters[0]);
      SetupDefineParameter (constructorBuilderMock, 2, "p2", ParameterAttributes.Out);

      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);

      _emitter.AddConstructor (_context, constructor);

      _typeBuilderMock.VerifyAllExpectations();
      constructorBuilderMock.VerifyAllExpectations();
      parameterBuilderMock.VerifyAllExpectations();

      Assert.That (_context.PostDeclarationsActionManager.Actions.Count(), Is.EqualTo (1));
      CheckBodyBuildAction (_context.PostDeclarationsActionManager.Actions.Single(), constructorBuilderMock, constructor);
    }

    [Test]
    public void AddConstructor_Static ()
    {
      var ctor = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      var constructorBuilderStub = MockRepository.GenerateStub<IConstructorBuilder> ();
      var attributes = MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      _typeBuilderMock.Expect (mock => mock.DefineConstructor (attributes, CallingConventions.Standard, Type.EmptyTypes)).Return (constructorBuilderStub);

      _emitter.AddConstructor (_context, ctor);

      _typeBuilderMock.VerifyAllExpectations ();
    }

    [Test]
    public void AddMethod ()
    {
      var method = MutableMethodInfoObjectMother.Create (
          ProxyTypeObjectMother.Create (baseType: typeof (DomainType)),
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

      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder>();
      _typeBuilderMock.Expect (mock => mock.DefineMethod ("Method", MethodAttributes.Virtual)).Return (methodBuilderMock);
      methodBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, method));

      methodBuilderMock.Expect (mock => mock.SetReturnType (typeof (string)));
      methodBuilderMock.Expect (mock => mock.SetParameters (new[] { typeof (int), typeof (double).MakeByRefType() }));

      var returnParameterBuilderMock = SetupDefineParameter (methodBuilderMock, 0, null, ParameterAttributes.None);
      SetupDefineCustomAttribute (returnParameterBuilderMock, method.MutableReturnParameter);
      var parameterBuilderMock = SetupDefineParameter (methodBuilderMock, 1, "i", ParameterAttributes.Reserved3);
      SetupDefineCustomAttribute (parameterBuilderMock, method.MutableParameters[0]);
      SetupDefineParameter (methodBuilderMock, 2, "d", ParameterAttributes.Out);
      SetupDefineCustomAttribute (methodBuilderMock, method);

      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);

      _emitter.AddMethod (_context, method);

      _typeBuilderMock.VerifyAllExpectations ();
      methodBuilderMock.VerifyAllExpectations ();
      parameterBuilderMock.VerifyAllExpectations();

      Assert.That (_context.MethodBuilders, Has.Count.EqualTo(1));
      Assert.That (_context.MethodBuilders[method], Is.SameAs(methodBuilderMock));

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
          name: "TParam", genericParameterAttributes: (GenericParameterAttributes) 7, constraints: new[] { baseTypeConstraint, interfaceConstraint });
      var method = MutableMethodInfoObjectMother.Create (
          genericParameters: new[] { genericParameter },
          returnType: genericParameter,
          parameters: new[] { ParameterDeclarationObjectMother.Create (genericParameter, "genericParam") });

      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder>();
      _typeBuilderMock.Stub (mock => mock.DefineMethod (method.Name, method.Attributes)).Return (methodBuilderMock);
      methodBuilderMock.Stub (mock => mock.RegisterWith (_emittableOperandProviderMock, method));

      var genericParameterBuilderMock = MockRepository.GenerateStrictMock<IGenericTypeParameterBuilder>();
      methodBuilderMock.Expect (mock => mock.DefineGenericParameters (new[] { "TParam" })).Return (new[] { genericParameterBuilderMock });
      genericParameterBuilderMock.Expect (mock => mock.RegisterWith (_emittableOperandProviderMock, genericParameter));
      genericParameterBuilderMock.Expect (mock => mock.SetGenericParameterAttributes ((GenericParameterAttributes) 7));
      genericParameterBuilderMock.Expect (mock => mock.SetBaseTypeConstraint (baseTypeConstraint));
      genericParameterBuilderMock.Expect (mock => mock.SetInterfaceConstraints (new[] { interfaceConstraint }));
      SetupDefineCustomAttribute (genericParameterBuilderMock, genericParameter);

      methodBuilderMock.Expect (mock => mock.SetReturnType (genericParameter));
      methodBuilderMock.Expect (mock => mock.SetParameters (new Type[] { genericParameter }));
      SetupDefineParameter (methodBuilderMock, 0, null, ParameterAttributes.None);
      SetupDefineParameter (methodBuilderMock, 1, "genericParam", ParameterAttributes.None);

      _emitter.AddMethod (_context, method);

      methodBuilderMock.VerifyAllExpectations();
      genericParameterBuilderMock.VerifyAllExpectations();
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

      var methodBuilderMock = MockRepository.GenerateStrictMock<IMethodBuilder> ();
      _typeBuilderMock.Stub (stub => stub.DefineMethod ("AbstractMethod", MethodAttributes.Abstract)).Return (methodBuilderMock);
      methodBuilderMock.Stub (stub => stub.RegisterWith (_emittableOperandProviderMock, method));

      methodBuilderMock.Stub (stub => stub.SetReturnType (typeof (int)));
      methodBuilderMock.Stub (stub => stub.SetParameters (Type.EmptyTypes));
      SetupDefineParameter (methodBuilderMock, 0, parameterName: null, parameterAttributes: ParameterAttributes.None);

      _emitter.AddMethod (_context, method);

      _typeBuilderMock.VerifyAllExpectations();
      methodBuilderMock.VerifyAllExpectations();
      var actions = _context.PostDeclarationsActionManager.Actions.ToArray();
      Assert.That (actions, Has.Length.EqualTo (1));

      // Executing the action has no side effect (strict mocks; empty override build action).
      _emittableOperandProviderMock.Stub (mock => mock.GetEmittableMethod (method));
      actions[0].Invoke();
    }

    [Test]
    public void AddProperty ()
    {
      var name = "Property";
      var attributes = (PropertyAttributes) 7;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var setMethodParameters = parameters.Concat (ParameterDeclarationObjectMother.Create (returnType));
      var indexParameterTypes = parameters.Select (p => p.Type).ToArray();
      var getMethod = MutableMethodInfoObjectMother.Create (returnType: returnType, parameters: parameters);
      var setMethod = MutableMethodInfoObjectMother.Create (parameters: setMethodParameters);
      var property = MutablePropertyInfoObjectMother.Create (name: name, attributes: attributes, getMethod: getMethod, setMethod: setMethod);

      var getMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      var setMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      _context.MethodBuilders.Add (getMethod, getMethodBuilder);
      _context.MethodBuilders.Add (setMethod, setMethodBuilder);

      var callingConventions = CallingConventions.Standard | CallingConventions.HasThis;
      var propertyBuilderMock = MockRepository.GenerateStrictMock<IPropertyBuilder>();
      _typeBuilderMock
          .Expect (mock => mock.DefineProperty (name, attributes, callingConventions, returnType, indexParameterTypes))
          .Return (propertyBuilderMock);
      SetupDefineCustomAttribute (propertyBuilderMock, property);
      propertyBuilderMock.Expect (mock => mock.SetGetMethod (getMethodBuilder));
      propertyBuilderMock.Expect (mock => mock.SetSetMethod (setMethodBuilder));

      _emitter.AddProperty (_context, property);

      _typeBuilderMock.VerifyAllExpectations();
      propertyBuilderMock.VerifyAllExpectations();
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

      var methodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      _context.MethodBuilders.Add (readOnlyProperty.MutableGetMethod, methodBuilder);
      _context.MethodBuilders.Add (writeOnlyProperty.MutableSetMethod, methodBuilder);

      var propertyBuilderMock1 = MockRepository.GenerateStrictMock<IPropertyBuilder>();
      var propertyBuilderMock2 = MockRepository.GenerateStrictMock<IPropertyBuilder>();
      _typeBuilderMock
          .Expect (mock => mock.DefineProperty (readOnlyProperty.Name, readOnlyProperty.Attributes, staticGetMethod.CallingConvention, typeof (int), Type.EmptyTypes))
          .Return (propertyBuilderMock1);
      _typeBuilderMock
          .Expect (mock => mock.DefineProperty (writeOnlyProperty.Name, writeOnlyProperty.Attributes, setMethod.CallingConvention, typeof (long), Type.EmptyTypes))
          .Return (propertyBuilderMock2);
      propertyBuilderMock1.Expect (mock => mock.SetGetMethod (methodBuilder));
      propertyBuilderMock2.Expect (mock => mock.SetSetMethod (methodBuilder));

      _emitter.AddProperty (_context, readOnlyProperty);
      _emitter.AddProperty (_context, writeOnlyProperty);

      _typeBuilderMock.VerifyAllExpectations();
      propertyBuilderMock1.VerifyAllExpectations();
      propertyBuilderMock2.VerifyAllExpectations();
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

      var addMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      var removeMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      var raiseMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      _context.MethodBuilders.Add (addMethod, addMethodBuilder);
      _context.MethodBuilders.Add (removeMethod, removeMethodBuilder);
      _context.MethodBuilders.Add (raiseMethod, raiseMethodBuilder);

      var eventBuilderMock = MockRepository.GenerateStrictMock<IEventBuilder>();
      _typeBuilderMock.Expect (mock => mock.DefineEvent (name, attributes, handlerType)).Return (eventBuilderMock);
      SetupDefineCustomAttribute (eventBuilderMock, event_);
      eventBuilderMock.Expect (mock => mock.SetAddOnMethod (addMethodBuilder));
      eventBuilderMock.Expect (mock => mock.SetRemoveOnMethod (removeMethodBuilder));
      eventBuilderMock.Expect (mock => mock.SetRaiseMethod (raiseMethodBuilder));

      _emitter.AddEvent (_context, event_);

      _typeBuilderMock.VerifyAllExpectations();
      eventBuilderMock.VerifyAllExpectations();
    }

    [Test]
    public void AddEvent_NoRaiseMethod ()
    {
      var event_ = MutableEventInfoObjectMother.CreateWithAccessors();
      Assert.That (event_.MutableRaiseMethod, Is.Null);

      var addMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      var removeMethodBuilder = MockRepository.GenerateStub<IMethodBuilder>();
      _context.MethodBuilders.Add (event_.MutableAddMethod, addMethodBuilder);
      _context.MethodBuilders.Add (event_.MutableRemoveMethod, removeMethodBuilder);

      var eventBuilderMock = MockRepository.GenerateStrictMock<IEventBuilder>();
      _typeBuilderMock.Stub (stub => stub.DefineEvent (event_.Name, event_.Attributes, event_.EventHandlerType)).Return (eventBuilderMock);
      eventBuilderMock.Expect (mock => mock.SetAddOnMethod (addMethodBuilder));
      eventBuilderMock.Expect (mock => mock.SetRemoveOnMethod (removeMethodBuilder));

      _emitter.AddEvent (_context, event_);

      eventBuilderMock.AssertWasNotCalled (mock => mock.SetRaiseMethod (Arg<IMethodBuilder>.Is.Anything));
      eventBuilderMock.VerifyAllExpectations();
    }

    private void SetupDefineCustomAttribute (ICustomAttributeTargetBuilder customAttributeTargetBuilderMock, IMutableInfo mutableInfo)
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create();
      mutableInfo.AddCustomAttribute (declaration);
      customAttributeTargetBuilderMock.Expect (mock => mock.SetCustomAttribute (declaration));
    }

    private IParameterBuilder SetupDefineParameter (
        IMethodBaseBuilder methodBaseBuilderMock, int position, string parameterName, ParameterAttributes parameterAttributes)
    {
      var parameterBuilderMock = MockRepository.GenerateStrictMock<IParameterBuilder>();
      methodBaseBuilderMock.Expect (mock => mock.DefineParameter (position, parameterAttributes, parameterName)).Return (parameterBuilderMock);
      return parameterBuilderMock;
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

    private void CheckExplicitOverrideAction (Action testedAction, MethodInfo overriddenMethod, MutableMethodInfo overridingMethod)
    {
      _emittableOperandProviderMock.BackToRecord();
      _typeBuilderMock.BackToRecord();

      var fakeOverriddenMethod = ReflectionObjectMother.GetSomeMethod();
      var fakeOverridingMethod = ReflectionObjectMother.GetSomeMethod();
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableMethod (overriddenMethod)).Return (fakeOverriddenMethod);
      _emittableOperandProviderMock.Expect (mock => mock.GetEmittableMethod (overridingMethod)).Return (fakeOverridingMethod);

      _typeBuilderMock.Expect (mock => mock.DefineMethodOverride (fakeOverridingMethod, fakeOverriddenMethod));

      _emittableOperandProviderMock.Replay ();
      _typeBuilderMock.Replay ();

      testedAction ();

      _emittableOperandProviderMock.VerifyAllExpectations ();
      _typeBuilderMock.VerifyAllExpectations ();
    }

    interface IDomainInterface { }
    public class DomainType : IDomainInterface
    {
      public virtual string ExplicitBaseDefinition (int i, out double d) { Dev.Null = i; d = Dev<double>.Null; return ""; }
    }
  }
}