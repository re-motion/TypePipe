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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Generates types for the classes in the specified <see cref="TypeContext"/> by creating a list of <see cref="IMutableTypeCodeGenerator"/>
  /// and calling their staged Define***- interleaved and in the correct order.
  /// This is necessary to allow the generation of types and method bodies which reference each other.
  /// </summary>
  public class TypeContextCodeGenerator : ITypeContextCodeGenerator
  {
    private readonly IReflectionEmitCodeGenerator _codeGenerator;
    private readonly IMemberEmitterFactory _memberEmitterFactory;
    private readonly IInitializationBuilder _initializationBuilder;
    private readonly IProxySerializationEnabler _proxySerializationEnabler;

    [CLSCompliant (false)]
    public TypeContextCodeGenerator (
        IReflectionEmitCodeGenerator codeGenerator,
        IMemberEmitterFactory memberEmitterFactory,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);
      ArgumentUtility.CheckNotNull ("memberEmitterFactory", memberEmitterFactory);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      _codeGenerator = codeGenerator;
      _memberEmitterFactory = memberEmitterFactory;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _codeGenerator; }
    }

    public Type GenerateProxy (TypeContext typeContext)
    {
      ArgumentUtility.CheckNotNull ("typeContext", typeContext);

      var me = _memberEmitterFactory.CreateMemberEmitter (_codeGenerator.EmittableOperandProvider);

      var x = new MutableTypeCodeGenerator (typeContext.ProxyType, _codeGenerator, me, _initializationBuilder, _proxySerializationEnabler);

      x.DefineType();
      x.DefineTypeFacet();
      return x.CreateType();
    }
  }
}