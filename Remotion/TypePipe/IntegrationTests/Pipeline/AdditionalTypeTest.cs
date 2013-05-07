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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class AdditionalTypeTest : IntegrationTestBase
  {
    [Test]
    public void ExistingType ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();

      var participant1 = CreateParticipant (additionalTypeFunc: (id, ctx) => null);
      var participant2 = CreateParticipant (additionalTypeFunc: (id, ctx) =>
      {
        Assert.That (id, Is.SameAs (additionalTypeID));
        return additionalType;
      });
      var participant3 = CreateParticipant (additionalTypeFunc: (id, ctx) => { throw new Exception ("Should not be called."); });
      var pipeline = CreatePipeline (participant1, participant2, participant3);

      var result = pipeline.ReflectionService.GetAdditionalType (additionalTypeID);

      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void GeneratedType_CachedViaParticipantState ()
    {
      var additionalTypeID = new object ();

      var participant = CreateParticipant (additionalTypeFunc: (id, ctx) =>
      {
        Assert.That (id, Is.SameAs (additionalTypeID));

        if (ctx.State.ContainsKey ("key"))
          return (Type) ctx.State["key"];

        var myClass = ctx.CreateClass ("Class", "My", typeof (object));
        ctx.GenerationCompleted += generatedTypeContext => { ctx.State["key"] = generatedTypeContext.GetGeneratedType (myClass); };

        return myClass;
      });
      var pipeline = CreatePipeline (participant);

      var result1 = pipeline.ReflectionService.GetAdditionalType (additionalTypeID);
      var result2 = pipeline.ReflectionService.GetAdditionalType (additionalTypeID);

      Assert.That (result1.FullName, Is.EqualTo ("My.Class"));
      Assert.That (result1, Is.SameAs (result2), "Cached by participant.");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "No participant provided an additional type for the given identifier.")]
    public void NoParticipantProvidesAdditionalType ()
    {
      var pipeline = CreatePipeline(/* no participants */);
      pipeline.ReflectionService.GetAdditionalType (new object());
    }
  }
}