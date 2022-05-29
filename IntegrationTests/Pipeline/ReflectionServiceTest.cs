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
using System.Reflection;
using NUnit.Framework;
using Remotion.TypePipe.Development.UnitTesting;
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class ReflectionServiceTest : IntegrationTestBase
  {
    private IPipeline _pipeline;
    private IReflectionService _reflectionService;

    public override void SetUp ()
    {
      base.SetUp ();

      var participant = CreateParticipant (
          ctx =>
          {
            if (ctx.RequestedType == typeof (RequestedType1))
              ctx.CreateAdditionalType (new object(), "AdditionalType", "MyNs", TypeAttributes.Class, typeof (object));
          });

      _pipeline = CreatePipeline (participant);
      _reflectionService = _pipeline.ReflectionService;
    }

    [Test]
    public void ServiceMethods ()
    {
      var assembledType1 = _reflectionService.GetAssembledType (typeof (RequestedType1));
      var assembledType2 = _pipeline.Create<RequestedType2>().GetType();
      var otherGeneratedType = assembledType1.Assembly.GetType ("MyNs.AdditionalType");
      var unrelatedType = typeof (Random);

      Assert.That (_reflectionService.IsAssembledType (assembledType1), Is.True);
      Assert.That (_reflectionService.IsAssembledType (assembledType2), Is.True);
      Assert.That (_reflectionService.IsAssembledType (otherGeneratedType), Is.False);
      Assert.That (_reflectionService.IsAssembledType (unrelatedType), Is.False);

      Assert.That (_reflectionService.GetRequestedType (assembledType1), Is.SameAs (typeof (RequestedType1)));
      Assert.That (_reflectionService.GetRequestedType (assembledType2), Is.SameAs (typeof (RequestedType2)));

      var message1 = "The argument type 'AdditionalType' is not an assembled type.";
      var message2 = "The argument type 'Random' is not an assembled type.";
      Assert.That (() => _reflectionService.GetRequestedType (otherGeneratedType), Throws.ArgumentException.With.Message.StartsWith (message1));
      Assert.That (() => _reflectionService.GetRequestedType (unrelatedType), Throws.ArgumentException.With.Message.StartsWith (message2));
    }

    [Test]
    public void GetAssembledType_ViaTypeIDForRequestedType ()
    {
      var assembledType = _reflectionService.GetAssembledType (typeof (RequestedType1));
      var typeID = _reflectionService.GetTypeIDForRequestedType (typeof (RequestedType1));
      Assert.That (typeID.RequestedType, Is.SameAs (typeof (RequestedType1)));

      var type1 = _reflectionService.GetAssembledType (typeID);
      var type2 = _reflectionService.InstantiateAssembledType (typeID, ParamList.Empty, false).GetType();

      Assert.That (type1, Is.SameAs (assembledType));
      Assert.That (type2, Is.SameAs (assembledType));
    }

    [Test]
    public void GetAssembledType_ViaTypeIDForAssembledType ()
    {
      var assembledType = _reflectionService.GetAssembledType (typeof (RequestedType1));
      var typeID = _reflectionService.GetTypeIDForAssembledType (assembledType);
      Assert.That (typeID.RequestedType, Is.SameAs (typeof (RequestedType1)));

      var type1 = _reflectionService.GetAssembledType (typeID);
      var type2 = _reflectionService.InstantiateAssembledType (typeID, ParamList.Empty, false).GetType();

      Assert.That (type1, Is.SameAs (assembledType));
      Assert.That (type2, Is.SameAs (assembledType));
    }

    [Test]
    public void InstantiateAssembledType ()
    {
      var pipelineFactory = new DefaultPipelineFactory();
      var equivalentReflectionService =  pipelineFactory.Create (_pipeline.ParticipantConfigurationID, new ModifyingParticipant()).ReflectionService;
      var otherReflectionService =  pipelineFactory.Create ("other id", new ModifyingParticipant()).ReflectionService;

      var assembledType1 = _reflectionService.GetAssembledType (typeof (RequestedType1));
      var assembledType2 = equivalentReflectionService.GetAssembledType (typeof (RequestedType1));
      var assembledType3 = otherReflectionService.GetAssembledType (typeof (RequestedType1));

      var instance1 = _reflectionService.InstantiateAssembledType (assembledType1, ParamList.Empty, false);
      var instance2 = _reflectionService.InstantiateAssembledType (assembledType2, ParamList.Empty, false);
      var instance3 = _reflectionService.InstantiateAssembledType (assembledType3, ParamList.Empty, false);
      // We though about disallowing instantiating assembled types created by a pipeline with a different participant configuration ID.
      // But the implementation would require us to add the [TypePipeAssemblyAttribute] attribute immediately, and query for it in a completely
      // different place. So we decided (for now) that it is not worth the effort.
      //var message = "The provided assembled type 'RequestedType1_Proxy_1' was generated by an incompatible pipeline 'other id'.";
      //Assert.That (() => _reflectionService.InstantiateAssembledType (assembledType3), Throws.ArgumentException.With.Message.EqualTo (message));

      Assert.That (instance1.GetType(), Is.SameAs (assembledType1));
      Assert.That (instance2.GetType(), Is.SameAs (assembledType2));
      Assert.That (instance3.GetType(), Is.SameAs (assembledType3));
    }

    public class RequestedType1 {}
    public class RequestedType2 {}
  }
}