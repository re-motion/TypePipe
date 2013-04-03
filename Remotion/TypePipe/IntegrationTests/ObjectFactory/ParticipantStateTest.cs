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
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.ObjectFactory
{
  [TestFixture]
  public class ParticipantStateTest : IntegrationTestBase
  {
    [Test]
    public void GlobalState ()
    {
      bool stateWasRead = false;

      var participant1 = CreateParticipant (ctx =>
      {
        if (ctx.RequestedType == typeof (RequestedType1))
        {
          Assert.That (ctx.State, Is.Empty);
          ctx.State.Add ("key", 7);
        }
        else
        {
          Assert.That (ctx.State["key"], Is.EqualTo (7), "Participant sees state even when requsted types differ.");
          stateWasRead = true;
        }
      });
      var participant2 = CreateParticipant (ctx => Assert.That (ctx.State["key"], Is.EqualTo (7), "Participant 2 sees state of participant 1."));

      var factory = CreateObjectFactory (participant1, participant2);

      Assert.That (() => factory.CreateObject<RequestedType1>(), Throws.Nothing);
      Assert.That (() => factory.CreateObject<RequestedType2>(), Throws.Nothing);

      Assert.That (stateWasRead, Is.True);
    }

    [Ignore ("TODO 5504")]
    [Test]
    public void RebuildStateFromLoadedTypes ()
    {
      var participant1 = CreateParticipant (ctx => ctx.CreateType ("AdditionalType", "MyNs", TypeAttributes.Class, typeof (object)));
      var savingFactory = CreateObjectFactory (participant1);

      Type loadedType = null;
      var participant2 = CreateParticipant (
          rebuildStateAction: ctx =>
          {
            var loadedProxy = ctx.ProxyTypes.Single();
            var additionalType = ctx.AdditionalTypes.Single();
            Assert.That (loadedProxy.RequestedType, Is.SameAs (typeof (RequestedType1)));
            Assert.That (additionalType.FullName, Is.EqualTo ("MyNs.AdditionalType"));
            loadedType = loadedProxy.GeneratedType;
          });
      var loadingFactory = CreateObjectFactory (participant2);

      savingFactory.GetAssembledType (typeof (RequestedType1));
      var assemblyPath = savingFactory.CodeManager.FlushCodeToDisk();
      loadingFactory.CodeManager.LoadFlushedCode (Assembly.LoadFrom (assemblyPath));

      Assert.That (loadedType, Is.Not.Null);
      Assert.That (loadingFactory.GetAssembledType (typeof (RequestedType1)), Is.SameAs (loadedType));
    }

    public class RequestedType1 {}
    public class RequestedType2 {}
  }
}