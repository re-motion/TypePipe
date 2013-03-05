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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ExplicitOverridesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void OverrideExplicitly ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethod");
      var type = AssembleType<B> (
          proxyType =>
          {
            var mutableMethodInfo = proxyType.AddMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  return ExpressionHelper.StringConcat (ctx.CallBase (overriddenMethod), Expression.Constant (" explicitly overridden"));
                });
            mutableMethodInfo.AddExplicitBaseDefinition (overriddenMethod);
            Assert.That (mutableMethodInfo.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { overriddenMethod }));
            Assert.That (mutableMethodInfo.BaseMethod, Is.Null);
            Assert.That (mutableMethodInfo.GetBaseDefinition(), Is.EqualTo (mutableMethodInfo));

            mutableMethodInfo.SetBody (
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  Assert.That (() => ctx.BaseMethod, Throws.TypeOf<NotSupportedException>());
                  return ctx.PreviousBody;
                });

            var allMethods = proxyType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That (allMethods, Has.Member (typeof (B).GetMethod ("OverridableMethod")));
            Assert.That (allMethods, Has.Member (mutableMethodInfo));
          });

      A instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "DifferentName");

      // Reflection doesn't handle explicit overrides in GetBaseDefinition.
      // If this changes, MutableMethodInfo.GetBaseDefinition() must be changed as well.
      Assert.That (method.GetBaseDefinition(), Is.EqualTo (method));

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("A explicitly overridden"));
      Assert.That (instance.OverridableMethod(), Is.EqualTo ("A explicitly overridden"));
    }

    [Test]
    public void OverrideExplicitly_WithNewSlotMethod ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethod");
      var type = AssembleType<B> (
          proxyType =>
          {
            var mutableMethodInfo = proxyType.AddMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                typeof (string),
                ParameterDeclaration.None,
                ctx => ExpressionHelper.StringConcat (ctx.CallBase (overriddenMethod), Expression.Constant (" explicitly overridden")));

            mutableMethodInfo.AddExplicitBaseDefinition (overriddenMethod);
          });

      A instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "DifferentName");
      Assert.That (method.Attributes & MethodAttributes.VtableLayoutMask, Is.EqualTo (MethodAttributes.NewSlot));

      Assert.That (instance.OverridableMethod (), Is.EqualTo ("A explicitly overridden"));
    }

    [Test]
    public void OverrideMultipleExplicitly ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethod");
      var otherOverriddenMetod = GetDeclaredMethod (typeof (A), "OverridableMethodWithSameSignature");
      var type = AssembleType<B> (
          proxyType =>
          {
            var mutableMethodInfo = proxyType.AddMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  return ExpressionHelper.StringConcat (ctx.CallBase (overriddenMethod), Expression.Constant (" explicitly overridden"));
                });
            mutableMethodInfo.AddExplicitBaseDefinition (overriddenMethod);
            mutableMethodInfo.AddExplicitBaseDefinition (otherOverriddenMetod);
            Assert.That (mutableMethodInfo.AddedExplicitBaseDefinitions, Is.EquivalentTo (new[] { overriddenMethod, otherOverriddenMetod }));
          });

      A instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "DifferentName");

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("A explicitly overridden"));
      Assert.That (instance.OverridableMethod (), Is.EqualTo ("A explicitly overridden"));
      Assert.That (instance.OverridableMethodWithSameSignature (), Is.EqualTo ("A explicitly overridden"));
    }

    [Test]
    public void TurnExistingMethodIntoOverrideForBaseMethod ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethod");
      var type = AssembleType<B> (
          proxyType =>
          {
            var baseMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((B obj) => obj.UnrelatedMethod());
            var overridingMethod = proxyType.GetOrAddOverride (baseMethod);
            overridingMethod.AddExplicitBaseDefinition (overriddenMethod);
            Assert.That (overridingMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { overriddenMethod }));
          });

      A instance = (B) Activator.CreateInstance (type);

      Assert.That (instance.OverridableMethod (), Is.EqualTo ("B unrelated"));
    }

    [Test]
    public void OverrideShadowedMethod_AndShadowingMethod ()
    {
      var overriddenShadowedMethod = GetDeclaredMethod (typeof (A), "MethodShadowedByB");
      var overriddenShadowingMethod = GetDeclaredMethod (typeof (B), "MethodShadowedByB");

      var type = AssembleType<C> (
          proxyType =>
          {
            proxyType.AddMethod (
                "MethodShadowedByB",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.True);
                  Assert.That (ctx.BaseMethod, Is.EqualTo (overriddenShadowingMethod));
                  return ExpressionHelper.StringConcat (ctx.CallBase (ctx.BaseMethod), Expression.Constant (" implicitly overridden"));
                });
            var overrideForShadowedMethod = proxyType.AddMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  return ExpressionHelper.StringConcat (ctx.CallBase (overriddenShadowedMethod), Expression.Constant (" explicitly overridden"));
                });
            overrideForShadowedMethod.AddExplicitBaseDefinition (overriddenShadowedMethod);
          });

      var instance = (C) Activator.CreateInstance (type);

      Assert.That (instance.MethodShadowedByB (), Is.EqualTo ("B implicitly overridden"));
      Assert.That (((A) instance).MethodShadowedByB (), Is.EqualTo ("A explicitly overridden"));
    }

    [Test]
    public void OverrideAbstractShadowedMethod ()
    {
      var shadowedMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((AbstractTypeBase obj) => obj.AbstractShadowedMethod());

      var type = AssembleType<AbstractType> (
          proxyType =>
          {
            /*var shadowingMethod = */proxyType.AddMethod (
                "AbstractShadowedMethod",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                typeof (string),
                ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  return Expression.Constant ("shadowing method");
                });
            // TODO 5059: Uncomment
            //Assert.That (ProxyType.GetMethod ("AbstractShadowedMethod"), Is.SameAs (shadowingMethod));
            Assert.That (proxyType.IsAbstract, Is.True);

            var overrideForAbstractShadowedMethod = proxyType.GetOrAddOverride (shadowedMethod);
            overrideForAbstractShadowedMethod.SetBody (
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  Assert.That (ctx.HasPreviousBody, Is.False);
                  return Expression.Constant ("override");
                });
            Assert.That (proxyType.IsAbstract, Is.False);
          });

      var instance = (AbstractType) Activator.CreateInstance (type, nonPublic: true);

      Assert.That (instance.AbstractShadowedMethod(), Is.EqualTo ("override"));
    }

    [Test]
    public void OverrideGenericMethodExplicitly ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableGenericMethod");
      var type = AssembleType<B> (
          proxyType =>
          {
            var mutableMethod = proxyType.AddGenericMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual,
                new[] { new GenericParameterDeclaration ("T") },
                ctx => typeof (string),
                ctx => ParameterDeclaration.None,
                ctx =>
                {
                  Assert.That (ctx.HasBaseMethod, Is.False);
                  var instantiatedBaseMethod = overriddenMethod.MakeTypePipeGenericMethod (ctx.GenericParameters[0]);
                  return ExpressionHelper.StringConcat (ctx.CallBase (instantiatedBaseMethod), Expression.Constant (" explicitly overridden"));
                });
            mutableMethod.AddExplicitBaseDefinition (overriddenMethod);
            Assert.That (mutableMethod.AddedExplicitBaseDefinitions, Is.EqualTo (new[] { overriddenMethod }));
            Assert.That (mutableMethod.BaseMethod, Is.Null);
            Assert.That (mutableMethod.GetBaseDefinition(), Is.EqualTo (mutableMethod));

            var allMethods = proxyType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That (allMethods, Has.Member (typeof (B).GetMethod ("OverridableMethod")));
            Assert.That (allMethods, Has.Member (mutableMethod));
          });

      A instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "DifferentName").MakeGenericMethod (typeof (string));

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("A String explicitly overridden"));
      Assert.That (instance.OverridableGenericMethod<int>(), Is.EqualTo ("A Int32 explicitly overridden"));
    }

// ReSharper disable VirtualMemberNeverOverriden.Global
    public class A
    {
      public virtual string OverridableMethod ()
      {
        return "A";
      }

      public virtual string OverridableMethodWithSameSignature ()
      {
        return "A";
      }

      public virtual string MethodShadowedByB ()
      {
        return "A";
      }

      public virtual string OverridableGenericMethod<T> ()
      {
        return "A " + typeof (T).Name;
      }
    }

    public class B : A
    {
      public virtual string UnrelatedMethod ()
      {
        return "B unrelated";
      }

      public new virtual string MethodShadowedByB ()
      {
        return "B";
      }
    }

    public class C : B { }

    public abstract class AbstractTypeBase
    {
      public abstract string AbstractShadowedMethod ();
    }
    public abstract class AbstractType : AbstractTypeBase { }
  }
// ReSharper restore VirtualMemberNeverOverriden.Global
}