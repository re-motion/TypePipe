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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class MethodFactoryTest
  {
    private IRelatedMethodFinder _relatedMethodFinderMock;

    private MethodFactory _factory;

    private ProxyType _proxyType;

    [SetUp]
    public void SetUp ()
    {
      var memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector> ();
      _relatedMethodFinderMock = MockRepository.GenerateMock<IRelatedMethodFinder> ();

      _factory = new MethodFactory (memberSelectorMock, _relatedMethodFinderMock);

      _proxyType = ProxyTypeObjectMother.Create (baseType: typeof (DomainType));
    }

    [Test]
    public void CreateMethod ()
    {
      var name = "Method";
      var attributes = MethodAttributes.Public;
      var baseConstraint = ReflectionObjectMother.GetSomeClassType ();
      var interfaceConstraint = ReflectionObjectMother.GetSomeInterfaceType ();
      GenericParameterContext genericParameterContext = null;
      Type firstGenericParameter = null;
      Func<GenericParameterContext, IEnumerable<Type>> constraintProvider = ctx =>
      {
        genericParameterContext = ctx;
        Assert.That (ctx.GenericParameters, Has.Count.EqualTo (2));
        Assert.That (ctx.GenericParameters[1].GenericParameterPosition, Is.EqualTo (1));

        firstGenericParameter = ctx.GenericParameters[0];
        Assert.That (firstGenericParameter.DeclaringMethod, Is.Null);
        Assert.That (firstGenericParameter.GenericParameterPosition, Is.EqualTo (0));
        Assert.That (firstGenericParameter.Name, Is.EqualTo ("T1"));
        Assert.That (firstGenericParameter.Namespace, Is.EqualTo (_proxyType.Namespace));
        Assert.That (firstGenericParameter.GenericParameterAttributes, Is.EqualTo (GenericParameterAttributes.Covariant));

        return new[] { baseConstraint, interfaceConstraint }.AsOneTime();
      };
      var genericParameters =
          new[]
          {
              GenericParameterDeclarationObjectMother.Create ("T1", GenericParameterAttributes.Covariant, constraintProvider),
              GenericParameterDeclarationObjectMother.Create()
          };
      var returnType = typeof (IComparable);
      Func<GenericParameterContext, Type> returnTypeProvider = ctx =>
      {
        Assert.That (ctx, Is.Not.Null.And.SameAs (genericParameterContext));
        return returnType;
      };
      var parameter = ParameterDeclarationObjectMother.Create (name: "paramName");
      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider = ctx =>
      {
        Assert.That (ctx, Is.Not.Null.And.SameAs (genericParameterContext));
        return new[] { parameter }.AsOneTime ();
      };
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.This.Type, Is.SameAs (_proxyType));
        Assert.That (ctx.Parameters.Single ().Name, Is.EqualTo ("paramName"));
        Assert.That (ctx.IsStatic, Is.False);
        Assert.That (ctx.GenericParameters, Is.EqualTo (genericParameterContext.GenericParameters));
        Assert.That (ctx.ReturnType, Is.SameAs (returnType));
        Assert.That (ctx.HasBaseMethod, Is.False);

        return fakeBody;
      };

      var method = _factory.CreateMethod (
          _proxyType, name, attributes, genericParameters.AsOneTime (), returnTypeProvider, parameterProvider, bodyProvider);

      Assert.That (method.DeclaringType, Is.SameAs (_proxyType));
      Assert.That (method.Name, Is.EqualTo (name));
      Assert.That (method.Attributes, Is.EqualTo (attributes));
      Assert.That (method.ReturnType, Is.SameAs (returnType));
      Assert.That (method.BaseMethod, Is.Null);

      var returnParameter = method.ReturnParameter;
      Assertion.IsNotNull (returnParameter);
      Assert.That (returnParameter.Position, Is.EqualTo (-1));
      Assert.That (returnParameter.Name, Is.Null);
      Assert.That (returnParameter.ParameterType, Is.SameAs (returnType));
      Assert.That (returnParameter.Attributes, Is.EqualTo (ParameterAttributes.None));

      Assert.That (method.GetGenericArguments (), Has.Length.EqualTo (2));
      var actualFirstGenericParameter = method.GetGenericArguments ()[0];
      Assert.That (actualFirstGenericParameter, Is.SameAs (firstGenericParameter));
      Assert.That (actualFirstGenericParameter.DeclaringMethod, Is.SameAs (method));
      Assert.That (actualFirstGenericParameter.GetGenericParameterConstraints (), Is.EqualTo (new[] { baseConstraint, interfaceConstraint }));

      Assert.That (method.GetParameters ().Single ().Name, Is.EqualTo (parameter.Name));
      var expectedBody = Expression.Convert (fakeBody, returnType);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void CreateMethod_Static ()
    {
      var name = "StaticMethod";
      var attributes = MethodAttributes.Static;
      var returnType = ReflectionObjectMother.GetSomeType ();
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);

      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.IsStatic, Is.True);

        return ExpressionTreeObjectMother.GetSomeExpression (returnType);
      };
      var method = CallCreateMethod (_proxyType, name, attributes, returnType, parameterDeclarations, bodyProvider);

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
      var method = CallCreateMethod (
          _proxyType,
          "ToString",
          nonVirtualAttributes,
          typeof (string),
          ParameterDeclaration.None,
          bodyProvider);

      Assert.That (method, Is.Not.Null.And.Not.EqualTo (shadowedMethod));
      Assert.That (method.BaseMethod, Is.Null);
      Assert.That (method.GetBaseDefinition (), Is.SameAs (method));
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
      var method = CallCreateMethod (
          _proxyType,
          "ToString",
          nonVirtualAttributes,
          typeof (string),
          ParameterDeclaration.None,
          bodyProvider);

      Assert.That (method, Is.Not.Null.And.Not.EqualTo (shadowedMethod));
      Assert.That (method.BaseMethod, Is.Null);
      Assert.That (method.GetBaseDefinition (), Is.SameAs (method));
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
      var method = CallCreateMethod (
          _proxyType,
          "Method",
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof (int),
          ParameterDeclaration.None,
          bodyProvider);

      _relatedMethodFinderMock.VerifyAllExpectations ();
      Assert.That (method.BaseMethod, Is.EqualTo (fakeOverridenMethod));
      Assert.That (method.GetBaseDefinition (), Is.EqualTo (fakeOverridenMethod.GetBaseDefinition ()));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException), ExpectedMessage = "Non-abstract methods must have a body.\r\nParameter name: bodyProvider")]
    public void CreateMethod_ThrowsIfNotAbstractAndNullBodyProvider ()
    {
      CallCreateMethod (_proxyType, "NotImportant", 0, typeof (void), ParameterDeclaration.None, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Abstract methods cannot have a body.\r\nParameter name: bodyProvider")]
    public void CreateMethod_ThrowsIfAbstractAndBodyProvider ()
    {
      CallCreateMethod (_proxyType, "NotImportant", MethodAttributes.Abstract, typeof (void), ParameterDeclaration.None, ctx => null);
    }

    [Test]
    public void CreateMethod_ThrowsForInvalidMethodAttributes ()
    {
      var message = "The following MethodAttributes are not supported for methods: RequireSecObject.\r\nParameter name: attributes";
      Assert.That (() => CreateMethod (_proxyType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Abstract methods must also be virtual.\r\nParameter name: attributes")]
    public void CreateMethod_ThrowsIfAbstractAndNotVirtual ()
    {
      CallCreateMethod (_proxyType, "NotImportant", MethodAttributes.Abstract, typeof (void), ParameterDeclaration.None, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "NewSlot methods must also be virtual.\r\nParameter name: attributes")]
    public void CreateMethod_ThrowsIfNonVirtualAndNewSlot ()
    {
      CallCreateMethod (_proxyType, "NotImportant", MethodAttributes.NewSlot, typeof (void), ParameterDeclaration.None, ctx => Expression.Empty ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Provider must not return null.\r\nParameter name: returnTypeProvider")]
    public void CreateMethod_ThrowsForNullReturningReturnTypeProvider ()
    {
      _factory.CreateMethod (
          _proxyType, "NotImportant", 0, GenericParameterDeclaration.None, ctx => null, ctx => ParameterDeclaration.None, ctx => Expression.Empty ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Provider must not return null.\r\nParameter name: parameterProvider")]
    public void CreateMethod_ThrowsForNullReturningParameterProvider ()
    {
      _factory.CreateMethod (
          _proxyType, "NotImportant", 0, GenericParameterDeclaration.None, ctx => typeof (int), ctx => null, ctx => Expression.Empty ());
    }

    [Test]
    public void CreateMethod_ThrowsIfAlreadyExists ()
    {
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty ();
      var method = _proxyType.AddMethod ("Method", 0, typeof (void), ParameterDeclarationObjectMother.CreateMultiple (2), bodyProvider);

      Assert.That (
          () => CallCreateMethod (_proxyType, "OtherName", 0, method.ReturnType, ParameterDeclaration.CreateForEquivalentSignature (method), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => CallCreateMethod (
              _proxyType, method.Name, 0, typeof (int), ParameterDeclaration.CreateForEquivalentSignature (method), ctx => Expression.Constant (7)),
          Throws.Nothing);

      Assert.That (
          () => CallCreateMethod (
              _proxyType, method.Name, 0, method.ReturnType, ParameterDeclarationObjectMother.CreateMultiple (3), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => CallCreateMethod (
              _proxyType, method.Name, 0, method.ReturnType, ParameterDeclaration.CreateForEquivalentSignature (method), bodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Method with equal name and signature already exists."));
    }

    [Test]
    public void CreateMethod_ThrowsIfAlreadyExists_Generic ()
    {
      var genericParameters = new[] { new GenericParameterDeclaration ("T1"), new GenericParameterDeclaration ("T2") };
      Func<GenericParameterContext, Type> returnTypeProvider = ctx => typeof (void);
      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider =
          ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "t1"), new ParameterDeclaration (ctx.GenericParameters[1], "t2") };
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty ();
      var method = _proxyType.AddGenericMethod ("GenericMethod", 0, genericParameters, returnTypeProvider, parameterProvider, bodyProvider);

      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider1 =
          ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "t1") };
      Assert.That (
          () =>
          _factory.CreateMethod (_proxyType, method.Name, 0, new[] { genericParameters[0] }, returnTypeProvider, parameterProvider1, bodyProvider),
          Throws.Nothing);

      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider2 =
          ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[1], "t1"), new ParameterDeclaration (ctx.GenericParameters[0], "t2") };
      Assert.That (
          () =>
          _factory.CreateMethod (_proxyType, method.Name, 0, genericParameters, returnTypeProvider, parameterProvider2, bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => _factory.CreateMethod (_proxyType, method.Name, 0, genericParameters, returnTypeProvider, parameterProvider, bodyProvider),
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

      CallCreateMethod (
          _proxyType,
          "MethodName",
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof (void),
          ParameterDeclaration.None,
          ctx => Expression.Empty ());
    }

    private MutableMethodInfo CreateMethod (ProxyType proxyType, MethodAttributes attributes)
    {
      return CallCreateMethod (
          proxyType,
          "dummy",
          attributes,
          typeof (void),
          ParameterDeclaration.None,
          ctx => Expression.Empty ());
    }

    private MutableMethodInfo CallCreateMethod (
        ProxyType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      return _factory.CreateMethod (
          declaringType, name, attributes, GenericParameterDeclaration.None, ctx => returnType, ctx => parameters, bodyProvider);
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

    public abstract class AbstractTypeWithOneMethod
    {
      public abstract void Method ();
    }
    public abstract class DerivedAbstractTypeLeavesAbstractBaseMethod : AbstractTypeWithOneMethod { }
  }
}