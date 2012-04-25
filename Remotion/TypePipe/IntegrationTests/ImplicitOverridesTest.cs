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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore("TODO 4817")]
  public class ImplicitOverridesTest : TypeAssemblerIntegrationTestBase
  {
    private MemberInfoEqualityComparer<MethodInfo> _memberInfoEqualityComparer;

    public override void SetUp ()
    {
      base.SetUp ();
      _memberInfoEqualityComparer = MemberInfoEqualityComparer<MethodInfo>.Instance;
    }

    [Test]
    public void OverrideMethod ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethod");
      var type = AssembleType<B> (
          mutableType =>
          {
            var mutableMethodInfo = mutableType.AddMethod (
                "OverridableMethod",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.EmptyParameters,
                ctx =>
                {
                  //Assert.That (ctx.IsOverridde, Is.True);
                  //Assert.That (_memberInfoEqualityComparer.Equals (ctx.OverriddenMethod, overriddenMethod), Is.True);
                  //return ExpressionHelper.StringConcat (ctx.GetBaseCall (ctx.OverriddenMethod), Expression.Constant (" overridden"));
                  return Expression.Default (typeof (string));
                });
            Assert.That (_memberInfoEqualityComparer.Equals (mutableMethodInfo.GetBaseDefinition(), overriddenMethod), Is.True);
          });

      var instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "OverridableMethod");

      Assert.That (_memberInfoEqualityComparer.Equals (method.GetBaseDefinition (), overriddenMethod), Is.True);

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("A overridden"));
      Assert.That (instance.OverridableMethod (), Is.EqualTo ("A overridden"));
    }

    [Test]
    public void ModifyExistingOverride ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "MethodOverriddenByB");
      var type = AssembleType<B> (
          mutableType =>
          {
            var mutableMethodInfo = mutableType.ExistingMutableMethods.Single (m => m.Name == "MethodOverriddenByB");
            Assert.That (_memberInfoEqualityComparer.Equals (mutableMethodInfo.GetBaseDefinition (), overriddenMethod), Is.True);
            mutableMethodInfo.SetBody(ctx =>
                {
                  //Assert.That (ctx.IsOverridde, Is.True);
                  //Assert.That (_memberInfoEqualityComparer.Equals (ctx.OverriddenMethod, overriddenMethod), Is.True);
                  //return ExpressionHelper.StringConcat (
                  //    ExpressionHelper.StringConcat (Expression.Constant ("Base: "), ctx.GetBaseCall (ctx.OverriddenMethod)),
                  //    ExpressionHelper.StringConcat (Expression.Constant (", previous body: "), ctx.GetPreviousBody()));
                  return Expression.Default (typeof (string));
                });
            
          });

      var instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "MethodOverriddenByB");

      Assert.That (_memberInfoEqualityComparer.Equals (method.GetBaseDefinition (), overriddenMethod), Is.True);

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("Base: A, previous body: B"));
      Assert.That (instance.MethodOverriddenByB(), Is.EqualTo ("Base: A, previous body: B"));
    }

    [Test]
    public void OverrideOverride ()
    {
      var overriddenMethodInA = GetDeclaredMethod (typeof (A), "MethodOverriddenByB");
      var overriddenMethodInB = GetDeclaredMethod (typeof (B), "MethodOverriddenByB");

      var type = AssembleType<C> (
          mutableType =>
          {
            var mutableMethodInfo = mutableType.AddMethod (
                "OverridableMethod",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.EmptyParameters,
                ctx =>
                {
                  //Assert.That (ctx.IsOverridde, Is.True);
                  //Assert.That (_memberInfoEqualityComparer.Equals (ctx.OverriddenMethod, overriddenMethodInB), Is.True);
                  //return ExpressionHelper.StringConcat (ctx.GetBaseCall (ctx.OverriddenMethod), Expression.Constant (" overridden")));
                  return Expression.Default (typeof (string));
                });
            Assert.That (_memberInfoEqualityComparer.Equals (mutableMethodInfo.GetBaseDefinition(), overriddenMethodInA), Is.True);
          });

      A instance = (C) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "MethodOverriddenByB");

      Assert.That (_memberInfoEqualityComparer.Equals (method.GetBaseDefinition (), overriddenMethodInA), Is.True);

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("B overridden"));
      Assert.That (instance.MethodOverriddenByB(), Is.EqualTo ("B overridden"));
    }

    private MethodInfo GetDeclaredMethod (Type type, string name)
    {
      var method = type.GetMethod (name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
      Assert.That (method, Is.Not.Null);
      return method;
    }

    public class A
    {
      public virtual string OverridableMethod ()
      {
        return "A";
      }

      public virtual string MethodOverriddenByB ()
      {
        return "A";
      }
    }

    public class B : A
    {
      public override string MethodOverriddenByB ()
      {
        return "B";
      }
    }

    public class C : B
    {
    }
  }
}