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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class TypeInstantiationInExpressionTrees : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ProxyTypeAsTypeArgument ()
    {
      var type = AssembleType<DomainType> (proxyType =>
      {
        var listType = typeof (List<>).MakeTypePipeGenericType (proxyType);
        var field = proxyType.AddField ("_list", listType);

        var ctor = proxyType.AddedConstructors.Single();
        ctor.SetBody (
            ctx => Expression.Block (
                Expression.Assign (Expression.Field (ctx.This, field), Expression.New (listType)),
                ctx.PreviousBody));

        proxyType.AddMethod (
            "Method",
            ctx =>
            {
              var list = Expression.Field (ctx.This, field);
              var addMethod = listType.GetMethod ("Add");
              var countProperty = listType.GetProperty ("Count");
              return Expression.Block (
                  Expression.Call (list, addMethod, Expression.New (proxyType)),
                  Expression.Call (list, addMethod, Expression.New (proxyType)),
                  Expression.Property (list, countProperty));
            },
            returnType: typeof (int));
      });

      var method = type.GetMethod ("Method");
      var instance = Activator.CreateInstance (type);
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo (2));
    }

    public class DomainType { }
  }
}