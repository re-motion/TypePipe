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
using System.Text.RegularExpressions;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Configuration;

namespace Remotion.TypePipe.IntegrationTests.Serialization
{
  [TestFixture]
  public class ComplexSerializationTest : SerializationTestBase
  {
    private const string c_participantConfigurationID = "ComplexSerializationTest";

    private Func<IParticipant>[] _participantProviders;

    protected override IPipeline CreatePipelineForSerialization (params Func<IParticipant>[] participantProviders)
    {
      _participantProviders = participantProviders;

      var participants = _participantProviders.Select (pp => pp()).ToArray();
      var settings = new PipelineSettings (c_participantConfigurationID) { EnableSerializationWithoutAssemblySaving = true };

      return CreatePipeline (settings, participants);
    }

    protected override Func<SerializationTestContext<T>, T> CreateDeserializationCallback<T> (SerializationTestContext<T> context)
    {
      // Do not flush generated assembly to disk to force complex serialization strategy.

      context.ParticipantProviders = _participantProviders;
      return ctx =>
      {
        var deserializedInstance = (T) DeserializeInstance (ctx.ParticipantProviders, ctx.SerializedData);

        // The assembly name must be different, i.e. the new app domain should use an in-memory assembly.
        var type = deserializedInstance.GetType();
        Assert.That (type.AssemblyQualifiedName, Is.Not.EqualTo (ctx.ExpectedAssemblyQualifiedName));
        Assert.That (type.Assembly.GetName().Name, Is.StringStarting ("TypePipe_GeneratedAssembly_"));
        Assert.That (type.Module.Name, Is.EqualTo ("<In Memory Module>"));

        // The generated type is always the single type in the assembly. Its name is therefore the same as the serialized type name, but with
        // "Proxy1" in the end.
        var expectedFullName = Regex.Replace (ctx.SerializedTypeFullName, @"Proxy\d+$", "Proxy1");
        Assert.That (type.FullName, Is.EqualTo (expectedFullName));

        return deserializedInstance;
      };
    }

    private static object DeserializeInstance (Func<IParticipant>[] participantProviders, byte[] serializedData)
    {
      var registry = SafeServiceLocator.Current.GetInstance<IPipelineRegistry>();
      var participants = participantProviders.Select (pp => pp());
      var pipeline = PipelineFactory.Create (c_participantConfigurationID, participants.ToArray());

      // Register a factory for deserialization in current (new) app domain.
      registry.Register (pipeline);
      try
      {
        return Serializer.Deserialize (serializedData);
      }
      finally
      {
        registry.Unregister (c_participantConfigurationID);
      }
    }
  }
}