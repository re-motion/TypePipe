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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class TypeInitializationTest : TypeAssemblerIntegrationTestBase
  {
    public override void TearDown ()
    {
      base.TearDown();

      DomainType.StaticField = null;
    }

    [Test]
    public void ExistingField ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.StaticField);
      Assert.That (DomainType.StaticField, Is.Null);

      var type = AssembleType<DomainType> (
          proxyType =>
          {
            Assert.That (proxyType.TypeInitializations, Is.Empty);

            var initializationExpression = Expression.Assign (Expression.Field (null, field), Expression.Constant ("abc"));
            proxyType.AddTypeInitialization (
                ctx =>
                {
                  Assert.That (ctx.IsStatic, Is.True);
                  return initializationExpression;
                });

            Assert.That (proxyType.TypeInitializations, Is.EqualTo (new[] { initializationExpression }));
          });

      RuntimeHelpers.RunClassConstructor (type.TypeHandle);
      Assert.That (DomainType.StaticField, Is.EqualTo ("abc"));
    }

    [Test]
    public void AddedField ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var field = proxyType.AddField ("s_field", typeof (string), FieldAttributes.Public | FieldAttributes.Static);
            var initializationExpression = Expression.Assign (Expression.Field (null, field), Expression.Constant ("abc"));
            proxyType.AddTypeInitialization (ctx => initializationExpression);
          });

      RuntimeHelpers.RunClassConstructor (type.TypeHandle);
      var fieldValue = type.GetField ("s_field").GetValue (null);
      Assert.That (fieldValue, Is.EqualTo ("abc"));
    }

    [Test]
    public void MultipleParticipants ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.StaticField);
      var fieldExpr = Expression.Field (null, field);

      var type = AssembleType<DomainType> (
          mt => mt.AddTypeInitialization (ctx => Expression.Assign (fieldExpr, ExpressionHelper.StringConcat (fieldExpr, Expression.Constant ("abc")))),
          mt => mt.AddTypeInitialization (ctx => Expression.Assign (fieldExpr, ExpressionHelper.StringConcat (fieldExpr, Expression.Constant ("def")))));

      RuntimeHelpers.RunClassConstructor (type.TypeHandle);
      Assert.That (DomainType.StaticField, Is.EqualTo ("abcdef"));
    }

    [Test]
    public void TypeInitializer ()
    {
      AssembleType<DomainType> (
          proxyType =>
          {
            var message = "Type initializers (static constructors) cannot be modified via this API, use ProxyType.AddTypeInitialization instead.";
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            Assert.That (() => proxyType.TypeInitializer, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
            Assert.That (() => proxyType.GetConstructors (bindingFlags), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
            Assert.That (
                () => proxyType.GetConstructor (bindingFlags, null, Type.EmptyTypes, null),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
            Assert.That (
                () => proxyType.AddConstructor (MethodAttributes.Static, ParameterDeclaration.EmptyParameters, ctx => null),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
                    "Type initializers (static constructors) cannot be added via this API, use ProxyType.AddTypeInitialization instead."));
          });
    }

    public class DomainType
    {
      public static string StaticField;
    }

    public class TypeWithInitializer
    {
      static TypeWithInitializer () { }
    }
  }
}