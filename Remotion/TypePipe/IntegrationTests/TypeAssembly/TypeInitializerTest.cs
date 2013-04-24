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
using Remotion.TypePipe.Dlr.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class TypeInitializerTest : TypeAssemblerIntegrationTestBase
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
            Assert.That (proxyType.TypeInitializer, Is.Null);
            Assert.That (proxyType.MutableTypeInitializer, Is.Null);

            var initializationExpression = Expression.Assign (Expression.Field (null, field), Expression.Constant ("abc"));
            var typeInitializer = proxyType.AddTypeInitializer (
                ctx =>
                {
                  Assert.That (ctx.IsStatic, Is.True);
                  return initializationExpression;
                });

            Assert.That (proxyType.TypeInitializer, Is.SameAs (typeInitializer));
            Assert.That (proxyType.MutableTypeInitializer, Is.SameAs (typeInitializer));
            Assert.That (proxyType.AddedConstructors, Has.No.Member (typeInitializer));
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
            var field = proxyType.AddField ("s_field", FieldAttributes.Public | FieldAttributes.Static, typeof (string));
            var initializationExpression = Expression.Assign (Expression.Field (null, field), Expression.Constant ("abc"));
            proxyType.AddTypeInitializer (ctx => initializationExpression);
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
          p => p.AddTypeInitializer (ctx => Expression.Assign (fieldExpr, ExpressionHelper.StringConcat (fieldExpr, Expression.Constant ("abc")))),
          p => p.MutableTypeInitializer.SetBody (
              ctx => Expression.Block (
                  ctx.PreviousBody, Expression.Assign (fieldExpr, ExpressionHelper.StringConcat (fieldExpr, Expression.Constant ("def"))))));

      RuntimeHelpers.RunClassConstructor (type.TypeHandle);
      Assert.That (DomainType.StaticField, Is.EqualTo ("abcdef"));
    }

    public class DomainType
    {
      public static string StaticField;
    }
  }
}