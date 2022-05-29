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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.Expressions;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.MutableReflection.MemberSignatures;
using Remotion.Utilities;
using Moq;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class MethodFactoryTest
  {
    private Mock<IRelatedMethodFinder> _relatedMethodFinderMock;

    private MethodFactory _factory;

    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _relatedMethodFinderMock = new Mock<IRelatedMethodFinder> (MockBehavior.Strict);

      _factory = new MethodFactory (_relatedMethodFinderMock.Object);

      _mutableType = MutableTypeObjectMother.Create (baseType: typeof (DomainType));
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
        Assert.That (firstGenericParameter.Namespace, Is.EqualTo (_mutableType.Namespace));
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
        Assert.That (ctx.This.Type, Is.SameAs (_mutableType));
        Assert.That (ctx.Parameters.Single ().Name, Is.EqualTo ("paramName"));
        Assert.That (ctx.IsStatic, Is.False);
        Assert.That (ctx.GenericParameters, Is.EqualTo (genericParameterContext.GenericParameters));
        Assert.That (ctx.ReturnType, Is.SameAs (returnType));
        Assert.That (ctx.HasBaseMethod, Is.False);

        return fakeBody;
      };

      var method = _factory.CreateMethod (
          _mutableType, name, attributes, genericParameters.AsOneTime(), returnTypeProvider, parameterProvider, bodyProvider);

      Assert.That (method.DeclaringType, Is.SameAs (_mutableType));
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
      var method = CallCreateMethod (_mutableType, name, attributes, returnType, parameterDeclarations, bodyProvider);

      Assert.That (method.IsStatic, Is.True);
    }

    [Test]
    public void CreateMethod_OnInterface ()
    {
      var mutableType = MutableTypeObjectMother.CreateInterface();
      var name = "name";
      var attributes = MethodAttributes.Virtual | MethodAttributes.ReuseSlot;

      CallCreateMethod (mutableType, name, attributes, typeof (void), ParameterDeclaration.None, ctx => Expression.Empty());

      var signature = new MethodSignature (typeof (void), Type.EmptyTypes, 0);
      _relatedMethodFinderMock.Verify (mock => mock.GetMostDerivedVirtualMethod (name, signature, null), Times.Never());
    }

    [Test]
    public void CreateMethod_Shadowing_NonVirtual ()
    {
      var shadowedMethod = _mutableType.GetMethod ("ToString");
      Assert.That (shadowedMethod, Is.Not.Null);
      Assert.That (shadowedMethod.DeclaringType, Is.SameAs (typeof (object)));

      var nonVirtualAttributes = (MethodAttributes) 0;
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.HasBaseMethod, Is.False);
        return Expression.Constant ("string");
      };
      var method = CallCreateMethod (
          _mutableType,
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
      var shadowedMethod = _mutableType.GetMethod ("ToString");
      Assert.That (shadowedMethod, Is.Not.Null);
      Assert.That (shadowedMethod.DeclaringType, Is.SameAs (typeof (object)));

      var nonVirtualAttributes = MethodAttributes.Virtual | MethodAttributes.NewSlot;
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.HasBaseMethod, Is.False);
        return Expression.Constant ("string");
      };
      var method = CallCreateMethod (
          _mutableType,
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
          .Setup (mock => mock.GetMostDerivedVirtualMethod ("Method", new MethodSignature (typeof (int), Type.EmptyTypes, 0), _mutableType.BaseType))
          .Returns (fakeOverridenMethod)
          .Verifiable();

      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.HasBaseMethod, Is.True);
        Assert.That (ctx.BaseMethod, Is.SameAs (fakeOverridenMethod));

        return Expression.Default (typeof (int));
      };
      var method = CallCreateMethod (
          _mutableType,
          "Method",
          MethodAttributes.Public | MethodAttributes.Virtual,
          typeof (int),
          ParameterDeclaration.None,
          bodyProvider);

      _relatedMethodFinderMock.Verify();
      Assert.That (method.BaseMethod, Is.EqualTo (fakeOverridenMethod));
      Assert.That (method.GetBaseDefinition (), Is.EqualTo (fakeOverridenMethod.GetBaseDefinition ()));
    }

    [Test]
    public void CreateMethod_ImplicitOverride_FinalBaseMethod ()
    {
      var signature = new MethodSignature (typeof (void), Type.EmptyTypes, 0);
      var fakeBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.FinalBaseMethodInB (7));
      _relatedMethodFinderMock
          .Setup (mock => mock.GetMostDerivedVirtualMethod ("MethodName", signature, _mutableType.BaseType))
          .Returns (fakeBaseMethod)
          .Verifiable();
      Assert.That (
          () => CallCreateMethod (
              _mutableType,
              "MethodName",
              MethodAttributes.Public | MethodAttributes.Virtual,
              typeof (void),
              ParameterDeclaration.None,
              ctx => Expression.Empty()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Cannot override final method 'B.FinalBaseMethodInB'."));
    }

    [Test]
    public void CreateMethod_ImplicitOverride_InaccessibleBaseMethod ()
    {
      var signature = new MethodSignature (typeof (void), Type.EmptyTypes, 0);
      var fakeBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.InaccessibleBaseMethodInB (7));
      _relatedMethodFinderMock
          .Setup (mock => mock.GetMostDerivedVirtualMethod ("MethodName", signature, _mutableType.BaseType))
          .Returns (fakeBaseMethod)
          .Verifiable();
      Assert.That (
          () => CallCreateMethod (
              _mutableType,
              "MethodName",
              MethodAttributes.Public | MethodAttributes.Virtual,
              typeof (void),
              ParameterDeclaration.None,
              ctx => Expression.Empty()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("Cannot override method 'B.InaccessibleBaseMethodInB' as it is not visible from the proxy."));
    }

    [Test]
    public void CreateMethod_ThrowsIfNotAbstractAndNullBodyProvider ()
    {
      Assert.That (
          () => CallCreateMethod (_mutableType, "NotImportant", 0, typeof (void), ParameterDeclaration.None, null),
          Throws.ArgumentNullException
              .With.ArgumentExceptionMessageEqualTo (
                  "Non-abstract methods must have a body.", "bodyProvider"));
    }

    [Test]
    public void CreateMethod_ThrowsIfAbstractAndBodyProvider ()
    {
      Assert.That (
          () => CallCreateMethod (_mutableType, "NotImportant", MethodAttributes.Abstract, typeof (void), ParameterDeclaration.None, ctx => null),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Abstract methods cannot have a body.", "bodyProvider"));
    }

    [Test]
    public void CreateMethod_ThrowsForInvalidMethodAttributes ()
    {
      var message = "The following MethodAttributes are not supported for methods: RequireSecObject.";
      var paramName = "attributes";
      Assert.That (() => CreateMethod (_mutableType, MethodAttributes.RequireSecObject), Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (message, paramName));
    }

    [Test]
    public void CreateMethod_ThrowsIfAbstractAndNotVirtual ()
    {
      Assert.That (
          () => CallCreateMethod (_mutableType, "NotImportant", MethodAttributes.Abstract, typeof (void), ParameterDeclaration.None, null),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Abstract methods must also be virtual.", "attributes"));
    }

    [Test]
    public void CreateMethod_ThrowsIfNonVirtualAndNewSlot ()
    {
      Assert.That (
          () => CallCreateMethod (_mutableType, "NotImportant", MethodAttributes.NewSlot, typeof (void), ParameterDeclaration.None, ctx => Expression.Empty ()),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "NewSlot methods must also be virtual.", "attributes"));
    }

    [Test]
    public void CreateMethod_ThrowsForNullReturningReturnTypeProvider ()
    {
      Assert.That (
          () => _factory.CreateMethod (
          _mutableType, "NotImportant", 0, GenericParameterDeclaration.None, ctx => null, ctx => ParameterDeclaration.None, ctx => Expression.Empty ()),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Provider must not return null.", "returnTypeProvider"));
    }

    [Test]
    public void CreateMethod_ThrowsForNullReturningParameterProvider ()
    {
      Assert.That (
          () => _factory.CreateMethod (
          _mutableType, "NotImportant", 0, GenericParameterDeclaration.None, ctx => typeof (int), ctx => null, ctx => Expression.Empty ()),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Provider must not return null.", "parameterProvider"));
    }

    [Test]
    public void CreateMethod_ThrowsIfAlreadyExists ()
    {
      Func<MethodBodyCreationContext, Expression> bodyProvider = ctx => Expression.Empty ();
      var method = _mutableType.AddMethod ("Method", 0, typeof (void), ParameterDeclarationObjectMother.CreateMultiple (2), bodyProvider);
      var methodParameters = method.GetParameters().Select (p => new ParameterDeclaration (p.ParameterType, p.Name, p.Attributes));

      Assert.That (() => CallCreateMethod (_mutableType, "OtherName", 0, method.ReturnType, methodParameters, bodyProvider), Throws.Nothing);

      Assert.That (
          () => CallCreateMethod (_mutableType, method.Name, 0, typeof (int), methodParameters, ctx => Expression.Constant (7)), Throws.Nothing);

      Assert.That (
          () => CallCreateMethod (_mutableType, method.Name, 0, method.ReturnType, ParameterDeclarationObjectMother.CreateMultiple (3), bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => CallCreateMethod (_mutableType, method.Name, 0, method.ReturnType, methodParameters, bodyProvider),
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
      var method = _mutableType.AddMethod ("GenericMethod", 0, genericParameters, returnTypeProvider, parameterProvider, bodyProvider);

      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider1 =
          ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "t1") };
      Assert.That (
          () =>
          _factory.CreateMethod (_mutableType, method.Name, 0, new[] { genericParameters[0] }, returnTypeProvider, parameterProvider1, bodyProvider),
          Throws.Nothing);

      Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider2 =
          ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[1], "t1"), new ParameterDeclaration (ctx.GenericParameters[0], "t2") };
      Assert.That (
          () =>
          _factory.CreateMethod (_mutableType, method.Name, 0, genericParameters, returnTypeProvider, parameterProvider2, bodyProvider),
          Throws.Nothing);

      Assert.That (
          () => _factory.CreateMethod (_mutableType, method.Name, 0, genericParameters, returnTypeProvider, parameterProvider, bodyProvider),
          Throws.InvalidOperationException.With.Message.EqualTo ("Method with equal name and signature already exists."));
    }

    private MutableMethodInfo CreateMethod (MutableType mutableType, MethodAttributes attributes)
    {
      return CallCreateMethod (
          mutableType,
          "dummy",
          attributes,
          typeof (void),
          ParameterDeclaration.None,
          ctx => Expression.Empty ());
    }

    private MutableMethodInfo CallCreateMethod (
        MutableType declaringType,
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
      internal virtual void InaccessibleBaseMethodInB (int i) { }
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