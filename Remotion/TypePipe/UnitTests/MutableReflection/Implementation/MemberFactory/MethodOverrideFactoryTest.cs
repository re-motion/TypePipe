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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.RhinoMocks.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class MethodOverrideFactoryTest
  {
    private IRelatedMethodFinder _relatedMethodFinderMock;
    private IMethodFactory _methodFactoryMock;

    private MethodOverrideFactory _factory;

    private ProxyType _proxyType;
    private bool _isNewlyCreated;
    private IMemberSelector _memberSelectorMock;

    [SetUp]
    public void SetUp ()
    {
      _relatedMethodFinderMock = MockRepository.GenerateStrictMock<IRelatedMethodFinder> ();
      _methodFactoryMock = MockRepository.GenerateStrictMock<IMethodFactory>();

      _factory = new MethodOverrideFactory (_relatedMethodFinderMock, _methodFactoryMock);

      _proxyType = ProxyTypeObjectMother.Create (baseType: typeof (DomainType));
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
    }

    [Test]
    public void CreateExplicitOverride ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => fakeBody;

      var fakeResult = MutableMethodInfoObjectMother.Create (
          _proxyType, attributes: MethodAttributes.Virtual, parameters: new[] { ParameterDeclarationObjectMother.Create (typeof (int)) });
      _methodFactoryMock
          .Expect (
              mock =>
              mock.CreateMethod (
                  Arg.Is (_proxyType),
                  Arg.Is ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory.MethodOverrideFactoryTest.A.OverrideHierarchy"),
                  Arg.Is (MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig),
                  Arg.Is (GenericParameterDeclaration.None),
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg<Func<MethodBodyCreationContext, Expression>>.Is.Anything))
          .Return (fakeResult)
          .WhenCalled (
              mi =>
              {
                var returnType = ((Func<GenericParameterContext, Type>) mi.Arguments[4]) (null);
                var parameters = ((Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>) mi.Arguments[5]) (null);
                var body = ((Func<MethodBodyCreationContext, Expression>) mi.Arguments[6]) (null);

                Assert.That (returnType, Is.SameAs (typeof (void)));
                var parameter = parameters.Single();
                Assert.That (parameter.Name, Is.EqualTo ("aaa"));
                Assert.That (parameter.Type, Is.SameAs (typeof (int)));
                Assert.That (body, Is.SameAs (fakeBody));
              });

      var result = _factory.CreateExplicitOverride (_proxyType, method, bodyProvider);

      _methodFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { method }));
    }

    [Ignore]
    [Test]
    public void CreateExplicitOverride_Generic ()
    {
      var method = typeof (DomainType).GetMethod ("GenericMethod");
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => fakeBody;

      var fakeResult = MutableMethodInfoObjectMother.Create (_proxyType, attributes: MethodAttributes.Virtual);
      _methodFactoryMock
          .Expect (
              mock =>
              mock.CreateMethod (
                  Arg.Is (_proxyType),
                  Arg.Is ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory.MethodOverrideFactoryTest.DomainType.GenericMethod"),
                  Arg.Is (MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig),
                  Arg<IEnumerable<GenericParameterDeclaration>>.Is.Anything,
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg<Func<MethodBodyCreationContext, Expression>>.Is.Anything))
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
                Assert.That (genericParameter.BaseConstraintProvider, Is.EqualTo (typeof (DomainType)));
                Assert.That (genericParameter.InterfaceConstraintsProvider, Is.EqualTo (new[] { typeof (IDisposable) }));

                Assert.That (returnType, Is.SameAs (fakeGenericParameter));
                ParameterDeclarationTest.CheckParameter (parameters[0], typeof (int), "arg1", ParameterAttributes.None);
                ParameterDeclarationTest.CheckParameter (parameters[1], fakeGenericParameter, "arg2", ParameterAttributes.None);
              });

      var result = _factory.CreateExplicitOverride (_proxyType, method, bodyProvider);

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
          .Expect (mock => mock.GetOverride (Arg.Is (baseDefinition), Arg<IEnumerable<MutableMethodInfo>>.List.Equal (_proxyType.AddedMethods)))
          .Return (fakeExistingOverride);

      var result = _factory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);

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
          baseMethod,
          inputMethod,
          isBaseDefinitionShadowed: false,
          expectedParameterName: "aaa",
          expectedAddedExplicitBaseDefinitions: new MethodInfo[0],
          expectedOverrideMethodName: "OverrideHierarchy",
          expectedOverrideAttributes: MethodAttributes.Public | MethodAttributes.ReuseSlot);
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
          proxyType: ProxyTypeObjectMother.Create (typeof (AbstractTypeWithOneMethod)),
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
    public void GetOrCreateOverride_InterfaceMethod_AddImplementation ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.AddedInterfaceMethod (7));
      _proxyType.AddInterface (typeof (IAddedInterface));

      var fakeResult = SetupExpectationsForCreateMethod (
          _methodFactoryMock,
          _proxyType,
          baseMethod: interfaceMethod,
          expectedParameterName: "addedInterface",
          expectedOverrideMethodName: "AddedInterfaceMethod",
          expectedOverrideAttributes: MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.NewSlot,
          skipBodyProviderCheck: true);

      var result = _factory.GetOrCreateOverride (_proxyType, interfaceMethod, out _isNewlyCreated);

      _methodFactoryMock.VerifyAllExpectations();
      Assert.That (_isNewlyCreated, Is.True);
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetOrCreateOverride_InterfaceMethod_OverrideImplementationInBase ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.InterfaceMethod (7));
      var implementation = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.InterfaceMethod (7));

      CallAndCheckGetOrAddOverride (
          implementation,
          interfaceMethod,
          implementation,
          false,
          "interfaceMethodOnDomainType",
          new MethodInfo[0],
          "InterfaceMethod",
          MethodAttributes.Public | MethodAttributes.ReuseSlot);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "Interface method 'InvalidCandidate' cannot be implemented because a method with equal name and signature already exists. "
        + "Use ProxyType.AddExplicitOverride to create an explicit implementation.")]
    public void GetOrCreateOverride_InterfaceMethod_InvalidCandidate ()
    {
      _proxyType.AddInterface (typeof (IAddedInterface));
      _proxyType.AddMethod ("InvalidCandidate"); // Not virtual, therefore no implicit override/implementation.
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.InvalidCandidate ());

      _methodFactoryMock
          .Expect (mock => mock.CreateMethod (null, "", 0, null, null, null, null)).IgnoreArguments()
          .Throw (new InvalidOperationException());

      _factory.GetOrCreateOverride (_proxyType, interfaceMethod, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method is declared by a type outside of the proxy base class hierarchy: 'String'.\r\nParameter name: overriddenMethod")]
    public void GetOrCreateOverride_UnrelatedDeclaringType ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((string obj) => obj.Trim ());
      _factory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method is declared by a type outside of the proxy base class hierarchy: 'Proxy'.\r\nParameter name: overriddenMethod")]
    public void GetOrCreateOverride_DeclaredOnProxyType ()
    {
      var method = _proxyType.AddMethod ("method", bodyProvider: ctx => Expression.Empty ());
      _factory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Only virtual methods can be overridden.")]
    public void GetOrCreateOverride_NonVirtualMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualBaseMethod ());
      _factory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot override final method 'B.FinalBaseMethodInB'.")]
    public void GetOrCreateOverride_FinalBaseMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.FinalBaseMethodInB (7));
      var inputMethod = baseDefinition;
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.FinalBaseMethodInB (7));
      var isBaseDefinitionShadowed = BooleanObjectMother.GetRandomBoolean ();

      SetupExpectationsForGetOrAddMethod (baseDefinition, baseMethod, isBaseDefinitionShadowed, null);

      _factory.GetOrCreateOverride (_proxyType, inputMethod, out _isNewlyCreated);
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
        ProxyType proxyType = null,
        bool skipBodyProviderCheck = false)
    {
      proxyType = proxyType ?? _proxyType;

      _relatedMethodFinderMock
          .Expect (mock => mock.GetOverride (Arg.Is (baseDefinition), Arg<IEnumerable<MutableMethodInfo>>.List.Equal (proxyType.AddedMethods)))
          .Return (null);
      _relatedMethodFinderMock.Expect (mock => mock.GetMostDerivedOverride (baseDefinition, proxyType.BaseType)).Return (baseMethod);
      _relatedMethodFinderMock
          .Expect (
              mock => mock.IsShadowed (
                  Arg.Is (baseDefinition),
                  Arg<IEnumerable<MethodInfo>>.List.Equivalent (proxyType.InvokeNonPublicMethod<IEnumerable<MethodInfo>> ("GetAllMethods"))))
          .Return (isBaseDefinitionShadowed);

      var fakeResult = SetupExpectationsForCreateMethod (_methodFactoryMock, proxyType, baseMethod, expectedParameterName, expectedOverrideMethodName, expectedOverrideAttributes, skipBodyProviderCheck);

      var result = _factory.GetOrCreateOverride (proxyType, inputMethod, out _isNewlyCreated);

      _methodFactoryMock.VerifyAllExpectations();
      _relatedMethodFinderMock.VerifyAllExpectations();
      Assert.That (_isNewlyCreated, Is.True);
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (expectedAddedExplicitBaseDefinitions));
    }

    private MutableMethodInfo SetupExpectationsForCreateMethod (
        IMethodFactory methodFactoryMock,
        ProxyType proxyType,
        MethodInfo baseMethod,
        string expectedParameterName,
        string expectedOverrideMethodName,
        MethodAttributes expectedOverrideAttributes,
        bool skipBodyProviderCheck)
    {
      var fakeResult = MutableMethodInfoObjectMother.Create (
          proxyType, attributes: MethodAttributes.Virtual, parameters: ParameterDeclaration.CreateForEquivalentSignature (baseMethod));
      methodFactoryMock
          .Expect (
              mock => mock.CreateMethod (
                  Arg.Is (proxyType),
                  Arg.Is (expectedOverrideMethodName),
                  Arg.Is (expectedOverrideAttributes | MethodAttributes.Virtual | MethodAttributes.HideBySig),
                  Arg.Is (GenericParameterDeclaration.None),
                  Arg<Func<GenericParameterContext, Type>>.Is.Anything,
                  Arg<Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>>.Is.Anything,
                  Arg<Func<MethodBodyCreationContext, Expression>>.Is.Anything))
          .WhenCalled (
              mi =>
              {
                var returnType = ((Func<GenericParameterContext, Type>) mi.Arguments[4]) (null);
                var parameters = ((Func<GenericParameterContext, IEnumerable<ParameterDeclaration>>) mi.Arguments[5]) (null);
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

    private void SetupExpectationsForGetOrAddMethod (
        MethodInfo baseDefinition, MethodInfo baseMethod, bool isBaseDefinitionShadowed, MethodInfo fakeBaseMethod, ProxyType proxyType = null)
    {
      proxyType = proxyType ?? _proxyType;

      _relatedMethodFinderMock
          .Expect (mock => mock.GetOverride (Arg.Is (baseDefinition), Arg<IEnumerable<MutableMethodInfo>>.List.Equal (proxyType.AddedMethods)))
          .Return (null);
      _relatedMethodFinderMock
          .Expect (mock => mock.GetMostDerivedOverride (baseDefinition, proxyType.BaseType))
          .Return (baseMethod);
      _relatedMethodFinderMock
          .Expect (
              mock => mock.IsShadowed (
                  Arg.Is (baseDefinition),
                  Arg<IEnumerable<MethodInfo>>.List.Equivalent (
                      proxyType.InvokeNonPublicMethod<IEnumerable<MethodInfo>> ("GetAllMethods"))))
          .Return (isBaseDefinitionShadowed);
      // Needed for CreateMethod (will only be called for implicit overrides)
      if (!isBaseDefinitionShadowed)
      {
        _relatedMethodFinderMock
            .Expect (mock => mock.GetMostDerivedVirtualMethod (baseMethod.Name, MethodSignature.Create (baseMethod), proxyType.BaseType))
            .Return (fakeBaseMethod);
      }
    }

    private MethodBodyCreationContext CreateMethodBodyCreationContext (MethodInfo baseMethod)
    {
      var parameterExpressions = ParameterDeclaration.CreateForEquivalentSignature (baseMethod).Select (p => p.Expression);
      return new MethodBodyCreationContext (
          _proxyType, false, parameterExpressions, Type.EmptyTypes, baseMethod.ReturnType, baseMethod, _memberSelectorMock);
    }

    public class A
    {
      // base definition
      public virtual void OverrideHierarchy (int aaa) { }

      public virtual void FinalBaseMethodInB (int i) { }
    }

    public class B : A
    {
      // CreateMethodOverride input
      public override void OverrideHierarchy (int bbb) { }

      protected internal virtual void ProtectedOrInternalVirtualNewSlotMethodInB (int protectedOrInternal) { }
      public override sealed void FinalBaseMethodInB (int i) { }
    }

    public class C : B
    {
      // base inputMethod
      public override void OverrideHierarchy (int ccc) { }
    }

    public class DomainType : C, IDomainInterface
    {
      public virtual void InterfaceMethod (int interfaceMethodOnDomainType) {}
      public virtual TPar GenericMethod<TPar> (int arg1, TPar arg2) where TPar : DomainType, IDisposable, new () { return arg2; }

      public void NonVirtualBaseMethod () {}
    }

    public interface IDomainInterface
    {
      void InterfaceMethod (int i);
    }
    public interface IAddedInterface
    {
      void AddedInterfaceMethod (int addedInterface);
      void InvalidCandidate ();
    }

    public abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method (int paramOnAbstractMethod);
    }
    public abstract class DerivedAbstractTypeLeavesAbstractBaseMethod : AbstractTypeWithOneMethod { }
  }
}