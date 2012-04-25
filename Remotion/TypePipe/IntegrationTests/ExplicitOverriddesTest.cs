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

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore ("TODO 4813")]
  public class ExplicitOverriddesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void OverrideExplicitly ()
    {
      var overriddenMethod = GetDeclaredMethod (typeof (A), "OverridableMethod");
      var type = AssembleType<B> (
          mutableType =>
          {
            var mutableMethodInfo = mutableType.AddMethod (
                "DifferentName",
                MethodAttributes.Private | MethodAttributes.Virtual,
                typeof (string),
                ParameterDeclaration.EmptyParameters,
                ctx =>
                {
                  //Assert.That (ctx.HasBaseMethod, Is.False);
                  //return ExpressionHelper.StringConcat (ctx.GetBaseCall (overriddenMethod), Expression.Constant (" explicitly overridden"));
                  return Expression.Default (typeof (string));
                });
            //mutableType.AddExplicitOverride (overriddenMethod, mutableMethodInfo);
            //Assert.That (mutableType.AddedExplicitOverrides[overriddenMethod], Is.SameAs (mutableMethodInfo));
            CheckMemberEquality (mutableMethodInfo, mutableMethodInfo.GetBaseDefinition ());

            //mutableMethodInfo.SetBody (ctx =>
            //{
            //  Assert.That (ctx.HasBaseMethod, Is.False);
            //  Assert.That (() => ctx.BaseMethod, Throws.TypeOf<NotSupportedException>());
            //  return ctx.GetPreviousBody(); 
            //});
          });

      A instance = (B) Activator.CreateInstance (type);
      var method = GetDeclaredMethod (type, "DifferentName");

      // Reflection doesn't handle explicit overrides in GetBaseDefinition.
      // If this changes, MutableMethodInfo.GetBaseDefinition() must be changed as well.
      CheckMemberEquality (method, method.GetBaseDefinition ());

      var result = method.Invoke (instance, null);
      Assert.That (result, Is.EqualTo ("A explicitly overridden"));
      Assert.That (instance.OverridableMethod (), Is.EqualTo ("A explicitly overridden"));
    }

    public class A
    {
      public virtual string OverridableMethod ()
      {
        return "A";
      }
    }

    public class B : A
    {
    }
  }
}