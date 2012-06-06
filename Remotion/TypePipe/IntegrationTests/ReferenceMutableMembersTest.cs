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
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ReferenceMutableMembersTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void UseMutableFieldAndMethodInBody ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var newMutableField = mutableType.AddField (typeof (string), "_newField");
            var existingMutableMethod = mutableType.ExistingMutableMethods.Single (m => m.Name == "Method");

            mutableType.AddMethod (
                "NewMethod",
                MethodAttributes.Public,
                typeof (void),
                ParameterDeclaration.EmptyParameters,
                // Without reflection method calls it would like this.
                // ctx => Expression.Assign (Expression.Field (ctx.This, newMutableField), Expression.Call (ctx.This, existingMutableMethod))
                ctx =>
                Expression.Call (
                    // TODO 4907
                    // TODO: ctx.GetMutableFieldReference (newMutableField),
                    Expression.Call (
                        Expression.Constant (newMutableField.DeclaringType, typeof (Type)),
                        typeof (Type).GetMethod ("GetField", new[] { typeof (string), typeof (BindingFlags) }),
                        Expression.Constant (newMutableField.Name),
                        Expression.Constant (BindingFlags.Instance | BindingFlags.NonPublic)),
                    //Expression.Constant (newMutableField),
                    "SetValue",
                    Type.EmptyTypes,
                    ctx.This,
                    Expression.Call (
                        // 4907
                        // TODO: ctx.GetMutableMethodReference (existingMutableMethod),
                        Expression.Constant (existingMutableMethod, typeof (MethodInfo)),
                        "Invoke",
                        Type.EmptyTypes,
                        ctx.This,
                        Expression.Constant (null, typeof (object[])))
                    )
                );
          });

      var instance = Activator.CreateInstance (type);
      var field = type.GetField ("_newField", BindingFlags.NonPublic | BindingFlags.Instance);
      var method = type.GetMethod ("NewMethod");

      Assert.That (field.GetValue (instance), Is.Null);
      method.Invoke (instance, null);
      Assert.That (field.GetValue (instance), Is.EqualTo ("existing method"));
    }

    public class DomainType
    {
      public string Method ()
      {
        return "existing method";
      }
    }
  }
}