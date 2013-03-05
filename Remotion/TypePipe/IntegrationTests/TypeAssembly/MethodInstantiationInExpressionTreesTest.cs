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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class MethodInstantiationInExpressionTreesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ExistingMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.Method());
      var genericMethodDefiniton = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Enumerable.Empty<Dev.T>());

      var type = AssembleType<DomainType> (
          p =>
          p.GetOrAddOverride (method)
           .SetBody (
               ctx =>
               {
                 var methodInstantiation = genericMethodDefiniton.MakeTypePipeGenericMethod (p);
                 Assert.That (methodInstantiation.IsGenericMethod, Is.True);
                 Assert.That (methodInstantiation.IsGenericMethodDefinition, Is.False);
                 Assert.That (methodInstantiation.GetGenericArguments(), Is.EqualTo (new[] { p }));

                 return Expression.Call (methodInstantiation);
               })); 

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.Method();

      Assert.That (result, Is.InstanceOf<IEnumerable<DomainType>>());
      var resultType = result.GetType();
      Assert.That (resultType.IsArray, Is.True);
      Assert.That (resultType.GetElementType(), Is.SameAs (type));
    }

    [Test]
    public void AddedMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.Method());
      var genericMethodDefiniton = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => Enumerable.Empty<Dev.T>());

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var addedGenericMethod = proxyType.AddGenericMethod (
                "GenericMethod",
                MethodAttributes.Public,
                new[] { new GenericParameterDeclaration ("T") },
                ctx => typeof (IEnumerable<>).MakeTypePipeGenericType (ctx.GenericParameters[0]),
                ctx => ParameterDeclaration.None,
                ctx => Expression.Call (genericMethodDefiniton.MakeTypePipeGenericMethod (ctx.GenericParameters[0])));

            proxyType.GetOrAddOverride (method).SetBody (
                ctx =>
                {
                  var methodInstantiation = addedGenericMethod.MakeTypePipeGenericMethod (proxyType);
                  Assert.That (methodInstantiation.IsGenericMethod, Is.True);
                  Assert.That (methodInstantiation.IsGenericMethodDefinition, Is.False);
                  Assert.That (methodInstantiation.GetGenericArguments(), Is.EqualTo (new[] { proxyType }));

                  return Expression.Call (ctx.This, methodInstantiation);
                });
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.Method();

      Assert.That (result, Is.InstanceOf<IEnumerable<DomainType>>());
      var resultType = result.GetType();
      Assert.That (resultType.IsArray, Is.True);
      Assert.That (resultType.GetElementType(), Is.SameAs (type));
    }

    public class DomainType {
      public virtual object Method () { return null; }
    }
  }
}