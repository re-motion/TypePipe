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
using Remotion.FunctionalProgramming;
using Remotion.Text;
using Remotion.TypePipe.CodeGeneration.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Sorts <see cref="MutableType"/> instances topologically according to their dependencies (i.e., <see cref="Type.BaseType"/> 
  /// and <see cref="Type.GetInterfaces"/>). Throws an exception when cycles exist within the dependency graph.
  /// </summary>
  public class DependentTypeSorter : IDependentTypeSorter
  {
    public IEnumerable<MutableType> Sort (IEnumerable<MutableType> types)
    {
      ArgumentUtility.CheckNotNull ("types", types);

      var remainingTypes = new HashSet<MutableType> (types);

      while (remainingTypes.Count > 0)
      {
        var independenType = remainingTypes.First (t => IsIndependent (t, remainingTypes), () => CreateDependencyCycleException (remainingTypes));
        remainingTypes.Remove (independenType);

        yield return independenType;
      }
    }

    private bool IsIndependent (MutableType type, HashSet<MutableType> types)
    {
      return !ContainsCycle (types, type.BaseType) && !type.GetInterfaces().Any (t => ContainsCycle (types, t));
    }

    private bool ContainsCycle (HashSet<MutableType> types, Type type)
    {
      // This short-circuit is based on the fact that RuntimeTypes can never contain CustomTypes as their type arguments.
      if (type == null || type.IsRuntimeType())
        return false;

      var mutableType = type as MutableType;
      if (mutableType != null)
        return types.Contains (mutableType);
      else
        return type.GetGenericArguments().Any (a => ContainsCycle (types, a));
    }

    private InvalidOperationException CreateDependencyCycleException (HashSet<MutableType> remainingTypes)
    {
      var remainingTypeNames = SeparatedStringBuilder.Build (", ", remainingTypes, t => "'" + t.Name + "'");
      var message =
          "MutableTypes must not contain cycles in their dependencies, i.e., an algorithm that recursively follows the types returned by "
          + "Type.BaseType and Type.GetInterfaces must terminate." + Environment.NewLine
          + "At least one of the following types is causing the dependency cycle: " + remainingTypeNames + ".";

      return new InvalidOperationException (message);
    }
  }
}