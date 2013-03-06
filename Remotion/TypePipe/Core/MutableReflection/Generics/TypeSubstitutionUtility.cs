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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// A utility that contains the common code for substitution of generic parameters.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="TypeInstantiation.SubstituteGenericParameters"/> and <see cref="MethodInstantiation.SubstituteGenericParameters"/>.
  /// </remarks>
  public static class TypeSubstitutionUtility
  {
    // TODO Review: Move to TypeInstantiationContext
    public static Type SubstituteGenericParameters (
        IDictionary<Type, Type> parametersToArguments, TypeInstantiationContext instantiationContext, Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("parametersToArguments", parametersToArguments);
      ArgumentUtility.CheckNotNull ("instantiationContext", instantiationContext);

      var typeArgument = parametersToArguments.GetValueOrDefault (type);
      if (typeArgument != null)
        return typeArgument;

      if (!type.IsGenericType)
        return type;

      Assertion.IsFalse (type.IsArray, "Not yet supported, TODO 5409");

      var oldTypeArguments = type.GetGenericArguments();
      var newTypeArguments = oldTypeArguments.Select (t => SubstituteGenericParameters (parametersToArguments, instantiationContext, t)).ToList();

      // No substitution necessary (this is an optimization only).
      if (oldTypeArguments.SequenceEqual (newTypeArguments))
        return type;

      var genericTypeDefinition = type.GetGenericTypeDefinition();
      var instantiationInfo = new TypeInstantiationInfo (genericTypeDefinition, newTypeArguments);

      return instantiationContext.Instantiate (instantiationInfo);
    }
  }
}