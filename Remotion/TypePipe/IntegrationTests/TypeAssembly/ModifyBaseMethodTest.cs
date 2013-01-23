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
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ModifyBaseMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void BaseMethodWithoutOverride ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.EqualTo (baseMethod));
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.Empty);

            CheckBodyOfAddedOverride (baseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));

            Assert.That (proxyType.GetOrAddOverride (baseMethod), Is.SameAs (mutableMethod));
          });

      var implicitOverride = type.GetMethod (baseMethod.Name);
      Assert.That (implicitOverride.DeclaringType, Is.SameAs (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = implicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("Base made mutable"));
      Assert.That (instance.BaseMethod(), Is.EqualTo("Base made mutable"));
    }

    [Test]
    public void BaseMethodWithAddedImplicitOverride ()
    {
      AssembleType<DomainType> (
          proxyType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());
            var attributes = baseMethod.Attributes & ~MethodAttributes.NewSlot;
            var implicitOverride = AddEquivalentMethod (proxyType, baseMethod, attributes);
            Assert.That (implicitOverride.BaseMethod, Is.EqualTo (baseMethod));

            var mutableMethod = proxyType.GetOrAddOverride (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (implicitOverride));
          });
    }

    [Test]
    public void BaseMethodWithAddedExplicitOverride ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.BaseMethod());
      var baseOverrideMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainTypeBase obj) => obj.ExistingOverride());
      var overrideMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ExistingOverride());

      AssembleType<DomainType> (
          proxyType =>
          {
            var explicitOverride = proxyType.GetOrAddOverride (overrideMethod);
            explicitOverride.AddExplicitBaseDefinition (baseMethod);

            Assert.That (explicitOverride.BaseMethod, Is.Not.EqualTo (baseMethod).And.EqualTo (overrideMethod));
            Assert.That (proxyType.GetOrAddOverride (baseMethod), Is.SameAs (explicitOverride));
            Assert.That (proxyType.GetOrAddOverride (baseOverrideMethod), Is.SameAs (explicitOverride));
          });
    }

    [Test]
    public void BaseMethod_ShadowedByModified_CausesExplicitOverride ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethodShadowedByModified ());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseMethod } ));

            CheckBodyOfAddedOverride (baseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, baseMethod);
      Assert.That (explicitOverride.DeclaringType, Is.SameAs (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = explicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("Base (shadowed) made mutable"));
      Assert.That (((DomainTypeBase) instance).BaseMethodShadowedByModified (), Is.EqualTo ("Base (shadowed) made mutable"));
      Assert.That (instance.BaseMethodShadowedByModified (), Is.EqualTo ("DomainType (shadowing)"));
    }

    [Test]
    public void BaseBaseMethod_ShadowedByBase_CausesExplicitOverride ()
    {
      var baseBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBaseBase> (x => x.BaseBaseMethodShadowedByBase ());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableMethod = proxyType.GetOrAddOverride (baseBaseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseBaseMethod }));

            CheckBodyOfAddedOverride (baseBaseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, baseBaseMethod);
      Assert.That (explicitOverride.DeclaringType, Is.SameAs (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = explicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("BaseBase (shadowed) made mutable"));
      Assert.That (((DomainTypeBaseBase) instance).BaseBaseMethodShadowedByBase (), Is.EqualTo ("BaseBase (shadowed) made mutable"));
      Assert.That (instance.BaseBaseMethodShadowedByBase (), Is.EqualTo ("Base (shadowing)"));
    }

    [Test]
    public void ModifyingShadowingAndShadowed_CausesImplicitAndExplicitOverride ()
    {
      var shadowedMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBaseBase> (x => x.BaseBaseMethodShadowedByBase());
      var shadowingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseBaseMethodShadowedByBase());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var mutableShadowedMethod = proxyType.GetOrAddOverride (shadowedMethod);

            Assert.That (mutableShadowedMethod.BaseMethod, Is.Null);
            Assert.That (mutableShadowedMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { shadowedMethod }));

            mutableShadowedMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable explicitly")));

            var mutableShadowingMethod = proxyType.GetOrAddOverride (shadowingMethod);

            Assert.That (mutableShadowingMethod.BaseMethod, Is.EqualTo (shadowingMethod));
            Assert.That (mutableShadowingMethod.AddedExplicitBaseDefinitions, Is.Empty);

            mutableShadowingMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable implicitly")));
            
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, shadowedMethod);
      var implicitOverride = GetDeclaredMethod (type, shadowedMethod.Name);

      var instance = (DomainType) Activator.CreateInstance (type);
      
      var explicitOverrideResult = explicitOverride.Invoke (instance, null);
      Assert.That (explicitOverrideResult, Is.EqualTo ("BaseBase (shadowed) made mutable explicitly"));

      var implicitOverrideResult = implicitOverride.Invoke (instance, null);
      Assert.That (implicitOverrideResult, Is.EqualTo ("Base (shadowing) made mutable implicitly"));

      Assert.That (((DomainTypeBaseBase) instance).BaseBaseMethodShadowedByBase(), Is.EqualTo ("BaseBase (shadowed) made mutable explicitly"));
      Assert.That (instance.BaseBaseMethodShadowedByBase(), Is.EqualTo ("Base (shadowing) made mutable implicitly"));
    }

    [Test]
    public void WorksForOverriddenAndOverridingMethod ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBaseBase> (x => x.BaseBaseMethodOverriddenInBase());
            var overridingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseBaseMethodOverriddenInBase());

            var mutableMethod = proxyType.GetOrAddOverride (overriddenMethod);

            Assert.That (mutableMethod.BaseMethod, Is.EqualTo (overridingMethod));
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.Empty);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));

            Assert.That (proxyType.GetOrAddOverride (overridingMethod), Is.SameAs (mutableMethod));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.BaseBaseMethodOverriddenInBase();

      Assert.That (result, Is.EqualTo ("Base (overriding) made mutable"));
    }

    [Test]
    public void BaseMethod_ShadowedByAdded_CausesExplicitOverride ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var shadowingAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual;
            AddEquivalentMethod (proxyType, baseMethod, shadowingAttributes, ctx => Expression.Constant("Shadowing method"));

            var mutableMethod = proxyType.GetOrAddOverride (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseMethod }));

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));
          });

      var shadowingMethod = GetDeclaredMethod (type, baseMethod.Name);
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (baseMethod.Invoke (instance, null), Is.EqualTo ("Base made mutable"));
      Assert.That (shadowingMethod.Invoke (instance, null), Is.EqualTo ("Shadowing method"));
    }

    [Test]
    public void NonVirtualBaseMethod_NotSupported ()
    {
      AssembleType<DomainType> (
          proxyType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.NonVirtualBaseMethod());
            Assert.That (baseMethod.IsVirtual, Is.False);

            Assert.That (
                () => proxyType.GetOrAddOverride (baseMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("Only virtual methods can be overridden."));
          });
    }

    [Test]
    public void FinalVirtualBaseMethod_NotSupported ()
    {
      AssembleType<DomainType> (
          proxyType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.FinalVirtualBaseMethod ());
            Assert.That (baseMethod.IsVirtual, Is.True);
            Assert.That (baseMethod.IsFinal, Is.True);

            Assert.That (
                () => proxyType.GetOrAddOverride (baseMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("Cannot override final method 'DomainTypeBase.FinalVirtualBaseMethod'."));
          });
    }

    private void CheckBodyOfAddedOverride (MethodInfo baseMethod, MutableMethodInfo mutableMethod)
    {
      Assert.That (mutableMethod.Body, Is.InstanceOf<MethodCallExpression>());
      var methodCallExpression = (MethodCallExpression) mutableMethod.Body;

      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression>());
      var thisExpression = ((ThisExpression) methodCallExpression.Object);
      Assert.That (thisExpression.Type, Is.SameAs (mutableMethod.DeclaringType));

      Assert.That (methodCallExpression.Method, Is.TypeOf<NonVirtualCallMethodInfoAdapter> ());
      Assert.That (((NonVirtualCallMethodInfoAdapter) methodCallExpression.Method).AdaptedMethod, Is.EqualTo (baseMethod));
    }

    public class DomainTypeBaseBase
    {
      public virtual void FinalVirtualBaseMethod () { }

      public virtual string BaseBaseMethodShadowedByBase () { return "BaseBase (shadowed)"; }

      public virtual string BaseBaseMethodOverriddenInBase () { return "BasesBase (overidden)"; }
    }

    public class DomainTypeBase : DomainTypeBaseBase
    {
      public virtual string BaseMethod () { return "Base"; }
      public virtual string ExistingOverride () { return "Base"; }

      public void NonVirtualBaseMethod () { }
      public override sealed void FinalVirtualBaseMethod () { }

      public virtual string BaseMethodShadowedByModified () { return "Base (shadowed)"; }
      public new virtual string BaseBaseMethodShadowedByBase () { return "Base (shadowing)"; }

      public override string BaseBaseMethodOverriddenInBase () { return "Base (overriding)"; }
    }

    public class DomainType : DomainTypeBase
    {
      public override string ExistingOverride () { return "DomainType"; }

      public new virtual string BaseMethodShadowedByModified () { return "DomainType (shadowing)"; }
    }
  }
}