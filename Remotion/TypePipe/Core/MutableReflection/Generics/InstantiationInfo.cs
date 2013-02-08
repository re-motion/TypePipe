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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// A class that holds the information needed to construct a generic type.
  /// </summary>
  /// <remarks>This is used by <see cref="TypeInstantiation"/> as the key in a context dictionary to break cyclic dependencies.</remarks>
  public class InstantiationInfo
  {
    private readonly Type _genericTypeDefinition;
    private readonly Type[] _typeArguments;
    private readonly object[] _key;

    public InstantiationInfo (Type genericTypeDefinition, Type[] typeArguments)
    {
      ArgumentUtility.CheckNotNull ("genericTypeDefinition", genericTypeDefinition);
      ArgumentUtility.CheckNotNullOrEmptyOrItemsNull ("typeArguments", typeArguments);

      _genericTypeDefinition = genericTypeDefinition;
      _typeArguments = typeArguments;
      _key = new object[] { genericTypeDefinition }.Concat (typeArguments).ToArray();
    }

    public Type GenericTypeDefinition
    {
      get { return _genericTypeDefinition; }
    }

    public Type[] TypeArguments
    {
      get { return _typeArguments; }
    }

    public override bool Equals (object obj)
    {
      var other = obj as InstantiationInfo;
      if (other == null)
        return false;

      return _key.SequenceEqual (other._key);
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (_key);
    }
  }
}