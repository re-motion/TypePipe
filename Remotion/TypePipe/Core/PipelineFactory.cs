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
using Remotion.Reflection;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Provides static methods that create instances of <see cref="IPipeline"/>, which are the main entry point of the pipeline.
  /// </summary>
  public class PipelineFactory : IPipelineFactory
  {
    private static readonly IPipelineFactory s_instance = SafeServiceLocator.Current.GetInstance<IPipelineFactory>();

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

      return s_instance.CreatePipeline (participantConfigurationID, participantsCollection, configurationProvider);
    }

    public virtual IPipeline CreatePipeline (
        string participantConfigurationID, IEnumerable<IParticipant> participants, IConfigurationProvider configurationProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("configurationProvider", configurationProvider);

      var reflectionEmitCodeGenerator = NewReflectionEmitCodeGenerator (configurationProvider);
      var typeAssembler = NewTypeAssembler (participantConfigurationID, participants);
      var synchronizationPoint = NewSynchronizationPoint (reflectionEmitCodeGenerator, typeAssembler);
      var typeCache = NewTypeCache (typeAssembler, synchronizationPoint, reflectionEmitCodeGenerator);
      var codeManager = NewCodeManager (synchronizationPoint, typeCache);
      var reflectionService = NewReflectionService (synchronizationPoint, typeCache);

      return NewPipeline (typeCache, codeManager, reflectionService);
    }

    protected virtual IPipeline NewPipeline (ITypeCache typeCache, ICodeManager codeManager, IReflectionService reflectionService)
    {
      return new Pipeline (typeCache, codeManager, reflectionService);
    }

    protected virtual ICodeManager NewCodeManager (ICodeManagerSynchronizationPoint codeManagerSynchronizationPoint, ITypeCache typeCache)
    {
      return new CodeManager (codeManagerSynchronizationPoint, typeCache);
    }

    protected virtual IReflectionService NewReflectionService (
        IReflectionServiceSynchronizationPoint reflectionServiceSynchronizationPoint, ITypeCache typeCache)
    {
      return new ReflectionService (reflectionServiceSynchronizationPoint, typeCache);
    }

    [CLSCompliant (false)]
    protected virtual ITypeCache NewTypeCache (
        ITypeAssembler typeAssembler,
        ITypeCacheSynchronizationPoint typeCacheSynchronizationPoint,
        IReflectionEmitCodeGenerator reflectionEmitCodeGenerator)
    {
      var mutableTypeBatchCodeGenerator = NewMutableTypeBatchCodeGenerator (reflectionEmitCodeGenerator);

      return new TypeCache (typeAssembler, typeCacheSynchronizationPoint, mutableTypeBatchCodeGenerator);
    }

    [CLSCompliant (false)]
    protected virtual ISynchronizationPoint NewSynchronizationPoint (IGeneratedCodeFlusher generatedCodeFlusher, ITypeAssembler typeAssembler)
    {
      var constructorFinder = NewConstructorFinder();
      var delegateFactory = NewDelegateFactory();

      return new SynchronizationPoint (generatedCodeFlusher, typeAssembler, constructorFinder, delegateFactory);
    }

    protected virtual ITypeAssembler NewTypeAssembler (string participantConfigurationID, IEnumerable<IParticipant> participants)
    {
      var mutableTypeFactory = NewMutableTypeFactory();

      return new TypeAssembler (participantConfigurationID, participants, mutableTypeFactory);
    }

    [CLSCompliant (false)]
    protected virtual IMutableTypeCodeGeneratorFactory NewMutableTypeCodeGeneratorFactory (IReflectionEmitCodeGenerator reflectionEmitCodeGenerator)
    {
      var memberEmitterFactory = NewMemberEmitterFactory();
      var initializationBuilder = NewInitializationBuilder();
      var proxySerializationEnabler = NewProxySerializationEnabler();

      return new MutableTypeCodeGeneratorFactory (memberEmitterFactory, reflectionEmitCodeGenerator, initializationBuilder, proxySerializationEnabler);
    }

    [CLSCompliant (false)]
    protected virtual IMutableTypeBatchCodeGenerator NewMutableTypeBatchCodeGenerator (IReflectionEmitCodeGenerator reflectionEmitCodeGenerator)
    {
      var dependentTypeSorter = NewDependentTypeSorter();
      var mutableTypeCodeGeneratorFactory = NewMutableTypeCodeGeneratorFactory (reflectionEmitCodeGenerator);

      return new MutableTypeBatchCodeGenerator (dependentTypeSorter, mutableTypeCodeGeneratorFactory);
    }

    [CLSCompliant (false)]
    protected virtual IReflectionEmitCodeGenerator NewReflectionEmitCodeGenerator (IConfigurationProvider configurationProvider)
    {
      var moduleBuilderFactory = NewModuleBuilderFactory();

      return new ReflectionEmitCodeGenerator (moduleBuilderFactory, configurationProvider);
    }

    protected virtual IDelegateFactory NewDelegateFactory ()
    {
      return new DelegateFactory();
    }

    protected virtual IConstructorFinder NewConstructorFinder ()
    {
      return new ConstructorFinder();
    }

    protected virtual IDependentTypeSorter NewDependentTypeSorter ()
    {
      return new DependentTypeSorter();
    }

    protected virtual IProxySerializationEnabler NewProxySerializationEnabler ()
    {
      var serializableFieldFinder = NewSerializableFieldFinder();

      return new ProxySerializationEnabler (serializableFieldFinder);
    }

    protected virtual ISerializableFieldFinder NewSerializableFieldFinder ()
    {
      return new SerializableFieldFinder();
    }

    protected virtual IInitializationBuilder NewInitializationBuilder ()
    {
      return new InitializationBuilder();
    }

    [CLSCompliant (false)]
    protected virtual IModuleBuilderFactory NewModuleBuilderFactory ()
    {
      return new ModuleBuilderFactory();
    }

    protected virtual IMemberEmitterFactory NewMemberEmitterFactory ()
    {
      return new MemberEmitterFactory();
    }

    protected virtual IMutableTypeFactory NewMutableTypeFactory ()
    {
      return new MutableTypeFactory();
    }
  }
}