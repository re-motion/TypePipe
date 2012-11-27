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
          mutableType =>
          {
            var mutableMethod = mutableType.GetOrAddMutableMethod (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.EqualTo (baseMethod));
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.Empty);

            CheckBodyOfAddedOverride (baseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));

            Assert.That (mutableType.GetOrAddMutableMethod (baseMethod), Is.SameAs (mutableMethod));
          });

      var implicitOverride = type.GetMethod (baseMethod.Name);
      Assert.That (implicitOverride.DeclaringType, Is.SameAs (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = implicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("Base made mutable"));
      Assert.That (instance.BaseMethod(), Is.EqualTo("Base made mutable"));
    }

    [Test]
    public void BaseMethodWithExistingOverride ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.ExistingOverride ());
            var existingOverride = mutableType.ExistingMutableMethods.Single (m => m.Name == "ExistingOverride");
            Assert.That (existingOverride.BaseMethod, Is.EqualTo (baseMethod));

            var mutableMethod = mutableType.GetOrAddMutableMethod (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (existingOverride));
          });
    }

    [Test]
    public void BaseMethodWithAddedImplicitOverride ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());
            var attributes = baseMethod.Attributes & ~MethodAttributes.NewSlot;
            var implicitOverride = AddEquivalentMethod (mutableType, baseMethod, attributes);
            Assert.That (implicitOverride.BaseMethod, Is.EqualTo (baseMethod));

            var mutableMethod = mutableType.GetOrAddMutableMethod (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (implicitOverride));
          });
    }

    [Test]
    public void BaseMethodWithAddedExplicitOverride ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());
            var explicitOverride = mutableType.ExistingMutableMethods.Single (m => m.Name == "ExistingOverride");
            explicitOverride.AddExplicitBaseDefinition (baseMethod);
            Assert.That (explicitOverride.BaseMethod, Is.Not.EqualTo (baseMethod));

            var mutableMethod = mutableType.GetOrAddMutableMethod (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (explicitOverride));

            var otherBaseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.ExistingOverride ());
            Assert.That (explicitOverride.BaseMethod, Is.EqualTo (otherBaseMethod));
            Assert.That (mutableType.GetOrAddMutableMethod (otherBaseMethod), Is.SameAs (explicitOverride));
          });
    }

    [Test]
    public void BaseMethod_ShadowedByModified_CausesExplicitOverride ()
    {
      var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethodShadowedByModified ());

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetOrAddMutableMethod (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseMethod } ));

            CheckBodyOfAddedOverride (baseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, baseMethod.Name);
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
          mutableType =>
          {
            var mutableMethod = mutableType.GetOrAddMutableMethod (baseBaseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseBaseMethod }));

            CheckBodyOfAddedOverride (baseBaseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, baseBaseMethod.Name);
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
          mutableType =>
          {
            var mutableShadowedMethod = mutableType.GetOrAddMutableMethod (shadowedMethod);

            Assert.That (mutableShadowedMethod.BaseMethod, Is.Null);
            Assert.That (mutableShadowedMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { shadowedMethod }));

            mutableShadowedMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable explicitly")));

            var mutableShadowingMethod = mutableType.GetOrAddMutableMethod (shadowingMethod);

            Assert.That (mutableShadowingMethod.BaseMethod, Is.EqualTo (shadowingMethod));
            Assert.That (mutableShadowingMethod.AddedExplicitBaseDefinitions, Is.Empty);

            mutableShadowingMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable implicitly")));
            
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, shadowedMethod.Name);
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
          mutableType =>
          {
            var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBaseBase> (x => x.BaseBaseMethodOverriddenInBase());
            var overridingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseBaseMethodOverriddenInBase());

            var mutableMethod = mutableType.GetOrAddMutableMethod (overriddenMethod);

            Assert.That (mutableMethod.BaseMethod, Is.EqualTo (overridingMethod));
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.Empty);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.PreviousBody, Expression.Constant (" made mutable")));

            Assert.That (mutableType.GetOrAddMutableMethod (overridingMethod), Is.SameAs (mutableMethod));
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
          mutableType =>
          {
            var shadowingAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual;
            AddEquivalentMethod (mutableType, baseMethod, shadowingAttributes, ctx => Expression.Constant("Shadowing method"));

            var mutableMethod = mutableType.GetOrAddMutableMethod (baseMethod);

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
          mutableType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.NonVirtualBaseMethod ());
            Assert.That (baseMethod.IsVirtual, Is.False);

            Assert.That (
                () => mutableType.GetOrAddMutableMethod (baseMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
                    "A method declared in a base type must be virtual in order to be modified."));
          });
    }

    [Test]
    public void FinalVirtualBaseMethod_NotSupported ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.FinalVirtualBaseMethod ());
            Assert.That (baseMethod.IsVirtual, Is.True);
            Assert.That (baseMethod.IsFinal, Is.True);

            Assert.That (
                () => mutableType.GetOrAddMutableMethod (baseMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("Cannot override final method 'DomainTypeBase.FinalVirtualBaseMethod'."));
          });
    }

    [Test]
    public void InterfaceMethod_NotSupported ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod<IInterface> (x => x.InterfaceMethod());
            Assert.That (baseMethod.IsVirtual, Is.True);

            Assert.That (
                () => mutableType.GetOrAddMutableMethod (baseMethod),
                Throws.ArgumentException.With.Message.EqualTo (
                    "Method is declared by a type outside of this type's class hierarchy: 'IInterface'.\r\nParameter name: method"));
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

    public class DomainType : DomainTypeBase, IInterface
    {
      public override string ExistingOverride () { return "DomainType"; }

      public new virtual string BaseMethodShadowedByModified () { return "DomainType (shadowing)"; }

      public void InterfaceMethod () { }
    }

    public interface IInterface
    {
      void InterfaceMethod ();
    }
  }
}