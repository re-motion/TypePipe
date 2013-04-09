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

using System.Collections.Generic;
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Creates instances of <see cref="IPipeline"/>, which are the main entry point of the pipeline.
  /// </summary>
  public static class PipelineFactory
  {
    /// <summary>
    /// Creates an <see cref="IPipeline"/> with the given participant configuration ID containing the specified participants.
    /// </summary>
    /// <param name="participantConfigurationID">The participant configuration ID.</param>
    /// <param name="participants">The participants that should be used by this object factory.</param>
    /// <returns>An new instance of <see cref="IPipeline"/>.</returns>
    public static IPipeline Create (string participantConfigurationID, params IParticipant[] participants)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("participants", participants);

      return Create (participantConfigurationID, (IEnumerable<IParticipant>) participants);
    }

    /// <summary>
    /// Creates an <see cref="IPipeline"/> with the given participant configuration ID containing the specified participants and optionally a
    /// custom configuration provider. If the configuration provider is omitted, the <c>App.config</c>-based configuration provider
    /// (<see cref="AppConfigBasedConfigurationProvider"/>) is used.
    /// </summary>
    /// <param name="participantConfigurationID">The participant configuration ID.</param>
    /// <param name="participants">The participants that should be used by this object factory.</param>
    /// <param name="configurationProvider">
    /// A configuration provider; or <see langword="null"/> for the default, AppConfig-based configuration provider.
    /// </param>
    /// <returns>An new instance of <see cref="IPipeline"/>.</returns>
    public static IPipeline Create (
        string participantConfigurationID, IEnumerable<IParticipant> participants, IConfigurationProvider configurationProvider = null)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("participants", participants);
      configurationProvider = configurationProvider ?? new AppConfigBasedConfigurationProvider();

      var participantsCollection = participants.ConvertToCollection();
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("participants", participantsCollection);

      var mutableTypeFactory = new MutableTypeFactory();
      var typeAssembler = new TypeAssembler (participantConfigurationID, participantsCollection, mutableTypeFactory);
      var memberEmitterFactory = new MemberEmitterFactory();
      var reflectionEmitCodeGenerator = new ReflectionEmitCodeGenerator (new ModuleBuilderFactory(), configurationProvider);
      var mutableTypeCodeGeneratorFactory = new MutableTypeCodeGeneratorFactory (
          memberEmitterFactory,
          reflectionEmitCodeGenerator,
          new InitializationBuilder(),
          new ProxySerializationEnabler (new SerializableFieldFinder()));
      var mutableTypeBatchCodeGenerator = new MutableTypeBatchCodeGenerator (new DependentTypeSorter(), mutableTypeCodeGeneratorFactory);
      var constructorFinder = new ConstructorFinder();
      var delegateFactory = new DelegateFactory();
      var lockingCodeGenerator = new LockingCodeGenerator (reflectionEmitCodeGenerator, constructorFinder, delegateFactory);
      var typeCache = new TypeCache (typeAssembler, lockingCodeGenerator, mutableTypeBatchCodeGenerator);
      var codeManager = new CodeManager (lockingCodeGenerator, typeCache);

      return new Pipeline (typeCache, codeManager);
    }
  }
}