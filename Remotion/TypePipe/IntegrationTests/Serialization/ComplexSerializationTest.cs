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
using Remotion.Development.TypePipe.UnitTesting.Serialization;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Implementation;
using Remotion.Utilities;
using Rhino.Mocks;

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
      var settings = PipelineSettings.New().SetEnableSerializationWithoutAssemblySaving (true).Build();

      return CreatePipelineWithIntegrationTestAssemblyLocation (c_participantConfigurationID, settings, participants);
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
        var expectedFullName = Regex.Replace (ctx.SerializedTypeFullName, @"Proxy_\d+$", "Proxy_1");
        Assert.That (type.FullName, Is.EqualTo (expectedFullName));

        return deserializedInstance;
      };
    }

    private static object DeserializeInstance (Func<IParticipant>[] participantProviders, byte[] serializedData)
    {
      // Register a factory for deserialization in current (new) app domain.

      Assert.That (PipelineRegistry.HasInstanceProvider, Is.False);
      var defaultPipelineMock = MockRepository.GenerateStrictMock<IPipeline>();
      defaultPipelineMock.Stub (_ => _.ParticipantConfigurationID).Return ("Mock Default Pipeline");
      IPipelineRegistry pipelineRegistry = new DefaultPipelineRegistry (defaultPipelineMock);
      PipelineRegistry.SetInstanceProvider (() => pipelineRegistry);

      var participants = participantProviders.Select (pp => pp()).Concat (new[] { new ModifyingParticipant() });
      // Avoid no-modification optimization.
      var pipeline = new DefaultPipelineFactory().Create (c_participantConfigurationID, participants.ToArray());
      pipelineRegistry.Register (pipeline);

      try
      {
        return Serializer.Deserialize (serializedData);
      }
      finally
      {
        PipelineRegistryTestHelper.ResetPipelineRegistry();
      }
    }

    [Test]
    public void UsesTypeIdentifierProvider ()
    {
      var pipeline = CreatePipelineForSerialization (CreateParticipantWithTypeIdentifierProvider);
      var instance = pipeline.Create<RequestedType>();

      CheckInstanceIsSerializable (instance, (deserializedInstance, ctx) => { });

      var typeIdentifierProvider = (TypeIdentifierProviderStub) pipeline.Participants.Single().PartialTypeIdentifierProvider;
      Assert.That (typeIdentifierProvider.GetFlattenedExpressionForSerializationWasCalled, Is.True);
    }

    private static IParticipant CreateParticipantWithTypeIdentifierProvider ()
    {
      return CreateParticipant (typeIdentifierProvider: new TypeIdentifierProviderStub());
    }

    private class TypeIdentifierProviderStub : ITypeIdentifierProvider
    {
      public bool GetFlattenedExpressionForSerializationWasCalled;

      public object GetID (Type requestedType)
      {
        return "identifier";
      }

      public Expression GetExpression (object id)
      {
        Assert.That (id, Is.EqualTo ("identifier"));

        return Expression.Constant ("identifier from code");
      }

      public Expression GetFlatValueExpressionForSerialization (object id)
      {
        Assert.That (id, Is.EqualTo ("identifier"));
        GetFlattenedExpressionForSerializationWasCalled = true;

        var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new FlatValueStub ("real value"));
        return Expression.New (constructor, Expression.Constant ("identifier"));
      }
    }

    [Serializable]
    public class RequestedType {}
  }
}