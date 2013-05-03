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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
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
              ctx.CreateType ("AdditionalType", "MyNs", TypeAttributes.Class, typeof (object));
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
      var unrelatedType = ReflectionObjectMother.GetSomeType();

      Assert.That (_reflectionService.IsAssembledType (assembledType1), Is.True);
      Assert.That (_reflectionService.IsAssembledType (assembledType2), Is.True);
      Assert.That (_reflectionService.IsAssembledType (otherGeneratedType), Is.False);
      Assert.That (_reflectionService.IsAssembledType (unrelatedType), Is.False);

      Assert.That (_reflectionService.GetRequestedType (assembledType1), Is.SameAs (typeof (RequestedType1)));
      Assert.That (_reflectionService.GetRequestedType (assembledType2), Is.SameAs (typeof (RequestedType2)));

      var message = "The argument type is not an assembled type.\r\nParameter name: assembledType";
      Assert.That (() => _reflectionService.GetRequestedType (otherGeneratedType), Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (() => _reflectionService.GetRequestedType (unrelatedType), Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    public void GetAssembledType_ViaTypeID ()
    {
      var assembledType = _reflectionService.GetAssembledType (typeof (RequestedType1));
      var typeID = _reflectionService.GetTypeID (assembledType);
      Assert.That (typeID.RequestedType, Is.SameAs (typeof (RequestedType1)));

      var type1 = _reflectionService.GetAssembledType (typeID);
      var type2 = _pipeline.Create (typeID).GetType();

      Assert.That (type1, Is.SameAs (assembledType));
      Assert.That (type2, Is.SameAs (assembledType));
    }

    public class RequestedType1 {}
    public class RequestedType2 {}
  }
}