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
using Remotion.TypePipe.TypeAssembly.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Implements <see cref="IPipeline"/> to act as a main entry point into the pipeline for generating types and instantiating them.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class Pipeline : IPipeline
  {
    private readonly PipelineSettings _settings;
    private readonly ICodeManager _codeManager;
    private readonly IReflectionService _reflectionService;
    private readonly ITypeAssembler _typeAssembler;

    public Pipeline (
        PipelineSettings settings,
        ICodeManager codeManager,
        IReflectionService reflectionService,
        ITypeAssembler typeAssembler)
    {
      ArgumentUtility.CheckNotNull ("settings", settings);
      ArgumentUtility.CheckNotNull ("codeManager", codeManager);
      ArgumentUtility.CheckNotNull ("reflectionService", reflectionService);
      ArgumentUtility.CheckNotNull ("typeAssembler", typeAssembler);

      _settings = settings;
      _codeManager = codeManager;
      _reflectionService = reflectionService;
      _typeAssembler = typeAssembler;
    }

    public string ParticipantConfigurationID
    {
      get { return _typeAssembler.ParticipantConfigurationID; }
    }

    public PipelineSettings Settings
    {
      get { return _settings; }
    }

    public IReadOnlyCollection<IParticipant> Participants
    {
      get { return _typeAssembler.Participants; }
    }

    public ICodeManager CodeManager
    {
      get { return _codeManager; }
    }

    public IReflectionService ReflectionService
    {
      get { return _reflectionService; }
    }

    public T Create<T> (ParamList constructorArguments = null, bool allowNonPublicConstructor = false)
        where T : class
    {
      return (T) Create (typeof (T), constructorArguments, allowNonPublicConstructor);
    }

    public object Create (Type requestedType, ParamList constructorArguments = null, bool allowNonPublicConstructor = false)
    {
      // Using Assertion.DebugAssert because it will be compiled away. Argument-check is performed by ReflectionService.
      Assertion.DebugAssert (requestedType != null, "requestedType cannot be null.");
      constructorArguments = constructorArguments ?? ParamList.Empty;

      var typeID = _reflectionService.GetTypeIDForRequestedType (requestedType);
      return _reflectionService.InstantiateAssembledType (typeID, constructorArguments, allowNonPublicConstructor);
    }
  }
}