﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Reflection;
using JetBrains.Annotations;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Dlr.Runtime.CompilerServices;
using Remotion.TypePipe.MutableReflection;
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
  /// <threadsafety static="true" instance="true"/>
  public class DefaultPipelineFactory : IPipelineFactory
  {
    /// <summary>
    /// Combines the interfaces <see cref="IReflectionEmitCodeGenerator"/> and <see cref="IGeneratedCodeFlusher"/> 
    /// to be used as return type of the factory method.
    /// </summary>
    [CLSCompliant (false)]
    protected interface IReflectionEmitCodeGeneratorAndGeneratedCodeFlusher : IReflectionEmitCodeGenerator, IGeneratedCodeFlusher
    {
    }

    /// <summary>
    /// Implements the interface <see cref="IReflectionEmitCodeGeneratorAndGeneratedCodeFlusher"/> 
    /// as a decorator for <see cref="ReflectionEmitCodeGenerator"/>.
    /// </summary>
    protected class ReflectionEmitCodeGeneratorDecoratorWithGeneratedCodeFlusherSemantics : IReflectionEmitCodeGeneratorAndGeneratedCodeFlusher
    {
      private readonly ReflectionEmitCodeGenerator _reflectionEmitCodeGenerator;

      public ReflectionEmitCodeGeneratorDecoratorWithGeneratedCodeFlusherSemantics (ReflectionEmitCodeGenerator reflectionEmitCodeGenerator)
      {
        ArgumentUtility.CheckNotNull ("reflectionEmitCodeGenerator", reflectionEmitCodeGenerator);
        
        _reflectionEmitCodeGenerator = reflectionEmitCodeGenerator;
      }

      DebugInfoGenerator IReflectionEmitCodeGenerator.DebugInfoGenerator
      {
        get { return _reflectionEmitCodeGenerator.DebugInfoGenerator; }
      }

      IEmittableOperandProvider IReflectionEmitCodeGenerator.CreateEmittableOperandProvider ()
      {
        return _reflectionEmitCodeGenerator.CreateEmittableOperandProvider();
      }

      ITypeBuilder IReflectionEmitCodeGenerator.DefineType (string name, TypeAttributes attributes, IEmittableOperandProvider emittableOperandProvider)
      {
        return _reflectionEmitCodeGenerator.DefineType (name, attributes, emittableOperandProvider);
      }

      string IGeneratedCodeFlusher.FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes)
      {
        return _reflectionEmitCodeGenerator.FlushCodeToDisk (assemblyAttributes);
      }
    }

    public virtual IPipeline Create (string participantConfigurationID, PipelineSettings settings, IEnumerable<IParticipant> participants)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("participantConfigurationID", participantConfigurationID);
      ArgumentUtility.CheckNotNull ("settings", settings);
      ArgumentUtility.CheckNotNull ("participants", participants);

      var typeAssembler = NewTypeAssembler (participantConfigurationID, participants, settings.EnableSerializationWithoutAssemblySaving);
      var assemblyContextPool = NewAssemblyContextPool (participantConfigurationID, settings);
      var typeCache = NewTypeCache (typeAssembler, assemblyContextPool);
      var codeManager = NewCodeManager (typeCache, typeAssembler, assemblyContextPool);
      var reflectionService = NewReflectionService (typeCache, typeAssembler);

      return NewPipeline (settings, codeManager, reflectionService, typeAssembler);
    }

    protected virtual IPipeline NewPipeline (
        PipelineSettings settings,
        ICodeManager codeManager,
        IReflectionService reflectionService,
        ITypeAssembler typeAssembler)
    {
      return new Pipeline (settings, codeManager, reflectionService, typeAssembler);
    }

    protected virtual ICodeManager NewCodeManager (ITypeCache typeCache, ITypeAssembler typeAssembler, IAssemblyContextPool assemblyContextPool)
    {
      return new CodeManager (typeCache, typeAssembler, assemblyContextPool);
    }

    protected virtual IReflectionService NewReflectionService (ITypeCache typeCache, ITypeAssembler typeAssembler)
    {
      var constructorDelegateFactory = NewConstructorDelegateFactory();
      var constructorCallCache = NewConstructorCallCache (typeCache, constructorDelegateFactory);
      var reverseTypeCache = NewConstructorForAssembledTypeCache (typeAssembler, constructorDelegateFactory);

      return new ReflectionService (typeAssembler, typeCache, constructorCallCache, reverseTypeCache);
    }

    [CLSCompliant (false)]
    protected virtual ITypeCache NewTypeCache (ITypeAssembler typeAssembler, IAssemblyContextPool assemblyContextPool)
    {
      return new TypeCache (typeAssembler, assemblyContextPool);
    }

    protected virtual IConstructorForAssembledTypeCache NewConstructorForAssembledTypeCache (
        ITypeAssembler typeAssembler,
        IConstructorDelegateFactory constructorDelegateFactory)
    {
      return new ConstructorForAssembledTypeCache (typeAssembler, constructorDelegateFactory);
    }

    protected virtual IAssemblyContextPool NewAssemblyContextPool (string participantConfigurationID, PipelineSettings settings)
    {
      var assemblyContexts = new List<AssemblyContext>();
      for (int i = 0; i < settings.DegreeOfParallelism; i++)
        assemblyContexts.Add (NewAssemblyContext (participantConfigurationID, settings));

      return new ThreadLocalAssemblyContextPoolDecorator (new AssemblyContextPool (assemblyContexts));
    }

    protected AssemblyContext NewAssemblyContext (string participantConfigurationID, PipelineSettings settings)
    {
      var reflectionEmitCodeGenerator = NewReflectionEmitCodeGenerator (
          participantConfigurationID,
          settings.ForceStrongNaming,
          settings.KeyFilePath,
          settings.AssemblyDirectory,
          settings.AssemblyNamePattern);
      
      var mutableTypeBatchCodeGenerator = NewMutableTypeBatchCodeGenerator (reflectionEmitCodeGenerator);

      return new AssemblyContext (mutableTypeBatchCodeGenerator, reflectionEmitCodeGenerator);
    }

    protected virtual ITypeAssembler NewTypeAssembler (
        string participantConfigurationID,
        IEnumerable<IParticipant> participants,
        bool enableSerializationWithoutAssemblySaving)
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
    protected virtual IReflectionEmitCodeGeneratorAndGeneratedCodeFlusher NewReflectionEmitCodeGenerator (
        string participantConfigurationID,
        bool forceStrongNaming,
        [CanBeNull] string keyFilePath,
        [CanBeNull] string assemblyDirectory,
        [NotNull] string assemblyNamePattern)
    {
      var moduleBuilderFactory = NewModuleBuilderFactory (participantConfigurationID);

      return new ReflectionEmitCodeGeneratorDecoratorWithGeneratedCodeFlusherSemantics (
          new ReflectionEmitCodeGenerator (moduleBuilderFactory, forceStrongNaming, keyFilePath, assemblyDirectory, assemblyNamePattern));
    }

    protected virtual ConstructorCallCache NewConstructorCallCache (ITypeCache typeCache, IConstructorDelegateFactory constructorDelegateFactory)
    {
      return new ConstructorCallCache (typeCache, constructorDelegateFactory);
    }

    protected virtual IConstructorDelegateFactory NewConstructorDelegateFactory ()
    {
      var constructorFinder = NewConstructorFinder();

      return new ConstructorDelegateFactory (constructorFinder);
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
    protected virtual IModuleBuilderFactory NewModuleBuilderFactory (string participantConfigurationID)
    {
      return new ModuleBuilderFactory (participantConfigurationID);
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