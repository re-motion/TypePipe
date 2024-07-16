﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.IntegrationTests.TypeAssembly;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class ObjectInitializationTest : IntegrationTestBase
  {
    private IPipeline _pipeline;

    public override void SetUp ()
    {
      base.SetUp();

      _pipeline = CreatePipeline();
    }

    [Test]
    public void CreateObject ()
    {
      var instance = _pipeline.Create<DomainType>();

      Assert.That (instance.String, Is.EqualTo ("construction"));
      Assert.That (instance.CtorCalled, Is.True);
    }

    [Test]
    public void GetAssembledType_CallWiredCtor ()
    {
      var type = _pipeline.ReflectionService.GetAssembledType (typeof (DomainType));
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.String, Is.EqualTo ("construction"));
      Assert.That (instance.CtorCalled, Is.True);
    }

    [Test]
    public void GetAssembledType_PrepareAssembledTypeInstance ()
    {
      var type = _pipeline.ReflectionService.GetAssembledType (typeof (DomainType));
      var instance = (DomainType) FormatterServices.GetUninitializedObject (type);

      Assert.That (instance.CtorCalled, Is.False);
      Assert.That (instance.String, Is.Null);

      _pipeline.ReflectionService.PrepareExternalUninitializedObject (instance, InitializationSemantics.Construction);
      Assert.That (instance.String, Is.EqualTo ("construction"));

      _pipeline.ReflectionService.PrepareExternalUninitializedObject (instance, InitializationSemantics.Deserialization);
      Assert.That (instance.String, Is.EqualTo ("construction deserialization"));
    }

    public class DomainType
    {
      public readonly bool CtorCalled;
      [UsedImplicitly] public string String;

      public DomainType () { CtorCalled = true; }
    }

    private IPipeline CreatePipeline ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.String);
      var participant = CreateParticipant (
          proxyType =>
          {
            Assert.That (proxyType.Initialization.Expressions, Is.Empty);

            proxyType.AddInitialization (
                ctx =>
                {
                  Assert.That (ctx.IsStatic, Is.False);

                  var fieldExpr = Expression.Field (ctx.This, field);
                  return Expression.Assign (
                      fieldExpr,
                      ExpressionHelper.StringConcat (
                          fieldExpr,
                          Expression.Condition (
                              Expression.Equal (ctx.InitializationSemantics, Expression.Constant (InitializationSemantics.Construction)),
                              Expression.Constant ("construction"),
                              Expression.Constant (" deserialization"))));
                });

            Assert.That (proxyType.Initialization.Expressions, Is.Not.Empty);
          });

      return CreatePipeline (new[] { participant });
    }
  }
}