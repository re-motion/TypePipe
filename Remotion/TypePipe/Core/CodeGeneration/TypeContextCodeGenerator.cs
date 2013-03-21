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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;
using Remotion.Collections;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Generates types for the classes in the specified <see cref="TypeContext"/> by creating a list of <see cref="IMutableTypeCodeGenerator"/>
  /// and calling their staged Define***- interleaved and in the correct order.
  /// This is necessary to allow the generation of types and method bodies which reference each other.
  /// </summary>
  public class TypeContextCodeGenerator : ITypeContextCodeGenerator
  {
    private readonly IDependentTypeSorter _dependentTypeSorter;
    private readonly IMutableTypeCodeGeneratorFactory _mutableTypeCodeGeneratorFactory;

    [CLSCompliant (false)]
    public TypeContextCodeGenerator (IDependentTypeSorter dependentTypeSorter, IMutableTypeCodeGeneratorFactory mutableTypeCodeGeneratorFactory)
    {
      ArgumentUtility.CheckNotNull ("dependentTypeSorter", dependentTypeSorter);
      ArgumentUtility.CheckNotNull ("mutableTypeCodeGeneratorFactory", mutableTypeCodeGeneratorFactory);

      _dependentTypeSorter = dependentTypeSorter;
      _mutableTypeCodeGeneratorFactory = mutableTypeCodeGeneratorFactory;
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _mutableTypeCodeGeneratorFactory.CodeGenerator; }
    }

    public GeneratedTypeContext GenerateTypes (ITypeContext typeContext)
    {
      ArgumentUtility.CheckNotNull ("typeContext", typeContext);

      var mutableTypes = typeContext.AdditionalTypes.Concat (typeContext.ProxyType);
      var sortedTypes = _dependentTypeSorter.Sort (mutableTypes);
      var typesAndGenerators = sortedTypes.Select (t => new { MutableType = t, Generator = _mutableTypeCodeGeneratorFactory.Create (t) }).ToList();

      foreach (var pair in typesAndGenerators)
        pair.Generator.DeclareType();
      foreach (var pair in typesAndGenerators)
        pair.Generator.DefineTypeFacet();

      var mutableToGeneratedTypes = typesAndGenerators.Select (pair => Tuple.Create (pair.MutableType, pair.Generator.CreateType()));

      return new GeneratedTypeContext (mutableToGeneratedTypes);
    }
  }
}