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
using System.Collections.ObjectModel;
using System.Linq;
using Remotion.Collections;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// A class that holds the information needed to construct a generic type instantiation.
  /// </summary>
  /// <remarks>This is used by <see cref="TypeInstantiation"/> as the key in a context dictionary to break cyclic dependencies.</remarks>
  public class TypeInstantiationInfo
  {
    private readonly Type _genericTypeDefinition;
    private readonly ReadOnlyCollection<Type> _typeArguments;

    public TypeInstantiationInfo (Type genericTypeDefinition, IEnumerable<Type> typeArguments)
    {
      ArgumentUtility.CheckNotNull ("genericTypeDefinition", genericTypeDefinition);
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);

      if (!genericTypeDefinition.IsGenericTypeDefinition)
        throw new ArgumentException ("Specified type must be a generic type definition.", "genericTypeDefinition");

      _genericTypeDefinition = genericTypeDefinition;
      _typeArguments = typeArguments.ToList().AsReadOnly();

      if (genericTypeDefinition.GetGenericArguments().Length != _typeArguments.Count)
        throw new ArgumentException (
            "Generic parameter count of the generic type definition does not match the number of supplied type arguments.", "typeArguments");
    }

    public Type GenericTypeDefinition
    {
      get { return _genericTypeDefinition; }
    }

    public ReadOnlyCollection<Type> TypeArguments
    {
      get { return _typeArguments; }
    }

    public Type Instantiate (Dictionary<TypeInstantiationInfo, TypeInstantiation> instantiations)
    {
      var typeInstantiation = instantiations.GetValueOrDefault (this);
      if (typeInstantiation != null)
        return typeInstantiation;

      // Make RuntimeType if all type arguments are RuntimeTypes.
      if (_typeArguments.All (typeArg => typeArg.IsRuntimeType()))
        return _genericTypeDefinition.MakeGenericType (_typeArguments.ToArray());

      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      return new TypeInstantiation (memberSelector, this, instantiations);
    }

    public override bool Equals (object obj)
    {
      var other = obj as TypeInstantiationInfo;
      if (other == null)
        return false;

// ReSharper disable CheckForReferenceEqualityInstead.2
      return Equals (_genericTypeDefinition, other.GenericTypeDefinition) && _typeArguments.SequenceEqual (other._typeArguments);
// ReSharper restore CheckForReferenceEqualityInstead.2
    }

    public override int GetHashCode ()
    {
      return _genericTypeDefinition.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (_typeArguments);
    }
  }
}