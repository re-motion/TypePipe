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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;

namespace TypePipe.IntegrationTests
{
  [Ignore("TODO 5119")]
  [TestFixture]
  public class AddTypeInitializerTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Standard ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.StaticField);

      var type = AssembleType<DomainType> (
          mutableType =>
          {
            Assert.That (mutableType.TypeInitializer, Is.Null);
            var typeInitializer = mutableType.GetOrAddTypeInitializer();

            Assert.That (mutableType.TypeInitializer, Is.SameAs (typeInitializer));
            Assert.That (mutableType.GetOrAddTypeInitializer(), Is.SameAs (typeInitializer));
            Assert.That (mutableType.AddedConstructors, Is.EqualTo (new[] { typeInitializer }));

            typeInitializer.SetBody (ctx => Expression.Assign (Expression.Field (null, field), Expression.Constant ("abc")));
          });

      // Force type to be loaded.
      // TODO 5119: Better alternative?
      Activator.CreateInstance (type);

      Assert.That (DomainType.StaticField, Is.EqualTo ("abc"));
    }

    // TODO 5119: With the subclass proxy model we can never prevent the base initializer from executing.
    // Should we therefore force the user to include the previous body into the new body?

    [Test]
    public void TypeInitializerCannotUseThis ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            var typeInitializer = mutableType.GetOrAddTypeInitializer();
            Assert.That (typeInitializer.IsStatic, Is.True);

            typeInitializer.SetBody (
                ctx =>
                {
                  Assert.That (() => ctx.This, Throws.InvalidOperationException.With.Message.EqualTo ("Static methods cannot use 'This'."));
                  return Expression.Empty();
                });
          });
    }

    public class DomainType
    {
      public static string StaticField;
    }
  }
}