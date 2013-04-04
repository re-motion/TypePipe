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
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation
{
  /// <summary>
  /// Holds the types that were loaded from a previously flushed assembly. Allows <see cref="IParticipant"/>s to rebuild their state, if necessary.
  /// </summary>
  public class LoadedTypesContext
  {
    private readonly ReadOnlyCollection<LoadedProxy> _proxyTypes;
    private readonly ReadOnlyCollection<Type> _additionalTypes;
    private readonly IDictionary<string, object> _state;

    public LoadedTypesContext (IEnumerable<Type> proxyTypes, IEnumerable<Type> additionalTypes, IDictionary<string, object> state)
    {
      ArgumentUtility.CheckNotNull ("proxyTypes", proxyTypes);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);
      ArgumentUtility.CheckNotNull ("state", state);

      _proxyTypes = proxyTypes.Select (p => new LoadedProxy (p)).ToList().AsReadOnly();
      _additionalTypes = additionalTypes.ToList().AsReadOnly();
      _state = state;
    }

    /// <summary>
    /// Gets the newly loaded proxy <see cref="Type"/>s.
    /// Note that proxy types that were already present in the cache are not contained in this collection.
    /// </summary>
    public ReadOnlyCollection<LoadedProxy> ProxyTypes
    {
      get { return _proxyTypes; }
    }

    /// <summary>
    /// Gets the loaded additional <see cref="Type"/>s.
    /// </summary>
    public ReadOnlyCollection<Type> AdditionalTypes
    {
      get { return _additionalTypes; }
    }

    /// <summary>
    /// A cache that <see cref="IParticipant"/>s can use to save state that should have the same lifetime as the generated types.
    /// </summary>
    public IDictionary<string, object> State
    {
      get { return _state; }
    }
  }
}