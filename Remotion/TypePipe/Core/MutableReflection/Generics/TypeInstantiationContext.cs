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
using Remotion.Collections;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a context that can be used to instantiate generic method definitions and for substitution of their generic type parameters 
  /// in other types.
  /// </summary>
  public class TypeInstantiationContext
  {
    private readonly Dictionary<TypeInstantiationInfo, TypeInstantiation> _instantiations = new Dictionary<TypeInstantiationInfo, TypeInstantiation>();

    public Type Instantiate (TypeInstantiationInfo instantiationInfo)
    {
      var typeInstantiation = _instantiations.GetValueOrDefault (instantiationInfo);
      if (typeInstantiation != null)
        return typeInstantiation;

      var genTypeDef = instantiationInfo.GenericTypeDefinition;
      var typeArgs = instantiationInfo.TypeArguments;

      if (genTypeDef.IsRuntimeType() && typeArgs.All (a => a.IsRuntimeType()))
        return genTypeDef.MakeGenericType (typeArgs.ToArray());

      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      return new TypeInstantiation (memberSelector, instantiationInfo, this);
    }

    public void Add (TypeInstantiationInfo instantiationInfo, TypeInstantiation typeInstantiation)
    {
      ArgumentUtility.CheckNotNull ("instantiationInfo", instantiationInfo);
      ArgumentUtility.CheckNotNull ("typeInstantiation", typeInstantiation);

      _instantiations.Add (instantiationInfo, typeInstantiation);
    }
  }
}