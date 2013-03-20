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

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Holds the <see cref="RequestedType"/> and <see cref="ProxyType"/> and allows generation of additional types.
  /// </summary>
  /// <remarks>
  /// The <see cref="ProxyType"/> represents the proxy type to be generated for the <see cref="RequestedType"/> including the modifications
  /// applied by preceding participants.
  /// Its mutating members (e.g. <see cref="MutableType.AddMethod"/>) can be used to specify the needed modifications.
  /// </remarks>
  public class TypeContext : IMutableTypeFactory
  {
    private readonly List<MutableType> _additionalTypes = new List<MutableType>();
    private readonly IMutableTypeFactory _mutableTypeFactory;
    private readonly Type _requestedType;
    private readonly MutableType _proxyType;
    private readonly IDictionary<string, object> _state;

    public TypeContext (IMutableTypeFactory mutableTypeFactory, Type requestedType, IDictionary<string, object> state)
    {
      ArgumentUtility.CheckNotNull ("mutableTypeFactory", mutableTypeFactory);
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);

      _mutableTypeFactory = mutableTypeFactory;
      _requestedType = requestedType;
      _proxyType = _mutableTypeFactory.CreateProxy (requestedType);
      _state = state;
    }

    /// <summary>
    /// The original <see cref="Type"/> that was requested by the user through an instance of <see cref="IObjectFactory"/>.
    /// </summary>
    public Type RequestedType
    {
      get { return _requestedType; }
    }

    /// <summary>
    /// The mutable proxy type that was created by the pipeline for the <see cref="RequestedType"/>.
    /// </summary>
    public MutableType ProxyType
    {
      get { return _proxyType; }
    }

    /// <summary>
    /// A global cache that is intended to hold the state of the <see cref="IParticipant"/>s.
    /// </summary>
    public IDictionary<string, object> State
    {
      get { return _state; }
    }

    /// <summary>
    /// Gets the additional <see cref="MutableType"/>s that should be generated alongside with the <see cref="ProxyType"/>.
    /// </summary>
    public ReadOnlyCollection<MutableType> AdditionalTypes
    {
      get { return _additionalTypes.AsReadOnly(); }
    }

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> that should be generated.
    /// </summary>
    /// <param name="name">The type name.</param>
    /// <param name="namespace">The namespace of the type.</param>
    /// <param name="attributes">The type attributes.</param>
    /// <param name="baseType">The base type of the new type.</param>
    /// <returns>A new mutable type.</returns>
    public MutableType CreateType (string name, string @namespace, TypeAttributes attributes, Type baseType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var type = _mutableTypeFactory.CreateType (name, @namespace, attributes, baseType);
      _additionalTypes.Add (type);

      return type;
    }

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> representing an interface.
    /// </summary>
    /// <param name="name">The interface name.</param>
    /// <param name="namespace">The namespace of the interface.</param>
    /// <returns>A new mutable type representing an interface.</returns>
    public MutableType CreateInterface (string name, string @namespace)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.

      var type = _mutableTypeFactory.CreateInterface (name, @namespace);
      _additionalTypes.Add (type);

      return type;
    }

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> that represents a proxy type for the specified base type.
    /// This method copies all accessible constructors of the base type.
    /// </summary>
    /// <param name="baseType">The proxied type.</param>
    /// <returns>A new mutable proxy type.</returns>
    public MutableType CreateProxy (Type baseType)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var type = _mutableTypeFactory.CreateProxy (baseType);
      _additionalTypes.Add (type);

      return type;
    }
  }
}