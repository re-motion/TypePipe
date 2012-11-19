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
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe;
using TypePipe.IntegrationTests.TypeAssembly;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ObjectInitializationTest : ObjectFactoryIntegrationTestBase
  {
    private IObjectFactory _factory;

    public override void SetUp ()
    {
      base.SetUp();

      _factory = CreateObjectFactory();
    }

    [Test]
    public void CreateObject ()
    {
      var instance = _factory.CreateObject<DomainType>();

      Assert.That (instance.String, Is.EqualTo ("initialized"));
      Assert.That (instance.CtorCalled, Is.True);
    }

    [Test]
    public void GetUninitializedObject ()
    {
      var instance = (DomainType) _factory.GetUninitializedObject (typeof (DomainType));

      Assert.That (instance.String, Is.EqualTo ("initialized"));
      Assert.That (instance.CtorCalled, Is.False);
    }

    [Test]
    public void GetAssembledType_CallWiredCtor ()
    {
      var type = _factory.GetAssembledType (typeof (DomainType));
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.String, Is.EqualTo ("initialized"));
      Assert.That (instance.CtorCalled, Is.True);
    }

    [Test]
    public void GetAssembledType_PrepareAssembledTypeInstance ()
    {
      var type = _factory.GetAssembledType (typeof (DomainType));
      var instance = (DomainType) FormatterServices.GetUninitializedObject (type);

      Assert.That (instance.CtorCalled, Is.False);
      Assert.That (instance.String, Is.Null);

      _factory.PrepareAssembledTypeInstance (instance);
      Assert.That (instance.String, Is.EqualTo ("initialized"));
      _factory.PrepareAssembledTypeInstance (instance);
      Assert.That (instance.String, Is.EqualTo ("initializedinitialized"));
    }

    public class DomainType
    {
      public readonly bool CtorCalled;
      [UsedImplicitly] public string String;

      public DomainType () { CtorCalled = true; }
    }

    private IObjectFactory CreateObjectFactory ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.String);
      var participant = CreateParticipant (
          mutableType =>
          {
            Assert.That (mutableType.InstanceInitializations, Is.Empty);

            mutableType.AddInstanceInitialization (
                ctx =>
                {
                  Assert.That (ctx.IsStatic, Is.False);

                  var fieldExpr = Expression.Field (ctx.This, field);
                  return Expression.Assign (fieldExpr, ExpressionHelper.StringConcat (fieldExpr, Expression.Constant ("initialized")));
                });

            Assert.That (mutableType.InstanceInitializations, Is.Not.Empty);
          });

      return CreateObjectFactory (new[] { participant }, stackFramesToSkip: 1);
    }
  }
}