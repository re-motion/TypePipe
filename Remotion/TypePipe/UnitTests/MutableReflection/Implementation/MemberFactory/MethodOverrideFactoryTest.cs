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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.TypePipe.UnitTesting.Expressions;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Generics;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class MethodOverrideFactoryTest
  {
    private IRelatedMethodFinder _relatedMethodFinderMock;
    private IMethodFactory _methodFactoryMock;

    private MethodOverrideFactory _factory;

    private MutableType _mutableType;
    private bool _isNewlyCreated;
    private GenericParameterContext _noGenericParameters;

    [SetUp]
    public void SetUp ()
    {
      _relatedMethodFinderMock = MockRepository.GenerateStrictMock<IRelatedMethodFinder> ();
      _methodFactoryMock = MockRepository.GenerateStrictMock<IMethodFactory>();

      _factory = new MethodOverrideFactory (_relatedMethodFinderMock, _methodFactoryMock);

      _mutableType = MutableTypeObjectMother.Create (name: "MyAbcType", baseType: typeof (DomainType));
      _noGenericParameters = new GenericParameterContext (Type.EmptyTypes);
    }

    [Test]
    public void CreateExplicitOverride ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;

      var fakeResult = MutableMethodInfoObjectMother.Create (
          _mutableType, attributes: MethodAttributes.Virtual, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (int)) });
      _methodFactoryMock
          .Expect (
              mock =>
              mock.CreateMethod (
                  Arg.Is (_mutableType),
                  Arg.Is ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory.MethodOverrideFactoryTest.A.OverrideHierarchy"),
                  Arg.Is (MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig),
                  Arg.Is (GenericParameterDeclaration.None),
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg.Is (bodyProvider)))
          .Return (fakeResult)
          .WhenCalled (
              mi =>
              {
                var returnType = ((Func<GenericParameterContext, Type>) mi.Arguments[4]) (_noGenericParameters);
                var parameters = ((Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>) mi.Arguments[5]) (_noGenericParameters);

                Assert.That (returnType, Is.SameAs (typeof (void)));
                var parameter = parameters.Single();
                Assert.That (parameter.Name, Is.EqualTo ("aaa"));
                Assert.That (parameter.Type, Is.SameAs (typeof (int)));
              });

      var result = _factory.CreateExplicitOverride (_mutableType, method, bodyProvider);

      _methodFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { method }));
    }

    [Test]
    public void CreateExplicitOverride_Generic ()
    {
      var method = typeof (DomainType).GetMethod ("GenericMethod");
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => null;

      var fakeResult = CreateFakeGenericMethod();
      _methodFactoryMock
          .Expect (
              mock =>
              mock.CreateMethod (
                  Arg.Is (_mutableType),
                  Arg.Is ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory.MethodOverrideFactoryTest.DomainType.GenericMethod"),
                  Arg.Is (MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig),
                  Arg<IEnumerable<GenericParameterDeclaration>>.Is.Anything,
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg.Is (bodyProvider)))
          .Return (fakeResult)
          .WhenCalled (
              mi =>
              {
                var fakeGenericParameter = ReflectionObjectMother.GetSomeType();
                var genericParameterContext = new GenericParameterContext (new[] { fakeGenericParameter });

                var genericParameters = (IEnumerable<GenericParameterDeclaration>) mi.Arguments[3];
                var returnType = ((Func<GenericParameterContext, Type>) mi.Arguments[4]) (genericParameterContext);
                var parameters = ((Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>) mi.Arguments[5]) (genericParameterContext).ToList();

                var genericParameter = genericParameters.Single();
                Assert.That (genericParameter.Name, Is.EqualTo ("TPar"));
                Assert.That (genericParameter.Attributes, Is.EqualTo (GenericParameterAttributes.DefaultConstructorConstraint));
                Assert.That (genericParameter.ConstraintProvider (genericParameterContext), Is.EqualTo (new[] { typeof (DomainType), typeof (IDisposable) }));

                Assert.That (returnType, Is.SameAs (fakeGenericParameter));
                ParameterDeclarationTest.CheckParameter (parameters[0], typeof (int), "arg1", ParameterAttributes.None);
                ParameterDeclarationTest.CheckParameter (parameters[1], fakeGenericParameter, "arg2", ParameterAttributes.None);
              });

      var result = _factory.CreateExplicitOverride (_mutableType, method, bodyProvider);

      _methodFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { method }));
    }

    [Test]
    public void GetOrCreateOverride_ExistingOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString ());
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ToString ());
      Assert.That (method, Is.Not.EqualTo (baseDefinition));

      var fakeExistingOverride = MutableMethodInfoObjectMother.Create ();
      _relatedMethodFinderMock
          .Expect (mock => mock.GetOverride (Arg.Is (baseDefinition), Arg<IEnumerable<MutableMethodInfo>>.List.Equal (_mutableType.AddedMethods)))
          .Return (fakeExistingOverride);

      var result = _factory.GetOrCreateOverride (_mutableType, method, out _isNewlyCreated);

      _relatedMethodFinderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeExistingOverride));
      Assert.That (_isNewlyCreated, Is.False);
    }

    [Test]
    public void GetOrCreateOverride_BaseMethod_ImplicitOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      var inputMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.OverrideHierarchy (7));
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((C obj) => obj.OverrideHierarchy (7));

      CallAndCheckGetOrAddOverride (
          baseDefinition,
          inputMethod,
          baseMethod,
          isBaseDefinitionShadowed: false,
          expectedParameterName: "ccc",
          expectedAddedExplicitBaseDefinitions: new MethodInfo[0],
          expectedOverrideMethodName: "OverrideHierarchy",
          expectedOverrideAttributes: MethodAttributes.Public | MethodAttributes.ReuseSlot);
    }

    [Test]
    public void GetOrCreateOverride_BaseMethod_ImplicitOverride_Generic ()
    {
      var baseDefinition = typeof (DomainType).GetMethod ("GenericMethod");
      var inputMethod = baseDefinition;
      var baseMethod = baseDefinition;

      _relatedMethodFinderMock.Expect (mock => mock.GetOverride (baseDefinition, _mutableType.AddedMethods)).Return (null);
      _relatedMethodFinderMock.Expect (mock => mock.GetMostDerivedOverride (baseDefinition, _mutableType.BaseType)).Return (baseMethod);
      _relatedMethodFinderMock
          .Expect (mock => mock.IsShadowed (Arg.Is (baseDefinition), Arg<IEnumerable<MethodInfo>>.List.Equivalent (_mutableType.GetAllMethods())))
          .Return (false);

      var fakeResult = CreateFakeGenericMethod ();
      _methodFactoryMock
          .Expect (
              mock =>
              mock.CreateMethod (
                  Arg.Is (_mutableType),
                  Arg.Is ("GenericMethod"),
                  Arg.Is (MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig),
                  Arg<IEnumerable<GenericParameterDeclaration>>.Is.Anything,
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg<Func<MethodBodyCreationContext, Expression>>.Is.Anything))
          .Return (fakeResult)
          .WhenCalled (
              mi =>
              {
                var fakeGenericParameter = typeof (TypeThatCompliesWithConstraints);
                var genericParameterContext = new GenericParameterContext (new[] { fakeGenericParameter });

                var genericParameters = (IEnumerable<GenericParameterDeclaration>) mi.Arguments[3];
                var returnType = ((Func<GenericParameterContext, Type>) mi.Arguments[4]) (genericParameterContext);
                var parameters = ((Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>) mi.Arguments[5]) (genericParameterContext).ToList();

                var genericParameter = genericParameters.Single ();
                Assert.That (genericParameter.Name, Is.EqualTo ("TPar"));
                Assert.That (genericParameter.Attributes, Is.EqualTo (GenericParameterAttributes.DefaultConstructorConstraint));
                Assert.That (genericParameter.ConstraintProvider (genericParameterContext), Is.EqualTo (new[] { typeof (DomainType), typeof (IDisposable) }));

                Assert.That (returnType, Is.SameAs (fakeGenericParameter));
                ParameterDeclarationTest.CheckParameter (parameters[0], typeof (int), "arg1", ParameterAttributes.None);
                ParameterDeclarationTest.CheckParameter (parameters[1], fakeGenericParameter, "arg2", ParameterAttributes.None);

                var parameterExpressions = parameters.Select (p => p.Expression).ToList();
                var bodyContext = new MethodBodyCreationContext (
                    _mutableType, false, parameterExpressions, new[] { fakeGenericParameter }, returnType, baseMethod);
                var body = ((Func<MethodBodyCreationContext, Expression>) mi.Arguments[6]) (bodyContext);

                var expectedBody = Expression.Call (
                    bodyContext.This, baseMethod.MakeTypePipeGenericMethod (fakeGenericParameter), parameterExpressions.Cast<Expression>());
                ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, body);
              });

      var result = _factory.GetOrCreateOverride (_mutableType, inputMethod, out _isNewlyCreated);

      _relatedMethodFinderMock.VerifyAllExpectations();
      _methodFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (_isNewlyCreated, Is.True);
    }

    [Test]
    public void GetOrCreateOverride_BaseMethod_ImplicitOverride_AdjustsAttributes ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.ProtectedOrInternalVirtualNewSlotMethodInB (7));
      var inputMethod = baseDefinition;
      var baseMethod = baseDefinition;
      Assert.That (baseMethod.IsFamilyOrAssembly, Is.True);
      Assert.That (baseMethod.Attributes.IsSet (MethodAttributes.NewSlot), Is.True);

      CallAndCheckGetOrAddOverride (
          baseDefinition,
          inputMethod,
          baseMethod,
          isBaseDefinitionShadowed: false,
          expectedParameterName: "protectedOrInternal",
          expectedAddedExplicitBaseDefinitions: new MethodInfo[0],
          expectedOverrideMethodName: "ProtectedOrInternalVirtualNewSlotMethodInB",
          expectedOverrideAttributes: MethodAttributes.Family | MethodAttributes.ReuseSlot);
    }

    [Test]
    public void GetOrCreateOverride_BaseMethod_ImplicitOverride_Abstract ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithOneMethod obj) => obj.Method (7));
      var inputMethod = baseDefinition;
      var baseMethod = baseDefinition;
      Assert.That (baseMethod.IsAbstract, Is.True);

      CallAndCheckGetOrAddOverride (
          baseDefinition,
          inputMethod,
          baseMethod,
          isBaseDefinitionShadowed: false,
          expectedParameterName: "paramOnAbstractMethod",
          expectedAddedExplicitBaseDefinitions: new MethodInfo[0],
          expectedOverrideMethodName: "Method",
          expectedOverrideAttributes: MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.ReuseSlot,
          mutableType: MutableTypeObjectMother.Create (typeof (AbstractTypeWithOneMethod)),
          skipBodyProviderCheck: true);
    }

    [Test]
    public void GetOrCreateOverride_ShadowedBaseMethod_ExplicitOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      var inputMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.OverrideHierarchy (7));
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((C obj) => obj.OverrideHierarchy (7));
      Assert.That (baseMethod.Attributes.IsSet (MethodAttributes.NewSlot), Is.False);

      CallAndCheckGetOrAddOverride (
          baseDefinition,
          inputMethod,
          baseMethod,
          isBaseDefinitionShadowed: true,
          expectedParameterName: "aaa",
          expectedAddedExplicitBaseDefinitions: new[] { baseDefinition },
          expectedOverrideMethodName:
              "Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory.MethodOverrideFactoryTest.A.OverrideHierarchy",
          expectedOverrideAttributes: MethodAttributes.Private | MethodAttributes.NewSlot);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Only virtual methods can be overridden.\r\nParameter name: overriddenMethod")]
    public void GetOrCreateOverride_NonVirtualMethod ()
    {
      var method = ReflectionObjectMother.GetSomeNonVirtualMethod();
      _factory.GetOrCreateOverride (_mutableType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "The specified method must be either a non-generic method or a generic method definition; "
                          + "it cannot be a method instantiation.\r\nParameter name: overriddenMethod")]
    public void GetOrCreateOverride_MethodInstantiation ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.GenericMethod<TypeThatCompliesWithConstraints> (7, null));
      _factory.GetOrCreateOverride (_mutableType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method is declared by type 'IDisposable' outside of the proxy base class hierarchy.\r\nParameter name: overriddenMethod")]
    public void GetOrCreateOverride_UnrelatedDeclaringType ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDisposable obj) => obj.Dispose());
      _factory.GetOrCreateOverride (_mutableType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method is declared by type 'MyAbcType' outside of the proxy base class hierarchy.\r\nParameter name: overriddenMethod")]
    public void GetOrCreateOverride_DeclaredOnProxyType ()
    {
      var method = _mutableType.AddMethod ("method", MethodAttributes.Virtual, bodyProvider: ctx => Expression.Empty());
      _factory.GetOrCreateOverride (_mutableType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "Interface method 'InvalidCandidate' cannot be implemented because a method with equal name and signature already exists. "
        + "Use AddExplicitOverride to create an explicit implementation.")]
    public void GetOrAddImplementation_InterfaceMethod_InvalidCandidate ()
    {
      _mutableType.AddInterface(typeof(IAddedInterface));
      _mutableType.AddMethod("InvalidCandidate"); // Not virtual, therefore no implicit override/implementation.
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod((IAddedInterface obj) => obj.InvalidCandidate());

      _methodFactoryMock
          .Expect(mock => mock.CreateMethod(null, "", 0, null, null, null, null)).IgnoreArguments()
          .Throw(new InvalidOperationException());

      _factory.GetOrCreateImplementation(_mutableType, interfaceMethod, out _isNewlyCreated);
    }

    private void CallAndCheckGetOrAddOverride (
        MethodInfo baseDefinition,
        MethodInfo inputMethod,
        MethodInfo baseMethod,
        bool isBaseDefinitionShadowed,
        string expectedParameterName,
        IEnumerable<MethodInfo> expectedAddedExplicitBaseDefinitions,
        string expectedOverrideMethodName,
        MethodAttributes expectedOverrideAttributes,
        MutableType mutableType = null,
        bool skipBodyProviderCheck = false)
    {
      mutableType = mutableType ?? _mutableType;

      _relatedMethodFinderMock.Expect (mock => mock.GetOverride (baseDefinition, mutableType.AddedMethods)).Return (null);
      _relatedMethodFinderMock.Expect (mock => mock.GetMostDerivedOverride (baseDefinition, mutableType.BaseType)).Return (baseMethod);
      _relatedMethodFinderMock
          .Expect (mock => mock.IsShadowed (Arg.Is (baseDefinition), Arg<IEnumerable<MethodInfo>>.List.Equivalent (mutableType.GetAllMethods())))
          .Return (isBaseDefinitionShadowed);

      var fakeResult = SetupExpectationsForCreateMethod (
          _methodFactoryMock,
          mutableType,
          baseMethod,
          expectedParameterName,
          expectedOverrideMethodName,
          expectedOverrideAttributes,
          skipBodyProviderCheck);

      var result = _factory.GetOrCreateOverride (mutableType, inputMethod, out _isNewlyCreated);

      _methodFactoryMock.VerifyAllExpectations();
      _relatedMethodFinderMock.VerifyAllExpectations();
      Assert.That (_isNewlyCreated, Is.True);
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (expectedAddedExplicitBaseDefinitions));
    }

    private MutableMethodInfo SetupExpectationsForCreateMethod (
        IMethodFactory methodFactoryMock,
        MutableType mutableType,
        MethodInfo baseMethod,
        string expectedParameterName,
        string expectedOverrideMethodName,
        MethodAttributes expectedOverrideAttributes,
        bool skipBodyProviderCheck)
    {
      var methodParameters = baseMethod.GetParameters().Select (p => new ParameterDeclaration (p.ParameterType, p.Name, p.Attributes));
      var fakeResult = MutableMethodInfoObjectMother.Create (mutableType, attributes: MethodAttributes.Virtual, parameters: methodParameters);
      methodFactoryMock
          .Expect (
              mock => mock.CreateMethod (
                  Arg.Is (mutableType),
                  Arg.Is (expectedOverrideMethodName),
                  Arg.Is (expectedOverrideAttributes | MethodAttributes.Virtual | MethodAttributes.HideBySig),
                  Arg.Is (GenericParameterDeclaration.None),
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg<Func<MethodBodyCreationContext, Expression>>.Is.Anything))
          .WhenCalled (
              mi =>
              {
                var returnType = ((Func<GenericParameterContext, Type>) mi.Arguments[4]) (_noGenericParameters);
                var parameters = ((Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>) mi.Arguments[5]) (_noGenericParameters);
                Assert.That (returnType, Is.SameAs (typeof (void)));
                var parameter = parameters.Single();
                Assert.That (parameter.Name, Is.EqualTo (expectedParameterName));
                Assert.That (parameter.Type, Is.SameAs (typeof (int)));

                if (skipBodyProviderCheck)
                  return;
                var body = ((Func<MethodBodyCreationContext, Expression>) mi.Arguments[6]) (CreateMethodBodyCreationContext (baseMethod));
                Assert.That (body, Is.InstanceOf<MethodCallExpression>());
                var methodCallExpression = (MethodCallExpression) body;
                Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>());
                var baceCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) methodCallExpression.Method;
                Assert.That (baceCallMethodInfoAdapter.AdaptedMethod, Is.SameAs (baseMethod));
              })
          .Return (fakeResult);

      return fakeResult;
    }

    private MethodBodyCreationContext CreateMethodBodyCreationContext (MethodInfo baseMethod)
    {
      var parameterExpressions = baseMethod.GetParameters().Select (p => Expression.Parameter (p.ParameterType, p.Name));
      return new MethodBodyCreationContext (_mutableType, false, parameterExpressions, Type.EmptyTypes, baseMethod.ReturnType, baseMethod);
    }

    private MutableMethodInfo CreateFakeGenericMethod ()
    {
      var genericParameter = MutableGenericParameterObjectMother.Create (position: 0);
      return MutableMethodInfoObjectMother.Create (
          _mutableType,
          attributes: MethodAttributes.Virtual,
          genericParameters: new[] { genericParameter },
          returnType: genericParameter,
          parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (int)), ParameterDeclarationObjectMother.Create (genericParameter) });
    }

    public class A
    {
      // base definition
      public virtual void OverrideHierarchy (int aaa) { }
    }
    public class B : A
    {
      // CreateMethodOverride input
      public override void OverrideHierarchy (int bbb) { }

      protected internal virtual void ProtectedOrInternalVirtualNewSlotMethodInB (int protectedOrInternal) { }
    }
    public class C : B
    {
      // base inputMethod
      public override void OverrideHierarchy (int ccc) { }
    }
    public class DomainType : C
    {
      public virtual void InterfaceMethod (int interfaceMethodOnDomainType) {}
      public virtual TPar GenericMethod<TPar> (int arg1, TPar arg2) where TPar : DomainType, IDisposable, new () { return arg2; }

      public void InvalidCandidate () {}
    }

    public interface IAddedInterface
    {
      void InvalidCandidate ();
    }

    public abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method (int paramOnAbstractMethod);
    }
    public abstract class DerivedAbstractTypeLeavesAbstractBaseMethod : AbstractTypeWithOneMethod { }

    public class TypeThatCompliesWithConstraints : DomainType, IDisposable
    {
      public void Dispose () { }
    }
  }
}