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
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class NonSubclassableTypeTest : IntegrationTestBase
  {
    [Test]
    public void Participant_Throws ()
    {
      var exception = new Exception();
      var reflectionService = GetReflectionService (t => { throw exception; });
      var type = ReflectionObjectMother.GetSomeNonSubclassableType();

      TestDelegate action = () => reflectionService.GetAssembledType (type);

      Assert.That (action, Throws.Exception.SameAs (exception));
    }

    [Test]
    public void Pipeline_Throws_IfNoParticipantThrows ()
    {
      var reflectionService = GetReflectionService (t => { });

      TestDelegate action = () => reflectionService.GetAssembledType (typeof (int));

      var message = "Cannot assemble type for the requested type 'Int32' because it cannot be subclassed.";
      Assert.That (action, Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
    }

    private IReflectionService GetReflectionService (Action<Type> handleNonSubclassableTypeAction)
    {
      var participant = CreateParticipant (handleNonSubclassableTypeAction: handleNonSubclassableTypeAction);
      return CreatePipeline (participant).ReflectionService;
    }
  }
}