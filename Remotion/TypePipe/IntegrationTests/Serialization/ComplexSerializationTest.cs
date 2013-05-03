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
using Remotion.Development.TypePipe.UnitTesting;
using Remotion.Development.UnitTesting;
using Remotion.FunctionalProgramming;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Dlr.Ast;

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
        var deserializedInstance = DeserializeInstance (ctx);

        // The assembly name must be different, i.e. the new app domain should use an in-memory assembly.
        var type = deserializedInstance.GetType();
        Assert.That (type.AssemblyQualifiedName, Is.Not.EqualTo (ctx.ExpectedAssemblyQualifiedName));
        Assert.That (type.Assembly.GetName().Name, Is.StringStarting ("TypePipe_GeneratedAssembly_"));
        Assert.That (type.Module.Name, Is.EqualTo ("<In Memory Module>"));

        // The generated type is always the single type in the assembly. Its name is therefore the same as the serialized type name, but with
        // "Proxy1" in the end.
        var expectedFullName = Regex.Replace (ctx.SerializedTypeFullName, @"Proxy_\d+$", "Proxy_1");
        Assert.That (type.FullName, Is.EqualTo (expectedFullName));

        return deserializedInstance;
      };
    }

    private static T DeserializeInstance<T> (SerializationTestContext<T> context)
    {
      var registry = SafeServiceLocator.Current.GetInstance<IPipelineRegistry>();
      var participants = context.ParticipantProviders.Select (pp => pp()).Concat (new ModifyingParticipant()); // Avoid no-modification optimization.
      context.Pipeline = PipelineFactory.Create (c_participantConfigurationID, participants.ToArray());

      // Register a factory for deserialization in current (new) app domain.
      registry.Register (context.Pipeline);
      try
      {
        return (T) Serializer.Deserialize (context.SerializedData);
      }
      finally
      {
        registry.Unregister (c_participantConfigurationID);
      }
    }

    [Test]
    public void UsesTypeIdentifierProvider ()
    {
      var pipeline = CreatePipelineForSerialization (CreateParticipantWithTypeIdentifierProvider);
      var instance = pipeline.Create<RequestedType>();

      CheckInstanceIsSerializable (
          instance,
          (deserializedInstance, ctx) =>
          {
            var deserializingTypeIdentifierProvider =
                (TypeIdentifierProviderStub) ctx.Pipeline.Participants.Select (p => p.PartialTypeIdentifierProvider).Single (p => p != null);
            Assert.That (deserializingTypeIdentifierProvider.GetFlattenedSerializeExpressionWasCalled, Is.False);
            Assert.That (deserializingTypeIdentifierProvider.DeserializeIDWasCalled, Is.True);
          });

      var typeIdentifierProvider = (TypeIdentifierProviderStub) pipeline.Participants.Single().PartialTypeIdentifierProvider;
      Assert.That (typeIdentifierProvider.GetFlattenedSerializeExpressionWasCalled, Is.True);
      Assert.That (typeIdentifierProvider.DeserializeIDWasCalled, Is.False);
    }

    private static IParticipant CreateParticipantWithTypeIdentifierProvider ()
    {
      return CreateParticipant (typeIdentifierProvider: new TypeIdentifierProviderStub());
    }

    private class TypeIdentifierProviderStub : ITypeIdentifierProvider
    {
      public bool GetFlattenedSerializeExpressionWasCalled;
      public bool DeserializeIDWasCalled;

      public object GetID (Type requestedType)
      {
        return "identifier";
      }

      public Expression GetExpression (object id)
      {
        Assert.That (id, Is.EqualTo ("identifier"));

        return Expression.Constant ("identifier from code");
      }

      public Expression GetFlattenedSerializeExpression (object id)
      {
        Assert.That (id, Is.EqualTo ("identifier"));
        GetFlattenedSerializeExpressionWasCalled = true;

        return Expression.Constant ("flattened identifier from code");
      }

      public object DeserializeID (object flattenedID)
      {
        Assert.That (flattenedID, Is.EqualTo ("flattened identifier from code"));
        DeserializeIDWasCalled = true;

        return "identifier";
      }
    }

    [Serializable]
    public class RequestedType {}
  }
}