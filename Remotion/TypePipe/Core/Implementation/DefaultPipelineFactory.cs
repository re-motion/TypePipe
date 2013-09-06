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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation.Synchronization;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Default implementation of <see cref="IPipelineFactory"/>. Derive from this class and override the appropriate methods to configure how
  /// <see cref="IPipeline"/> instances are built.
  /// </summary>
  public class DefaultPipelineFactory : IPipelineFactory
  {
    public virtual IPipeline CreatePipeline (string participantConfigurationID, PipelineSettings settings, IEnumerable<IParticipant> participants)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("settings", settings);
      ArgumentUtility.CheckNotNull ("participants", participants);

      var reflectionEmitCodeGenerator = NewReflectionEmitCodeGenerator (settings.ForceStrongNaming, settings.KeyFilePath);
      var mutableTypeBatchCodeGenerator = NewMutableTypeBatchCodeGenerator (reflectionEmitCodeGenerator);
      var assemblyContext = new AssemblyContext (mutableTypeBatchCodeGenerator, reflectionEmitCodeGenerator);
      var typeAssembler = NewTypeAssembler (participantConfigurationID, participants, settings.EnableSerializationWithoutAssemblySaving);
      var constructorDelegateFactory = NewConstructorDelegateFactory();
      var synchronizationPoint = NewSynchronizationPoint (typeAssembler, assemblyContext);
      var typeCache = NewTypeCache (typeAssembler, constructorDelegateFactory, synchronizationPoint);
      var reverseTypeCache = NewReverseTypeCache (typeAssembler, constructorDelegateFactory);
      var codeManager = NewCodeManager (synchronizationPoint, typeCache);
      var reflectionService = NewReflectionService (synchronizationPoint, typeCache, reverseTypeCache);

      return NewPipeline (settings, typeCache, codeManager, reflectionService);
    }

    protected virtual IPipeline NewPipeline (
        PipelineSettings settings, ITypeCache typeCache, ICodeManager codeManager, IReflectionService reflectionService)
    {
      return new Pipeline (settings, typeCache, codeManager, reflectionService);
    }

    protected virtual ICodeManager NewCodeManager (ICodeManagerSynchronizationPoint codeManagerSynchronizationPoint, ITypeCache typeCache)
    {
      return new CodeManager (codeManagerSynchronizationPoint, typeCache);
    }

    protected virtual IReflectionService NewReflectionService (
        IReflectionServiceSynchronizationPoint reflectionServiceSynchronizationPoint, ITypeCache typeCache, IReverseTypeCache reverseTypeCache)
    {
      return new ReflectionService (reflectionServiceSynchronizationPoint, typeCache, reverseTypeCache);
    }

    [CLSCompliant (false)]
    protected virtual ITypeCache NewTypeCache (
        ITypeAssembler typeAssembler,
        IConstructorDelegateFactory constructorDelegateFactory,
        ITypeCacheSynchronizationPoint typeCacheSynchronizationPoint)
    {
      return new TypeCache (typeAssembler, constructorDelegateFactory, typeCacheSynchronizationPoint);
    }

    protected virtual IReverseTypeCache NewReverseTypeCache (ITypeAssembler typeAssembler, IConstructorDelegateFactory constructorDelegateFactory)
    {
      return new ReverseTypeCache (typeAssembler, constructorDelegateFactory);
    }

    protected virtual ISynchronizationPoint NewSynchronizationPoint (ITypeAssembler typeAssembler, AssemblyContext assemblyContext)
    {
      return new SynchronizationPoint (typeAssembler, assemblyContext);
    }

    protected virtual ITypeAssembler NewTypeAssembler (
        string participantConfigurationID, IEnumerable<IParticipant> participants, bool enableSerializationWithoutAssemblySaving)
    {
      var mutableTypeFactory = NewMutableTypeFactory();
      var complexSerializationEnabler = ComplexSerializationEnabler (enableSerializationWithoutAssemblySaving);

      return new TypeAssembler (participantConfigurationID, participants, mutableTypeFactory, complexSerializationEnabler);
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
    protected virtual IReflectionEmitCodeGenerator NewReflectionEmitCodeGenerator (bool forceStrongNaming, string keyFilePath)
    {
      var moduleBuilderFactory = NewModuleBuilderFactory();

      return new ReflectionEmitCodeGenerator (moduleBuilderFactory, forceStrongNaming, keyFilePath);
    }

    protected virtual IConstructorDelegateFactory NewConstructorDelegateFactory ()
    {
      var constructorFinder = NewConstructorFinder();
      var delegateFactory = NewDelegateFactory();

      return new ConstructorDelegateFactory (constructorFinder, delegateFactory);
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

    protected virtual IComplexSerializationEnabler ComplexSerializationEnabler (bool enableComplexSerialization)
    {
      return enableComplexSerialization ? (IComplexSerializationEnabler) new ComplexSerializationEnabler() : new NullComplexSerializationEnabler();
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