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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Represents a <see cref="ITypeModificationTracker"/> for a proxy type that has copied constructors.
  /// </summary>
  public class ProxyTypeModificationTracker : ITypeModificationTracker
  {
    private readonly MutableType _proxyType;
    private readonly IReadOnlyCollection<Expression> _constructorBodies;

    public ProxyTypeModificationTracker (MutableType proxyType, IEnumerable<Expression> constructorBodies)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNull ("constructorBodies", constructorBodies);

      _proxyType = proxyType;
      _constructorBodies = constructorBodies.ToList().AsReadOnly();
    }

    public MutableType Type
    {
      get { return _proxyType; }
    }

    public IReadOnlyCollection<Expression> ConstructorBodies
    {
      get { return _constructorBodies; }
    }

    public bool IsModified ()
    {
      return HasAddedItems() || HasModifiedConstructors();
    }

    private bool HasAddedItems ()
    {
      return _proxyType.AddedCustomAttributes.Count > 0
             || _proxyType.AddedNestedTypes.Count > 0
             || _proxyType.AddedInterfaces.Count > 0
             || _proxyType.AddedFields.Count > 0
             // There are always ctors because they are copied from the base type.
             || _proxyType.AddedConstructors.Count > _constructorBodies.Count
             || _proxyType.AddedMethods.Count > 0;
      // Properties and events are covered via methods.
    }

    private bool HasModifiedConstructors ()
    {
      var ctors = _proxyType.AddedConstructors;
      return ctors.Any (c => c.AddedCustomAttributes.Count > 0) || !ctors.Select (c => c.Body).SequenceEqual (_constructorBodies);
    }
  }
}