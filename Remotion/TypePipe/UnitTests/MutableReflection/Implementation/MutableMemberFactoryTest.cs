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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MutableMemberFactoryTest
  {
    private MutableMemberFactory _mutableMemberFactory;

    private IRelatedMethodFinder _relatedMethodFinderMock;

    private ProxyType _proxyType;
    private bool _isNewlyCreated;

    [SetUp]
    public void SetUp ()
    {
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _relatedMethodFinderMock = MockRepository.GenerateMock<IRelatedMethodFinder>();

      _mutableMemberFactory = new MutableMemberFactory (memberSelectorMock, _relatedMethodFinderMock);

      _proxyType = ProxyTypeObjectMother.Create (typeof (DomainType));
    }

    [Test]
    public void CreateInitialization ()
    {
      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();

      var result = _mutableMemberFactory.CreateInitialization (
          _proxyType,
          ctx =>
          {
            Assert.That (ctx.DeclaringType, Is.SameAs (_proxyType));
            Assert.That (ctx.IsStatic, Is.False);

            return fakeExpression;
          });

      Assert.That (result, Is.SameAs (fakeExpression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Body provider must return non-null body.")]
    public void CreateInitialization_NullBody ()
    {
      _mutableMemberFactory.CreateInitialization (_proxyType, ctx => null);
    }

    [Test]
    public void CreateField ()
    {
      var field = _mutableMemberFactory.CreateField (_proxyType, "_newField", typeof (string), FieldAttributes.FamANDAssem);

      Assert.That (field.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (field.Name, Is.EqualTo ("_newField"));
      Assert.That (field.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (field.Attributes, Is.EqualTo (FieldAttributes.FamANDAssem));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Field cannot be of type void.\r\nParameter name: type")]
    public void CreateField_VoidType ()
    {
      _mutableMemberFactory.CreateField (_proxyType, "NotImportant", typeof (void), FieldAttributes.ReservedMask);
    }

    [Test]
    public void CreateField_ThrowsIfAlreadyExist ()
    {
      var field = _proxyType.AddField ("Field", typeof (int));

      Assert.That (
          () => _mutableMemberFactory.CreateField (_proxyType, "OtherName", field.FieldType, 0),
          Throws.Nothing);

      Assert.That (
          () => _mutableMemberFactory.CreateField (_proxyType, field.Name, typeof (string), 0),
          Throws.Nothing);

      Assert.That (
          () => _mutableMemberFactory.CreateField (_proxyType, field.Name, field.FieldType, 0),
          Throws.InvalidOperationException.With.Message.EqualTo ("Field with equal name and signature already exists."));
    }

    [Test]
    public void CreateConstructor ()
    {
      var attributes = MethodAttributes.Public;
      var parameters =
          new[]
          {
              ParameterDeclarationObjectMother.Create (typeof (string), "param1"),
              ParameterDeclarationObjectMother.Create (typeof (int), "param2")
          };
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.This.Type, Is.SameAs (_proxyType));
        Assert.That (ctx.IsStatic, Is.False);
        Assert.That (ctx.Parameters.Select (p => p.Type), Is.EqualTo (new[] { typeof (string), typeof (int) }));
        Assert.That (ctx.Parameters.Select (p => p.Name), Is.EqualTo (new[] { "param1", "param2" }));

        return fakeBody;
      };

      var ctor = _mutableMemberFactory.CreateConstructor (_proxyType, attributes, parameters.AsOneTime(), bodyProvider);

      Assert.That (ctor.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (ctor.Name, Is.EqualTo (".ctor"));
      Assert.That (ctor.Attributes, Is.EqualTo (attributes));
      var expectedParameterInfos =
          new[]
          {
              new { ParameterType = parameters[0].Type },
              new { ParameterType = parameters[1].Type }
          };
      var actualParameterInfos = ctor.GetParameters ().Select (pi => new { pi.ParameterType });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, ctor.Body);
    }

    [Test]
    public void CreateConstructor_Static ()
    {
      var attributes = MethodAttributes.Static;
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.IsStatic, Is.True);
        return Expression.Empty();
      };

      var ctor = _mutableMemberFactory.CreateConstructor (_proxyType, attributes, ParameterDeclaration.EmptyParameters, bodyProvider);

      Assert.That (ctor.IsStatic, Is.True);
    }

    [Test]
    public void CreateConstructor_ThrowsForInvalidMethodAttributes ()
    {
      const string message = "The following MethodAttributes are not supported for constructors: " +
                             "Abstract, PinvokeImpl, RequireSecObject, UnmanagedExport, Virtual.\r\nParameter name: attributes";
      Assert.That (() => AddConstructor (_proxyType, MethodAttributes.Abstract), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => AddConstructor (_proxyType, MethodAttributes.PinvokeImpl), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => AddConstructor (_proxyType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => AddConstructor (_proxyType, MethodAttributes.UnmanagedExport), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => AddConstructor (_proxyType, MethodAttributes.Virtual), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "A type initializer (static constructor) cannot have parameters.\r\nParameter name: parameters")]
    public void CreateConstructor_ThrowsIfStaticAndNonEmptyParameters ()
    {
      _mutableMemberFactory.CreateConstructor (_proxyType, MethodAttributes.Static, ParameterDeclarationObjectMother.CreateMultiple (1), ctx => null);
    }

    [Test]
    public void CreateConstructor_ThrowsIfAlreadyExists ()
    {
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      var parametersForEquivalentSignature = ParameterDeclaration.CreateForEquivalentSignature (ctor);
      Func<ConstructorBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty();

      Assert.That (
          () => _mutableMemberFactory.CreateConstructor (_proxyType, 0, ParameterDeclarationObjectMother.CreateMultiple (2), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () =>
          _mutableMemberFactory.CreateConstructor (_proxyType, MethodAttributes.Static, parametersForEquivalentSignature, bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => _mutableMemberFactory.CreateConstructor (_proxyType, 0, parametersForEquivalentSignature, bodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Constructor with equal signature already exists."));
    }

    [Test]
    public void CreateMethod ()
    {
      var name = "Method";
      var attributes = MethodAttributes.Public;
      var returnType = typeof (IComparable);
      var parameterDeclarations =
          new[]
          {
              ParameterDeclarationObjectMother.Create (typeof (double), "hans"),
              ParameterDeclarationObjectMother.Create (typeof (string), "franz")
          };
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.This.Type, Is.SameAs (_proxyType));
        Assert.That (ctx.Parameters.Select (p => p.Type), Is.EqualTo (new[] { typeof (double), typeof (string) }));
        Assert.That (ctx.Parameters.Select (p => p.Name), Is.EqualTo (new[] { "hans", "franz" }));
        Assert.That (ctx.IsStatic, Is.False);
        Assert.That (ctx.HasBaseMethod, Is.False);

        return fakeBody;
      };

      var method = _mutableMemberFactory.CreateMethod (
          _proxyType, name, attributes, returnType, parameterDeclarations.AsOneTime(), bodyProvider);

      Assert.That (method.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (method.Name, Is.EqualTo (name));
      Assert.That (method.Attributes, Is.EqualTo (attributes));
      Assert.That (method.ReturnType, Is.EqualTo (returnType));

      var returnParameter = method.ReturnParameter;
      Assertion.IsNotNull (returnParameter);
      Assert.That (returnParameter.Position, Is.EqualTo (-1));
      Assert.That (returnParameter.Name, Is.Null);
      Assert.That (returnParameter.ParameterType, Is.SameAs (returnType));
      Assert.That (returnParameter.Attributes, Is.EqualTo (ParameterAttributes.None));

      Assert.That (method.BaseMethod, Is.Null);
      Assert.That (method.IsGenericMethod, Is.False);
      Assert.That (method.IsGenericMethodDefinition, Is.False);
      Assert.That (method.ContainsGenericParameters, Is.False);
      var expectedParameterInfos =
          new[]
          {
              new { ParameterType = parameterDeclarations[0].Type },
              new { ParameterType = parameterDeclarations[1].Type }
          };
      var actualParameterInfos = method.GetParameters ().Select (pi => new { pi.ParameterType });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
      var expectedBody = Expression.Convert (fakeBody, returnType);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void CreateMethod_Static ()
    {
      var name = "StaticMethod";
      var attributes = MethodAttributes.Static;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);

      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.IsStatic, Is.True);

        return ExpressionTreeObjectMother.GetSomeExpression (returnType);
      };
      var method = _mutableMemberFactory.CreateMethod (_proxyType, name, attributes, returnType, parameterDeclarations, bodyProvider);

      Assert.That (method.IsStatic, Is.True);
    }

    [Test]
    public void CreateMethod_Shadowing_NonVirtual ()
    {
      var shadowedMethod = _proxyType.GetMethod ("ToString");
      Assert.That (shadowedMethod, Is.Not.Null);
      Assert.That (shadowedMethod.DeclaringType, Is.SameAs (typeof (object)));

      var nonVirtualAttributes = (MethodAttributes) 0;
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.HasBaseMethod, Is.False);
        return Expression.Constant ("string");
      };
      var method = _mutableMemberFactory.CreateMethod (
          _proxyType,
          "ToString",
          nonVirtualAttributes,
          typeof (string),
          ParameterDeclaration.EmptyParameters,
          bodyProvider);

      Assert.That (method, Is.Not.Null.And.Not.EqualTo (shadowedMethod));
      Assert.That (method.BaseMethod, Is.Null);
      Assert.That (method.GetBaseDefinition(), Is.SameAs (method));
    }

    [Test]
    public void CreateMethod_Shadowing_VirtualAndNewSlot ()
    {
      var shadowedMethod = _proxyType.GetMethod ("ToString");
      Assert.That (shadowedMethod, Is.Not.Null);
      Assert.That (shadowedMethod.DeclaringType, Is.SameAs (typeof (object)));

      var nonVirtualAttributes = MethodAttributes.Virtual | MethodAttributes.NewSlot;
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.HasBaseMethod, Is.False);
        return Expression.Constant ("string");
      };
      var method = _mutableMemberFactory.CreateMethod (
          _proxyType,
          "ToString",
          nonVirtualAttributes,
          typeof (string),
          ParameterDeclaration.EmptyParameters,
          bodyProvider);

      Assert.That (method, Is.Not.Null.And.Not.EqualTo (shadowedMethod));
      Assert.That (method.BaseMethod, Is.Null);
      Assert.That (method.GetBaseDefinition(), Is.SameAs (method));
    }

    [Test]
    public void CreateMethod_ImplicitOverride ()
    {
      var fakeOverridenMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.OverrideHierarchy (7));
      _relatedMethodFinderMock
          .Expect (mock => mock.GetMostDerivedVirtualMethod ("Method", new MethodSignature (typeof (int), Type.EmptyTypes, 0), _proxyType.BaseType))
          .Return (fakeOverridenMethod);

      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.HasBaseMethod, Is.True);
        Assert.That (ctx.BaseMethod, Is.SameAs (fakeOverridenMethod));

        return Expression.Default (typeof (int));
      };
      var method = _mutableMemberFactory.CreateMethod (
          _proxyType,
          "Method",
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof (int),
          ParameterDeclaration.EmptyParameters,
          bodyProvider);

      _relatedMethodFinderMock.VerifyAllExpectations ();
      Assert.That (method.BaseMethod, Is.EqualTo (fakeOverridenMethod));
      Assert.That (method.GetBaseDefinition (), Is.EqualTo (fakeOverridenMethod.GetBaseDefinition ()));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Non-abstract methods must have a body.\r\nParameter name: bodyProvider")]
    public void CreateMethod_ThrowsIfNotAbstractAndNullBodyProvider ()
    {
      _mutableMemberFactory.CreateMethod (_proxyType, "NotImportant", 0, typeof (void), ParameterDeclaration.EmptyParameters, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Abstract methods cannot have a body.\r\nParameter name: bodyProvider")]
    public void CreateMethod_ThrowsIfAbstractAndBodyProvider ()
    {
      _mutableMemberFactory.CreateMethod (
          _proxyType, "NotImportant", MethodAttributes.Abstract, typeof (void), ParameterDeclaration.EmptyParameters, ctx => null);
    }

    [Test]
    public void CreateMethod_ThrowsForInvalidMethodAttributes ()
    {
      const string message = "The following MethodAttributes are not supported for methods: " +
                             "PinvokeImpl, RequireSecObject, UnmanagedExport.\r\nParameter name: attributes";
      Assert.That (() => CreateMethod (_proxyType, MethodAttributes.PinvokeImpl), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateMethod (_proxyType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => CreateMethod (_proxyType, MethodAttributes.UnmanagedExport), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Abstract methods must also be virtual.\r\nParameter name: attributes")]
    public void CreateMethod_ThrowsIfAbstractAndNotVirtual ()
    {
      _mutableMemberFactory.CreateMethod (
          _proxyType, "NotImportant", MethodAttributes.Abstract, typeof (void), ParameterDeclaration.EmptyParameters, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "NewSlot methods must also be virtual.\r\nParameter name: attributes")]
    public void CreateMethod_ThrowsIfNonVirtualAndNewSlot ()
    {
      _mutableMemberFactory.CreateMethod (
          _proxyType, "NotImportant", MethodAttributes.NewSlot, typeof (void), ParameterDeclaration.EmptyParameters, ctx => Expression.Empty());
    }

    [Test]
    public void CreateMethod_ThrowsIfAlreadyExists ()
    {
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty();
      var method = _proxyType.AddMethod ("Method", 0, typeof (void), ParameterDeclarationObjectMother.CreateMultiple (2), bodyProvider);

      Assert.That (
          () => _mutableMemberFactory.CreateMethod (
              _proxyType, "OtherName", 0, method.ReturnType, ParameterDeclaration.CreateForEquivalentSignature (method), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => _mutableMemberFactory.CreateMethod (
              _proxyType, method.Name, 0, typeof (int), ParameterDeclaration.CreateForEquivalentSignature (method), ctx => Expression.Constant (7)),
          Throws.Nothing);

      Assert.That (
          () => _mutableMemberFactory.CreateMethod (
              _proxyType, method.Name, 0, method.ReturnType, ParameterDeclarationObjectMother.CreateMultiple (3), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => _mutableMemberFactory.CreateMethod (
              _proxyType, method.Name, 0, method.ReturnType, ParameterDeclaration.CreateForEquivalentSignature (method), bodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Method with equal name and signature already exists."));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot override final method 'B.FinalBaseMethodInB'.")]
    public void CreateMethod_ThrowsIfOverridingFinalMethod ()
    {
      var signature = new MethodSignature (typeof (void), Type.EmptyTypes, 0);
      var fakeBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.FinalBaseMethodInB (7));
      _relatedMethodFinderMock
          .Expect (mock => mock.GetMostDerivedVirtualMethod ("MethodName", signature, _proxyType.BaseType))
          .Return (fakeBaseMethod);

      _mutableMemberFactory.CreateMethod (
          _proxyType,
          "MethodName",
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof (void),
          ParameterDeclaration.EmptyParameters,
          ctx => Expression.Empty ());
    }

    [Test]
    public void CreateExplicitOverride ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => body;

      var result = _mutableMemberFactory.CreateExplicitOverride (_proxyType, method, bodyProvider);

      Assert.That (result.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (result.Name, Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MutableMemberFactoryTest.A.OverrideHierarchy"));
      Assert.That (result.Attributes, Is.EqualTo (MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig));
      Assert.That (result.ReturnType, Is.SameAs (typeof (void)));
      var parameters = result.GetParameters();
      Assert.That (parameters, Has.Length.EqualTo (1));
      Assert.That (parameters[0].Name, Is.EqualTo ("aaa"));
      Assert.That (parameters[0].ParameterType, Is.SameAs (typeof (int)));
      Assert.That (result.Body, Is.SameAs (body));
      Assert.That (result.BaseMethod, Is.Null);
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { method }));
    }

    [Test]
    public void GetOrCreateMethodOverride_ExistingOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ToString());
      Assert.That (method, Is.Not.EqualTo (baseDefinition));

      var fakeExistingOverride = MutableMethodInfoObjectMother.Create();
      _relatedMethodFinderMock
          .Expect (mock => mock.GetOverride (Arg.Is (baseDefinition), Arg<IEnumerable<MutableMethodInfo>>.List.Equal (_proxyType.AddedMethods)))
          .Return (fakeExistingOverride);

      var result = _mutableMemberFactory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);

      _relatedMethodFinderMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeExistingOverride));
      Assert.That (_isNewlyCreated, Is.False);
    }

    [Test]
    public void GetOrCreateMethodOverride_BaseMethod_ImplicitOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      var inputMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.OverrideHierarchy (7));
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((C obj) => obj.OverrideHierarchy (7));

      CallAndCheckGetOrAddMethod (
          baseDefinition,
          inputMethod,
          baseMethod,
          false,
          "aaa",
          baseMethod,
          new MethodInfo[0],
          "OverrideHierarchy",
          MethodAttributes.Public,
          MethodAttributes.ReuseSlot);
    }

    [Test]
    public void GetOrCreateMethodOverride_BaseMethod_ImplicitOverride_AdjustsAttributes ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.ProtectedOrInternalVirtualNewSlotMethodInB (7));
      var inputMethod = baseDefinition;
      var baseMethod = baseDefinition;
      Assert.That (baseMethod.IsFamilyOrAssembly, Is.True);
      Assert.That (baseMethod.Attributes.IsSet (MethodAttributes.NewSlot), Is.True);

      CallAndCheckGetOrAddMethod (
          baseDefinition,
          inputMethod,
          baseMethod,
          false,
          "protectedOrInternal",
          baseMethod,
          new MethodInfo[0],
          "ProtectedOrInternalVirtualNewSlotMethodInB",
          MethodAttributes.Family,
          MethodAttributes.ReuseSlot);
    }

    [Test]
    public void GetOrCreateMethodOverride_BaseMethod_ImplicitOverride_Abstract ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeWithOneMethod obj) => obj.Method());
      Assert.That (baseMethod.Attributes.IsSet (MethodAttributes.Abstract), Is.True);
      var proxyType = ProxyTypeObjectMother.Create (
          typeof (DerivedAbstractTypeLeavesAbstractBaseMethod), relatedMethodFinder: _relatedMethodFinderMock);
      SetupExpectationsForGetOrAddMethod (baseMethod, baseMethod, false, baseMethod, proxyType);

      var result = _mutableMemberFactory.GetOrCreateOverride (proxyType, baseMethod, out _isNewlyCreated);

      _relatedMethodFinderMock.VerifyAllExpectations();
      Assert.That (result.BaseMethod, Is.SameAs (baseMethod));
      Assert.That (result.IsAbstract, Is.True);
      Assert.That (_isNewlyCreated, Is.True);
    }

    [Test]
    public void GetOrCreateMethodOverride_ShadowedBaseMethod_ExplicitOverride ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.OverrideHierarchy (7));
      var inputMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.OverrideHierarchy (7));
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((C obj) => obj.OverrideHierarchy (7));
      Assert.That (baseMethod.Attributes.IsSet (MethodAttributes.NewSlot), Is.False);

      CallAndCheckGetOrAddMethod (
          baseDefinition,
          inputMethod,
          baseMethod,
          true,
          "aaa",
          null,
          new[] { baseDefinition },
          "Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MutableMemberFactoryTest.A.OverrideHierarchy",
          MethodAttributes.Private,
          MethodAttributes.NewSlot);
    }

    [Test]
    public void GetOrCreateMethodOverride_InterfaceMethod_AddImplementation ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.AddedInterfaceMethod (7));
      _proxyType.AddInterface (typeof (IAddedInterface));

      var result = _mutableMemberFactory.GetOrCreateOverride (_proxyType, interfaceMethod, out _isNewlyCreated);

      Assert.That (_isNewlyCreated, Is.True);
      CheckMethodData (
          result,
          "AddedInterfaceMethod",
          MethodAttributes.Public,
          MethodAttributes.NewSlot | MethodAttributes.Abstract,
          expectedParameterName: "addedInterface",
          expectedBaseMethod: null,
          expectedAddedExplicitBaseDefinitions: new MethodInfo[0]);

      Assert.That (result.GetBaseDefinition(), Is.SameAs (result));
      Assert.That (result.IsAbstract, Is.True);
    }

    [Test]
    public void GetOrCreateMethodOverride_InterfaceMethod_OverrideImplementationInBase ()
    {
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.InterfaceMethod (7));
      var implementation = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.InterfaceMethod (7));

      CallAndCheckGetOrAddMethod (
          implementation,
          interfaceMethod,
          implementation,
          false,
          "interfaceMethodOnDomainType",
          implementation,
          new MethodInfo[0],
          "InterfaceMethod",
          MethodAttributes.Public,
          MethodAttributes.ReuseSlot);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "Interface method 'InvalidCandidate' cannot be implemented because a method with equal name and signature already exists. "
        + "Use ProxyType.AddExplicitOverride to create an explicit implementation.")]
    public void GetOrCreateMethodOverride_InterfaceMethod_InvalidCandidate ()
    {
      _proxyType.AddInterface (typeof (IAddedInterface));
      _proxyType.AddMethod ("InvalidCandidate"); // Not virtual, therefore no implicit override/implementation.
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.InvalidCandidate());
      _mutableMemberFactory.GetOrCreateOverride (_proxyType, interfaceMethod, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method is declared by a type outside of the proxy base class hierarchy: 'String'.\r\nParameter name: baseMethod")]
    public void GetOrCreateMethodOverride_UnrelatedDeclaringType ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((string obj) => obj.Trim ());
      _mutableMemberFactory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Only virtual methods can be overridden.")]
    public void GetOrCreateMethodOverride_NonVirtualMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualBaseMethod());
      _mutableMemberFactory.GetOrCreateOverride (_proxyType, method, out _isNewlyCreated);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot override final method 'B.FinalBaseMethodInB'.")]
    public void GetOrCreateMethodOverride_FinalBaseMethod ()
    {
      var baseDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((A obj) => obj.FinalBaseMethodInB (7));
      var inputMethod = baseDefinition;
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.FinalBaseMethodInB (7));
      var isBaseDefinitionShadowed = BooleanObjectMother.GetRandomBoolean ();

      SetupExpectationsForGetOrAddMethod (baseDefinition, baseMethod, isBaseDefinitionShadowed, null);

      _mutableMemberFactory.GetOrCreateOverride (_proxyType, inputMethod, out _isNewlyCreated);
    }

    private void CallAndCheckGetOrAddMethod (
        MethodInfo baseDefinition,
        MethodInfo inputMethod,
        MethodInfo baseMethod,
        bool isBaseDefinitionShadowed,
        string expectedParameterName,
        MethodInfo expectedBaseMethod,
        IEnumerable<MethodInfo> expectedAddedExplicitBaseDefinitions,
        string expectedOverrideMethodName,
        MethodAttributes expectedOverrideVisibility,
        MethodAttributes expectedVtableLayout)
    {
      SetupExpectationsForGetOrAddMethod (baseDefinition, baseMethod, isBaseDefinitionShadowed, expectedBaseMethod);

      var result = _mutableMemberFactory.GetOrCreateOverride (_proxyType, inputMethod, out _isNewlyCreated);

      _relatedMethodFinderMock.VerifyAllExpectations();
      Assert.That (_isNewlyCreated, Is.True);

      CheckMethodData (
          result,
          expectedOverrideMethodName,
          expectedOverrideVisibility,
          expectedVtableLayout,
          expectedParameterName,
          expectedBaseMethod,
          expectedAddedExplicitBaseDefinitions);

      Assert.That (result.Body, Is.InstanceOf<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) result.Body;
      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter>());
      var baceCallMethodInfoAdapter = (NonVirtualCallMethodInfoAdapter) methodCallExpression.Method;
      Assert.That (baceCallMethodInfoAdapter.AdaptedMethod, Is.SameAs (baseMethod));
    }

    private static void CheckMethodData (
        MutableMethodInfo result,
        string expectedMethodName,
        MethodAttributes expectedVisibility,
        MethodAttributes expectedVtableLayout,
        string expectedParameterName,
        MethodInfo expectedBaseMethod,
        IEnumerable<MethodInfo> expectedAddedExplicitBaseDefinitions)
    {
      Assert.That (result.Name, Is.EqualTo (expectedMethodName));
      var expectedAttributes = expectedVisibility | expectedVtableLayout | MethodAttributes.Virtual | MethodAttributes.HideBySig;
      Assert.That (result.Attributes, Is.EqualTo (expectedAttributes));
      Assert.That (result.ReturnType, Is.SameAs (typeof (void)));
      var parameter = result.GetParameters().Single();
      Assert.That (parameter.ParameterType, Is.SameAs (typeof (int)));
      Assert.That (parameter.Name, Is.EqualTo (expectedParameterName));
      Assert.That (result.BaseMethod, Is.SameAs (expectedBaseMethod));
      Assert.That (result.AddedExplicitBaseDefinitions, Is.EqualTo (expectedAddedExplicitBaseDefinitions));
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
          .Expect (mock => mock.IsShadowed (Arg.Is (baseDefinition), Arg<IEnumerable<MethodInfo>>.List.Equal (GetAllMethods (proxyType))))
          .Return (isBaseDefinitionShadowed);
      // Needed for CreateMethod (will only be called for implicit overrides)
      if (!isBaseDefinitionShadowed)
      {
        _relatedMethodFinderMock
            .Expect (mock => mock.GetMostDerivedVirtualMethod (baseMethod.Name, MethodSignature.Create (baseMethod), proxyType.BaseType))
            .Return (fakeBaseMethod);
      }
    }
    
    private MutableConstructorInfo AddConstructor (ProxyType proxyType, MethodAttributes attributes)
    {
      return _mutableMemberFactory.CreateConstructor (
          proxyType, attributes, ParameterDeclaration.EmptyParameters, ctx => Expression.Empty ());
    }

    private MethodInfo CreateMethod (ProxyType proxyType, MethodAttributes attributes)
    {
      return _mutableMemberFactory.CreateMethod (
          proxyType,
          "dummy",
          attributes,
          typeof (void),
          ParameterDeclaration.EmptyParameters,
          ctx => Expression.Empty());
    }

    private IEnumerable<MethodInfo> GetAllMethods (ProxyType proxyType)
    {
      return (IEnumerable<MethodInfo>) PrivateInvoke.InvokeNonPublicMethod (proxyType, "GetAllMethods");
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
      public virtual void InterfaceMethod (int interfaceMethodOnDomainType) { }
      public void NonVirtualBaseMethod () { }
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

    abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method ();
    }
    abstract class DerivedAbstractTypeLeavesAbstractBaseMethod : AbstractTypeWithOneMethod { }

  }
}