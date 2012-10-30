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
using Microsoft.Scripting.Ast;
using NUnit.Framework;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore ("TODO 5095")]
  public class LambdaExpressionTest : TypeAssemblerIntegrationTestBase
  {
    public override void SetUp ()
    {
      base.SetUp ();
      SkipDeletion();
    }

    [Test]
    public void InvokeLambda ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.AllMutableMethods.Single (m => m.Name == "InvokeLambda");
            method.SetBody (ctx => Expression.Invoke (Expression.Lambda (Expression.Add (Expression.Field (ctx.This, "Field"), ctx.PreviousBody))));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var result = instance.InvokeLambda (3);

      //Assert.That (result, Is.EqualTo (5));
    }

    [Test]
    public void ReturnLambda_NoClosure ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.AllMutableMethods.Single (m => m.Name == "ReturnLambda");
            method.SetBody (ctx => Expression.Lambda (Expression.Field (ctx.This, "Field")));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var lambda = instance.ReturnLambda (-7);
      var result = lambda();

      Assert.That (result, Is.EqualTo (1));
    }

    [Test]
    public void ReturnLambda_StaticClosure ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.AllMutableMethods.Single (m => m.Name == "ReturnLambda");
            method.SetBody (ctx => Expression.Lambda (ctx.Parameters[0]));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var lambda = instance.ReturnLambda (7);
      var result = lambda ();

      Assert.That (result, Is.EqualTo (7));
    }

    [Test]
    public void ReturnLambda_InstanceClosure ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var method = mutableType.AllMutableMethods.Single (m => m.Name == "ReturnLambda");
            method.SetBody (ctx => Expression.Lambda (Expression.Add (Expression.Field (ctx.This, "Field"), ctx.Parameters[0])));
          });

      var instance = (DomainType) Activator.CreateInstance (type);
      var lambda = instance.ReturnLambda (7);
      var result = lambda ();

      Assert.That (result, Is.EqualTo (8));
    }

    public class DomainType
    {
      public int Field = 1;

      public virtual int InvokeLambda (int x)
      {
        return x + Field;
      }

      public virtual Func<int> ReturnLambda (int x)
      {
        return () => x + Field;
      }
    }
  }
}