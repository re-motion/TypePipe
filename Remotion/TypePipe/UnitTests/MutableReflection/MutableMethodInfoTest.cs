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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.UnitTests.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableMethodInfoTest
  {
    private ProxyType _declaringType;

    private MutableMethodInfo _method;
    private MutableMethodInfo _virtualMethod;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ProxyTypeObjectMother.Create (baseType: typeof (DomainType));

      _method = MutableMethodInfoObjectMother.Create (_declaringType, "NonVirtualMethod");
      _virtualMethod = MutableMethodInfoObjectMother.Create (_declaringType, attributes: MethodAttributes.Virtual);
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = ProxyTypeObjectMother.Create();
      var name = "abc";
      var attributes = (MethodAttributes) 7 | MethodAttributes.Virtual;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var baseMethod = ReflectionObjectMother.GetSomeVirtualMethod();
      var body = ExpressionTreeObjectMother.GetSomeExpression (returnType);

      var method = new MutableMethodInfo (
          declaringType, name, attributes, new MutableGenericParameter[0], returnType, parameters.AsOneTime(), baseMethod, body);

      Assert.That (method.DeclaringType, Is.SameAs (declaringType));
      Assert.That (method.MutableDeclaringType, Is.SameAs (declaringType));
      Assert.That (method.Name, Is.EqualTo (name));
      Assert.That (method.Attributes, Is.EqualTo(attributes));
      Assert.That (method.IsGenericMethod, Is.False);

      CustomParameterInfoTest.CheckParameter (method.ReturnParameter, method, -1, null, returnType, ParameterAttributes.None);
      Assert.That (method.MutableReturnParameter, Is.SameAs (method.ReturnParameter));

      var actualParameters = method.GetParameters();
      Assert.That (actualParameters, Has.Length.EqualTo (2));
      CustomParameterInfoTest.CheckParameter (actualParameters[0], method, 0, parameters[0].Name, parameters[0].Type, parameters[0].Attributes);
      CustomParameterInfoTest.CheckParameter (actualParameters[1], method, 1, parameters[1].Name, parameters[1].Type, parameters[1].Attributes);
      Assert.That (method.MutableParameters, Is.EqualTo (actualParameters));

      var paramExpressions = method.ParameterExpressions;
      Assert.That (paramExpressions, Has.Count.EqualTo (2));
      Assert.That (paramExpressions[0], Has.Property ("Name").EqualTo (parameters[0].Name).And.Property ("Type").SameAs (parameters[0].Type));
      Assert.That (paramExpressions[1], Has.Property ("Name").EqualTo (parameters[1].Name).And.Property ("Type").SameAs (parameters[1].Type));
      
      Assert.That (method.BaseMethod, Is.SameAs (baseMethod));
      Assert.That (method.Body, Is.SameAs (body));
    }

    [Test]
    public void Initialization_GenericMethodDefinition ()
    {
      var genericParameters = MutableGenericParameterObjectMother.CreateMultiple (2);
      var method = MutableMethodInfoObjectMother.Create (genericParameters: genericParameters);

      Assert.That (method.IsGenericMethod, Is.True);
      Assert.That (method.IsGenericMethodDefinition, Is.True);
      Assert.That (method.MutableGenericParameters, Is.EqualTo (genericParameters));

      var genParas = method.GetGenericArguments();
      Assert.That (genParas, Is.EqualTo (genericParameters));
      Assert.That (genParas, Has.All.Matches<Type> (g => g.DeclaringMethod == method));
    }

    [Test]
    public void Initialization_NoBasedMethod_NoBody ()
    {
      MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Abstract, baseMethod: null, body: null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "An abstract method has no body.")]
    public void Body_ThrowsForAbstractMethod ()
    {
      var method = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Abstract);

      Dev.Null = method.Body;
    }

    [Test]
    public void GetBaseDefinition ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.OverridingMethod());
      var rootDefinition = baseMethod.GetBaseDefinition();
      Assert.That (rootDefinition, Is.Not.EqualTo (baseMethod));
      var method = MutableMethodInfoObjectMother.Create (baseMethod: baseMethod);

      Assert.That (method.GetBaseDefinition(), Is.SameAs (rootDefinition));
    }

    [Test]
    public void GetBaseDefinition_NoBaseMethod ()
    {
      var method = MutableMethodInfoObjectMother.Create (baseMethod: null);

      Assert.That (method.GetBaseDefinition(), Is.SameAs (method));
    }

    [Test]
    public void AddExplicitBaseDefinition ()
    {
      var overriddenMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod());

      _virtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition);

      Assert.That (_virtualMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { overriddenMethodDefinition }));
    }

    [Test]
    public void AddExplicitBaseDefinition_AllowsMethodsFromHierarchy ()
    {
      Assert.That (_declaringType.GetInterfaces(), Has.Member (typeof (IExistingInterface)));
      _declaringType.AddInterface (typeof (IAddedInterface));
      var overriddenMethodDefinition1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.InterfaceMethod());
      var overriddenMethodDefinition2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.VirtualMethod());

      _virtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition1);
      _virtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition2);

      Assert.That (_virtualMethod.AddedExplicitBaseDefinitions, Is.EquivalentTo (new[] { overriddenMethodDefinition1, overriddenMethodDefinition2 }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot add an explicit base definition to the non-virtual method 'NonVirtualMethod'.")]
    public void AddExplicitBaseDefinition_CannotAddExplicitBaseDefinition ()
    {
      var overriddenMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod ());

      _method.AddExplicitBaseDefinition (overriddenMethodDefinition);
    }

    [Test]
    public void AddExplicitBaseDefinition_FinalAndVirtualMethods ()
    {
      var nonVirtualMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualMethod ());
      var finalMethodDefinition = typeof (DomainType).GetMethod ("FinalMethod");

      var message = "Method must be virtual and non-final.\r\nParameter name: overriddenMethodBaseDefinition";
      Assert.That (
          () => _virtualMethod.AddExplicitBaseDefinition (nonVirtualMethodDefinition),
          Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (
          () => _virtualMethod.AddExplicitBaseDefinition (finalMethodDefinition),
          Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method signatures must be equal.\r\nParameter name: overriddenMethodBaseDefinition")]
    public void AddExplicitBaseDefinition_IncompatibleSignatures ()
    {
      var differentSignatureMethodDefinition =
          NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethodWithDifferentSignature (7));

      _virtualMethod.AddExplicitBaseDefinition (differentSignatureMethodDefinition);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The overridden method must be from the same type hierarchy.\r\nParameter name: overriddenMethodBaseDefinition")]
    public void AddExplicitBaseDefinition_UnrelatedMethod ()
    {
      var unrelatedMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnrelatedType obj) => obj.VirtualMethod ());

      _virtualMethod.AddExplicitBaseDefinition (unrelatedMethodDefinition);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The given method must be a root method definition. (Use GetBaseDefinition to get a root method.)\r\n"
        + "Parameter name: overriddenMethodBaseDefinition")]
    public void AddExplicitBaseDefinition_NoRootMethod ()
    {
      var nonBaseDefinitionMethod = typeof (DomainType).GetMethod ("OverridingMethod");

      _virtualMethod.AddExplicitBaseDefinition (nonBaseDefinitionMethod);
    }

    [Test]
    public void AddExplicitBaseDefinition_TwiceWithSameMethod ()
    {
      var overriddenMethodDefinition1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod ());
      var overriddenMethodDefinition2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod2 ());

      _virtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition1);
      _virtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition2);
      Assert.That (
          () => _virtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition2),
          Throws.InvalidOperationException.With.Message.EqualTo ("The given method has already been added to the list of explicit base definitions."));

      Assert.That (
          _virtualMethod.AddedExplicitBaseDefinitions,
          Is.EquivalentTo (new[] { overriddenMethodDefinition1, overriddenMethodDefinition2 }));
    }

    [Test]
    public void SetBody ()
    {
      var declaringType = ProxyTypeObjectMother.Create();
      var attribtes = MethodAttributes.Virtual; // Methods which have a base method must be virtual.
      var returnType = typeof (object);
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var baseMethod = ReflectionObjectMother.GetSomeVirtualMethod(); // Base method must be virtual.
      var genericParameters = new[] { MutableGenericParameterObjectMother.Create() };
      var method = MutableMethodInfoObjectMother.Create (
          declaringType, "Method", attribtes, returnType, parameters, baseMethod, genericParameters: genericParameters);

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      Func<MethodBodyModificationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.DeclaringType, Is.SameAs (declaringType));
        Assert.That (ctx.IsStatic, Is.False);
        Assert.That (ctx.Parameters, Is.EqualTo (method.ParameterExpressions).And.Not.Empty);
        Assert.That (ctx.GenericParameters, Is.EqualTo (genericParameters));
        Assert.That (ctx.ReturnType, Is.SameAs (returnType));
        Assert.That (ctx.BaseMethod, Is.SameAs (baseMethod));
        Assert.That (ctx.PreviousBody, Is.SameAs (method.Body));

        return fakeBody;
      };

      method.SetBody (bodyProvider);

      var expectedBody = Expression.Convert (fakeBody, returnType);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void SetBody_Static ()
    {
      var method = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);
      Func<MethodBodyModificationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.IsStatic, Is.True);
        return ExpressionTreeObjectMother.GetSomeExpression (method.ReturnType);
      };

      method.SetBody (bodyProvider);
    }

    [Test]
    public void SetBody_ImplementsAbstractMethod ()
    {
      var method = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Abstract, body: null);
      Assert.That (method.IsAbstract, Is.True);

      method.SetBody (
          ctx =>
          {
            Assert.That (ctx.HasPreviousBody, Is.False);
            return Expression.Default (method.ReturnType);
          });

      Assert.That (method.IsAbstract, Is.False);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _method.AddCustomAttribute (declaration);

      Assert.That (_method.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_method.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString is defined in CustomMethodInfo base class.
      var method = MutableMethodInfoObjectMother.Create (
          declaringType: ProxyTypeObjectMother.Create (name: "AbcProxy"),
          name: "Xxx",
          returnType: typeof (void),
          parameters: new[] { new ParameterDeclaration (typeof (int), "p1") });

      var expected = "MutableMethod = \"Void Xxx(Int32)\", DeclaringType = \"AbcProxy\"";
      Assert.That (method.ToDebugString (), Is.EqualTo (expected));
    }

    public class DomainTypeBase
    {
      public virtual void OverridingMethod () { }
      public virtual void FinalMethod () { }
    }

    public class DomainType : DomainTypeBase, IExistingInterface
    {
      public virtual void VirtualMethod () { }
      public virtual void VirtualMethod2 () { }
      public virtual void VirtualMethodWithDifferentSignature (int i) { Dev.Null = i; }
      public void NonVirtualMethod () { }

      public override void OverridingMethod () { }
      public sealed override void FinalMethod () { }
      public void InterfaceMethod () { }
    }

    public class UnrelatedType
    {
      public virtual void VirtualMethod () { }
    }

    public interface IAddedInterface
    {
      void VirtualMethod ();
    }

    public interface IExistingInterface
    {
      void InterfaceMethod ();
    }
  }
}