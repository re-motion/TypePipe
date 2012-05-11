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
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore ("TODO 4814")]
  public class GetMutableMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void BaseMethodWithoutOverride ()
    {
      var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod());

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.EqualTo (baseMethod));
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.Empty);

            CheckBodyOfAddedOverride (baseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody(), Expression.Constant (" made mutable")));

            Assert.That (mutableType.GetMutableMethod (baseMethod), Is.SameAs (mutableMethod));
          });

      var implicitOverride = type.GetMethod (baseMethod.Name);
      Assert.That (implicitOverride.DeclaringType, Is.TypeOf (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = implicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("Base made mutable"));
      Assert.That (instance.BaseMethod(), Is.EqualTo("Base made mutable"));
    }

    [Test]
    public void NonVirtualBaseMethod ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.NonVirtualBaseMethod ());
            Assert.That (baseMethod.IsVirtual, Is.False);

            Assert.That (
                mutableType.GetMutableMethod (baseMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("Non-virtual methods cannot be modified."));
          });
    }

    [Test]
    public void FinalVirtualBaseMethod ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.FinalVirtualBaseMethod ());
            Assert.That (baseMethod.IsVirtual, Is.True);
            Assert.That (baseMethod.IsFinal, Is.True);

            Assert.That (
                mutableType.GetMutableMethod (baseMethod),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("Final methods cannot be modified."));
          });
    }

    [Test]
    public void BaseMethodWithExistingOverride ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.ExistingOverride ());
            var existingOverride = mutableType.ExistingMutableMethods.Single (m => m.Name == "ExistingOverride");
            Assert.That (existingOverride.BaseMethod, Is.EqualTo (baseMethod));

            var mutableMethod = mutableType.GetMutableMethod (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (existingOverride));
          });
    }

    [Test]
    public void BaseMethodWithAddedImplicitOverride ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());
            var implicitOverride = AddEquivalentMethod(mutableType, baseMethod, baseMethod.Attributes);
            Assert.That (implicitOverride.BaseMethod, Is.EqualTo (baseMethod));

            var mutableMethod = mutableType.GetMutableMethod (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (implicitOverride));
          });
    }

    [Test]
    public void BaseMethodWithAddedExplicitOverride ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());
            var explicitOverride = mutableType.ExistingMutableMethods.Single (m => m.Name == "ExistingOverride");
            explicitOverride.AddExplicitBaseDefinition (baseMethod);
            Assert.That (explicitOverride.BaseMethod, Is.Not.EqualTo (baseMethod));

            var mutableMethod = mutableType.GetMutableMethod (baseMethod);

            Assert.That (mutableMethod, Is.SameAs (explicitOverride));

            var otherBaseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.ExistingOverride ());
            Assert.That (explicitOverride.BaseMethod, Is.EqualTo (otherBaseMethod));
            Assert.That (mutableType.GetMutableMethod (otherBaseMethod), Is.SameAs (explicitOverride));
          });
    }

    [Test]
    public void BaseMethod_ShadowedByModified_CausesExplicitOverride ()
    {
      var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethodShadowedByModified ());

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseMethod } ));

            CheckBodyOfAddedOverride (baseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody (), Expression.Constant (" made mutable")));
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, baseMethod.Name);
      Assert.That (explicitOverride.DeclaringType, Is.TypeOf (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = explicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("Base (shadowed) made mutable"));
      Assert.That (((DomainTypeBase) instance).BaseMethodShadowedByModified (), Is.EqualTo ("Base (shadowed) made mutable"));
      Assert.That (instance.BaseMethodShadowedByModified (), Is.EqualTo ("DomainType (shadowing)"));
    }

    [Test]
    public void BaseBaseMethod_ShadowedByBase_CausesExplicitOverride ()
    {
      var baseBaseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBaseBase> (x => x.BaseBaseMethodShadowedByBase ());

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableMethod = mutableType.GetMutableMethod (baseBaseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseBaseMethod }));

            CheckBodyOfAddedOverride (baseBaseMethod, mutableMethod);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody (), Expression.Constant (" made mutable")));
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, baseBaseMethod.Name);
      Assert.That (explicitOverride.DeclaringType, Is.TypeOf (type));

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = explicitOverride.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("BaseBase (shadowed) made mutable"));
      Assert.That (((DomainTypeBaseBase) instance).BaseBaseMethodShadowedByBase (), Is.EqualTo ("BaseBase (shadowed) made mutable"));
      Assert.That (instance.BaseBaseMethodShadowedByBase (), Is.EqualTo ("Base (shadowing)"));
    }

    [Test]
    public void ModifyingShadowingAndShadowed_CausesImplicitAndExplicitOverride ()
    {
      var shadowedMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethodShadowedByModified ());
      var shadowingMethod = MemberInfoFromExpressionUtility.GetMethod<DomainType> (x => x.BaseMethodShadowedByModified ());

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var mutableShadowedMethod = mutableType.GetMutableMethod (shadowedMethod);

            Assert.That (mutableShadowedMethod.BaseMethod, Is.Null);
            Assert.That (mutableShadowedMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { shadowedMethod }));

            mutableShadowedMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody(), Expression.Constant (" made mutable explicitly")));

            var mutableShadowingMethod = mutableType.GetMutableMethod (shadowingMethod);

            Assert.That (mutableShadowingMethod.BaseMethod, Is.EqualTo (shadowingMethod));
            Assert.That (mutableShadowingMethod.AddedExplicitBaseDefinitions, Is.Empty);

            mutableShadowingMethod.SetBody (
                ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody(), Expression.Constant (" made mutable implicitly")));
            
          });

      var explicitOverride = GetDeclaredExplicitOverrideMethod (type, shadowedMethod.Name);
      var implicitOverride = GetDeclaredMethod (type, shadowedMethod.Name);

      var instance = (DomainType) Activator.CreateInstance (type);
      
      var explicitOverrideResult = explicitOverride.Invoke (instance, null);
      Assert.That (explicitOverrideResult, Is.EqualTo ("Base (shadowed) made mutable explicitly"));

      var implicitOverrideResult = implicitOverride.Invoke (instance, null);
      Assert.That (implicitOverrideResult, Is.EqualTo ("DomainType (shadowing) made mutable implicitly"));

      Assert.That (((DomainTypeBase) instance).BaseMethodShadowedByModified (), Is.EqualTo ("Base (shadowed) made mutable explicitly"));
      Assert.That (instance.BaseMethodShadowedByModified (), Is.EqualTo ("DomainType (shadowing) made mutable implicitly"));
    }

    [Test]
    public void WorksForOverriddenAndOverridingMethod ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var overriddenMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBaseBase> (x => x.BaseBaseMethodOverriddenInBase());
            var overridingMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseBaseMethodOverriddenInBase());

            var mutableMethod = mutableType.GetMutableMethod (overriddenMethod);

            Assert.That (mutableMethod.BaseMethod, Is.EqualTo (overridingMethod));
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.Empty);

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody(), Expression.Constant (" made mutable")));

            Assert.That (mutableType.GetMutableMethod (overridingMethod), Is.SameAs (mutableMethod));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.BaseBaseMethodOverriddenInBase();

      Assert.That (result, Is.EqualTo ("Base (overriding) made mutable"));
    }

    [Test]
    public void BaseMethod_ShadowedByAdded_CausesExplicitOverride ()
    {
      var baseMethod = MemberInfoFromExpressionUtility.GetMethod<DomainTypeBase> (x => x.BaseMethod ());

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var shadowingAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual;
            AddEquivalentMethod (mutableType, baseMethod, shadowingAttributes, ctx => Expression.Constant("Shadowing method"));

            var mutableMethod = mutableType.GetMutableMethod (baseMethod);

            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { baseMethod }));

            mutableMethod.SetBody (ctx => ExpressionHelper.StringConcat (ctx.GetPreviousBody (), Expression.Constant (" made mutable")));
          });

      var shadowingMethod = GetDeclaredMethod (type, baseMethod.Name);
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (baseMethod.Invoke (instance, null), Is.EqualTo ("Base made mutable"));
      Assert.That (shadowingMethod.Invoke (instance, null), Is.EqualTo ("ShadowingMethod"));
    }

    private void CheckBodyOfAddedOverride (MethodInfo baseMethod, MutableMethodInfo mutableMethod)
    {
      Assert.That (mutableMethod.Body, Is.TypeOf<MethodCallExpression> ());
      var methodCallExpression = (MethodCallExpression) mutableMethod.Body;

      Assert.That (methodCallExpression.Object, Is.TypeOf<ThisExpression> ());
      var thisExpression = methodCallExpression.Object;
      Assert.That (thisExpression.Type, Is.EqualTo (typeof (DomainType)));

      Assert.That (methodCallExpression.Method, Is.TypeOf<BaseCallMethodInfoAdapter> ());
      Assert.That (((BaseCallMethodInfoAdapter) methodCallExpression.Method).AdaptedMethodInfo, Is.EqualTo (baseMethod));
    }

    private MutableMethodInfo AddEquivalentMethod (
        MutableType mutableType,
        MethodInfo template,
        MethodAttributes methodAttributes,
        Func<MethodBodyCreationContext, Expression> bodyProvider = null)
    {
      return mutableType.AddMethod (
          template.Name,
          methodAttributes,
          template.ReturnType,
          ParameterDeclaration.CreateForEquivalentSignature (template),
          bodyProvider ?? (ctx => Expression.Default (template.ReturnType)));
    }

    private class DomainTypeBaseBase
    {
      public virtual void FinalVirtualBaseMethod () { }

      public virtual string BaseBaseMethodShadowedByBase () { return "BaseBase (shadowed)"; }

      public virtual string BaseBaseMethodOverriddenInBase () { return "BasesBase (overidden)"; }
    }

    private class DomainTypeBase : DomainTypeBaseBase
    {
      public virtual string BaseMethod () { return "Base"; }
      public virtual string ExistingOverride () { return "Base"; }

      public void NonVirtualBaseMethod () { }
      public override sealed void FinalVirtualBaseMethod () { }

      public virtual string BaseMethodShadowedByModified () { return "Base (shadowed)"; }
      public new virtual string BaseBaseMethodShadowedByBase () { return "Base (shadowing)"; }

      public override string BaseBaseMethodOverriddenInBase () { return "Base (overriding)"; }
    }

    private class DomainType : DomainTypeBase
    {
      public override string ExistingOverride () { return "DomainType"; }

      public new virtual string BaseMethodShadowedByModified () { return "DomainType (shadowing)"; }
    }
  }
}