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
using System.Reflection;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Represents a multi-dimensional array <see cref="Type"/>.
  /// </summary>
  public class MultiDimensionalArrayType : ArrayTypeBase
  {
    public MultiDimensionalArrayType (CustomType elementType, int rank, IMemberSelector memberSelector)
        : base (elementType, rank, memberSelector)
    {
    }

    protected override IEnumerable<Type> CreateInterfaces (CustomType elementType)
    {
      return typeof (Array).GetInterfaces();
    }

    protected override IEnumerable<ConstructorInfo> CreateConstructors (int rank)
    {
      var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      var lengthParameters = Enumerable.Range (0, rank).Select (i => new ParameterDeclaration (typeof (int), "length" + i)).ToList();
      var lowerBoundParameters = Enumerable.Range (0, rank).Select (i => new ParameterDeclaration (typeof (int), "lowerBound" + i));

      yield return new ConstructorOnCustomType (this, attributes, lengthParameters);
      yield return new ConstructorOnCustomType (this, attributes, lowerBoundParameters.Interleave (lengthParameters));
    }
  }
}