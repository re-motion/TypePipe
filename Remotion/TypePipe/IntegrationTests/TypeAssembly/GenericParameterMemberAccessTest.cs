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
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenericParameterMemberAccessTest : TypeAssemblerIntegrationTestBase
  {
    [Ignore("TODO 5444")]
    [Test]
    public void AccessMembers ()
    {
      var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.GenericMethod<Constraint>(null));
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((Constraint o) => o.Field);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((Constraint o) => o.Method());
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((Constraint o) => o.Property);

      var type = AssembleType<DomainType> (p => p.GetOrAddOverride (overriddenMethod).SetBody (ctx =>
      {
        var parameter = ctx.Parameters[0];
        return Expression.Block (
            Expression.Assign (Expression.Field (parameter, field), Expression.Call (parameter, method)),
            Expression.Assign (Expression.Property (parameter, property), Expression.Field (parameter, field)));

      }));

      var instance = (DomainType) Activator.CreateInstance (type);
      var arg = new Constraint();

      instance.GenericMethod (arg);

      Assert.That (arg.Field, Is.EqualTo ("method"));
      Assert.That (arg.Property, Is.EqualTo ("method"));
    }

    [Ignore ("TODO 5444")]
    [Test]
    public void CallVirtualMethod ()
    {
      var overriddenMethod = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.GenericMethod<Constraint> (null));
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((Constraint o) => o.Field);
      var virtualMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((Constraint o) => o.Method());

      var type = AssembleType<DomainType> (
          p =>
          p.GetOrAddOverride (overriddenMethod)
           .SetBody (ctx => Expression.Assign (Expression.Field (ctx.Parameters[0], field), Expression.Call (ctx.Parameters[0], virtualMethod))));

      var instance = (DomainType) Activator.CreateInstance (type);
      var arg = new Constraint();

      instance.GenericMethod (arg);

      Assert.That (arg.Field, Is.EqualTo ("virtual method"));
    }

    public class DomainType
    {
      public virtual void GenericMethod<T> (T t) where T : Constraint {}
    }

    public class Constraint
    {
      [UsedImplicitly] public string Field;
      // Constructors are not called via 'this' reference.
      public string Method () { return "method"; }
      public string Property { get; set; }
      // Events do not have a representation in expression trees.

      public virtual string VirtualMethod () { return "virtual method"; }
    }
  }
}