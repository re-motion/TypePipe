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

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class ModifyGenericMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    [Ignore ("TODO 4774")]
    public void ExistingMethodWithGenericParameters ()
    {
      var baseMethod = GetDeclaredMethod (typeof (DomainType), "GenericMethod");

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var genericMethodOverride = proxyType.GetOrAddOverride (baseMethod);
            Assert.That (genericMethodOverride.IsGenericMethod, Is.True);
            Assert.That (genericMethodOverride.IsGenericMethodDefinition, Is.True);
            var genericParameters = genericMethodOverride.GetGenericArguments();
            Assert.That (genericParameters.Select (t => t.Name), Is.EqualTo (new[] { "TKey", "TValue" }));
            var keyParameter = genericParameters[0];
            var valueParameter = genericParameters[1];
            var keyParameterConstraint = keyParameter.GetGenericParameterConstraints().Single();
            Assert.That (keyParameterConstraint.GetGenericTypeDefinition(), Is.SameAs (typeof (IComparable<>)));
            Assert.That (keyParameterConstraint.GetGenericArguments().Single(), Is.SameAs (keyParameter));
            Assert.That (valueParameter.GenericParameterAttributes, Is.EqualTo (GenericParameterAttributes.ReferenceTypeConstraint));

            genericMethodOverride.SetBody (
                ctx =>
                {
                  Assert.That (ctx.GenericParameters, Is.EqualTo (genericParameters));
                  Assert.That (ctx.Parameters[0].Type, Is.EqualTo (typeof (IDictionary<,>).MakeTypePipeGenericType (genericParameters)), "TODO: 5452 Change to Is.SameAs");

                  var containsKeyMethod = ctx.Parameters[0].Type.GetMethod ("ContainsKey");
                  return Expression.Condition (
                      Expression.Call (ctx.Parameters[0], containsKeyMethod, ctx.Parameters[1]),
                      ctx.PreviousBody,
                      Expression.Default (ctx.GenericParameters[1]));
                });
          });

      var method = type.GetMethod ("GenericMethod").MakeGenericMethod (typeof (int), typeof (string));
      var instance = (DomainType) Activator.CreateInstance (type);

      var dict = new Dictionary<int, string> { { 7, "seven" } };
      var result1 = method.Invoke (instance, new object[] { dict, 7 });
      var result2 = method.Invoke (instance, new object[] { dict, 8 });

      Assert.That (result1, Is.EqualTo ("seven"));
      Assert.That (result2, Is.Null);
    }

    public class DomainType
    {
      public virtual TValue GenericMethod<TKey, TValue> (IDictionary<TKey, TValue> dict, TKey key)
          where TKey : IComparable<TKey>
          where TValue : class
      {
        return dict[key];
      }
    }
  }
}