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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class TypeInstantiationInExpressionTreesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void LocalVariable ()
    {
      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "Method",
              MethodAttributes.Public,
              typeof(void),
              ParameterDeclaration.None,
              ctx =>
              {
                var instantiation = typeof (Func<>).MakeTypePipeGenericType (p);
                var localVariable = Expression.Parameter (instantiation);
                return Expression.Block (new[] { localVariable }, Expression.Empty());
              }));

      var methodBody = type.GetMethod ("Method").GetMethodBody();
      Assertion.IsNotNull (methodBody);
      var localVariableType = methodBody.LocalVariables.Single().LocalType;
      Assertion.IsNotNull (localVariableType);

      Assert.That (localVariableType.GetGenericTypeDefinition(), Is.SameAs (typeof (Func<>)));
      Assert.That (localVariableType.GetGenericArguments().Single(), Is.SameAs (type));
    }

    [Test]
    public void AddField_UseConstructor_UseMethod ()
    {
      var type = AssembleType<DomainType> (proxyType =>
      {
        var listType = typeof (List<>).MakeTypePipeGenericType (proxyType);
        var field = proxyType.AddField ("_list", FieldAttributes.Private, listType);

        var ctor = proxyType.AddedConstructors.Single();
        ctor.SetBody (
            ctx => Expression.Block (
                Expression.Assign (Expression.Field (ctx.This, field), Expression.New (listType)),
                ctx.PreviousBody));

        proxyType.AddMethod (
            "Method",
            MethodAttributes.Public,
            typeof (int),
            ParameterDeclaration.None,
            ctx =>
            {
              var list = Expression.Field (ctx.This, field);
              var addMethod = listType.GetMethod ("Add");
              var countProperty = listType.GetProperty ("Count");
              return Expression.Block (
                  Expression.Call (list, addMethod, Expression.New (proxyType)),
                  Expression.Call (list, addMethod, Expression.New (proxyType)),
                  Expression.Property (list, countProperty));
            });
      });

      var method = type.GetMethod ("Method");
      var instance = Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo (2));
    }

    public class DomainType { }
  }
}