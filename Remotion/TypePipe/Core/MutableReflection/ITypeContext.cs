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
using Remotion.TypePipe.Caching;

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
  public interface ITypeContext
  {
    /// <summary>
    /// An event that is raised when the generation of types was completed. The generated members can be accessed
    /// by <see cref="GeneratedTypeContext.GetGeneratedMember"/>.
    /// </summary>
    event Action<GeneratedTypeContext> GenerationCompleted;

    /// <summary>
    /// The original <see cref="Type"/> that was requested by the user through an instance of <see cref="IObjectFactory"/>.
    /// </summary>
    Type RequestedType { get; }

    /// <summary>
    /// The mutable proxy type that was created by the pipeline for the <see cref="RequestedType"/>.
    /// </summary>
    MutableType ProxyType { get; }

    /// <summary>
    /// A cache stored in the scope of the respective <see cref="TypeCache"/> that is intended to hold state of the <see cref="IParticipant"/>s that
    /// should have the same cache lifetime as the generated types.
    /// </summary>
    IDictionary<string, object> State { get; }

    /// <summary>
    /// Gets the additional <see cref="MutableType"/>s that should be generated alongside with the <see cref="ProxyType"/>.
    /// </summary>
    ReadOnlyCollection<MutableType> AdditionalTypes { get; }

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> that should be generated.
    /// </summary>
    /// <param name="name">The type name.</param>
    /// <param name="namespace">The namespace of the type.</param>
    /// <param name="attributes">The type attributes.</param>
    /// <param name="baseType">The base type of the new type.</param>
    /// <returns>A new mutable type.</returns>
    MutableType CreateType (string name, string @namespace, TypeAttributes attributes, Type baseType);

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> representing an interface.
    /// </summary>
    /// <param name="name">The interface name.</param>
    /// <param name="namespace">The namespace of the interface.</param>
    /// <returns>A new mutable type representing an interface.</returns>
    MutableType CreateInterface (string name, string @namespace);

    /// <summary>
    /// Creates an additional <see cref="MutableType"/> that represents a proxy type for the specified base type.
    /// This method copies all accessible constructors of the base type.
    /// </summary>
    /// <param name="baseType">The proxied type.</param>
    /// <returns>A new mutable proxy type.</returns>
    MutableType CreateProxy (Type baseType);
  }
}