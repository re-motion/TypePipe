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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Remotion.Collections;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Generates <see cref="Type"/>s for the specified <see cref="MutableType"/> instances by creating a set of <see cref="IMutableTypeCodeGenerator"/>
  /// and calling their staged Define***- interleaved and in the correct order.
  /// This is necessary to allow the generation of types and method bodies which reference each other.
  /// </summary>
  public class MutableTypeBatchCodeGenerator : IMutableTypeBatchCodeGenerator
  {
    private readonly IDependentTypeSorter _dependentTypeSorter;
    private readonly IMutableTypeCodeGeneratorFactory _mutableTypeCodeGeneratorFactory;

    [CLSCompliant (false)]
    public MutableTypeBatchCodeGenerator (IDependentTypeSorter dependentTypeSorter, IMutableTypeCodeGeneratorFactory mutableTypeCodeGeneratorFactory)
    {
      ArgumentUtility.CheckNotNull ("dependentTypeSorter", dependentTypeSorter);
      ArgumentUtility.CheckNotNull ("mutableTypeCodeGeneratorFactory", mutableTypeCodeGeneratorFactory);

      _dependentTypeSorter = dependentTypeSorter;
      _mutableTypeCodeGeneratorFactory = mutableTypeCodeGeneratorFactory;
    }

    public IEnumerable<KeyValuePair<MutableType, Type>> GenerateTypes (IEnumerable<MutableType> mutableTypes)
    {
      ArgumentUtility.CheckNotNull ("mutableTypes", mutableTypes);

      var sortedTypes = _dependentTypeSorter.Sort (mutableTypes);
      var generators = _mutableTypeCodeGeneratorFactory.CreateGenerators (sortedTypes).ToList();

      // For all types, declare the types first, then declare the members/base type/interfaces/etc., then finish the type (including bodies, etc.).
      // That way, it is ensured that types can reference each other.

      foreach (var g in generators)
        g.DeclareType();
      foreach (var g in generators)
        g.DefineTypeFacets();

      return generators.Select (g => new KeyValuePair<MutableType, Type> (g.MutableType, g.CreateType()));
    }
  }
}