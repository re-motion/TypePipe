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
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.IntegrationTests.Serialization
{
  [Ignore ("TODO 5500")]
  [TestFixture]
  public class SimpleSerializationTest : SerializationTestBase
  {
    [MethodImpl (MethodImplOptions.NoInlining)]
    protected override IObjectFactory CreateObjectFactoryForSerialization (params Func<IParticipant>[] participantProviders)
    {
      var participants = participantProviders.Select (pp => pp());
      var factory = CreateObjectFactory (participants, stackFramesToSkip: 1);
      factory.CodeGenerator.SetAssemblyDirectory (AppDomain.CurrentDomain.BaseDirectory);

      return factory;
    }

    protected override Func<SerializationTestContext<T>, T> CreateDeserializationCallback<T> (SerializationTestContext<T> context)
    {
      // Flush generated assembly to disk to enable simple serialization strategy.
      Flush();

      return ctx =>
      {
        var deserializedInstance = (T) Serializer.Deserialize (ctx.SerializedData);
        Assert.That (deserializedInstance.GetType().AssemblyQualifiedName, Is.EqualTo (ctx.ExpectedAssemblyQualifiedName));

        return deserializedInstance;
      };
    }
  }
}