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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.FunctionalProgramming;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Serialization;

namespace Remotion.TypePipe.IntegrationTests.Serialization
{
  [TestFixture]
  public class ComplexSerializationTest : SerializationTestBase
  {
    private const string c_factoryIdentifier = "abc";

    private static IParticipant CreateSerializationParticipant ()
    {
      return new SerializationParticipant (c_factoryIdentifier);
    }

    private Func<IParticipant>[] _participantProviders;

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected override IObjectFactory CreateObjectFactoryForSerialization (params Func<IParticipant>[] participantProviders)
    {
      _participantProviders = participantProviders.Concat (CreateSerializationParticipant).ToArray();
      var allParticipants = _participantProviders.Select (pp => pp());
      var factory = CreateObjectFactory (allParticipants, stackFramesToSkip: 1);

      return factory;
    }

    protected override Func<SerializationTestContext, SerializableType> CreateDeserializationCallback (SerializationTestContext context)
    {
      // Do not flush generated assembly to disk to force complex serialization strategy.

      context.ParticipantProviders = _participantProviders;
      return ctx =>
      {
        var registry = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();

        SetUpDeserialization (registry, ctx.ParticipantProviders);
        var deserializedInstance = (SerializableType) Serializer.Deserialize (ctx.SerializedData);
        TearDownDeserialization (registry);

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

    private static void SetUpDeserialization (IObjectFactoryRegistry registry, IEnumerable<Func<IParticipant>> participantProviders)
    {
      var participantCreators = participantProviders.Select<Func<IParticipant>, Func<Object>> (pp => () => pp()).ToArray();

      using (new ServiceLocatorScope (typeof (IParticipant), participantCreators))
      {
        var factory = SafeServiceLocator.Current.GetInstance<IObjectFactory>();
        // Register a factory for deserialization in current (new) app domain.
        registry.Register (c_factoryIdentifier, factory);
      }
    }

    private static void TearDownDeserialization (IObjectFactoryRegistry registry)
    {
      registry.Unregister (c_factoryIdentifier);
    }
  }
}