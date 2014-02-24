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
using Remotion.TypePipe.Dlr.Dynamic.Utils;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Serves as a factory for instances of <see cref="IMutableTypeCodeGenerator"/>.
  /// </summary>
  /// <threadsafety static="true" instance="false"/>
  public class MutableTypeCodeGeneratorFactory : IMutableTypeCodeGeneratorFactory
  {
    private readonly IMemberEmitterFactory _memberEmitterFactory;
    private readonly IReflectionEmitCodeGenerator _codeGenerator;
    private readonly IInitializationBuilder _initializationBuilder;
    private readonly IProxySerializationEnabler _proxySerializationEnabler;

    [CLSCompliant (false)]
    public MutableTypeCodeGeneratorFactory (
        IMemberEmitterFactory memberEmitterFactory,
        IReflectionEmitCodeGenerator codeGenerator,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("memberEmitterFactory", memberEmitterFactory);
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      _memberEmitterFactory = memberEmitterFactory;
      _codeGenerator = codeGenerator;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;
    }

    [CLSCompliant (false)]
    public IEnumerable<IMutableTypeCodeGenerator> CreateGenerators (IEnumerable<MutableType> mutableTypes)
    {
      ArgumentUtility.CheckNotNull ("mutableTypes", mutableTypes);

      var emittableOperandProvider = _codeGenerator.CreateEmittableOperandProvider();
      var memberEmitter = _memberEmitterFactory.CreateMemberEmitter (emittableOperandProvider);

      return mutableTypes.Select (mt => CreateGenerator (mt, emittableOperandProvider, memberEmitter));
    }

    private IMutableTypeCodeGenerator CreateGenerator (
        MutableType mutableType, IEmittableOperandProvider emittableOperandProvider, IMemberEmitter memberEmitter)
    {
      var nestedTypeCodeGeneratorFactory = new MutableNestedTypeCodeGeneratorFactory (
          _codeGenerator, emittableOperandProvider, memberEmitter, _initializationBuilder, _proxySerializationEnabler);

      return new MutableTypeCodeGenerator (
          mutableType,
          nestedTypeCodeGeneratorFactory,
          _codeGenerator,
          emittableOperandProvider,
          memberEmitter,
          _initializationBuilder,
          _proxySerializationEnabler);
    }
  }
}