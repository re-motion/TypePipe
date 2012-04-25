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
using System.Linq;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore("TODO 4818")]
  public class ImplicitOverridesTest : TypeAssemblerIntegrationTestBase
  {
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
                  //Assert.That (ctx.HasBaseMethod, Is.True);
                  //CheckMemberEquality (overriddenMethod, ctx.BaseMethod);
                  //return ExpressionHelper.StringConcat (ctx.GetBaseCall (ctx.BaseMethod), Expression.Constant (" overridden"));
                  return Expression.Default (typeof (string));
                });
            CheckMemberEquality (overriddenMethod, mutableMethodInfo.GetBaseDefinition());
          });

      var instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "OverridableMethod");

      CheckMemberEquality (overriddenMethod, method.GetBaseDefinition ());

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("A overridden"));
      Assert.That (instance.OverridableMethod (), Is.EqualTo ("A overridden"));
    }

    [Test]
    public void OverrideMethod_WithParameters ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethodWithParameters");
      var type = AssembleType<B> (
          mutableType =>
          {
            var mutableMethodInfo = mutableType.AddMethod (
                "OverridableMethodWithParameters",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof (string),
                new[] { new ParameterDeclaration (typeof (string), "p1") },
                ctx =>
                {
                  //Assert.That (ctx.HasBaseMethod, Is.True);
                  //CheckMemberEquality (overriddenMethod, ctx.BaseMethod);
                  //return ExpressionHelper.StringConcat (ctx.GetBaseCall (ctx.BaseMethod, ctx.Parameters), Expression.Constant (" overridden"));
                  return Expression.Default (typeof (string));
                });
            CheckMemberEquality (overriddenMethod, mutableMethodInfo.GetBaseDefinition ());
          });

      var instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "OverridableMethodWithParameters");

      CheckMemberEquality (overriddenMethod, method.GetBaseDefinition ());

      var result = method.Invoke (instance, new object[] { "xxx" });
      Assert.That (result, Is.EqualTo ("A xxx overridden"));
      Assert.That (instance.OverridableMethod (), Is.EqualTo ("A xxx overridden"));
    }

    [Test]
    public void ModifyExistingOverride ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "MethodOverriddenByB");
      var type = AssembleType<B> (
          mutableType =>
          {
            var mutableMethodInfo = mutableType.ExistingMutableMethods.Single (m => m.Name == "MethodOverriddenByB");
            CheckMemberEquality (overriddenMethod, mutableMethodInfo.GetBaseDefinition ());
            mutableMethodInfo.SetBody(ctx =>
                {
                  //Assert.That (ctx.HasBaseMethod, Is.True);
                  //CheckMemberEquality (overriddenMethod, ctx.BaseMethod);
                  //return ExpressionHelper.StringConcat (
                  //    ExpressionHelper.StringConcat (Expression.Constant ("Base: "), ctx.GetBaseCall (ctx.BaseMethod)),
                  //    ExpressionHelper.StringConcat (Expression.Constant (", previous body: "), ctx.GetPreviousBody()));
                  return Expression.Default (typeof (string));
                });
            
          });

      var instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "MethodOverriddenByB");

      CheckMemberEquality (overriddenMethod, method.GetBaseDefinition ());

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
                  //Assert.That (ctx.HasBaseMethod, Is.True);
                  //Assert.That (_memberInfoEqualityComparer.Equals (ctx.BaseMethod, overriddenMethodInB), Is.True);
                  //return ExpressionHelper.StringConcat (ctx.GetBaseCall (ctx.BaseMethod), Expression.Constant (" overridden")));
                  return Expression.Default (typeof (string));
                });
            CheckMemberEquality (overriddenMethodInA, mutableMethodInfo.GetBaseDefinition());
          });

      A instance = (C) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "MethodOverriddenByB");

      CheckMemberEquality (overriddenMethodInA, method.GetBaseDefinition ());

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("B overridden"));
      Assert.That (instance.MethodOverriddenByB(), Is.EqualTo ("B overridden"));
    }

    public class A
    {
      public virtual string OverridableMethod ()
      {
        return "A";
      }

      public virtual string OverridableMethodWithParameters (string s)
      {
        return "A " + s;
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