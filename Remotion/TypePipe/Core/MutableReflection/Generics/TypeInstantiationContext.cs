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
  /// Represents a context that allows instantiation generic type definitions.
  /// In addition <see cref="SubstituteGenericParameters"/> can be used for substitution of generic type parameters in other types.
  /// </summary>
  public class TypeInstantiationContext
  {
    private readonly Dictionary<TypeInstantiationInfo, TypeInstantiation> _instantiations = new Dictionary<TypeInstantiationInfo, TypeInstantiation>();

    public Type Instantiate (TypeInstantiationInfo instantiationInfo)
    {
      ArgumentUtility.CheckNotNull ("instantiationInfo", instantiationInfo);

      TypeInstantiation typeInstantiation;
      if (_instantiations.TryGetValue (instantiationInfo, out typeInstantiation))
        return typeInstantiation;

      var genTypeDef = instantiationInfo.GenericTypeDefinition;
      var typeArgs = instantiationInfo.TypeArguments;

      if (genTypeDef.IsRuntimeType() && typeArgs.All (a => a.IsRuntimeType()))
        return genTypeDef.MakeGenericType (typeArgs.ToArray());

      return new TypeInstantiation (instantiationInfo, this);
    }

    public void Add (TypeInstantiationInfo instantiationInfo, TypeInstantiation typeInstantiation)
    {
      ArgumentUtility.CheckNotNull ("instantiationInfo", instantiationInfo);
      ArgumentUtility.CheckNotNull ("typeInstantiation", typeInstantiation);

      _instantiations.Add (instantiationInfo, typeInstantiation);
    }

    public Type SubstituteGenericParameters (Type type, IDictionary<Type, Type> parametersToArguments)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("parametersToArguments", parametersToArguments);

      Type typeArgument;
      if (parametersToArguments.TryGetValue (type, out typeArgument))
        return Assertion.IsNotNull (typeArgument, "Type-argument for type-parameter '{0}' was null.", type);

      if (type.IsArray)
        return SubstituteArrayElementType (type, parametersToArguments);
      if (type.IsByRef)
        return SubstituteByRefElementType (type, parametersToArguments);
      if (!type.IsGenericType)
        return type;

      var oldTypeArguments = type.GetGenericArguments();
      var newTypeArguments = oldTypeArguments.Select (t => SubstituteGenericParameters (t, parametersToArguments)).ToList();

      // No substitution necessary (this is an optimization only).
      if (oldTypeArguments.SequenceEqual (newTypeArguments))
        return type;

      var genericTypeDefinition = type.GetGenericTypeDefinition();
      var instantiationInfo = new TypeInstantiationInfo (genericTypeDefinition, newTypeArguments);

      return Instantiate (instantiationInfo);
    }

    private Type SubstituteArrayElementType (Type arrayType, IDictionary<Type, Type> parametersToArguments)
    {
      var elementType = arrayType.GetElementType();
      Assertion.IsNotNull(elementType);

      // ReSharper disable CheckForReferenceEqualityInstead.1 - Equals because TypePipe does not unify types.
      var isVector = arrayType.Equals(elementType.MakeArrayType());
      // ReSharper restore CheckForReferenceEqualityInstead.1

      var substitutedElementType = SubstituteGenericParameters(elementType, parametersToArguments);
      return isVector ? substitutedElementType.MakeArrayType() : substitutedElementType.MakeArrayType (arrayType.GetArrayRank());
    }

    private Type SubstituteByRefElementType (Type byRefType, IDictionary<Type, Type> parametersToArguments)
    {
      return SubstituteGenericParameters (byRefType.GetElementType(), parametersToArguments).MakeByRefType();
    }
  }
}