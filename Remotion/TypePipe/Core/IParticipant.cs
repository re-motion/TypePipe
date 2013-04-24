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
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Participates in the assembly of a type by calling mutating members of <see cref="MutableType"/>.
  /// Framework authors implement this interface in order to take part in the code generation process.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Instances of this interface are used to populate <see cref="IPipeline.Participants"/>.
  /// Every participant has the chance the specify type modifications via the provided <see cref="MutableType"/> instance.
  /// The <see cref="MutableType"/> is a representation of the type to be generated for the requested type.
  /// In addition, it contains all modifications applied by preceding participants in the pipeline.
  /// </para>
  /// <para>
  /// Note that implementations of this interface may be shared across multiple <see cref="IPipeline"/> instances.
  /// Participants therefore must not hold any mutable state directly.
  /// If there is the need to hold state a participant should use <see cref="TypeAssemblyContext.State"/>.
  /// </para>
  /// </remarks>
  [ConcreteImplementation ("Remotion.Mixins.CodeGeneration.TypePipe.MixinParticipant, Remotion.Mixins, "
                           + "Version=<version>, Culture=neutral, PublicKeyToken=<publicKeyToken>", ignoreIfNotFound: true, Position = 1)]
  [ConcreteImplementation ("Remotion.Data.DomainObjects.Infrastructure.TypePipe.DomainObjectParticipant, Remotion.Data.DomainObjects, "
                           + "Version=<version>, Culture=neutral, PublicKeyToken=<publicKeyToken>", ignoreIfNotFound: true, Position = 2)]
  [ConcreteImplementation (typeof (SerializationParticipant), Position = 3)]
  public interface IParticipant
  {
    /// <summary>
    /// A participant must provide a <see cref="ICacheKeyProvider"/> if generated types cannot be cached unconditionally, i.e.,
    /// the modifications depend not solely on the requested type.
    /// If generated types can be cached unconditionally, <see cref="PartialCacheKeyProvider"/> should return <see langword="null"/>.
    /// </summary>
    /// <value>
    /// The partial cache key provider, or <see langword="null"/>.
    /// </value>
    ICacheKeyProvider PartialCacheKeyProvider { get; }

    /// <summary>
    /// This method allows participants to specify their code generation needs.
    /// The provided <see cref="ITypeAssemblyContext"/> contains the type requested by the user and the mutable proxy type that was created for it by
    /// the pipeline.
    /// </summary>
    /// <param name="typeAssemblyContext">The type assembly context.</param>
    void Participate (ITypeAssemblyContext typeAssemblyContext);

    /// <summary>
    /// This method allows participants to react when the pipeline loads a set of types from a previously flushed assembly.
    /// </summary>
    /// <param name="loadedTypesContext">The loaded types context.</param>
    void RebuildState (LoadedTypesContext loadedTypesContext);

    // TODO 5370
    void HandleNonSubclassableType (Type requestedType);
  }
}