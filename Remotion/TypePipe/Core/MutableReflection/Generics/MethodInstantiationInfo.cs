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
using System.Reflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// A class that holds the information needed to construct a generic method instantiation.
  /// </summary>
  public class MethodInstantiationInfo
  {
    private readonly MethodInfo _genericMethodDefinition;
    private readonly ReadOnlyCollection<Type> _typeArguments;

    public MethodInstantiationInfo (MethodInfo genericMethodDefinition, IEnumerable<Type> typeArguments)
    {
      ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition);
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);

      if (!genericMethodDefinition.IsGenericMethodDefinition)
        throw new ArgumentException ("Specified method must be a generic method definition.", "genericMethodDefinition");

      _genericMethodDefinition = genericMethodDefinition;
      _typeArguments = typeArguments.ToList().AsReadOnly();

      if (genericMethodDefinition.GetGenericArguments().Length != _typeArguments.Count)
        throw new ArgumentException (
            "Generic parameter count of the generic method definition does not match the number of supplied type arguments.", "typeArguments");
    }

    public MethodInfo GenericMethodDefinition
    {
      get { return _genericMethodDefinition; }
    }

    public ReadOnlyCollection<Type> TypeArguments
    {
      get { return _typeArguments; }
    }

    public MethodInfo Instantiate ()
    {
      // Make RuntimeMethod if all type arguments are RuntimeTypes.
      if (_typeArguments.All (typeArg => typeArg.IsRuntimeType()))
        return _genericMethodDefinition.MakeGenericMethod (_typeArguments.ToArray());

      return new MethodInstantiation (_genericMethodDefinition, _typeArguments);
    }
  }
}